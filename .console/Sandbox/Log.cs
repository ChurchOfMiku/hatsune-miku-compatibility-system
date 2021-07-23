using System;

#nullable enable

namespace Sandbox
{
    static internal class Log
    {
        private static void WriteWithColor( ConsoleColor color, string text )
        {
            (color, Console.ForegroundColor) = (Console.ForegroundColor, color);
            Console.Write( text );
            Console.ForegroundColor = color;
        }

        private static readonly object _infoLock = new();
        public static void Info( object? value )
        {
            lock ( _infoLock )
            {
                WriteWithColor( ConsoleColor.Blue, "[INF] " );
                Console.WriteLine( value );
            }
        }

        private static readonly object _warningLock = new();
        public static void Warning( object? value )
        {
            lock ( _warningLock )
            {
                WriteWithColor( ConsoleColor.Yellow, "[WRN] " );
                Console.WriteLine( value );
            }
        }

        private static readonly object _errorLock = new();
        public static void Error( object? value )
        {
            lock ( _errorLock )
            {
                WriteWithColor( ConsoleColor.Red, "[ERR] " );
                Console.WriteLine( value );
            }
        }
    }
}
