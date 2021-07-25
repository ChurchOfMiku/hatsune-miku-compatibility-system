using System;
using System.Collections.Generic;
using System.Text;
using Sandbox;
using System.Diagnostics;

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
		const float LOAD_FACTOR = 0.9f;

		// We don't bother with lazy init. The table that wraps us should take care of that.
		private KeyValuePair[] Pairs = new KeyValuePair[INIT_CAPACITY];
		// PSLs are offset by one. This is so we can quickly scan for empty slots with psl = 0
		private byte[] PSLs = new byte[5];

		public int Count { get; private set; }
		private int Capacity => Pairs.Length;
		private int ResizeThreshold = (int)(INIT_CAPACITY * LOAD_FACTOR);

		public ValueSlot Get(ValueSlot key)
		{
			if (key.Kind == ValueKind.Nil)
			{
				return ValueSlot.NIL;
			}

			uint hash = (uint)key.GetHashCode();

			uint index = hash % (uint)Pairs.Length;

			while ( PSLs[index] != 0 )
			{
				if (Pairs[index].Key.FastEquals( key))
				{
					return Pairs[index].Value;
				}

				index = (index < Pairs.Length - 1) ? index + 1 : 0;
			}

			return ValueSlot.NIL;
		}

		/*public void Set( ValueSlot key, ValueSlot val )
		{
			SetInternal( key, val, true );
		}*/

		private void Set( ValueSlot key, ValueSlot val, bool can_replace = true )
		{
			if ( key.Kind == ValueKind.Nil )
			{
				return;
			}
			if ( val.Kind == ValueKind.Nil )
			{
				throw new Exception( "TODO deletion" );
			}
			if (Count >= ResizeThreshold)
			{
				//Log.Info( "grow from "+Capacity );
				Grow();
			}
			var new_pair = new KeyValuePair( key, val );

			uint hash = (uint)key.GetHashCode();

			uint index = hash % (uint)Pairs.Length;
			byte psl = 1;

			while ( PSLs[index] != 0 )
			{
				// NOTE: this should not be an option once a slot is stolen
				if ( can_replace && Pairs[index].Key.FastEquals( key ) )
				{
					// Just replace the value
					Pairs[index] = new_pair;
					return;
				}
				if ( psl > PSLs[index] )
				{
					// steal the slot
					var tmp_pair = Pairs[index];
					var tmp_psl = PSLs[index];
					Pairs[index] = new_pair;
					PSLs[index] = psl;
					new_pair = tmp_pair;
					psl = tmp_psl;
					// replacement should not be possible or necessary
					// once we're just shifting pairs around
					can_replace = false;
				}

				index = (index < Pairs.Length-1) ? index + 1 : 0;
				if ( psl == 255 )
				{
					// Don't allow probe lengths to outgrow a byte.
					Grow();
					Set( key, val );
					return;
				}
				psl++;
			}

			// Place
			Count++;
			Pairs[index] = new_pair;
			PSLs[index] = psl;
		}

		void Grow()
		{
			var old_pairs = Pairs;
			var old_psls = PSLs;

			int new_size;
			for (int i=0; ;i++ )
			{
				if (PRIMES[i]> old_pairs.Length)
				{
					new_size = PRIMES[i];
					break;
				}
			}

			Pairs = new KeyValuePair[new_size];
			PSLs = new byte[new_size];
			Count = 0;
			ResizeThreshold = (int)(Capacity * LOAD_FACTOR);

			for (int i=0;i< old_pairs.Length; i++)
			{
				if ( old_psls[i] != 0 )
				{
					Set( old_pairs[i].Key, old_pairs[i].Value, false );
				}
			}
		}

		void Validate()
		{
			for ( int i = 0; i < Pairs.Length; i++ )
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
			}
		}

		void Dump()
		{
			Log.Info( Count + " / " + Capacity );
			for (int i=0;i< Pairs.Length; i++)
			{
				Log.Info( i + " " + Pairs[i].Key + " " + Pairs[i].Value + " " + PSLs[i] + " " + (uint)Pairs[i].Key.GetHashCode() % (uint)Pairs.Length );
			}
		}

		public static void Bench()
		{
			int REPEATS = 1;
			int COUNT = 1_000_000;
			long sum_dict = 0;
			long sum_miku = 0;
			var timer = new Stopwatch();
			for (int j=0;j<REPEATS;j++)
			{
				{
					timer.Restart();
					var dict = new MikuDict();
					for (int i=0;i< COUNT; i++ )
					{
						dict.Set( "key"+i.ToString(), "value"+i.ToString() );
					}
					
					for ( int i = 0; i < COUNT; i++ )
					{
						var res = dict.Get( "key" + i.ToString() ).CheckString();
						if (res != "value"+i.ToString() )
						{
							throw new Exception("validation failed miku "+i+" "+res);
						}
					}
					sum_miku += timer.ElapsedTicks;
					dict.Validate();
				}
				{
					timer.Restart();
					var dict = new Dictionary<ValueSlot,ValueSlot>();
					for ( int i = 0; i < COUNT; i++ )
					{
						dict.Add( "key" + i.ToString(), "value" + i.ToString() );
					}
					
					for (int i = 0; i < COUNT; i++ )
					{
						var res = dict["key" + i.ToString()].CheckString();
						if ( res != "value" + i.ToString() )
						{
							throw new Exception( "validation failed dict " + i + " " + res );
						}
					}
					sum_dict += timer.ElapsedTicks;
				}
			}

			Log.Info( $"MIKU {sum_miku:n0}" );
			Log.Info( $".NET {sum_dict:n0}" );
		}
	}
}
