using System;
using System.Collections.Generic;
using System.Text;
using Sandbox;
using System.Diagnostics;

namespace Miku.Lua.Experimental
{
    class MikuDict
    {
		// Uses https://github.com/slembcke/Chipmunk2D/blob/master/src/prime.h
		// Based on http://planetmath.org/encyclopedia/GoodHashTablePrimes.html [dead link]
		readonly int[] PRIMES = new int[] { 13, 23, 47, 97, 193, 389, 769, 1543, 3079, 6151, 12289, 24593 };
		const int INIT_CAPACITY = 5;
		const float LOAD_FACTOR = 0.9f;

		// We don't bother with lazy init. The table that wraps us should take care of that.
		private ValueSlot[] Keys = new ValueSlot[INIT_CAPACITY];
		private ValueSlot[] Values = new ValueSlot[INIT_CAPACITY];
		// PSLs are offset by one. This is so we can quickly scan for empty slots with psl = 0
		private byte[] PSLs = new byte[5];

		public int Count { get; private set; }
		private int Capacity => Keys.Length;
		private int ResizeThreshold = (int)(INIT_CAPACITY * LOAD_FACTOR);

		public ValueSlot Get(ValueSlot key)
		{
			if (key.Kind == ValueKind.Nil)
			{
				return ValueSlot.NIL;
			}

			uint hash = (uint)key.GetHashCode();

			uint index = hash % (uint)Keys.Length;

			while (Keys[index].Kind != ValueKind.Nil)
			{
				if (Keys[index].FastEquals( key))
				{
					return Values[index];
				}

				index = (index + 1) % (uint)Keys.Length;
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

			uint hash = (uint)key.GetHashCode();

			uint index = hash % (uint)Keys.Length;
			byte psl = 1;

			while ( PSLs[index] != 0 )
			{
				// NOTE: this should not be an option once a slot is stolen
				if ( can_replace && Keys[index].FastEquals( key ) )
				{
					// Just replace the value
					Values[index] = val;
					return;
				}
				if ( psl > PSLs[index] )
				{
					// steal the slot
					var tmp_key = Keys[index];
					var tmp_val = Values[index];
					var tmp_psl = PSLs[index];
					Keys[index] = key;
					Values[index] = val;
					PSLs[index] = psl;
					key = tmp_key;
					val = tmp_val;
					psl = tmp_psl;
					// replacement should not be possible or necessary
					// once we're just shifting pairs around
					can_replace = false;
				}

				index = (index < Keys.Length-1) ? index + 1 : 0;
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
			Keys[index] = key;
			Values[index] = val;
			PSLs[index] = psl;
		}

		void Grow()
		{
			var old_keys = Keys;
			var old_values = Values;

			int new_size;
			for (int i=0; ;i++ )
			{
				if (PRIMES[i]>old_keys.Length)
				{
					new_size = PRIMES[i];
					break;
				}
			}

			Keys = new ValueSlot[new_size];
			Values = new ValueSlot[new_size];
			PSLs = new byte[new_size];
			Count = 0;
			ResizeThreshold = (int)(Capacity * LOAD_FACTOR);

			for (int i=0;i<old_keys.Length; i++)
			{
				if ( PSLs[i] != 0 )
				{
					Set( old_keys[i], old_values[i], false );
				}
			}
		}

		void Dump()
		{
			Log.Info( Count + " / " + Capacity );
			for (int i=0;i< Keys.Length; i++)
			{
				Log.Info( i + " " + Keys[i] + " " + Values[i] + " " + PSLs[i] + " " + (uint)Keys[i].GetHashCode() % (uint)Keys.Length );
			}
		}

		public static void Bench()
		{
			int REPEATS = 1000;
			int COUNT = 10000;
			var data = new long[COUNT, 2];
			var timer = new Stopwatch();
			for (int j=0;j<REPEATS;j++)
			{
				{
					var dict = new MikuDict();
					for (int i=0;i< COUNT; i++ )
					{
						timer.Restart();
						dict.Set( "key"+i.ToString(), "value"+i.ToString() );
						data[i, 0] += timer.Elapsed.Ticks;
					}
				}
				{
					var dict = new Dictionary<ValueSlot,ValueSlot>();
					for ( int i = 0; i < COUNT; i++ )
					{
						timer.Restart();
						dict.Add( "key" + i.ToString(), "value" + i.ToString() );
						data[i, 1] += timer.Elapsed.Ticks;
					}
				}
			}
			System.Console.WriteLine("Miku,Dict");
			data[0, 0] = 0;
			data[0, 1] = 0;
			long sum0 = 0;
			long sum1 = 0;
			for (int i=0;i<COUNT;i++ )
			{
				System.Console.WriteLine(data[i,0]+","+data[i,1]);
				sum0 += data[i, 0];
				sum1 += data[i, 1];
			}
			Log.Info( $"MIKU {sum0:n0}" );
			Log.Info( $".NET {sum1:n0}" );
		}
	}
}
