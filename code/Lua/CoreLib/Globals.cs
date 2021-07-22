#nullable enable

using System;
using System.Text;
using Sandbox;

namespace Miku.Lua.CoreLib
{
	class Globals
	{
		public Globals( LuaMachine machine )
		{
			var env = machine.Env;

			env.DefineFunc( "print", ( Executor ex ) => {
				var builder = new StringBuilder();
				for (int i=0;i<ex.GetArgCount();i++ )
				{
					builder.Append( ex.GetArg(i).ToString() );
					builder.Append( "\t" );
				}
				var str = builder.ToString();
				Log.Info( "LUA: " + str );
				return null;
			} );

			env.DefineFunc( "error", ( Executor ex ) => {
				throw new LuaException( ex.GetArg(0).ToString() );
			} );

			env.DefineFunc( "tonumber", ( Executor ex ) => {
				var x = ex.GetArg(0);
				if (ex.GetArgCount() > 1)
				{
					throw new Exception("tonumber base NYI");
				}
				if ( x.Kind == ValueKind.String )
				{
					double result = 0;
					string str = x.CheckString();
					if (str.StartsWith("0x"))
					{
						// TODO probably not totally sufficient
						return Convert.ToInt32( str, 16 );
					}
					// WARNING: this might be locale sensitive -- last time I checked the locale-independent parsing crap wasn't whitelisted
					if ( double.TryParse( str, out result ) )
					{
						return result;
					}
				}
				if ( x.Kind == ValueKind.Number )
				{
					return x;
				}
				throw new Exception( "can't convert to number: " + x );
			} );

			env.DefineFunc( "tostring", ( Executor ex ) =>
			{
				// Todo this may need to call __tostring meta or whatever.
				var x = ex.GetArg(0);
				if ( x.Kind == ValueKind.String )
				{
					return x;
				}
				return x.ToString();
			} );

			env.DefineFunc( "next", ( Executor ex ) => {
				var tab = ex.GetArg( 0 ).CheckTable();
				var prev_key = ex.GetArg( 1 );
				tab.Next( prev_key, ex );
				return null;
			} );

			env.DefineFunc( "setmetatable", ( Executor ex ) => {
				var table = ex.GetArg( 0 ).CheckTable();
				if ( ex.GetArgCount() == 1 || ex.GetArg( 1 ).Kind == ValueKind.Nil)
				{
					table.MetaTable = null;
					return table;
				}
				var metatable = ex.GetArg( 1 ).CheckTable();
				table.MetaTable = metatable;
				return table;
			} );

			env.DefineFunc( "getmetatable", ( Executor ex ) => {
				var val = ex.GetArg( 0 );
				var prim_mt = ex.Machine.PrimitiveMeta.Get( val );
				if (prim_mt != null)
				{
					return prim_mt;
				}
				return null;
			} );

			env.DefineFunc( "setfenv", ( Executor ex ) =>
			{
				var env = ex.GetArg( 1 ).CheckTable();
				var func_or_loc = ex.GetArg( 0 );
				if (func_or_loc.Kind == ValueKind.Function)
				{
					func_or_loc.CheckFunction().Env = env;
					return func_or_loc;
				} else
				{
					int stack_level = (int)func_or_loc.CheckNumber();
					var func = ex.GetFunctionAtLevel(stack_level + 1);
					if (func == null)
					{
						throw new Exception( "Bad stack level?" );
					}
					func.Env = env;
					return null;
				}
			} );

			env.DefineFunc( "pcall", ( Executor ex ) => {
				var func = ex.GetArg( 0 ).CheckFunction(); // assume this is a lua function
				var func_args = new ValueSlot[ex.GetArgCount() - 1];
				for ( int i = 0; i < func_args.Length; i++ )
				{
					func_args[i] = ex.GetArg( i + 1 );
				}
				Executor call_results;

				// We make an entirely new executor to call the function in.
				// Throwing exceptions could put the executor in a bad state.
				try
				{
					call_results = func.Call( machine, func_args );
				}
				catch ( Exception e )
				{
					ex.Return(false);
					return e.Message;
				}

				int result_count = call_results.GetResultCount();

				ex.Return(true);
				for ( int i = 0; i < result_count; i++ )
				{
					ex.Return( call_results.GetResult(i) );
				}

				return null;
			} );

			env.DefineFunc( "type", ( Executor ex ) => {
				string result;
				switch ( ex.GetArg( 0 ).Kind )
				{
					case ValueKind.String: result = "string"; break;
					case ValueKind.Number: result = "number"; break;
					case ValueKind.Table: result = "table"; break;
					case ValueKind.Function: result = "function"; break;
					case ValueKind.True:
					case ValueKind.False:
						result = "boolean"; break;
					case ValueKind.Nil:
						result = "nil"; break;
					default:
						throw new Exception( "typeof " + ex.GetArg( 0 ) );
				}
				return result;
			} );

			string[] INCLUDE_PATHS = new[] {
				"glib_official/garrysmod/lua/"
			};
			env.DefineFunc( "include", ( Executor ex ) => {
				string filename = ex.GetArg( 0 ).CheckString();

				if ( ex.Machine == null )
				{
					throw new Exception( "Include: No LuaMachine?" );
				}

				// TODO do we check absolute or relative first?

				foreach (var path in INCLUDE_PATHS)
				{
					string fullpath = path + filename;

					if (ex.Machine.CheckFile( fullpath ) )
					{
						ex.Machine.RunFile( fullpath );
						return null;
					}
				}

				{
					string fullpath = ex.GetDirectory(-1) + filename;

					ex.Machine.RunFile(fullpath);
					return null;
				}
			} );
		}
	}
}
