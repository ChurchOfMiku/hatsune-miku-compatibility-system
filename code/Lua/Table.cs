#nullable enable

using System.Collections.Generic;
using System;

using Sandbox;

namespace Miku.Lua
{
	class Table
	{
		private Dictionary<ValueSlot, ValueSlot>? Dict = null;
		private List<ValueSlot>? Array = null;

		public object? UserData = null;
		public Table? MetaTable = null;
		public string? DebugLibName = null;

		public int GetLength()
		{
			if (Array != null)
			{
				int len;
				for ( len = Array.Count;len>0;len--)
				{
					if (ArrayGet( len ).Kind != ValueKind.Nil)
					{
						break;
					}
				}
				return len;
			}
			return 0;
		}

		private ValueSlot ArrayGet(int i) { return Array![i - 1]; }
		private void ArraySet( int i, ValueSlot val ) { Array![i - 1] = val; }

		private void MigrateSubsequentKeys(int i)
		{
			if (Dict == null)
			{
				// Nothing to migrate.
				return;
			}

			while (true)
			{
				i++;

				ValueSlot result;
				if ( !Dict.TryGetValue( ValueSlot.Number(i), out result ) )
				{
					return;
				}
				Array!.Add(result);
			}
		}

		public void PushVal( ValueSlot val )
		{
			Set( ValueSlot.Number( GetLength() + 1 ), val );
		}

		public void Set( ValueSlot key, ValueSlot val )
		{
			if (key.Kind == ValueKind.Number)
			{
				var i_dbl = key.CheckNumber();
				int i = (int)i_dbl;
				if (i_dbl == i && i >= 1)
				{
					if (Array == null)
					{
						if (i == 1)
						{
							if (val.Kind == ValueKind.Nil)
							{
								return;
							}
							Array = new List<ValueSlot>();
							Array.Add(val);
							MigrateSubsequentKeys(i);
							return;
						}
					} else
					{
						if (i == Array.Count + 1)
						{
							if ( val.Kind == ValueKind.Nil )
							{
								return;
							}
							Array.Add( val );
							MigrateSubsequentKeys( i );
							return;
						} else if (i <= Array.Count)
						{
							ArraySet( i, val );
							// no need to migrate keys here, we're not touching the end
							return;
						}
					}
				}
			}

			if (Dict == null)
			{
				if ( val.Kind == ValueKind.Nil )
				{
					return;
				}
				Dict = new Dictionary<ValueSlot, ValueSlot>();
			}
			if ( val.Kind == ValueKind.Nil )
			{
				Dict.Remove( key );
			} else
			{
				Dict[key] = val;
			}
		}

		public void Set( string key, ValueSlot val ) { Set( ValueSlot.String( key ), val ); }
		public void Set( int key, ValueSlot val ) { Set( ValueSlot.Number( key ), val ); }

		public ValueSlot Get( ValueSlot key )
		{
			if (Array != null)
			{
				if ( key.Kind == ValueKind.Number )
				{
					var i_dbl = key.CheckNumber();
					int i = (int)i_dbl;
					if ( i_dbl == i && i >= 1 && i <= Array.Count )
					{
						return ArrayGet( i );
					}
				}
			}

			if (Dict != null)
			{
				ValueSlot result;
				if (Dict.TryGetValue( key, out result ))
				{
					return result;
				}
			}

			return ValueSlot.NIL;
		}

		public ValueSlot Get( string key ) { return Get( ValueSlot.String( key ) ); }
		public ValueSlot Get( int key ) { return Get( ValueSlot.Number( key ) ); }

		// Looking at the reference source, it should be safe to not worry about disposing enumerators.
		private Dictionary<ValueSlot,ValueSlot>.Enumerator CachedEnumerator;
		private void SetupEnumerator( ValueSlot prev_key )
		{

		}
		public void Next(ValueSlot prev_key, Executor ex)
		{
			// Start enumeration.
			if (prev_key.Kind == ValueKind.Nil)
			{
				if (Array != null && Array.Count > 0)
				{
					ex.Return( ValueSlot.Number( 1 ) );
					ex.Return( ArrayGet( 1 ) );
					return;
				}
				if (Dict != null && Dict.Count > 0)
				{
					CachedEnumerator = Dict.GetEnumerator();
					CachedEnumerator.MoveNext();

					var pair = CachedEnumerator.Current;
					ex.Return( pair.Key );
					ex.Return( pair.Value );
					return;
				}
			} else
			{
				// Try the next array slot.
				if ( Array != null )
				{
					if ( prev_key.Kind == ValueKind.Number )
					{
						var i_dbl = prev_key.CheckNumber() + 1;
						int i = (int)i_dbl;
						if ( i_dbl == i && i >= 1 && i <= Array.Count )
						{
							ex.Return( ValueSlot.Number( i ) );
							ex.Return( ArrayGet( i ) );
							return;
						}
					}
				}
				if (Dict != null)
				{
					// TODO: the enumerator might be totally invalid, check for exceptions
					if (CachedEnumerator.Current.Key.Equals(prev_key))
					{
						if (CachedEnumerator.MoveNext())
						{
							var pair = CachedEnumerator.Current;
							ex.Return( pair.Key );
							ex.Return( pair.Value );
							return;
						} else
						{
							return;
						}
					}
					throw new Exception( "cached enumerator miss" );
				}
			}

			return;
		}

		public Table CloneProto()
		{
			var result = new Table();
			if (Array != null)
			{
				result.Array = new List<ValueSlot>();
				foreach (var slot in Array)
				{
					result.Array.Add(slot.CloneCheck());
				}
			}

			if (Dict != null)
			{
				result.Dict = new Dictionary<ValueSlot, ValueSlot>();
				foreach (var pair in Dict)
				{
					result.Dict[pair.Key.CloneCheck()] = pair.Value.CloneCheck();
				}
			}
			return result;
		}

		/*public void Log()
		{
			Sandbox.Log.Info( $"LEN = {GetLength()}" );
			if (Array != null)
			{
				for (int i=0;i<Array.Count;i++ )
				{
					Sandbox.Log.Info( $"[{i+1}] = {Array[i]}" );
				}
			}
			if (Dict != null)
			{
				foreach ( var pair in Dict )
				{
					Sandbox.Log.Info( $"[{pair.Key}] = {pair.Value}" );
				}
			}
		}*/

		public ValueSlot DefineFunc(string name, Func<Executor, ValueSlot?> func)
		{
			if (DebugLibName == null)
			{
				throw new Exception("DefineFunc should only be called on libraries/classes with debug names.");
			}
			var wrapper = new ProtoFunction();
			wrapper.DebugName = "[CSHARP] "+DebugLibName+"."+name;
			wrapper.UserFunc = func;

			var val = ValueSlot.Function( new Function( wrapper, null! ) );

			Set(name, val);

			return val;
		}

		public Table DefineLib(string name)
		{
			if ( DebugLibName == null )
			{
				throw new Exception( "DefineLib should only be called on Env or libraries with debug names." );
			}
			var table = new Table();
			if (DebugLibName == "_G")
			{
				table.DebugLibName = name;
			} else
			{
				table.DebugLibName = DebugLibName + "." + name;
			}

			Set( name, ValueSlot.Table(table) );
			return table;
		}

		// Used to generate the status pages that tell us how much of the API is implemented.
		public void Dump(string filename)
		{
			var dict = new Dictionary<string, string>();

			DumpInternal( "", dict, 3 );

			FileSystem.Data.WriteJson(filename,dict);
		}

		private void DumpInternal(string prefix, Dictionary<string, string> dict, int levels)
		{
			if (levels <= 0)
			{
				return;
			}
			levels--;
			if (Dict != null)
			{
				foreach (var pair in Dict)
				{
					if (pair.Key.Kind == ValueKind.String)
					{
						var key_str = pair.Key.CheckString();
						if ( key_str == "_G" || key_str.Contains('.') || key_str.Contains( '/' ))
						{
							continue;
						}

						if (pair.Value.Kind == ValueKind.Function)
						{
							var func = pair.Value.CheckFunction();
							string debug_name = func.Prototype.DebugName;
							if (func.Prototype.UserFunc != null)
							{
								if (debug_name.StartsWith("?"))
								{
									dict[prefix + key_str] = "ERROR Legacy C# function decl.";
								} else
								{
									dict[prefix + key_str] = "CSHARP";
								}
							} else
							{
								if ( debug_name.Contains("/glib_official/"))
								{
									dict[prefix + key_str] = "FP-LUA";
								} else if ( debug_name.Contains( "/stubs.lua" ) )
								{
									dict[prefix + key_str] = "STUB";
								} else
								{
									dict[prefix + key_str] = "LUA";
								}
							}
						} else if (pair.Value.Kind == ValueKind.Table)
						{
							pair.Value.CheckTable().DumpInternal( prefix + key_str + ".", dict, levels );
						}
					}
				}
			}
		}
	}
}
