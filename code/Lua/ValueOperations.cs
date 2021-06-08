using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Lua
{
	class ValueOperations
	{
		public static ValueSlot Len(ValueSlot val)
		{
			if (val.Kind == ValueKind.Table)
			{
				return ValueSlot.Number( val.GetTable().GetLength() );
			} else if (val.Kind == ValueKind.String)
			{
				return ValueSlot.Number( val.GetString().Length );
			}
			throw new Exception( "Attemtp to get length of " + val );
		}
	}
}
