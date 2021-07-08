#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Miku.Lua
{
	partial class Executor
	{
		class ValueOperations
		{
			public static ValueSlot Len(ValueSlot val)
			{
				if (val.Kind == ValueKind.Table)
				{
					return val.CheckTable().GetLength();
				} else if (val.Kind == ValueKind.String)
				{
					return val.CheckString().Length;
				}
				throw new Exception( $"Attempt to get length of {val.Kind}." );
			}

			public static void Get(Executor ex, int out_slot, ValueSlot arg, ValueSlot key, ValueSlot? orig_arg = null )
			{
				bool is_table = arg.Kind == ValueKind.Table;
				if ( is_table )
				{
					var table = arg.CheckTable();
					var result = table.Get( key );
					if ( result.Kind != ValueKind.Nil )
					{
						ex.StackSet( out_slot, result );
						return;
					}
				}

				var meta_index = ex.Machine.PrimitiveMeta.Get(arg)?.Get("__index");

				if ( meta_index != null)
				{
					if (meta_index.Value.Kind == ValueKind.Table)
					{
						// DANGER: could result in an infinite loop!
						Get( ex, out_slot, meta_index.Value, key, arg );
						return;
					} else if (meta_index.Value.Kind == ValueKind.Function)
					{
						ex.CallPrepare( meta_index.Value, out_slot, 1 );
						ex.StackSet( 0, orig_arg ?? arg );
						ex.StackSet( 1, key );
						ex.CallArgsReady( 2 );
						return;
					}
				}

				if ( is_table )
				{
					ex.StackSet( out_slot, ValueSlot.NIL );
					// Print debug info, but only if this is the first lookup in the meta chain.
					//if (orig_arg == null)
					/*{
						var table = arg.CheckTable();
						if (table.DebugLibName != null)
						{
							Log.Warning( "GET " + table.DebugLibName + "." + key );
						}
					}*/
				} else
				{
					throw new Exception( "Attempt to index " + arg );
				}
			}

			/*private static ValueSlot MetaGet(Table mt, ValueSlot arg, ValueSlot key, PrimitiveMetaTables prim_meta )
			{
				var mt_index = mt.Get( "__index" );
				if ( mt_index.Kind == ValueKind.Table )
				{
					// TODO we might need the original arg, for __index functions?
					return Get( mt_index, key, prim_meta ); // TODO this might result in an infinite loop!
				}
				if (mt_index.Kind == ValueKind.Function)
				{
					throw new Exception( "Attempt to use function as index. " + mt_index.CheckFunction().Prototype.DebugName );
				}
				if (mt_index.Kind != ValueKind.Nil )
				{
					throw new Exception( $"Attempt to use {mt_index.Kind} as __index." );
				}
				return ValueSlot.NIL;
			}*/
		}
	}
}
