using System;

namespace Miku.Lua
{

	using UserFunction = Func<ValueSlot[], Table, ValueSlot[]?>;

	enum ValueKind
	{
		Nil,
		True,
		False,
		Number,
		String,
		Table,
		Function,
		UserFunction,
		ProtoFunction
	}

	struct ValueSlot
	{
		public ValueKind Kind { get; private set; }
		private object Reference;
		private double NumberValue;

		public static ValueSlot Nil()
		{
			return new ValueSlot() { Kind = ValueKind.Nil };
		}

		public static ValueSlot String(string x)
		{
			return new ValueSlot() { Kind = ValueKind.String, Reference = x };
		}

		public static ValueSlot Number( double x )
		{
			return new ValueSlot() { Kind = ValueKind.Number, NumberValue = x };
		}

		public static ValueSlot Bool( bool x )
		{
			if (x)
			{
				return new ValueSlot() { Kind = ValueKind.True };
			} else
			{
				return new ValueSlot() { Kind = ValueKind.False };
			}
		}

		public static ValueSlot Prim(uint t)
		{
			if ( t == 1 )
			{
				return new ValueSlot() { Kind = ValueKind.False };
			}
			else if ( t == 2 )
			{
				return new ValueSlot() { Kind = ValueKind.True };
			}
			else
			{
				return ValueSlot.Nil();
			}
		}

		public static ValueSlot Table( Table x )
		{
			return new ValueSlot() { Kind = ValueKind.Table, Reference = x };
		}

		public static ValueSlot ProtoFunction( ProtoFunction x )
		{
			return new ValueSlot() { Kind = ValueKind.ProtoFunction, Reference = x };
		}

		public static ValueSlot Function( Function x )
		{
			return new ValueSlot() { Kind = ValueKind.Function, Reference = x };
		}

		public static ValueSlot UserFunction( UserFunction x )
		{
			return new ValueSlot() { Kind = ValueKind.UserFunction, Reference = x };
		}

		public bool IsNil()
		{
			return Kind == ValueKind.Nil;
		}

		public bool IsFunction()
		{
			return Kind == ValueKind.Function;
		}

		public bool IsTruthy()
		{
			return !(Kind == ValueKind.Nil || Kind == ValueKind.False);
		}

		public Table CheckTable()
		{
			if ( this.Kind == ValueKind.Table )
			{
				return (Table)this.Reference;
			}
			throw new Exception( $"{this} is not a table." );
		}

		public double GetNumber()
		{
			if ( this.Kind == ValueKind.Number )
			{
				return this.NumberValue;
			}
			throw new Exception( $"{this} is not a number." );
		}

		public ProtoFunction GetProtoFunction()
		{
			if (this.Kind == ValueKind.ProtoFunction)
			{
				return (ProtoFunction)this.Reference;
			}
			throw new System.Exception("wrong kind");
		}

		public string GetString()
		{
			if ( this.Kind == ValueKind.String )
			{
				return (string)this.Reference;
			}
			throw new System.Exception( "wrong kind" );
		}

		public Function GetFunction()
		{
			if ( this.Kind == ValueKind.Function )
			{
				return (Function)this.Reference;
			}
			throw new System.Exception( $"{this} is not a function." );
		}

		public UserFunction GetUserFunction()
		{
			if ( this.Kind == ValueKind.UserFunction )
			{
				return (UserFunction)this.Reference;
			}
			throw new System.Exception( $"{this} is not a user function." );
		}

		public ValueSlot CloneCheck()
		{
			switch (this.Kind)
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
			if (this.Kind == ValueKind.String)
			{
				return this.Reference.ToString();
			}
			if (this.Kind == ValueKind.Number)
			{
				return this.NumberValue.ToString();
			}
			return this.Kind.ToString();
		}

		public override int GetHashCode()
		{
			int hash = Kind.GetHashCode();
			if (Kind == ValueKind.Number)
			{
				hash ^= NumberValue.GetHashCode();
			}
			if (Reference != null)
			{
				hash ^= Reference.GetHashCode();
			}
			return hash;
		}

		public override bool Equals( object obj )
		{
			if (obj is ValueSlot)
			{
				var val = (ValueSlot)obj;
				if (this.Kind == val.Kind)
				{
					if (this.Kind == ValueKind.Number)
					{
						return this.NumberValue == val.NumberValue;
					} else
					{
						return this.Reference.Equals( val.Reference );
					}
				}
			}
			return false;
		}
	}
}
