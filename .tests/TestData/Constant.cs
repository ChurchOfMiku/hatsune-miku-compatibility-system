using System.Collections.Generic;
using Miku.Lua.Objects;

namespace Miku.Tests.TestData
{
	internal sealed class Constant
	{
		public ConstantType Type { get; }
		public object? Value { get; }

		private Constant( ConstantType type, object value )
		{
			Type = type;
			Value = value;
		}

		public Constant()
		{
			Type = ConstantType.Nop;
		}

		public Constant( string str ) : this( ConstantType.String, str )
		{
		}

		public Constant( double num ) : this( ConstantType.Number, num )
		{
		}

		public Constant( LuaJitFunction function ) : this( ConstantType.Function, function )
		{
		}

		public Constant( Dictionary<Constant, Constant> table ) : this( ConstantType.Table, table )
		{
		}
	}
}
