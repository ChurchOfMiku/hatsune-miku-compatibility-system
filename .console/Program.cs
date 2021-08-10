//#define PROFILING
//#define TEST_HASH

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Miku.Lua;
using Miku.Lua.Objects;
using Tsu.CLI.Commands;
using Tsu.Numerics;

namespace Miku.Console
{
	using System;

	public static class Program
	{
		private static readonly Process _currentProcess = Process.GetCurrentProcess();
		private static readonly Action profilerDump =
			typeof( Profiler ).GetMethod( "Dump", BindingFlags.Static | BindingFlags.NonPublic )!
							  .CreateDelegate<Action>();
		private static readonly LuaMachine _luaMachine = CreateLuaMachine();
		private static Function _compile;
		private static int _consoleCodeCounter = 0;
		private static ConsoleCommandManager _commandManager = null!;

		[MemberNotNull( nameof( _compile ) )]
		private static LuaMachine CreateLuaMachine()
		{
			LuaMachine luaMachine = new();

			Table osLib = luaMachine.Env.Get( "os" ).CheckTable();
			osLib.DefineFunc( "clock", static ex => _currentProcess.TotalProcessorTime.TotalSeconds );

			_compile = (Function)typeof( LuaMachine )
				.GetField( "CompileFunction", BindingFlags.NonPublic | BindingFlags.Instance )!
				.GetValue( luaMachine )!;

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
		private static void CompileFiles( params string[] globs )
		{
			IEnumerable<string> files = globs.SelectMany( glob => Directory.EnumerateFiles( ".", glob, _enumOptions ) );

			foreach ( string path in files )
			{
				string code = File.ReadAllText( path );
				long start = Stopwatch.GetTimestamp();
				_compile.Call( _luaMachine, new ValueSlot[] { code, path } );
				long end = Stopwatch.GetTimestamp();
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
