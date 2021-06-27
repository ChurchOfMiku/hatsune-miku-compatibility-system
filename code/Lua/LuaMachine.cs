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
		protected bool DoBaseClassReplacement = false;

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

		public ValueSlot BootstrapRequire(string name)
		{
			string filename = name.Replace('.','/');

			var bytes = FileSystem.Mounted.ReadAllBytes( $"lua/core/{filename}.bc" );

			var proto = Dump.Read( bytes.ToArray() );
			var func = new Function( proto, Env, PrimitiveMeta );
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
		private Table Registry = new Table();
		protected PrimitiveMetaTables PrimitiveMeta = new PrimitiveMetaTables();
		private Function CompileFunction = null!;

		public LuaMachine()
		{
			// Set up global table.
			Env.DebugLibName = "_G";
			Env.Set( "_G", ValueSlot.Table( Env ) );
			Env.Set( "_R", ValueSlot.Table( Registry ) );

			// Load libraries: TODO all should use: new CoreLib.X( this );
			CoreLib.Bit.Init(Env);
			new CoreLib.String(this);

			CoreLib.Misc.Init(Env);

			new CoreLib.Globals( this );

			Registry.Set( "miku_require", ValueSlot.UserFunction( (ValueSlot[] args, Executor ex ) => {
				string mod_name = args[0].CheckString();

				var file_name = "glib_official/garrysmod/lua/includes/modules/" + mod_name + ".lua";
				if ( CompileFunction != null && CheckFile( file_name ) )
				{
					// TODO: Normal require doesn't actually return anything.
					RunFile( file_name );
					return null;
				}

				// NOTE: Uses original env, which I assume is what we want?
				var res = BootstrapRequire( mod_name );
				return new[] {res};
			}));

			Registry.Set( "miku_debug_lib", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var tab = args[0].CheckTable();
				var name = args[1].CheckString();
				tab.DebugLibName = name;
				return null;
			}));

			BootstrapRequire( "core" );
			CompileFunction = BootstrapRequire( "lang.compile" ).CheckTable().Get( "string" ).CheckFunction();
		}

		public void RunString(string code,string name)
		{
			if (DoBaseClassReplacement)
			{
				code = code.Replace( "DEFINE_BASECLASS", "local BaseClass = baseclass.Get" );
			}

			var cache_file = CodeCache.GetCacheFileName(code);
			byte[]? dump_bytes = CodeCache.GetCode( cache_file );

			double time_compile = -1;

			if (dump_bytes == null)
			{
				Stopwatch sw_compile = Stopwatch.StartNew();
				var results = CompileFunction.Call( this, new ValueSlot[] { ValueSlot.String(code), ValueSlot.String(name) } );
				time_compile = sw_compile.Elapsed.TotalMilliseconds;

				if ( results[0].Kind != ValueKind.True )
				{
					Log.Error( results[1] );
					throw new Exception( "Lua compile failed." );
				}

				var dump = results[1].CheckTable();
				dump_bytes = new byte[dump.GetLength()];
				for (int i=0;i< dump_bytes.Length; i++ )
				{
					int n = (int)dump.Get( i + 1 ).CheckNumber();
					if (n > 255 || n < 0)
					{
						throw new Exception( "dump contained non-byte!" );
					}
					dump_bytes[i] = (byte)n;
				}

				CodeCache.SetCode(cache_file, dump_bytes);
			}

			var new_proto = Dump.Read( dump_bytes );
			var new_func = new Function( new_proto, Env, PrimitiveMeta );

			Stopwatch sw_exec = Stopwatch.StartNew();
			new_func.Call( this );
			string time_compile_str = time_compile > 0 ? time_compile + " ms" : "CACHED";
			Log.Warning( $"Finished {name}; C = {time_compile_str}; E = {sw_exec.Elapsed.TotalMilliseconds} ms" );
		}

		private static string GetFilePath(string filename)
		{
			return $"lua/{filename}".Replace( "//", "/" );
		}

		public void RunFile(string filename )
		{
			string fullname = GetFilePath( filename );
			var code = FileSystem.Mounted.ReadAllText( fullname );
			RunString( code, fullname );
		}

		public bool CheckFile(string filename)
		{
			string fullname = GetFilePath( filename );
			return FileSystem.Mounted.FileExists( fullname );
		}
	}
}
