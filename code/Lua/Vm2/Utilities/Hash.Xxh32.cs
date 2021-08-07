using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Miku.Lua.Vm2
{
	// This was implemented based on the spec:
	// https://github.com/Cyan4973/xxHash/blob/dev/doc/xxhash_spec.md
	internal static partial class Hash
	{
		private const uint Xxh32Prime1 = 0x9E3779B1u;  // 0b10011110001101110111100110110001
		private const uint Xxh32Prime2 = 0x85EBCA77u;  // 0b10000101111010111100101001110111
		private const uint Xxh32Prime3 = 0xC2B2AE3Du;  // 0b11000010101100101010111000111101
		private const uint Xxh32Prime4 = 0x27D4EB2Fu;  // 0b00100111110101001110101100101111
		private const uint Xxh32Prime5 = 0x165667B1u;  // 0b00010110010101100110011110110001

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static uint Xxh32Round( uint acc, uint lane )
		{
			acc += lane * Xxh32Prime2;
			acc = BitOperations.RotateLeft( acc, 13 );
			acc *= Xxh32Prime1;
			return acc;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static uint Xxh32Avalanche( uint acc )
		{
			acc ^= acc >> 15;
			acc *= Xxh32Prime2;
			acc ^= acc >> 13;
			acc *= Xxh32Prime3;
			acc ^= acc >> 16;
			return acc;
		}

		public static uint GetXXHash32HashCode( ReadOnlySpan<byte> data, uint seed = 0 )
		{
			if ( data.IsEmpty )
			{
				return Xxh32Avalanche( seed + Xxh32Prime5 );
			}

			uint acc;
			if ( data.Length >= 16 )
			{
				uint acc1 = seed + Xxh32Prime1 + Xxh32Prime2;
				uint acc2 = seed + Xxh32Prime2;
				uint acc3 = seed + 0;
				uint acc4 = seed - Xxh32Prime1;

				while ( data.Length >= 16 )
				{
					acc1 = Xxh32Round( acc1, BitConverter.ToUInt32( data ) );
					data = data[4..];
					acc2 = Xxh32Round( acc2, BitConverter.ToUInt32( data ) );
					data = data[4..];
					acc3 = Xxh32Round( acc3, BitConverter.ToUInt32( data ) );
					data = data[4..];
					acc4 = Xxh32Round( acc4, BitConverter.ToUInt32( data ) );
					data = data[4..];
				}

				acc = BitOperations.RotateLeft( acc1, 1 )
					+ BitOperations.RotateLeft( acc2, 7 )
					+ BitOperations.RotateLeft( acc3, 12 )
					+ BitOperations.RotateLeft( acc4, 18 );
			}
			else
			{
				acc = seed + Xxh32Prime5;
			}

			acc += (uint)data.Length;

			while ( data.Length >= 4 )
			{
				uint lane = BitConverter.ToUInt32( data );
				acc += lane * Xxh32Prime3;
				acc = BitOperations.RotateLeft( acc, 17 ) * Xxh32Prime4;
				data = data[4..];
			}

			for ( int idx = 0; idx < data.Length; idx++ )
			{
				uint lane = data[idx];
				acc += lane * Xxh32Prime5;
				acc = BitOperations.RotateLeft( acc, 11 ) * Xxh32Prime1;
			}

			return Xxh32Avalanche( acc );
		}
	}
}
