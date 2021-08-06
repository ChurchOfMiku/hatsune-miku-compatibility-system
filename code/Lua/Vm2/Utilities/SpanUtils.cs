using System;
using System.Collections.Immutable;

namespace Miku.Lua.Vm2
{
	internal static class SpanUtils
	{
		public static ImmutableArray<T> ToImmutableArray<T>( this ReadOnlySpan<T> span )
		{
			var builder = ImmutableArray.CreateBuilder<T>( span.Length );
			foreach ( T val in span )
				builder.Add( val );
			return builder.MoveToImmutable();
		}
	}
}