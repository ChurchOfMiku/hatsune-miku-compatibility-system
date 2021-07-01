using System;
using Sandbox;

namespace Miku.Lua.CoreLib
{
	class Misc
	{
		public Misc( LuaMachine machine )
		{
			var env = machine.Env;

			// TABLE
			var table_lib = env.DefineLib( "table" );

			table_lib.Set( "insert", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				Assert.True( args.Length == 2 );
				var table = args[0].CheckTable();
				var new_val = args[1];
				table.PushVal( new_val );
				return null;
			} ) );

			// MATH
			var math_lib = env.DefineLib( "math" );

			math_lib.DefineFunc( "floor", ( Executor ex ) => {
				double n = ex.GetArg(0).CheckNumber();
				return ValueSlot.Number( Math.Floor( n ) );
			} );

			math_lib.DefineFunc( "ceil", ( Executor ex ) => {
				double n = ex.GetArg( 0 ).CheckNumber();
				return ValueSlot.Number( Math.Ceiling( n ) );
			} );

			// OS
			var os_lib = env.DefineLib( "os" );
			os_lib.DefineFunc( "time", ( Executor ex ) => {
				var time = DateTimeOffset.Now.ToUnixTimeSeconds();
				return ValueSlot.Number( time );
			} );

			// MISC
			env.DefineLib( "ffi" );
			env.DefineLib( "jit" );
			env.DefineLib( "debug" );
			env.DefineLib( "package" );
		}
	}
}
