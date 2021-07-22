using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Miku.GMod.Assets
{
	interface KVVisitor
	{
		void AddKeyValue( string key, string value );
		KVVisitor AddKeyBlock( string key );

		void Finish();
	}

    class KVParser
	{
		string Text;
		int Index = 0;

		public KVParser(string path, KVVisitor visitor)
		{
			Log.Info( "parser created" );
			Text = FileSystem.Mounted.ReadAllText( path );
			ParseBlock( visitor, false );
		}

		void ParseBlock(KVVisitor visitor, bool terminated_by_brace)
		{
			while (true)
			{
				SkipSpace();
				if (Index >= Text.Length)
				{
					if (terminated_by_brace)
					{
						throw new Exception( "Missing closing brace." );
					} else
					{
						visitor.Finish();
						return;
					}
				}
				if ( Text[Index] == '}' )
				{
					if ( terminated_by_brace )
					{
						Index++;
						visitor.Finish();
						return;
					}
					else
					{
						throw new Exception( "Too many closing braces." );
					}
				}
				var key = ReadString();
				SkipSpace();
				if ( Text[Index] == '{' )
				{
					Index++;
					var child = visitor.AddKeyBlock( key );
					ParseBlock( child, true );
				}
				else
				{
					var value = ReadString();
					visitor.AddKeyValue( key, value );
				}
			}
		}

		void SkipSpace()
		{
			while ( Index < Text.Length )
			{
				char c = Text[Index];
				switch ( c )
				{
					case '/':
						{
							char c2 = Text[Index+1];
							if ( c2 == '/' )
							{
								while ( Text[Index++] != '\n' );
							}
							else
							{
								throw new Exception( "c2 = " + c2 );
							}
							break;
						}
					case '\n':
					case '\r':
					case ' ':
					case '\t':
						Index++;
						break;
					default:
						return;
				}
			}
		}

		string ReadString()
		{
			StringBuilder builder = new StringBuilder();
			if ( Text[Index] != '"' )
			{
				throw new Exception( "!! " + Text.Substring( Index, 32 ) );
			}
			Index++;
			while (true)
			{
				char c = Text[Index];
				if (c == '"')
				{
					Index++;
					return builder.ToString();
				} else if (c == '\\')
				{
					throw new Exception( "ESCAPE" );
				} else
				{
					builder.Append(c);
				}
				Index++;
			}

		}
	}
}
