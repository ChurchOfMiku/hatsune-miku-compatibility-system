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
				return ValueSlot.Number( val.CheckTable().GetLength() );
			} else if (val.Kind == ValueKind.String)
			{
				return ValueSlot.Number( val.GetString().Length );
			}
			throw new Exception( $"Attempt to get length of {val.Kind}." );
		}

		public static ValueSlot Get(ValueSlot arg, ValueSlot key)
		{
			if ( arg.Kind == ValueKind.Table )
			{
				var tab = arg.CheckTable();
				var result = tab.Get(key);
				if (result.Kind == ValueKind.Nil && tab.MetaTable != null )
				{
					var mt_index = tab.MetaTable.Get( "__index" );
					if (mt_index.Kind == ValueKind.Table)
					{
						return mt_index.CheckTable().Get( key );
					} else
					{
						throw new Exception( $"Attempt to use {mt_index.Kind} as __index." );
					}
				}
				return result;
			} else
			{
				throw new Exception( $"Attempt to index {arg.Kind}." );
			}
		}
	}
}
