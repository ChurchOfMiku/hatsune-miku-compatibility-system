#nullable enable

using System;
using Sandbox;

namespace Miku.Lua.CoreLib
{
	class Globals
	{
		public Globals( LuaMachine machine )
		{
			var env = machine.Env;

			env.Set( "print", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				string str = LuaMachine.Concat( args );
				Log.Info( "LUA: " + str );
				return null;
			}));

			env.Set( "error", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				throw new LuaException( args[0].ToString() );
			} ));

			env.Set( "tonumber", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var x = args[0];
				if (args.Length > 1)
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
						return new[] { ValueSlot.Number( Convert.ToInt32( str, 16 ) ) };
					}
					// WARNING: this might be locale sensitive -- last time I checked the locale-independent parsing crap wasn't whitelisted
					if ( double.TryParse( str, out result ) )
					{
						return new[] { ValueSlot.Number( result ) };
					}
				}
				if ( x.Kind == ValueKind.Number )
				{
					return new[] { x };
				}
				throw new Exception( "can't convert to number: " + x );
			} ) );

			env.Set( "tostring", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) =>
			{
				// Todo this may need to call __tostring meta or whatever.
				var x = args[0];
				if ( x.Kind == ValueKind.String )
				{
					return new[] { x };
				}
				return new[] { ValueSlot.String(x.ToString()) };
			} ) );

			env.Set( "next", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var tab = args[0].CheckTable();
				var prev_key = args[1];
				return tab.Next(prev_key);
			} ) );

			env.Set( "setmetatable", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var table = args[0].CheckTable();
				var metatable = args[1].CheckTable();
				table.MetaTable = metatable;
				return new ValueSlot[] { ValueSlot.Table( table ) };
			} ) );

			env.Set( "getmetatable", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var val = args[0];
				if (val.Kind == ValueKind.Table)
				{
					var table = val.CheckTable();
					if (table.MetaTable != null)
					{
						return new ValueSlot[] { ValueSlot.Table( table.MetaTable ) };
					}
				}
				var prim_mt = ex.Machine.PrimitiveMeta.Get( val );
				if (prim_mt != null)
				{
					return new ValueSlot[] { ValueSlot.Table( prim_mt ) };
				}
				return null;
			} ) );

			env.Set( "setfenv", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) =>
			{
				var env = args[1].CheckTable();
				var func_or_loc = args[0];
				if (func_or_loc.Kind == ValueKind.Function)
				{
					func_or_loc.CheckFunction().Env = env;
					return new[] { func_or_loc };
				} else
				{
					int stack_level = (int)func_or_loc.CheckNumber();
					var func = ex.GetFunctionAtLevel(stack_level);
					if (func == null)
					{
						throw new Exception( "Bad stack level?" );
					}
					func.Env = env;
					return null;
				}
			} ) );

			env.Set( "pcall", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var func = args[0].CheckFunction(); // assume this is a lua function
				var func_args = new ValueSlot[args.Length - 1];
				for ( int i = 0; i < func_args.Length; i++ )
				{
					func_args[i] = args[i + 1];
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
					return new[] { ValueSlot.FALSE, ValueSlot.String( e.Message ) };
				}

				int result_count = call_results.GetResultCount();
				ValueSlot[] pcall_results = new ValueSlot[result_count + 1];
				pcall_results[0] = ValueSlot.TRUE;
				for ( int i = 0; i < result_count; i++ )
				{
					pcall_results[i + 1] = call_results.GetResult(i);
				}

				return pcall_results;
			} ) );

			env.Set( "type", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				string result;
				switch ( args[0].Kind )
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
						throw new Exception( "typeof " + args[0].Kind );
				}
				return new ValueSlot[] { ValueSlot.String( result ) };
			} ) );

			string[] INCLUDE_PATHS = new[] {
				"glib_official/garrysmod/lua/"
			};
			env.Set( "include", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				string filename = args[0].CheckString();


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
					string fullpath = ex.GetDirectory() + filename;

					ex.Machine.RunFile(fullpath);
					return null;
				}
			} ) );
		}
	}
}
