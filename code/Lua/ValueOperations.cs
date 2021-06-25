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
				return ValueSlot.Number( val.CheckString().Length );
			}
			throw new Exception( $"Attempt to get length of {val.Kind}." );
		}

		public static ValueSlot Get(ValueSlot arg, ValueSlot key, PrimitiveMetaTables prim_meta )
		{
			if ( arg.Kind == ValueKind.Table )
			{
				var tab = arg.CheckTable();
				var result = tab.Get(key);
				if (result.Kind == ValueKind.Nil && tab.MetaTable != null )
				{
					return MetaGet(tab.MetaTable, arg, key, prim_meta);
				}
				if (result.Kind == ValueKind.Nil && tab.DebugLibName != null)
				{
					Sandbox.Log.Info( "GET " + tab.DebugLibName + "." + key );
				}
				return result;
			} else
			{
				var meta = prim_meta.Get(arg);
				if (meta != null)
				{
					var result = MetaGet( meta, arg, key, prim_meta );
					if (result.Kind != ValueKind.Nil)
					{
						return result;
					}
				}
				throw new Exception( $"Attempt to index {arg.Kind} {arg} with {key}." );
			}
		}

		private static ValueSlot MetaGet(Table mt, ValueSlot arg, ValueSlot key, PrimitiveMetaTables prim_meta )
		{
			var mt_index = mt.Get( "__index" );
			if ( mt_index.Kind == ValueKind.Table )
			{
				// TODO we might need the original arg, for __index functions?
				return Get( mt_index, key, prim_meta ); // TODO this might result in an infinite loop!
			}
			if (mt_index.Kind != ValueKind.Nil )
			{
				throw new Exception( $"Attempt to use {mt_index.Kind} as __index." );
			}
			return ValueSlot.NIL;
		}
	}
}
