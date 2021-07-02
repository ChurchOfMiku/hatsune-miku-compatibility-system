using System;

namespace Miku.Lua.CoreLib
{
	class Bit
	{
		public Bit(LuaMachine machine)
		{
			var env = machine.Env;

			var lib = env.DefineLib( "bit" );

			lib.DefineFunc( "band", ( Executor ex ) => {
				int x = (int)ex.GetArg( 0 ).CheckNumber();
				int y = (int)ex.GetArg( 1 ).CheckNumber();
				return ValueSlot.Number( x & y );
			} );

			lib.DefineFunc( "bor", ( Executor ex ) => {
				int x = (int)ex.GetArg( 0 ).CheckNumber();
				int y = (int)ex.GetArg( 1 ).CheckNumber();
				return ValueSlot.Number( x | y );
			} );

			lib.DefineFunc( "rshift", ( Executor ex ) => {
				uint x = (uint)ex.GetArg( 0 ).CheckNumber(); // LOGICAL SHIFT = uint !!!
				int y = (int)ex.GetArg( 1 ).CheckNumber();
				return ValueSlot.Number( x >> y );
			} );

			lib.DefineFunc( "lshift", ( Executor ex ) => {
				int x = (int)ex.GetArg( 0 ).CheckNumber();
				int y = (int)ex.GetArg( 1 ).CheckNumber();
				return ValueSlot.Number( x << y );
			} );

			lib.DefineFunc( "bnot", ( Executor ex ) => {
				int x = (int)ex.GetArg( 0 ).CheckNumber();
				return ValueSlot.Number( ~x );
			} );

			// HACK: used by parser
			lib.DefineFunc( "get_double_parts", ( Executor ex ) => {
				double x = ex.GetArg( 0 ).CheckNumber();
				long bits = BitConverter.DoubleToInt64Bits( x );
				int high = (int)(bits >> 32);
				int low = (int)bits;
				ex.Return( ValueSlot.Number( low ) );
				return ValueSlot.Number( high );
			} );

			// HACK: used by parser
			lib.DefineFunc( "make_double", ( Executor ex ) => {
				long low = (uint)ex.GetArg( 0 ).CheckNumber();
				long high = (uint)ex.GetArg( 1 ).CheckNumber();
				long merged = low | (high << 32);
				double d = BitConverter.Int64BitsToDouble( merged );
				return ValueSlot.Number( d );
			} );
		}
	}
}
