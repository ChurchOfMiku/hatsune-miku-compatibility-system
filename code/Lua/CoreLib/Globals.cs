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
				if ( x.Kind == ValueKind.String )
				{
					double result = 0;
					// WARNING: this might be locale sensitive -- last time I checked the locale-independent parsing crap wasn't whitelisted
					if ( double.TryParse( x.CheckString(), out result ) )
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
				var table = args[0].CheckTable();
				var result = table.MetaTable != null ? ValueSlot.Table( table.MetaTable ) : ValueSlot.NIL;
				return new ValueSlot[] { result };
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
				ValueSlot[] call_results;
				try
				{
					call_results = func.Call( machine, func_args );
				}
				catch ( Exception e )
				{
					Log.Warning( e.Message );
					throw new Exception( "TODO pcall error handling!" );
				}

				ValueSlot[] pcall_results = new ValueSlot[call_results.Length + 1];
				pcall_results[0] = ValueSlot.Bool( true );
				for ( int i = 0; i < call_results.Length; i++ )
				{
					pcall_results[i + 1] = call_results[i];
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

			env.Set( "include", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				string filename = args[0].CheckString();
				string fullpath = ex.GetDirectory() + filename;

				if (ex.Machine == null)
				{
					throw new Exception("Include: No LuaMachine?");
				}
				ex.Machine.RunFile(fullpath);
				return null;
			} ) );
		}
	}
}
