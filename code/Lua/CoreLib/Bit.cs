using System;

namespace Miku.Lua.CoreLib
{
	class Bit
	{
		public static void Init(Table env)
		{
			var lib = new Table();
			lib.DebugLibName = "bit";
			env.Set( "bit", ValueSlot.Table( lib ) );

			lib.Set( "band", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				int x = (int)args[0].CheckNumber();
				int y = (int)args[1].CheckNumber();
				return new ValueSlot[] { ValueSlot.Number( x & y ) };
			} ) );

			lib.Set( "bor", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				int x = (int)args[0].CheckNumber();
				int y = (int)args[1].CheckNumber();
				return new ValueSlot[] { ValueSlot.Number( x | y ) };
			} ) );

			lib.Set( "rshift", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				uint x = (uint)args[0].CheckNumber(); // LOGICAL SHIFT = uint !!!
				int y = (int)args[1].CheckNumber();
				return new ValueSlot[] { ValueSlot.Number( x >> y ) };
			} ) );

			lib.Set( "lshift", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				int x = (int)args[0].CheckNumber();
				int y = (int)args[1].CheckNumber();
				return new ValueSlot[] { ValueSlot.Number( x << y ) };
			} ) );

			lib.Set( "bnot", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				int x = (int)args[0].CheckNumber();
				return new ValueSlot[] { ValueSlot.Number( ~x ) };
			} ) );

			// HACK: used by parser
			lib.Set( "get_double_parts", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				double x = args[0].CheckNumber();
				long bits = BitConverter.DoubleToInt64Bits( x );
				int high = (int)(bits >> 32);
				int low = (int)bits;
				return new ValueSlot[] { ValueSlot.Number( low ), ValueSlot.Number( high ) };
			} ) );
		}
	}
}
