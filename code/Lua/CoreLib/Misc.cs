using System;
using Sandbox;

namespace Miku.Lua.CoreLib
{
	class Misc
	{
		public static void Init( Table env )
		{
			var table_lib = new Table();
			table_lib.DebugLibName = "table";
			env.Set( "table", ValueSlot.Table( table_lib ) );
			table_lib.Set( "insert", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				Assert.True( args.Length == 2 );
				var table = args[0].CheckTable();
				var new_val = args[1];
				table.PushVal( new_val );
				return null;
			} ) );

			var math_lib = new Table();
			math_lib.DebugLibName = "math";
			env.Set( "math", ValueSlot.Table( math_lib ) );
			math_lib.Set( "floor", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				double n = args[0].CheckNumber();
				return new ValueSlot[] { ValueSlot.Number( Math.Floor( n ) ) };
			} ) );
		}
	}
}
