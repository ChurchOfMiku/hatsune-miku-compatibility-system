#nullable enable

using System.Collections.Generic;
using System;

namespace Miku.Lua
{
	class Table
	{
		//private List<ValueSlot> array = new List<ValueSlot>();
		private Dictionary<ValueSlot, ValueSlot>? Dict = null;
		private List<ValueSlot>? Array = null;

		public Table? MetaTable = null;

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
			if (val.Kind == ValueKind.Nil)
			{
				return;
			}

			if (key.Kind == ValueKind.Number)
			{
				var i_dbl = key.GetNumber();
				int i = (int)i_dbl;
				if (i_dbl == i && i >= 1)
				{
					if (Array == null)
					{
						if (i == 1)
						{
							Array = new List<ValueSlot>();
							Array.Add(val);
							MigrateSubsequentKeys(i);
							return;
						}
					} else
					{
						if (i == Array.Count + 1)
						{
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
				Dict = new Dictionary<ValueSlot, ValueSlot>();
			}
			Dict[key] = val;
		}

		public void Set( string key, ValueSlot val ) { Set( ValueSlot.String( key ), val ); }
		public void Set( int key, ValueSlot val ) { Set( ValueSlot.Number( key ), val ); }

		public ValueSlot Get( ValueSlot key )
		{
			if (Array != null)
			{
				if ( key.Kind == ValueKind.Number )
				{
					var i_dbl = key.GetNumber();
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
				if (!Dict.TryGetValue( key, out result ))
				{
					result = ValueSlot.Nil();
				}
				return result;
			}
			return ValueSlot.Nil();
		}

		public ValueSlot Get( string key ) { return Get( ValueSlot.String( key ) ); }
		public ValueSlot Get( int key ) { return Get( ValueSlot.Number( key ) ); }

		public Table CloneProto()
		{
			var result = new Table();
			if (Array != null)
			{
				result.Array = new List<ValueSlot>();
				foreach (var slot in Array)
				{
					result.Array.Add(slot);
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

		public void CheckMetaTableMembers()
		{
			if (Dict != null)
			{
				foreach (var key in Dict)
				{
					if (key.Key.Kind == ValueKind.String)
					{
						string str = key.Key.GetString();
						if (str.StartsWith("__"))
						{
							switch (str)
							{
								case "__index":
									break;
								default:
									throw new Exception("NYI meta member = "+str);
							}
						}
					}
				}
			}
		}

		public void Log()
		{
			Sandbox.Log.Info( $"LEN = {GetLength()}" );
			Sandbox.Log.Info( $"TODO LOG ARRAY!" );
			if (Dict != null)
			{
				foreach ( var pair in Dict )
				{
					Sandbox.Log.Info( $"[{pair.Key}] = {pair.Value}" );
				}
			}
		}
	}
}
