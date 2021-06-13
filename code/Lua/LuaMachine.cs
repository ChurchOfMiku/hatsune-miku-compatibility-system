#nullable enable

using System;
using System.Diagnostics;
using System.Text;
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
		public static string Concat( ValueSlot[] args )
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

		public ValueSlot BootstrapRequire(Table env, string name)
		{
			string filename = name.Replace('.','/');

			var bytes = FileSystem.Mounted.ReadAllBytes( $"lua/core/{filename}.bc" );

			var proto = Dump.Read( bytes.ToArray() );
			var func = new Function( env, proto, new Executor.UpValueBox[0] );
			var res = func.Call(this);
			if (res.Length != 0)
			{
				if ( res.Length != 1)
				{
					throw new Exception( "Module returned multiple values." );
				}
				return res[0];
			}
			return ValueSlot.NIL;
		}

		public Table Env = new Table();
		private Function CompileFunction;

		public LuaMachine()
		{
			Env.DebugLibName = "_G";

			CoreLib.Bit.Init(Env);
			CoreLib.String.Init(Env);

			CoreLib.Misc.Init(Env);

			new CoreLib.Globals( this );

			Env.Set( "_MIKU_BOOTSTRAP_REQUIRE", ValueSlot.UserFunction( (ValueSlot[] args, Executor ex ) => {
				string mod_name = args[0].CheckString();
				// NOTE: Uses original env, which I assume is what we want?
				var res = BootstrapRequire( Env, mod_name );
				return new ValueSlot[] {res};
			}));

			Env.Set( "_MIKU_DEBUG_LIB", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var tab = args[0].CheckTable();
				var name = args[1].CheckString();
				tab.DebugLibName = name;
				return null;
			}));

			BootstrapRequire( Env, "core" );
			CompileFunction = BootstrapRequire( Env, "lang.compile" ).CheckTable().Get( "string" ).CheckFunction();
		}

		public void RunString(string code,string name)
		{
			Stopwatch sw = Stopwatch.StartNew();
			var results = CompileFunction.Call( this, new ValueSlot[] { ValueSlot.String(code), ValueSlot.String(name) } );
			double compile_time = sw.Elapsed.TotalMilliseconds;
			if (results[0].Kind == ValueKind.True)
			{
				var dump = results[1].CheckTable();
				byte[] dump_bytes = new byte[dump.GetLength()];
				for (int i=0;i< dump_bytes.Length; i++ )
				{
					int n = (int)dump.Get( i + 1 ).CheckNumber();
					if (n > 255 || n < 0)
					{
						throw new Exception( "dump contained non-byte!" );
					}
					dump_bytes[i] = (byte)n;
				}
				var new_proto = Dump.Read( dump_bytes );
				var new_func = new Function( Env, new_proto, new Executor.UpValueBox[0] );

				Stopwatch sw2 = Stopwatch.StartNew();
				new_func.Call(this);
				Log.Warning( $"Finished {name}; C = {compile_time} ms; E = {sw2.Elapsed.TotalMilliseconds} ms" );
			} else
			{
				throw new Exception( "compile failed" );
			}
		}

		public void RunFile(string filename )
		{
			string fullname = $"lua/{filename}".Replace("//","/");
			var code = FileSystem.Mounted.ReadAllText( fullname );
			RunString( code, fullname );
		}
	}
}
