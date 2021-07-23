using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Sandbox
{
    internal static class Assert
    {
        //[Conditional( "Debug" )]
        public static void True( [DoesNotReturnIf( false )] bool value ) => Debug.Assert( value );

        //[Conditional( "Debug" )]
        public static void NotNull( [NotNull] object? value ) => Debug.Assert( value is not null );
    }
}
