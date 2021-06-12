﻿using System;
using Sandbox;

namespace Miku.Lua.CoreLib
{
	class Globals
	{
		public static void Init( Table env )
		{
			env.Set( "print", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				string str = LuaMachine.Concat( args );
				Log.Info( "LUA: " + str );
				return null;
			}));

			env.Set( "error", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				throw new LuaException( args[0].ToString() );
			} ));

			env.Set( "tonumber", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var x = args[0];
				if ( x.Kind == ValueKind.String )
				{
					double result = 0;
					if ( double.TryParse( x.CheckString(), out result ) )
					{
						return new ValueSlot[] { ValueSlot.Number( result ) };
					}
				}
				throw new Exception( "can't convert to number: " + x );
			} ) );

			env.Set( "setmetatable", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var table = args[0].CheckTable();
				var metatable = args[1].CheckTable();
				metatable.CheckMetaTableMembers();
				table.MetaTable = metatable;
				return new ValueSlot[] { ValueSlot.Table( table ) };
			} ) );

			env.Set( "getmetatable", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var table = args[0].CheckTable();
				var result = table.MetaTable != null ? ValueSlot.Table( table.MetaTable ) : ValueSlot.NIL;
				return new ValueSlot[] { result };
			} ) );

			env.Set( "pcall", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var func = args[0].CheckFunction(); // assume this is a lua function
				var func_args = new ValueSlot[args.Length - 1];
				for ( int i = 0; i < func_args.Length; i++ )
				{
					func_args[i] = args[i + 1];
				}
				ValueSlot[] call_results;
				try
				{
					call_results = func.Call( func_args );
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

			env.Set( "type", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
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
		}
	}
}
