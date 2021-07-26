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
			/*public KeyValuePair(ValueSlot key, ValueSlot val)
			{
				Key = key;
				Value = val;
			}*/
			public void Set( ValueSlot key, ValueSlot val )
			{
				Key = key;
				Value = val;
			}
			public ValueSlot Key;
			public ValueSlot Value;
		}

		// Uses https://github.com/slembcke/Chipmunk2D/blob/master/src/prime.h
		// Based on http://planetmath.org/encyclopedia/GoodHashTablePrimes.html [dead link]
		const int INIT_CAPACITY = 5;
		readonly int[] LUT_CAPACITY = new int[] {
			5, 13, 23, 47, 97, 193, 389, 769, 1543, 3079, 6151, 12289, 24593, 49157,
			98317, 196613, 393241, 786433, 1572869, 3145739
		};
		const float RESIZE_THRESHOLD = 0.75f;

		private uint ComputeIndex(uint hash)
		{
			switch ( SizeIndex )
			{
				case 0: return hash % 5;
				case 1: return hash % 13;
				case 2: return hash % 23;
				case 3: return hash % 47;
				case 4: return hash % 97;
				case 5: return hash % 193;
				case 6: return hash % 389;
				case 7: return hash % 769;
				case 8: return hash % 1543;
				case 9: return hash % 3079;
				default: return hash % (uint)Pairs.Length;
			}
		}

		const uint HASH_EMPTY = 0;
		const uint HASH_TOMBSTONE = 1;

		// We don't bother with lazy init. The table that wraps us should take care of that.
		private KeyValuePair[] Pairs = new KeyValuePair[INIT_CAPACITY];
		// Hashes are cached for faster lookups and rehashes.
		// They are also used to quickly scan for empty slots.
		private uint[] Hashes = new uint[INIT_CAPACITY];
		// If used to index into LUT_CAPACITY, this should give us our current capacity.
		private int SizeIndex = 0;

		public int Count { get; private set; }
		private int Capacity => Pairs.Length;
		private int ResizeThreshold = (int)(INIT_CAPACITY * RESIZE_THRESHOLD);

		public ValueSlot Get(ValueSlot key)
		{
			if (key.Kind == ValueKind.Nil)
			{
				return ValueSlot.NIL;
			}

			uint hash = (uint)key.GetHashCode();
			if ( hash < 2 )
			{
				hash += 2;
			}

			uint index = ComputeIndex( hash );

			while ( Hashes[index] != HASH_EMPTY )
			{
				if ( hash == Hashes[index] && Pairs[index].Key.FastEquals(key))
				{
					return Pairs[index].Value;
				}

				index = (index < Pairs.Length - 1) ? index + 1 : 0;
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
			uint hash = (uint)key.GetHashCode();
			if ( hash < 2 )
			{
				hash += 2;
			}
			
			uint index = ComputeIndex( hash );
			uint current_hash;
			while ( (current_hash = Hashes[index]) != HASH_EMPTY )
			{
				// Short path: compare the hash before checking equality
				if ( hash == current_hash && Pairs[index].Key.FastEquals( key ) )
				{
					// Just replace the value
					Pairs[index].Value = val;
					return;
				}

				index = (index < Pairs.Length - 1) ? index + 1 : 0;
			}

			// Place
			Count++;
			Pairs[index].Set( key, val ); // this is measurably faster than constructing a new Pair.
			Hashes[index] = hash;
		}

		void Grow()
		{
			var old_pairs = Pairs;
			var old_hashes = Hashes;

			SizeIndex++;
			int new_size = LUT_CAPACITY[SizeIndex];

			Pairs = new KeyValuePair[new_size];
			Hashes = new uint[new_size];
			ResizeThreshold = (int)(Capacity * RESIZE_THRESHOLD);

			for (int i=0;i< old_pairs.Length; i++)
			{
				uint hash = old_hashes[i];
				if ( hash >= 2 )
				{
					uint index = ComputeIndex( hash );

					// Skip filled slots. No need to check for equality, we know all keys are unique.
					while ( Hashes[index] != HASH_EMPTY )
					{
						index = (index < Pairs.Length - 1) ? index + 1 : 0;
					}

					// Place
					Pairs[index] = old_pairs[i];
					Hashes[index] = hash;
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

		void Dump()
		{
			Log.Info( Count + " / " + Capacity );
			for (int i=0;i< Pairs.Length; i++)
			{
				Log.Info( i + " " + Pairs[i].Key + " " + Pairs[i].Value + " " + Hashes[i] + " " + (uint)Pairs[i].Key.GetHashCode() % (uint)Pairs.Length );
			}
		}

		public static void Bench()
		{
			int SIZE = 144;
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

			System.Console.WriteLine("MIKU,NET");
			for ( int i = 0; i < SIZE; i++ )
			{
				System.Console.WriteLine(data[i,0]+","+data[i,1]);
			}

			Log.Info( $"MIKU {sum_miku:n0}" );
			Log.Info( $".NET {sum_dict:n0}" );
			Log.Info( $"MIKU is {(float)sum_miku / sum_dict * 100}% of .NET" );
		}
	}
}
