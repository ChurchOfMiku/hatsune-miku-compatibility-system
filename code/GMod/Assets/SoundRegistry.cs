using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Miku.GMod.Assets
{
    class SoundRegistry
	{
		class SoundIndex : KVVisitor
		{
			public KVVisitor AddKeyBlock( string key )
			{
				return new SoundEntry(key);
			}

			public void AddKeyValue( string key, string value )
			{
				throw new Exception( "simple value at top level of sound script" );
			}

			public void Finish()
			{
				// don't care
			}
		}

		class SoundEntry : KVVisitor
		{
			string Name;
			SoundEvent Sound;

			public SoundEntry(string name)
			{
				Name = name;
				Sound = new SoundEvent();
			}

			public KVVisitor AddKeyBlock( string key )
			{
				if (key == "rndwave" )
				{
					return new SoundWaveList(Sound);
				}
				throw new Exception( "block = " + key );
			}

			public void AddKeyValue( string key, string value )
			{
				switch (key)
				{
					case "channel":
					case "soundlevel":
					case "pitch":
						// TODO?
						break;
					case "volume":
						if (value == "VOL_NORM" )
						{
							// just leave it as the default, TODO maybe it should be 1?
						} else if (value.Contains(','))
						{
							var parts = value.Split( ',' );
							Assert.Equals( parts.Length, 2 );
							float low = float.Parse( parts[0].Trim() );
							float high = float.Parse( parts[1].Trim() );
							Log.Info( "volume range, " + low + " - " + high );
						} else
						{
							Sound.Volume = float.Parse(value);
						}
						break;
					case "wave":
						Sound.Sounds = new[] { FixPath( value ) };
						break;
					default:
						Log.Info( "kv = "+key+" "+value );
						break;
				}
			}

			public void Finish()
			{
				//Log.Info( "registering " + Name );
				Sound.StaticRuntimeInit( Name );
			}
		}

		class SoundWaveList : KVVisitor
		{
			SoundEvent Sound;
			List<string> Paths = new List<string>();

			public SoundWaveList( SoundEvent sound )
			{
				Sound = sound;
			}

			public KVVisitor AddKeyBlock( string key )
			{
				throw new Exception( "key (block) in wave list = " + key );
			}

			public void AddKeyValue( string key, string value )
			{
				if (key == "wave")
				{
					Paths.Add( FixPath( value ));
				} else
				{
					throw new Exception( "key in wave list = "+key );
				}
			}

			public void Finish()
			{
				Sound.Sounds = Paths.ToArray();
			}
		}

		public static void ParseSoundScript(string path)
		{
			new KVParser(path, new SoundIndex() );
		}

		private static string FixPath(string path)
		{
			int start = 0;
			for ( ;start<2; start++ )
			{
				switch (path[start])
				{
					// These all have some significant meaning, but so far as I can tell aren't supported by sbox.
					case '*':
					case '#':
					case '@':
					case '<':
					case '>':
					case '^':
					case '(':
					case ')':
					case '}':
					case '$':
					case '!':
					case '?':
					case '&':
					case '~':
					case '`':
					case '+':
					case '%':
						break;
					default:
						goto stop;
				}
			}
			stop:
			if (start != 0)
			{
				path = path.Substring(start);
			}
			if (path[0] == '/')
			{
				throw new Exception( "blarg" );
			}
			if (path.EndsWith(".wav"))
			{
				// The game must be started in -tools mode (sbox-dev.exe) in order to compile these for some reason.
				path = path.Replace( ".wav", ".vsnd" );
			} else
			{
				throw new Exception( "bad file extension on sound? " + path );
			}
			return "sounds/" + path;
		}
	}
}
