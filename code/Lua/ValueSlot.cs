#nullable enable

using System;
using System.Diagnostics;
using Miku.Lua.Objects;

namespace Miku.Lua
{
	enum ValueKind
	{
		Nil,
		True,
		False,
		Number,
		String,
		Table,
		Function,
		ProtoFunction,
		UserData
	}

	readonly struct ValueSlot : IEquatable<ValueSlot>
	{
		public readonly ValueKind Kind;
		private readonly object? Reference;
		private readonly double NumberValue;

		// primitives
		public static readonly ValueSlot NIL = new ValueSlot( ValueKind.Nil );
		public static readonly ValueSlot TRUE = new ValueSlot( ValueKind.True );
		public static readonly ValueSlot FALSE = new ValueSlot( ValueKind.False );

		private ValueSlot( ValueKind kind, object? ref_obj = null, double n = 0 )
		{
			Kind = kind;
			Reference = ref_obj;
			NumberValue = n;
		}

		public static ValueSlot Prim( int t )
		{
			if ( t == 1 )
			{
				return FALSE;
			}
			else if ( t == 2 )
			{
				return TRUE;
			}
			else
			{
				return NIL;
			}
		}

		// Just use implicit conversion in most places.
		public static implicit operator ValueSlot( double x ) => new ValueSlot( ValueKind.Number, null, x );
		public static implicit operator ValueSlot( string x ) => new ValueSlot( ValueKind.String, x );
		public static implicit operator ValueSlot( bool x ) => x ? TRUE : FALSE;

		public static implicit operator ValueSlot( Table x ) => new ValueSlot( ValueKind.Table, x );
		public static implicit operator ValueSlot( Function x ) => new ValueSlot( ValueKind.Function, x );
		public static implicit operator ValueSlot( ProtoFunction x ) => new ValueSlot( ValueKind.ProtoFunction, x );
		public static implicit operator ValueSlot( UserData x ) => new ValueSlot( ValueKind.UserData, x );

		public bool IsTruthy()
		{
			return !(Kind == ValueKind.Nil || Kind == ValueKind.False);
		}

		public Table CheckTable()
		{
			if ( Kind == ValueKind.Table )
			{
				return (Table)Reference!;
			}
			throw new Exception( $"{this} is not a table." );
		}

		public Table UnsafeGetTable() => (Table)Reference!;

		public double CheckNumber()
		{
			if ( Kind == ValueKind.Number )
			{
				return NumberValue;
			}
			throw new Exception( $"{this} is not a number." );
		}

		public double UnsafeGetNumber() => NumberValue;

		public ProtoFunction CheckProtoFunction()
		{
			if ( Kind == ValueKind.ProtoFunction )
			{
				return (ProtoFunction)Reference!;
			}
			throw new Exception( $"{this} is not a function prototype." );
		}

		public string CheckString()
		{
			if ( Kind == ValueKind.String )
			{
				return (string)Reference!;
			}
			throw new Exception( $"{this} is not a string." );
		}

		public string UnsafeGetString() => (string)Reference!;

		public double? TryGetNumber()
		{
			if ( Kind == ValueKind.Number )
			{
				return NumberValue;
			}
			return null;
		}

		public string? TryGetString()
		{
			if ( Kind == ValueKind.String )
			{
				return (string)Reference!;
			}
			return null;
		}

		public UserData? TryGetUserData()
		{
			if (Kind == ValueKind.UserData)
			{
				return (UserData)Reference!;
			}
			return null;
		}

		public Function CheckFunction()
		{
			if ( Kind == ValueKind.Function )
			{
				return (Function)Reference!;
			}
			throw new Exception( $"{this} is not a function." );
		}

		public UserData CheckUserData()
		{
			if ( Kind == ValueKind.UserData )
			{
				return (UserData)Reference!;
			}
			throw new Exception( $"{this} is not user data." );
		}

		public ValueSlot CloneCheck()
		{
			switch ( this.Kind )
			{
				case ValueKind.Nil:
				case ValueKind.True:
				case ValueKind.False:
				case ValueKind.Number:
				case ValueKind.String:
					return this;
				default:
					throw new System.Exception( $"cc: {this.Kind}" );
			}
		}

		public override string ToString()
		{
			if ( Kind == ValueKind.String )
			{
				return Reference!.ToString()!;
			}
			if ( Kind == ValueKind.Number )
			{
				return NumberValue.ToString();
			}
			return Kind.ToString();
		}

		public override int GetHashCode()
		{
			int hash = Kind.GetHashCode();
			if ( Kind == ValueKind.Number )
			{
				hash ^= NumberValue.GetHashCode();
			}
			if ( Reference != null )
			{
				hash ^= Reference.GetHashCode();
			}
			return hash;
		}

		public override bool Equals( object? obj )
		{
			if ( obj is ValueSlot )
			{
				var val = (ValueSlot)obj;
				return Equals( val );
			}
			return false;
		}

		public bool Equals( ValueSlot other )
		{
			if ( this.Kind == other.Kind )
			{
				if ( Kind == ValueKind.Number )
				{
					return this.NumberValue == other.NumberValue;
				}
				else
				{
					if ( this.Reference == null )
					{
						return other.Reference == null;
					}
					return ReferenceEquals( Reference, other.Reference )
						   || this.Reference.Equals( other.Reference );
				}
			}
			return false;
		}
	}
}
