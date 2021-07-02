using System;
using System.Text;
using System.Text.RegularExpressions;
using Sandbox;
using System.Collections.Generic;

namespace Miku.Lua.CoreLib
{
	class PatternConverter
	{
		// Lua = https://www.lua.org/pil/20.2.html
		// C# = https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference
		// Anchors: multiline garbage?
		// Empty char classes are invalid.
		// Make sure empty patterns are handled correctly.
		public static Regex Convert( string pattern )
		{
			var converter = new PatternConverter(pattern);
			return converter.Run();
		}

		private string LuaPattern;
		private int index = 0;

		StringBuilder Result = new StringBuilder();

		char GetChar()
		{
			return LuaPattern[index++];
		}

		bool HasChar()
		{
			return index < LuaPattern.Length;
		}

		private PatternConverter(string pattern)
		{
			LuaPattern = pattern;
		}

		private Regex Run()
		{
			while (HasChar())
			{
				char c = GetChar();
				switch (c)
				{
					// Safe literal chars:
					case char cc when (Char.IsLetter( cc )):
					case '_':
					case '@':
					case '\'':
					case '\0': // REALLY not sure about this one.
						Result.Append( c );
						break;

					// Special chars that map 1:1
					case '(':
					case ')':
						// TODO regex ctor should reject mismatched parens?
						// NOTE: open paren must not be followed by a ?
					case '.':
					case '+':
						Result.Append( c );
						break;
					// Anchors. TODO figure out if we should use alternatives that don't care about multiline crap.
					case '^':
						if (index == 1)
						{
							Result.Append('^');
						} else
						{
							Result.Append( @"\^" );
						}
						break;
					case '$':
						if (index == LuaPattern.Length)
						{
							Result.Append( '$' );
						} else
						{
							Result.Append( @"\$" );
						}
						break;
					case '%':
						{
							char char_class = GetChar();
							switch (char_class)
							{
								case 'x':
									Result.Append( "[0-9A-Fa-f]" );
									break;
								default:
									throw new Exception( "HANDLE CLASS " + LuaPattern + " " + char_class );
							}

							break;
						}
					default:
						throw new Exception( "HANDLE CHAR " + LuaPattern + " " + c );
				}

			}
			return new Regex(Result.ToString());
		}
	}

	class String
	{
		public String( LuaMachine machine )
		{
			var lib = machine.Env.DefineLib( "string" );

			// Set string metatable.
			machine.PrimitiveMeta.MetaString = new Table();
			machine.PrimitiveMeta.MetaString.Set( "__index", ValueSlot.Table(lib) );

			lib.DefineFunc( "byte", ( Executor ex ) => {
				var str = ex.GetArg( 0 ).CheckString();
				int index = 0;
				if ( ex.GetArgCount() > 1 )
				{
					index = (int)ex.GetArg( 1 ).CheckNumber() - 1;
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
					return ValueSlot.Number( result );
				}
			} );

			lib.DefineFunc( "char", ( Executor ex ) => {
				char c = (char)ex.GetArg( 0 ).CheckNumber();
				return ValueSlot.String( c.ToString() );
			} );

			lib.DefineFunc( "lower", ( Executor ex ) => {
				var str = ex.GetArg(0).CheckString();
				return ValueSlot.String( str.ToLower() );
			} );

			lib.DefineFunc( "sub", ( Executor ex ) => {
				// TODO: see how this is actually implemented in lua, there is no way this is totally consistent
				var str = ex.GetArg( 0 ).CheckString();
				int start = (int)ex.GetArg( 1 ).CheckNumber() - 1;
				int length = str.Length;
				if ( start < 0 )
				{
					start = str.Length + start + 1;
				}
				if ( ex.GetArgCount() > 2 )
				{
					int arg2 = (int)ex.GetArg( 2 ).CheckNumber();
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
				return ValueSlot.String( str.Substring( start, length ) );
			} );

			lib.DefineFunc( "find", ( Executor ex ) =>
			{
				var str = ex.GetArg( 0 ).CheckString();
				var pattern = ex.GetArg( 1 ).CheckString();
				if ( ex.GetArgCount() > 2 )
				{
					throw new Exception( "string.find offset/noPatterns NYI" );
				}

				var regex = PatternConverter.Convert( pattern );
				var match = regex.Match( str );

				if (!match.Success)
				{
					return null;
				}

				if ( match.Groups.Count > 1 )
				{
					throw new Exception( "string.find groups" );
				}

				ex.Return( ValueSlot.Number( match.Index + 1 ) );
				return ValueSlot.Number(match.Index + match.Length);
			} );

			lib.DefineFunc( "match", ( Executor ex ) => {
				var str = ex.GetArg( 0 ).CheckString();
				var pattern = ex.GetArg( 1 ).CheckString();
				if ( ex.GetArgCount() > 2 )
				{
					throw new Exception( "string.match offset NYI" );
				}
				var regex = PatternConverter.Convert( pattern );
				var match = regex.Match( str );

				//Sandbox.Log.Info( "==> " + str + " " + pattern + " " + match );

				if (!match.Success)
				{
					return null;
				}

				// TODO: trying to repeat ()'s results in nil return
				if (match.Groups.Count > 1)
				{
					var results = new ValueSlot[match.Groups.Count - 1];
					for (int i=1;i<match.Groups.Count;i++ )
					{
						ex.Return( ValueSlot.String( match.Groups[i].Value ) );
					}
					return null;
				}

				return ValueSlot.String( match.Value );
			} );

			lib.DefineFunc( "gsub", ( Executor ex ) =>
			{
				var str = ex.GetArg( 0 ).CheckString();
				var pattern = ex.GetArg( 1 ).CheckString();
				var replace = ex.GetArg( 2 );
				if ( ex.GetArgCount() > 3 )
				{
					throw new Exception( "string.gsub maxReplaces NYI" );
				}

				var regex = PatternConverter.Convert( pattern );
				if (replace.Kind == ValueKind.String)
				{
					var result = regex.Replace( str, replace.CheckString() );
					return ValueSlot.String( result );
				} else
				{
					throw new Exception( "string.gsub replace = " + replace );
				}

			} );

			var format_regex = new Regex(@"%([\-+ #0]*)(\d*)(\.\d*)?([\w%])");
			lib.DefineFunc( "format", ( Executor ex ) => {
				string format = ex.GetArg( 0 ).CheckString();

				// TODO escape {'s
				var format_params = new List<object>();

				string converted_format = format_regex.Replace( format, ( match ) =>
				{
					var flags = match.Groups[1].Value;
					var width = match.Groups[2].Value;
					var precision = match.Groups[3].Value;
					var spec = match.Groups[4].Value;

					Assert.True( flags.Length == 0 );
					Assert.True( width.Length == 0 );
					Assert.True( precision.Length == 0 );

					int spec_index = format_params.Count;
					ValueSlot val = ex.GetArg(spec_index + 1);

					if ( spec == "s" )
					{
						// TODO is this the right behavior?
						format_params.Add( val.ToString() );
					} else if (spec == "d" || spec == "i") {
						format_params.Add( val.CheckNumber() );
					} else
					{
						throw new Exception( "format spec = " + spec );
					}
					return "{" + spec_index + "}";
				} );

				var result = string.Format( converted_format, format_params.ToArray() );
				return ValueSlot.String( result );
			} );
		}
	}
}
