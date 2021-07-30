#define PROFILING
//#define TEST_HASH

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Miku.Lua;
using Miku.Lua.Objects;
using Tsu.CLI.Commands;
using Tsu.Numerics;

namespace Miku.Console
{
	using System;
	using System.Threading;

	public static class Program
	{
		private static readonly Process _currentProcess = Process.GetCurrentProcess();
		private static readonly Action profilerDump =
			typeof( Profiler ).GetMethod( "Dump", BindingFlags.Static | BindingFlags.NonPublic )
							  .CreateDelegate<Action>();
		private static readonly LuaMachine _luaMachine = CreateLuaMachine();
		private static ConsoleCommandManager _commandManager;
		private static Function _compile;
		private static int _consoleCodeCounter = 0;

		private static LuaMachine CreateLuaMachine()
		{
			var luaMachine = new LuaMachine();

			var osLib = luaMachine.Env.Get( "os" ).CheckTable();
			osLib.DefineFunc( "clock", static ex => _currentProcess.TotalProcessorTime.TotalSeconds );

			return luaMachine;
		}

		public static void Main()
		{
			if ( !Directory.Exists( "data" ) )
			{
				Directory.CreateDirectory( "data" );
			}

#if TEST_HASH
			Lua.Objects.Internal.MikuDict.Bench();
			return;
#endif

#if !PROFILING
			_commandManager = new();
			_commandManager.LoadCommands( typeof( Program ), null );
			_commandManager.AddHelpCommand();
			_commandManager.AddStopCommand( "exit", "stop", "quit" );
#endif

#if PROFILING
			var fullRunTimer = Stopwatch.StartNew();
			RunFile( "test\\speedtest.lua" );
			LuaStats();
			Sandbox.Log.Info( "Full runtime: " + fullRunTimer.Elapsed.TotalSeconds );
#else
			_commandManager.Start();
#endif
		}

		private static readonly EnumerationOptions _enumOptions = new EnumerationOptions
		{
			MatchType = MatchType.Win32,
			RecurseSubdirectories = true
		};

		[Command( "compile" )]
		private static void Compile( params string[] globs )
		{
			var files = globs.SelectMany( glob => Directory.EnumerateFiles( ".", glob, _enumOptions ) );

			foreach ( var path in files )
			{
				var code = File.ReadAllText( path );
				var start = Stopwatch.GetTimestamp();
				_compile.Call( _luaMachine, new ValueSlot[] { code, path } );
				var end = Stopwatch.GetTimestamp();
				Console.WriteLine( $"{path} {end - start} {Duration.Format( end - start )}" );
			}
		}

		[Command( "gmod" )]
		private static void LoadGmod()
		{
			_luaMachine.Env.Set( "SERVER", true );
			_luaMachine.Env.Set( "CLIENT", false );
			_luaMachine.RunFile( "glib_official/garrysmod/lua/includes/init.lua" );
		}

		[Command( "stats" )]
		private static void LuaStats()
		{
			profilerDump();
		}

		[Command( "runf" ), Command( "run-file" )]
		[RawInput]
		private static void RunFile( string path )
		{
			_luaMachine.RunFile( path );
		}

		[Command( "runs" ), Command( "run-string" )]
		[RawInput]
		private static void RunString( string code )
		{
			_luaMachine.RunString( code, $"Console.{Interlocked.Increment( ref _consoleCodeCounter ):000}" );
		}
	}
}
