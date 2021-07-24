//#define PROFILING
#define TEST_HASH

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Miku.Lua;
using Tsu.CLI.Commands;
using Tsu.Numerics;

namespace Miku.Console
{
    using System;

    public static class Program
    {
        private static readonly Action profilerDump =
            typeof( Profiler ).GetMethod( "Dump", BindingFlags.Static | BindingFlags.NonPublic )
                              .CreateDelegate<Action>();
        private static ConsoleCommandManager _commandManager;
        private static LuaMachine _luaMachine;
        private static Function _compile;

        public static void Main()
        {
            if ( !Directory.Exists( "data" ) )
            {
                Directory.CreateDirectory( "data" );
            }
#if TEST_HASH
			Lua.Experimental.MikuDict.Bench();
			return;
#endif

#if !PROFILING
			_commandManager = new();
            _commandManager.LoadCommands( typeof( Program ), null );
            _commandManager.AddHelpCommand();
            _commandManager.AddStopCommand( "exit", "stop", "quit" );
#endif
            _luaMachine = new LuaMachine();
            _compile = (Function)typeof( LuaMachine )
                .GetField( "CompileFunction", BindingFlags.NonPublic | BindingFlags.Instance )
                .GetValue( _luaMachine );

#if PROFILING
            Compile(
                "lua\\core\\*.lua",
                "lua\\glib\\*.lua",
                "lua\\glib_official\\garrysmod\\lua\\*.lua" );
			LuaStats();
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
        private static void LuaStats() => profilerDump();

        [Command( "run" )]
        private static void RunFile( string path )
        {
            _luaMachine.RunFile( path );
        }
    }
}
