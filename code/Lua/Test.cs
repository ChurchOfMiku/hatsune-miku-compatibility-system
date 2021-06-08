using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Miku.Lua
{
	class LuaException : Exception
	{
		public LuaException(string msg) : base(msg)
		{

		}
	}
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

		private static string Concat( ValueSlot[] args )
		{
			var builder = new StringBuilder();
			foreach ( var arg in args )
			{
				// incompatible: glua fails to concat nil, possibly others for whatever reason
				builder.Append( arg.ToString() );
				builder.Append( "\t" );
			}
			return builder.ToString();
		}

		private static ValueSlot[]? Print( ValueSlot[] args, Table env )
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

		private static ValueSlot[]? Error( ValueSlot[] args, Table env )
		{
			throw new LuaException( args[0].ToString() );
		}

		public static ValueSlot BootstrapRequire(Table env, string name)
		{
			Log.Info("Require: "+name);
			string filename;
			if ( name == "miku.core_lib" )
			{
				filename = "core_lib";
			}
			else if ( name.StartsWith( "lang." ) )
			{
				var mod_name = name.Substring( 5 );
				filename = $"front_end/{mod_name}";
			} else
			{
				throw new Exception( "can't require: " + name );
			}

			var bytes = FileSystem.Mounted.ReadAllBytes( $"lua_bootstrap/{filename}.bc" );

			var proto = Dump.Read( bytes.ToArray() );
			var func = new Function( env, proto, new Executor.UpValueBox[0] );
			var res = func.Call();
			if (res.Length != 0)
			{
				if ( res.Length != 1)
				{
					throw new Exception( "Module returned multiple values." );
				}
				return res[0];
			}
			return ValueSlot.Nil();
		}

		public static void Run()
		{
			var env = new Table();

			var lib_bit = new Table();
			env.Set( "bit", ValueSlot.Table( lib_bit ) );
			lib_bit.Set( "band", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				int x = (int)args[0].GetNumber();
				int y = (int)args[1].GetNumber();
				return new ValueSlot[] { ValueSlot.Number(x & y) };
			}));

			lib_bit.Set( "bnot", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				int x = (int)args[0].GetNumber();
				return new ValueSlot[] { ValueSlot.Number( ~x ) };
			}));

			var string_lib = new Table();
			env.Set( "string", ValueSlot.Table( string_lib ) );

			string_lib.Set( "sub", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var str = args[0].GetString();
				int start = (int)args[1].GetNumber() - 1;
				int length = str.Length;
				if (args.Length > 2)
				{
					length = (int)args[2].GetNumber() - start;
				}
				length = Math.Max( length, 0 );
				length = Math.Min(length, str.Length - start);
				return new ValueSlot[] { ValueSlot.String(str.Substring(start,length)) };
			}));

			string_lib.Set( "match", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var str = args[0].GetString();
				var pattern = args[1].GetString();
				bool result = false;
				switch ( pattern )
				{
					case "^TK_": result = str.StartsWith( "TK_" ); break;
					default: throw new Exception( "match " + pattern );
				}
				return new ValueSlot[] { ValueSlot.Bool( result ) };
			}));

			string_lib.Set( "format", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				string joined = Concat( args );
				return new ValueSlot[] { ValueSlot.String( "["+joined+"]" ) };
			}));

			/*var table_lib = new Table();
			env.Set( "table", ValueSlot.Table(table_lib) );
			table_lib.Set( "insert", ValueSlot.UserFunction( TableInsert ) );

			var math_lib = new Table();
			env.Set( "math", ValueSlot.Table( math_lib ) );
			math_lib.Set( "abs", ValueSlot.UserFunction( MathAbs ) );*/

			env.Set( "print", ValueSlot.UserFunction( Print ) );
			env.Set( "error", ValueSlot.UserFunction( Error ) );

			env.Set( "setmetatable", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var table = args[0].GetTable();
				var metatable = args[1].GetTable();
				metatable.CheckMetaTableMembers();
				table.MetaTable = metatable;
				return new ValueSlot[] { ValueSlot.Table( table ) };
			} ));

			env.Set( "pcall", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var func = args[0].GetFunction(); // assume this is a lua function
				var func_args = new ValueSlot[args.Length - 1];
				for (int i=0;i<func_args.Length;i++ )
				{
					func_args[i] = args[i + 1];
				}
				func.Call( func_args );
				throw new Exception( "lol" );
			} ) );

			env.Set( "_MIKU_BOOTSTRAP_REQUIRE", ValueSlot.UserFunction( (ValueSlot[] args, Table env) => {
				string mod_name = args[0].GetString();
				var res = BootstrapRequire( env, mod_name );
				return new ValueSlot[] {res};
			}));

			Stopwatch sw = Stopwatch.StartNew();

			BootstrapRequire( env, "miku.core_lib" );
			var compile = BootstrapRequire( env, "lang.compile" ).GetTable().Get( "string" ).GetFunction();

			compile.Call( new ValueSlot[] { ValueSlot.String("print('lol') print('x')") } );

			Log.Warning( $"TOOK: {sw.ElapsedMilliseconds}" );
		}
	}
}
