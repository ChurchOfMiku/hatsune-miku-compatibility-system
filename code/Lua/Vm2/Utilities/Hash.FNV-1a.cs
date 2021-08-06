using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Miku.Lua.Vm2.Utilities
{
	internal static partial class Hash
	{
		/// <summary>
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		private const int Fnv1aPrime = 0x01000193;

		/// <summary>
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		private const int Fnv1aOffsetBasis = unchecked((int)0x811C9DC5);

		/// <summary>
		/// Calculates the FNV-1a hash of the provided byte sequence.
		/// </summary>
		internal static int GetFNV1aHashCode( byte[] data )
		{
			int hashCode = Fnv1aOffsetBasis;
			for ( int i = 0; i < data.Length; i++ )
			{
				hashCode = unchecked((hashCode ^ data[i]) * Fnv1aPrime);
			}
			return hashCode;
		}

		/// <summary>
		/// Calculates the FNV-1a hash of the provided byte sequence.
		/// </summary>
		internal static int GetFNV1aHashCode( ArraySegment<byte> data )
		{
			int hashCode = Fnv1aOffsetBasis;
			for ( int i = 0; i < data.Count; i++ )
			{
				hashCode = unchecked((hashCode ^ data[i]) * Fnv1aPrime);
			}
			return hashCode;
		}

		/// <summary>
		/// Calculates the FNV-1a hash of the provided byte sequence.
		/// </summary>
		internal static int GetFNV1aHashCode( ReadOnlySpan<byte> data )
		{
			int hashCode = Fnv1aOffsetBasis;
			for ( int i = 0; i < data.Length; i++ )
			{
				hashCode = unchecked((hashCode ^ data[i]) * Fnv1aPrime);
			}
			return hashCode;
		}

		/// <summary>
		/// Calculates the FNV-1a hash of the provided byte sequence.
		/// </summary>
		internal static int GetFNV1aHashCode( ImmutableArray<byte> data )
		{
			int hashCode = Fnv1aOffsetBasis;
			for ( int i = 0; i < data.Length; i++ )
			{
				hashCode = unchecked((hashCode ^ data[i]) * Fnv1aPrime);
			}
			return hashCode;
		}

		/// <summary>
		/// Calculates the FNV-1a hash of the provided byte sequence.
		/// </summary>
		internal static int GetFNV1aHashCode( IEnumerable<byte> data )
		{
			int hashCode = Fnv1aOffsetBasis;
			foreach ( byte v in data )
			{
				hashCode = unchecked((hashCode ^ v) * Fnv1aPrime);
			}
			return hashCode;
		}

		/// <summary>
		/// Calculates the FNV-1a hash of the provided char sequence.
		/// </summary>
		internal static int GetFNV1aHashCode( ReadOnlySpan<char> data )
		{
			int hashCode = Fnv1aOffsetBasis;
			for ( int i = 0; i < data.Length; i++ )
			{
				hashCode = unchecked((hashCode ^ data[i]) * Fnv1aPrime);
			}
			return hashCode;
		}

		/// <summary>
		/// Compute the hashcode of a sub-string using FNV-1a
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		internal static int GetFNV1aHashCode( string text, int start, int length )
		{
			return GetFNV1aHashCode( text.AsSpan( start, length ) );
		}
	}
}
