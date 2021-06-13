using System;

namespace Miku.Lua.CoreLib
{
	class String
	{
		public static void Init( Table env )
		{
			var lib = new Table();
			lib.DebugLibName = "string";
			env.Set( "string", ValueSlot.Table( lib ) );

			lib.Set( "byte", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var str = args[0].CheckString();
				int index = 0;
				if ( args.Length > 1 )
				{
					index = (int)args[1].CheckNumber() - 1;
				}

				{
					if ( index < 0 || index >= str.Length )
					{
						throw new Exception( "string.byte oob" );
					}
					int result = (byte)str[index];
					if ( result > 255 )
					{
						throw new Exception( "string.byte got non-byte result, this could be a problem" );
					}
					return new ValueSlot[] { ValueSlot.Number( result ) };
				}
			} ) );

			lib.Set( "lower", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var str = args[0].CheckString();
				return new ValueSlot[] { ValueSlot.String( str.ToLower() ) };
			} ) );

			lib.Set( "sub", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				// TODO: see how this is actually implemented in lua, there is no way this is totally consistent
				var str = args[0].CheckString();
				int start = (int)args[1].CheckNumber() - 1;
				int length = str.Length;
				if ( start < 0 )
				{
					start = str.Length + start + 1;
				}
				if ( args.Length > 2 )
				{
					int arg2 = (int)args[2].CheckNumber();
					if ( arg2 >= 0 )
					{
						length = arg2 - start;
					}
					else
					{
						length = (str.Length + arg2 + 1) - start;
					}
				}
				start = Math.Max( start, 0 );
				length = Math.Max( length, 0 );
				length = Math.Min( length, str.Length - start );
				return new ValueSlot[] { ValueSlot.String( str.Substring( start, length ) ) };
			} ) );

			lib.Set( "match", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var str = args[0].CheckString();
				var pattern = args[1].CheckString();
				bool result = false;
				switch ( pattern )
				{
					case "^TK_": result = str.StartsWith( "TK_" ); break;
					case "^@(.+)$":
						{
							if ( str.StartsWith( "@" ) )
							{
								return new ValueSlot[] { ValueSlot.String( str.Substring( 1 ) ) };
							}
							result = false;
							break;
						}
					default: throw new Exception( "match " + pattern );
				}
				return new ValueSlot[] { ValueSlot.Bool( result ) };
			} ) );

			lib.Set( "format", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				string joined = LuaMachine.Concat( args );
				return new ValueSlot[] { ValueSlot.String( "[" + joined + "]" ) };
			} ) );
		}
	}
}
