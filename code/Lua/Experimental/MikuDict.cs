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
		readonly int[] PRIMES = new int[] { 13, 23, 47, 97, 193, 389, 769, 1543, 3079 };


		// We don't bother with lazy init. The table that wraps us should take care of that.
		private ValueSlot[] Keys = new ValueSlot[5];
		private ValueSlot[] Values = new ValueSlot[5];
		private byte[] PSLs = new byte[5];

		public int Count { get; private set; }
		private int Capacity => Keys.Length;

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
				if (Keys[index].Equals(key))
				{
					return Values[index];
				}

				index = (index + 1) % (uint)Keys.Length;
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

			uint hash = (uint)key.GetHashCode();

			uint index = hash % (uint)Keys.Length;
			byte psl = 0;

			while ( Keys[index].Kind != ValueKind.Nil )
			{
				// NOTE: this should not be an option once a slot is stolen
				if ( Keys[index].Equals( key ) )
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
				}

				index = (index + 1) % (uint)Keys.Length;
				if ( psl == 255 || psl >= Keys.Length )
				{
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

			for (int i=0;i<old_keys.Length; i++)
			{
				if ( old_keys[i].Kind != ValueKind.Nil)
				{
					Set( old_keys[i], old_values[i] );
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
			int COUNT = 1000;
			{
				Stopwatch timer = new();
				timer.Start();
				var dict = new MikuDict();
				for (int i=0;i< COUNT; i++ )
				{
					dict.Set( "key"+i.ToString(), "value"+i.ToString() );
				}
				var elapsed = timer.Elapsed.TotalMilliseconds;
				//dict.Dump();
				Log.Info( "MIKU " + elapsed );
			}
			{
				Stopwatch timer = new();
				timer.Start();
				var dict = new Dictionary<ValueSlot,ValueSlot>();
				for ( int i = 0; i < COUNT; i++ )
				{
					dict.Add( "key" + i.ToString(), "value" + i.ToString() );
				}
				var elapsed = timer.Elapsed.TotalMilliseconds;
				//dict.Dump();
				Log.Info( "DICT " + elapsed );
			}
		}
	}
}
