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
	class LuaMachine
	{
		private static ValueSlot[]? TableInsert( ValueSlot[] args, Table env )
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
				builder.Append( arg.ToString() );
				builder.Append( "\t" );
				if (arg.Kind == ValueKind.Table)
				{
					arg.GetTable().Log();
				}
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
			string filename;
			if ( name == "core" )
			{
				filename = "core";
			}
			else if ( name.StartsWith( "lang." ) )
			{
				var mod_name = name.Substring( 5 );
				filename = $"lang/{mod_name}";
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

		public LuaMachine()
		{
			var env = new Table();

			var lib_bit = new Table();
			env.Set( "bit", ValueSlot.Table( lib_bit ) );
			lib_bit.Set( "band", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				int x = (int)args[0].GetNumber();
				int y = (int)args[1].GetNumber();
				return new ValueSlot[] { ValueSlot.Number(x & y) };
			}));

			lib_bit.Set( "bor", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				int x = (int)args[0].GetNumber();
				int y = (int)args[1].GetNumber();
				return new ValueSlot[] { ValueSlot.Number( x | y ) };
			} ) );

			lib_bit.Set( "rshift", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				uint x = (uint)args[0].GetNumber(); // LOGICAL SHIFT = uint !!!
				int y = (int)args[1].GetNumber();
				return new ValueSlot[] { ValueSlot.Number( x >> y ) };
			}));

			lib_bit.Set( "bnot", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				int x = (int)args[0].GetNumber();
				return new ValueSlot[] { ValueSlot.Number( ~x ) };
			}));

			var string_lib = new Table();
			env.Set( "string", ValueSlot.Table( string_lib ) );

			string_lib.Set( "byte", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var str = args[0].GetString();
				int index = 0;
				if (args.Length>1)
				{
					index = (int)args[1].GetNumber() - 1;
				}

				{
					if (index < 0 || index >= str.Length)
					{
						throw new Exception( "string.byte oob" );
					}
					int result = (byte)str[index];
					if (result > 255)
					{
						throw new Exception( "string.byte got non-byte result, this could be a problem" );
					}
					return new ValueSlot[] { ValueSlot.Number( result ) };
				}
			}));

			string_lib.Set( "sub", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var str = args[0].GetString();
				int start = (int)args[1].GetNumber() - 1;
				int length = str.Length;
				if (args.Length > 2)
				{
					int arg2 = (int)args[2].GetNumber();
					if (arg2 >= 0)
					{
						length = arg2 - start;
					} else
					{
						length = (str.Length + arg2 + 1) - start;
					}
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

			var table_lib = new Table();
			env.Set( "table", ValueSlot.Table(table_lib) );
			table_lib.Set( "insert", ValueSlot.UserFunction( TableInsert ) );

			env.Set( "print", ValueSlot.UserFunction( Print ) );
			env.Set( "error", ValueSlot.UserFunction( Error ) );

			env.Set( "setmetatable", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var table = args[0].GetTable();
				var metatable = args[1].GetTable();
				metatable.CheckMetaTableMembers();
				table.MetaTable = metatable;
				return new ValueSlot[] { ValueSlot.Table( table ) };
			} ));

			env.Set( "getmetatable", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var table = args[0].GetTable();
				var result = table.MetaTable != null ? ValueSlot.Table( table ) : ValueSlot.Nil();
				return new ValueSlot[] { result };
			}));

			env.Set( "pcall", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var func = args[0].GetFunction(); // assume this is a lua function
				var func_args = new ValueSlot[args.Length - 1];
				for (int i=0;i<func_args.Length;i++ )
				{
					func_args[i] = args[i + 1];
				}
				ValueSlot[] call_results;
				try
				{
					call_results = func.Call( func_args );
				} catch (Exception e)
				{
					Log.Warning( e.Message );
					throw new Exception( "TODO pcall error handling!" );
				}

				ValueSlot[] pcall_results = new ValueSlot[call_results.Length + 1];
				pcall_results[0] = ValueSlot.Bool( true );
				for (int i=0;i<call_results.Length;i++ )
				{
					pcall_results[i + 1] = call_results[i];
				}

				return pcall_results;
			}));

			env.Set( "type", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				string result;
				switch (args[0].Kind)
				{
					case ValueKind.String: result = "string"; break;
					case ValueKind.Number: result = "number"; break;
					case ValueKind.Table: result = "table"; break;
					case ValueKind.Function: result = "function"; break;
					default:
						throw new Exception( "typeof " + args[0].Kind );
				}
				return new ValueSlot[] { ValueSlot.String(result) };
			} ));

			env.Set( "_MIKU_BOOTSTRAP_REQUIRE", ValueSlot.UserFunction( (ValueSlot[] args, Table env) => {
				string mod_name = args[0].GetString();
				var res = BootstrapRequire( env, mod_name );
				return new ValueSlot[] {res};
			}));

			Stopwatch sw = Stopwatch.StartNew();

			BootstrapRequire( env, "core" );
			var compile = BootstrapRequire( env, "lang.compile" ).GetTable().Get( "string" ).GetFunction();

			var results = compile.Call( new ValueSlot[] { ValueSlot.String("print('lol') print('x')") } );
			if (results[0].Kind == ValueKind.True)
			{
				var dump = results[1].GetTable();
				byte[] dump_bytes = new byte[dump.GetLength()];
				for (int i=0;i< dump_bytes.Length; i++ )
				{
					int n = (int)dump.Get( i + 1 ).GetNumber();
					if (n > 255 || n < 0)
					{
						throw new Exception( "dump contained non-byte!" );
					}
					dump_bytes[i] = (byte)n;
					//Log.Info( $"{i}: {n}" );
				}
				var new_proto = Dump.Read( dump_bytes );
				var new_func = new Function( env, new_proto, new Executor.UpValueBox[0] );
				new_func.Call();
			} else
			{
				throw new Exception( "compile failed" );
			}

			Log.Warning( $"TOOK: {sw.ElapsedMilliseconds}" );
		}
	}
}
