using System.Collections.Generic;

namespace Miku.Lua
{
	class Table
	{
		//private List<ValueSlot> array = new List<ValueSlot>();
		private int next_int_slot = 1;
		private Dictionary<ValueSlot, ValueSlot> dict = new Dictionary<ValueSlot, ValueSlot>();

		public int GetLength()
		{
			return next_int_slot - 1;
		}

		public void PushVal( ValueSlot val )
		{
			Set( ValueSlot.Number( next_int_slot ), val );
		}

		public void Set( ValueSlot key, ValueSlot val )
		{
			dict[key] = val;
			// Increment our array length.
			// TODO actually migrate values to array portion.
			while (dict.ContainsKey( ValueSlot.Number(next_int_slot) )) {
				next_int_slot++;
			}
		}

		// For convenience.
		public void Set( string key, ValueSlot val )
		{
			Set( ValueSlot.String( key ), val );
		}

		// Special case for integer keys: Try using the array section first.
		public void Set( int key, ValueSlot val )
		{
			Set( ValueSlot.Number( key ), val );
		}

		public ValueSlot Get( ValueSlot key )
		{
			ValueSlot result;
			if (!dict.TryGetValue( key, out result ))
			{
				result = ValueSlot.Nil();
			}
			return result;
		}

		// For convenience.
		public ValueSlot Get( string key )
		{
			return Get( ValueSlot.String( key ) );
		}

		// Special case for integer keys: Try using the array section first.
		public ValueSlot Get( int key )
		{
			return Get( ValueSlot.Number( key ) );
		}

		public Table CloneProto()
		{
			var result = new Table();
			/*for (int i=0;i<this.array.Count;i++ )
			{
				result.PushVal(this.array[i].CloneCheck());
			}*/
			foreach (var pair in this.dict)
			{
				result.Set( pair.Key.CloneCheck(), pair.Value.CloneCheck() );
			}
			return result;
		}

		public void Log()
		{
			Sandbox.Log.Info( $"LEN = {GetLength()}" );
			foreach ( var pair in this.dict )
			{
				Sandbox.Log.Info( $"[{pair.Key}] = {pair.Value}" );
			}
		}
	}
}
