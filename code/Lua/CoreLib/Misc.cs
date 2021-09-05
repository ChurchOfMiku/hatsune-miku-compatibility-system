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

			table_lib.DefineFunc( "insert", ( Executor ex ) => {
				var table = ex.GetArg(0).CheckTable();
				if (ex.GetArgCount() == 2)
				{
					var new_val = ex.GetArg( 1 );
					table.PushVal( new_val );
				} else
				{
					Assert.True( ex.GetArgCount() == 3 );
					int index = (int)ex.GetArg( 1 ).CheckNumber();
					var new_val = ex.GetArg( 2 );
					// not optimal, but should handle all cases without turning into an over-engineered mess
					while (true)
					{
						var tmp = table.Get( index );
						table.Set( index, new_val );
						if (tmp.Kind == ValueKind.Nil)
						{
							break;
						}
						index++;
						new_val = tmp;
					}
				}
				return null;
			} );

			// MATH
			var math_lib = env.DefineLib( "math" );

			math_lib.DefineFunc( "floor", ( Executor ex ) => {
				double n = ex.GetArg(0).CheckNumber();
				return Math.Floor( n );
			} );

			math_lib.DefineFunc( "ceil", ( Executor ex ) => {
				double n = ex.GetArg( 0 ).CheckNumber();
				return Math.Ceiling( n );
			} );

			math_lib.DefineFunc( "log10", ( Executor ex ) => {
				double n = ex.GetArg( 0 ).CheckNumber();
				return Math.Log10( n );
			} );

			// OS
			var os_lib = env.DefineLib( "os" );
			os_lib.DefineFunc( "time", ( Executor ex ) => {
				var time = DateTimeOffset.Now.ToUnixTimeSeconds();
				return time;
			} );

			// MISC
			env.DefineLib( "ffi" );
			env.DefineLib( "jit" );
			env.DefineLib( "debug" );
			env.DefineLib( "package" );
		}
	}
}
