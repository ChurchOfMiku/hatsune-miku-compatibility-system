using System;
using System.Collections.Generic;
using Sandbox;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Miku.Lua.Experimental
{
	// TODO BUG: A full table will never return when trying to access a missing key?
	// Should be fine as long as we never allow full tables, which we don't.
    class MikuDict
    {
		private struct KeyValuePair
		{
			public void Set( ValueSlot key, ValueSlot val )
			{
				Key = key;
				Value = val;
			}
			public ValueSlot Key;
			public ValueSlot Value;
		}

		private struct HashIndex
		{
			public void Set(uint hash, uint index)
			{
				Hash = hash;
				Index = index;
			}
			public uint Hash;
			public uint Index;
			//public uint Distance;
		}

		// Uses https://github.com/slembcke/Chipmunk2D/blob/master/src/prime.h
		// Based on http://planetmath.org/encyclopedia/GoodHashTablePrimes.html [dead link]
		const int INIT_CAPACITY = 5;
		readonly int[] LUT_CAPACITY = new int[] {
			5, 13, 23, 47, 97, 193, 389, 769, 1543, 3079, 6151, 12289, 24593, 49157,
			98317, 196613, 393241, 786433, 1572869, 3145739
		};

		private uint ComputeIndex(uint hash)
		{
			return FastMod(hash, (uint)HashIndices.Length, FastModMul);
		}

		const float RESIZE_THRESHOLD = 0.75f;

		const uint HASH_EMPTY = 0;
		const uint HASH_TOMBSTONE = 1;

		// We don't bother with lazy init. The table that wraps us should take care of that.
		private KeyValuePair[] PairChunk = new KeyValuePair[INIT_CAPACITY];
		// Hashes are cached for faster lookups and rehashes.
		// They are also used to quickly scan for empty slots.
		private HashIndex[] HashIndices = new HashIndex[INIT_CAPACITY];
		// If used to index into LUT_CAPACITY, this should give us our current capacity.
		private int SizeIndex = 0;
		private ulong FastModMul = GetFastModMultiplier(INIT_CAPACITY);

		public int Count { get; private set; }
		private int Capacity => HashIndices.Length;
		private int ResizeThreshold = (int)(INIT_CAPACITY * RESIZE_THRESHOLD);

		public ValueSlot Get(ValueSlot key)
		{
			if (key.Kind == ValueKind.Nil)
			{
				return ValueSlot.NIL;
			}

			uint hash = Math.Max( (uint)key.GetHashCode(), 2 );

			uint index = ComputeIndex( hash );

			while ( HashIndices[index].Hash != HASH_EMPTY )
			{
				if ( hash == HashIndices[index].Hash && PairChunk[HashIndices[index].Index].Key.FastEquals(key))
				{
					return PairChunk[HashIndices[index].Index].Value;
				}

				index = (index < HashIndices.Length - 1) ? index + 1 : 0;
			}

			return ValueSlot.NIL;
		}

		public void Set( ValueSlot key, ValueSlot val )
		{
			if ( key.Kind == ValueKind.Nil )
			{
				return;
			}
			if ( val.Kind == ValueKind.Nil )
			{
				throw new Exception( "TODO deletion" );
			}
			if ( Count >= ResizeThreshold )
			{
				Grow();
			}
			uint hash = Math.Max( (uint)key.GetHashCode(), 2);
			
			uint index = ComputeIndex( hash );

			while ( true )
			{
				ref HashIndex current_hi = ref HashIndices[index];
				// Empty slot, insert.
				if (current_hi.Hash == HASH_EMPTY)
				{
					uint pair_index = (uint)Count; // TODO we're going to need to do other stuff here if we want to handle deletes
					Count++;
					PairChunk[pair_index].Set( key, val ); // this is measurably faster than constructing a new Pair.
					current_hi.Set( hash, pair_index );
					return;
				}
				// Short path: compare the hash before checking equality
				if ( hash == current_hi.Hash && PairChunk[current_hi.Index].Key.FastEquals( key ) )
				{
					// Just replace the value
					PairChunk[current_hi.Index].Value = val;
					return;
				}

				index = (index < HashIndices.Length - 1) ? index + 1 : 0;
				//index = Math.Min( index + 1, index - (uint)HashIndices.Length + 1 );
			}
		}

		void Grow()
		{
			var old_chunk = PairChunk;
			var old_his = HashIndices;

			SizeIndex++;
			int new_size = LUT_CAPACITY[SizeIndex];

			PairChunk = new KeyValuePair[new_size];
			HashIndices = new HashIndex[new_size];
			ResizeThreshold = (int)(Capacity * RESIZE_THRESHOLD);
			FastModMul = GetFastModMultiplier( (uint)new_size );

			Array.Copy( old_chunk, PairChunk, old_chunk.Length );

			for (int i=0;i< old_his.Length; i++)
			{
				HashIndex old_hi = old_his[i];
				if ( old_hi.Hash >= 2 )
				{
					uint index = ComputeIndex( old_hi.Hash );

					// Skip filled slots. No need to check for equality, we know all keys are unique.
					while ( true )
					{
						ref HashIndex new_hi = ref HashIndices[index];
						if ( new_hi.Hash == HASH_EMPTY ) {
							// Place
							new_hi = old_hi;
							break;
						}
						index = (index < HashIndices.Length - 1) ? index + 1 : 0;
					}
				}
			}
		}

		void Validate()
		{
			/*for ( int i = 0; i < Pairs.Length; i++ )
			{
				if (PSLs[i] == 0)
				{
					if ( Pairs[i].Key.Kind != ValueKind.Nil || Pairs[i].Value.Kind != ValueKind.Nil ) {
						throw new Exception( "bad slot, should be empty" );
					}
				} else
				{
					uint goal_slot = (uint)(Pairs[i].Key.GetHashCode() + PSLs[i] - 1) % (uint)Pairs.Length;
					if (i != goal_slot)
					{
						throw new Exception( "bad slot, hash/psl mismatch" );
					}
				}
			}*/
		}

		void FindClusters()
		{
			var dist_map = new SortedDictionary<uint, uint>();
			for (int i=0;i<HashIndices.Length;i++ )
			{
				if (HashIndices[i].Hash != HASH_EMPTY)
				{
					int count = 0;
					while (HashIndices[(i+count) % HashIndices.Length].Hash != HASH_EMPTY)
					{
						count++;
					}
					Log.Info( "C = " + count );
				}
			}
			Log.Info( "LF = " + ((float)Count / (float)HashIndices.Length) );
			throw new Exception( "stop" );
		}

		void Dump()
		{
			/*Log.Info( Count + " / " + Capacity );
			for (int i=0;i< Pairs.Length; i++)
			{
				Log.Info( i + " " + Pairs[i].Key + " " + Pairs[i].Value + " " + Hashes[i] + " " + (uint)Pairs[i].Key.GetHashCode() % (uint)Pairs.Length );
			}*/
		}

		public static void Bench()
		{
			int SIZE = 35;
			long sum_dict = 0;
			long sum_miku = 0;
			var timer = new Stopwatch();
			var keys = new ValueSlot[SIZE];
			var values = new ValueSlot[SIZE];
			var data = new long[SIZE, 2];
			for ( int i = 0; i < SIZE; i++ )
			{
				keys[i] = "key" + i.ToString();
				values[i] = "value" + i.ToString();
			}

			while (sum_dict < 10_000_000)
			{
				{
					var dict = new MikuDict();
					for (int i=0;i< SIZE; i++ )
					{
						timer.Restart();
						dict.Set( keys[i], values[i] );
						data[i,0] += timer.ElapsedTicks;
						sum_miku += timer.ElapsedTicks;
					}
					//dict.FindClusters();
					for ( int i = 0; i < SIZE; i++ )
					{
						//timer.Restart();
						var res = dict.Get( keys[i] );
						//data[i, 0] += timer.ElapsedTicks;
						//sum_miku += timer.ElapsedTicks;
						if ( !res.FastEquals( values[i] ) )
						{
							throw new Exception("validation failed miku "+i+" "+res);
						}
					}
					dict.Validate();
				}
				{
					var dict = new Dictionary<ValueSlot,ValueSlot>();
					for ( int i = 0; i < SIZE; i++ )
					{
						timer.Restart();
						dict.Add( keys[i], values[i] );
						data[i, 1] += timer.ElapsedTicks;
						sum_dict += timer.ElapsedTicks;
					}
					
					for (int i = 0; i < SIZE; i++ )
					{
						//timer.Restart();
						var res = dict[keys[i]];
						//data[i, 1] += timer.ElapsedTicks;
						//sum_dict += timer.ElapsedTicks;
						if ( !res.FastEquals( values[i] ) )
						{
							throw new Exception( "validation failed dict " + i + " " + res );
						}
					}
				}
			}

			if (SIZE <= 1000)
			{
				System.Console.WriteLine("MIKU,NET");
				for ( int i = 0; i < SIZE; i++ )
				{
					System.Console.WriteLine(data[i,0]+","+data[i,1]);
				}
			}

			Log.Info( $"MIKU {sum_miku:n0}" );
			Log.Info( $".NET {sum_dict:n0}" );
			Log.Info( $"MIKU is {(float)sum_miku / sum_dict * 100}% of .NET" );
		}

		// From https://source.dot.net/#System.Private.CoreLib/HashHelpers.cs,de3ba4873d4ad06a

		/// <summary>Returns approximate reciprocal of the divisor: ceil(2**64 / divisor).</summary>
		/// <remarks>This should only be used on 64-bit.</remarks>
		public static ulong GetFastModMultiplier( uint divisor ) =>
			ulong.MaxValue / divisor + 1;

		/// <summary>Performs a mod operation using the multiplier pre-computed with <see cref="GetFastModMultiplier"/>.</summary>
		/// <remarks>This should only be used on 64-bit.</remarks>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static uint FastMod( uint value, uint divisor, ulong multiplier )
		{
			// We use modified Daniel Lemire's fastmod algorithm (https://github.com/dotnet/runtime/pull/406),
			// which allows to avoid the long multiplication if the divisor is less than 2**31.
			// This is equivalent of (uint)Math.BigMul(multiplier * value, divisor, out _). This version
			// is faster than BigMul currently because we only need the high bits.
			uint highbits = (uint)(((((multiplier * value) >> 32) + 1) * divisor) >> 32);
			return highbits;
		}

	}
}
