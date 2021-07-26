using System;
using System.Collections.Generic;
using Sandbox;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Miku.Lua.Experimental
{
    class MikuDict
    {
		private readonly struct KeyValuePair
		{
			public KeyValuePair(ValueSlot key, ValueSlot val)
			{
				Key = key;
				Value = val;
			}
			public readonly ValueSlot Key;
			public readonly ValueSlot Value;
		}

		// Uses https://github.com/slembcke/Chipmunk2D/blob/master/src/prime.h
		// Based on http://planetmath.org/encyclopedia/GoodHashTablePrimes.html [dead link]
		readonly int[] PRIMES = new int[] {
			13, 23, 47, 97, 193, 389, 769, 1543, 3079, 6151, 12289, 24593, 49157,
			98317, 196613, 393241, 786433, 1572869, 3145739
		};
		const int INIT_CAPACITY = 5;
		const float MAX_LOAD_FACTOR = 1.0f;

		// We don't bother with lazy init. The table that wraps us should take care of that.
		private KeyValuePair[] Pairs = new KeyValuePair[INIT_CAPACITY];
		private uint[] Hashes = new uint[INIT_CAPACITY];

		const uint HASH_EMPTY = 0;
		const uint HASH_TOMBSTONE = 1;
		// PSLs are offset by one. This is so we can quickly scan for empty slots with psl = 0
		//private byte[] PSLs = new byte[INIT_CAPACITY];
		// We cache hashes, which is useful for quickly rejecting possible matches and rehashing.
		//private uint[] Hashes = new uint[INIT_CAPACITY];

		public int Count { get; private set; }
		private int Capacity => Pairs.Length;
		private int ResizeThreshold = (int)(INIT_CAPACITY * MAX_LOAD_FACTOR);

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
			uint index = hash % (uint)Pairs.Length;

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
			var new_pair = new KeyValuePair( key, val );
			uint index = hash % (uint)Pairs.Length;

			while ( Hashes[index] != HASH_EMPTY )
			{
				// Short path: compare the hash before checking equality
				if ( hash == Hashes[index] && Pairs[index].Key.FastEquals( new_pair.Key ) )
				{
					// Just replace the value
					Pairs[index] = new_pair;
					return;
				}

				index = (index < Pairs.Length - 1) ? index + 1 : 0;
			}

			// Place
			Count++;
			Pairs[index] = new_pair;
			Hashes[index] = hash;
		}

		void Grow()
		{
			var old_pairs = Pairs;
			var old_hashes = Hashes;

			int new_size;
			for (int i=0; ;i++ )
			{
				if (PRIMES[i] > old_pairs.Length)
				{
					new_size = PRIMES[i];
					break;
				}
			}

			Pairs = new KeyValuePair[new_size];
			Hashes = new uint[new_size];
			ResizeThreshold = (int)(Capacity * MAX_LOAD_FACTOR);

			for (int i=0;i< old_pairs.Length; i++)
			{
				uint hash = old_hashes[i];
				if ( hash >= 2 )
				{
					uint index = hash % (uint)Pairs.Length;

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
			int SIZE = 10;
			//int REPEATS = 1000000;
			//int SIZE = 10;
			long sum_dict = 0;
			long sum_miku = 0;
			var timer = new Stopwatch();
			var keys = new ValueSlot[SIZE];
			var values = new ValueSlot[SIZE];
			for ( int i = 0; i < SIZE; i++ )
			{
				keys[i] = "key" + i.ToString();
				values[i] = "value" + i.ToString();
			}

			while (sum_dict < 10_000_000)
			{
				{
					timer.Restart();
					var dict = new MikuDict();
					for (int i=0;i< SIZE; i++ )
					{
						dict.Set( keys[i], values[i] );
					}
					sum_miku += timer.ElapsedTicks;
					
					for ( int i = 0; i < SIZE; i++ )
					{
						var res = dict.Get( keys[i] );
						if ( !res.FastEquals( values[i] ) )
						{
							throw new Exception("validation failed miku "+i+" "+res);
						}
					}
					dict.Validate();
				}
				{
					timer.Restart();
					var dict = new Dictionary<ValueSlot,ValueSlot>();
					for ( int i = 0; i < SIZE; i++ )
					{
						dict.Add( keys[i], values[i] );
					}
					sum_dict += timer.ElapsedTicks;
					
					for (int i = 0; i < SIZE; i++ )
					{
						var res = dict[keys[i]];
						if ( !res.FastEquals( values[i] ) )
						{
							throw new Exception( "validation failed dict " + i + " " + res );
						}
					}
				}
			}

			Log.Info( $"MIKU {sum_miku:n0}" );
			Log.Info( $".NET {sum_dict:n0}" );
			Log.Info( $"MIKU is {(float)sum_miku / sum_dict * 100}% of .NET" );
		}
	}
}
