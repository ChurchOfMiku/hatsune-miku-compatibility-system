using System.Collections.Generic;

namespace Miku.Lua
{
	class Table
	{
		private List<ValueSlot> array = new List<ValueSlot>();
		private Dictionary<ValueSlot, ValueSlot> dict = new Dictionary<ValueSlot, ValueSlot>();
		public void PushVal( ValueSlot val )
		{
			this.array.Add( val );
		}

		public void SetKeyVal(ValueSlot key, ValueSlot val)
		{
			this.dict.Add( key, val );
		}
	}
}
