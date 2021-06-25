#nullable enable

using System.Collections.Generic;
using System;

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

			if (DebugLibName != null)
			{
				Sandbox.Log.Info( "GET " + DebugLibName + "." + key );
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
		public ValueSlot[]? Next(ValueSlot prev_key)
		{
			// Start enumeration.
			if (prev_key.Kind == ValueKind.Nil)
			{
				if (Array != null && Array.Count > 0)
				{
					return new[] { ValueSlot.Number( 1 ), ArrayGet(1) };
				}
				if (Dict != null && Dict.Count > 0)
				{
					CachedEnumerator = Dict.GetEnumerator();
					CachedEnumerator.MoveNext();

					var pair = CachedEnumerator.Current;
					return new[] { pair.Key, pair.Value };
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
							return new[] { ValueSlot.Number( i ), ArrayGet( i ) };
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
							return new[] { pair.Key, pair.Value };
						} else
						{
							return null;
						}
					}
					throw new Exception( "cached enumerator miss" );
				}
			}

			return null;
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

		public void Log()
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
		}
	}
}
