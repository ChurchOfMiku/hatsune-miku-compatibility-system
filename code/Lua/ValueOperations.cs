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

				var meta_index = ex.Machine.PrimitiveMeta.Get(arg)?.Get("__index") ?? ValueSlot.NIL;


				if (meta_index.Kind == ValueKind.Table)
				{
					// DANGER: could result in an infinite loop!
					Get( ex, out_slot, meta_index, key, orig_arg ?? arg );
					return;
				} else if (meta_index.Kind == ValueKind.Function)
				{
					ex.CallPrepare( meta_index, out_slot, 1 );
					ex.StackSet( 0, orig_arg ?? arg );
					ex.StackSet( 1, key );
					ex.CallArgsReady( 2 );
					return;
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

			public static void Set(Executor ex, ValueSlot arg, ValueSlot key, ValueSlot val, ValueSlot? orig_arg = null)
			{
				var meta_new_index = ex.Machine.PrimitiveMeta.Get( arg )?.Get( "__newindex" ) ?? ValueSlot.NIL;

				bool is_table = arg.Kind == ValueKind.Table;
				// Handle the simple case: No __newindex method.
				if (meta_new_index.Kind == ValueKind.Nil)
				{
					if ( is_table )
					{
						arg.CheckTable().Set( key, val );
						return;
					}
					else
					{
						throw new Exception( "Attempt to index " + arg );
					}
				}

				// If the key already exists on a table, we set it without using __newindex.
				if ( is_table )
				{
					var table = arg.CheckTable();
					if (table.Get(key).Kind != ValueKind.Nil)
					{
						table.Set(key,val);
						return;
					}
				}

				if ( meta_new_index.Kind == ValueKind.Table )
				{
					// DANGER: could result in an infinite loop!
					Set( ex, meta_new_index, key, val, orig_arg ?? arg );
					return;
				}
				else if ( meta_new_index.Kind == ValueKind.Function )
				{
					ex.CallPrepare( meta_new_index );
					ex.StackSet( 0, orig_arg ?? arg );
					ex.StackSet( 1, key );
					ex.StackSet( 2, val );
					ex.CallArgsReady( 3 );
					return;
				}

				throw new Exception( "Attempt to index " + arg + " -- meta = " + meta_new_index );

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
