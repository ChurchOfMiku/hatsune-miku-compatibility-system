using System;
using Sandbox;

namespace Miku.Lua.CoreLib
{
	class Misc
	{
		public Misc( LuaMachine machine )
		{
			var env = machine.Env;

			var table_lib = new Table();
			table_lib.DebugLibName = "table";
			env.Set( "table", ValueSlot.Table( table_lib ) );
			table_lib.Set( "insert", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				Assert.True( args.Length == 2 );
				var table = args[0].CheckTable();
				var new_val = args[1];
				table.PushVal( new_val );
				return null;
			} ) );

			var math_lib = env.DefineLib( "math" );

			math_lib.DefineFunc( "floor", ( Executor ex ) => {
				double n = ex.GetArg(0).CheckNumber();
				return ValueSlot.Number( Math.Floor( n ) );
			} );

			math_lib.DefineFunc( "ceil", ( Executor ex ) => {
				double n = ex.GetArg( 0 ).CheckNumber();
				return ValueSlot.Number( Math.Ceiling( n ) );
			} );
		}
	}
}
