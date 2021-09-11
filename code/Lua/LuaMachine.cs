#nullable enable

using System;
using System.Diagnostics;
using System.Text;
using Sandbox;

using Miku.Lua.Objects;

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

		public ValueSlot BootstrapRequire(string name)
		{
			string filename = name.Replace('.','/');

			var bytes = FileSystem.Mounted.ReadAllBytes( $"lua/core/{filename}.bc" );

			var proto = Dump.Read( bytes.ToArray() );
			var func = new Function( proto, Env );
			return func.Call( this ).GetResult(0);
		}

		public readonly Table Env = new Table();
		public readonly Table Registry = new Table();
		public readonly MetaTables PrimitiveMeta = new MetaTables();

		private Function CompileFunction = null!;

		public LuaMachine()
		{
			// Set up global table and registry.
			Env.DebugLibName = "_G";
			Registry.DebugLibName = "_R";
			Env.Set( "_G", Env );
			Env.Set( "_R", Registry );

			// Load libraries: TODO all should use: new CoreLib.X( this );
			new CoreLib.Bit(this);
			new CoreLib.String(this);
			new CoreLib.Misc(this);

			new CoreLib.Globals( this );

			Registry.DefineFunc( "miku_require", ( Executor ex ) => {
				string mod_name = ex.GetArg(0).CheckString();

				var file_name = "glib_official/garrysmod/lua/includes/modules/" + mod_name + ".lua";
				if ( CompileFunction != null && CheckFile( file_name ) )
				{
					// TODO: Normal require doesn't actually return anything.
					RunFile( file_name );
					return null;
				}

				// NOTE: Uses original env, which I assume is what we want?
				var res = BootstrapRequire( mod_name );
				return res;
			});

			Registry.DefineFunc( "miku_debug_lib", ( Executor ex ) => {
				var tab = ex.GetArg( 0 ).CheckTable();
				var name = ex.GetArg( 1 ).CheckString();
				tab.DebugLibName = name;
				return null;
			});

			BootstrapRequire( "core" );
			CompileFunction = BootstrapRequire( "lang.compile" ).CheckTable().Get( "string" ).CheckFunction();
		}

		public void RunString(string code,string name)
		{
			if (DoBaseClassReplacement)
			{
				code = code.Replace( "DEFINE_BASECLASS", "local BaseClass = baseclass.Get" );
			}

			var cache_file = CodeCache.GetCacheFileName(name, code);
			byte[]? dump_bytes = CodeCache.GetCode( cache_file );

			double time_compile = -1;

			if (dump_bytes == null)
			{
				Stopwatch sw_compile = Stopwatch.StartNew();
				var results = CompileFunction.Call( this, new ValueSlot[] { code, name } );
				time_compile = sw_compile.Elapsed.TotalMilliseconds;


				if ( results.GetResult( 0 ).Kind != ValueKind.True )
				{
					Log.Error( results.GetResult( 1 ) );
					throw new Exception( "Lua compile failed." );
				}

				var dump = results.GetResult( 1 ).CheckTable();
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
			var new_func = new Function( new_proto, Env );

			Stopwatch sw_exec = Stopwatch.StartNew();
			new_func.Call( this );
			string time_compile_str = time_compile > 0 ? time_compile + " ms" : "CACHED";
			//Log.Info( $"Finished {name}; C = {time_compile_str}; E = {sw_exec.Elapsed.TotalMilliseconds} ms" );
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

		public Table DefineClass(string name)
		{
			var old_value = Registry.Get( name );
			var table = (old_value.Kind == ValueKind.Table) ? old_value.CheckTable() : new Table();
			table.DebugLibName = "[class "+name+"]";
			Registry.Set( name, table );
			return table;
		}
	}
}
