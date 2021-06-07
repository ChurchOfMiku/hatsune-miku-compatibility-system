using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Miku.Lua
{
	class Test
	{
		private static ValueSlot[]? TableInsert( ValueSlot[] args )
		{
			Assert.True(args.Length == 2);
			var table = args[0].GetTable();
			var new_val = args[1];
			table.PushVal(new_val);
			return null;
		}

		private static ValueSlot[]? MathAbs( ValueSlot[] args )
		{
			double result = Math.Abs( args[0].GetNumber() );
			return new ValueSlot[] { ValueSlot.Number( result ) };
		}

		private static ValueSlot[]? Print( ValueSlot[] args )
		{
			var builder = new StringBuilder("LUA: ");
			foreach ( var arg in args )
			{
				// incompatible: glua fails to concat nil, possibly others for whatever reason
				builder.Append( arg.ToString() );
				builder.Append( "\t" );
			}
			Log.Info( builder.ToString() );
			return null;
		}

		public static void Run()
		{
			var env = new Table();

			var table_lib = new Table();
			env.Set( "table", ValueSlot.Table(table_lib) );
			table_lib.Set( "insert", ValueSlot.UserFunction( TableInsert ) );

			var math_lib = new Table();
			env.Set( "math", ValueSlot.Table( math_lib ) );
			math_lib.Set( "abs", ValueSlot.UserFunction( MathAbs ) );

			env.Set( "print", ValueSlot.UserFunction( Print ) );

			var proto = Dump.Read( new byte[] { 27, 76, 74, 2, 0, 9, 64, 116, 101, 115, 116, 46, 108, 117, 97, 130, 3, 2, 0, 15, 0, 3, 9, 45, 103, 0, 15, 53, 0, 0, 0, 42, 1, 0, 0, 42, 2, 1, 0,
				42, 3, 2, 0, 77, 1, 39, 128, 39, 5, 1, 0, 42, 6, 3, 0, 42, 7, 4, 0, 42, 8, 5, 0, 77, 6, 30, 128, 41, 10, 0, 0, 41, 11, 0, 0, 41, 12, 0, 0,
				41, 13, 100, 0, 1, 12, 13, 0, 88, 13, 19, 128, 85, 13, 18, 128, 34, 13, 10, 10, 34, 14, 11, 11, 32, 13, 14, 13, 41, 14, 4, 0, 3, 14, 13, 0, 88, 13, 1, 128,
				88, 13, 11, 128, 34, 13, 11, 11, 34, 14, 10, 10, 33, 13, 14, 13, 32, 13, 9, 13, 29, 14, 6, 11, 34, 14, 10, 14, 32, 14, 4, 14, 22, 12, 7, 12, 18, 10, 14, 0,
				18, 11, 13, 0, 88, 13, 234, 127, 18, 13, 5, 0, 26, 14, 8, 12, 56, 14, 14, 0, 38, 5, 14, 13, 79, 6, 226, 127, 54, 6, 2, 0, 18, 7, 5, 0, 66, 6, 2, 1,
				79, 1, 217, 127, 75, 0, 1, 0, 10, 112, 114, 105, 110, 116, 5, 1, 10, 0, 6, 32, 6, 46, 6, 58, 6, 45, 6, 61, 6, 43, 6, 42, 6, 35, 6, 37, 6, 64, 155, 179,
				230, 204, 25, 204, 153, 211, 255, 11, 155, 179, 230, 204, 25, 204, 153, 211, 255, 3, 181, 230, 204, 153, 19, 153, 179, 230, 253, 3, 155, 179, 230, 204, 25, 204, 153, 131, 128, 12, 181, 230,
				204, 153, 19, 153, 179, 198, 255, 3, 247, 209, 240, 250, 8, 225, 245, 145, 253, 3, 4, 2, 20, 2, 3, 3, 3, 3, 4, 5, 5, 5, 5, 6, 6, 6, 7, 7, 7, 7, 8, 8,
				8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 11, 11, 11, 11, 5, 13, 13, 13, 3, 14, 99, 104, 97, 114, 109, 97, 112, 0, 2, 44, 1, 3, 40, 2,
				0, 40, 3, 0, 40, 121, 0, 1, 38, 108, 105, 110, 101, 0, 1, 37, 1, 3, 31, 2, 0, 31, 3, 0, 31, 120, 0, 1, 29, 122, 105, 0, 3, 26, 122, 114, 0, 0, 26, 105,
				0, 0, 26, 0, 0} );
			var func = new Function( env, proto );
			var exec = new Executor( func );

			Stopwatch sw = Stopwatch.StartNew();
			exec.Run();
			exec.LogState();
			Log.Warning( $"TOOK: {sw.ElapsedMilliseconds}" );
		}
	}
}
