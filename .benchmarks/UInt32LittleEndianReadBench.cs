using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Miku.Benchmarks
{
	[SimpleJob( RuntimeMoniker.Net60 )]
	[MemoryDiagnoser]
	[PlainExporter, HtmlExporter, MarkdownExporterAttribute.GitHub]
	public class UInt32LittleEndianReadBench
	{
		[Params( 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 )]
		public int Length { get; set; }

		[MethodImpl( MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization )]
		private static uint ReadUInt32LittleEndian( ReadOnlySpan<byte> bytes, int offset ) =>
			bytes[offset + 0] | ((uint)bytes[offset + 1] << 8) | ((uint)bytes[offset + 2] << 16) | ((uint)bytes[offset + 3] << 24);

		private byte[] _data = Array.Empty<byte>();

		[GlobalSetup]
		public void Setup()
		{
			_data = new byte[Length];
			new Random( 123 ).NextBytes( _data );
		}

		[Benchmark( Baseline = true )]
		public uint BitConverter_ReadUInt32()
		{
			var span = _data.AsSpan();
			var acc = 0u;
			while ( span.Length >= 4 )
			{
				acc += BitConverter.ToUInt32( span );
				span = span[4..];
			}
			return acc;
		}

		[Benchmark]
		public uint ManualReadUInt32()
		{
			var span = _data.AsSpan();
			var acc = 0u;
			var idx = 0;
			var end = span.Length - 4;
			while ( idx <= end )
			{
				acc += ReadUInt32LittleEndian( span, idx );
				idx += 4;
			}
			return acc;
		}
	}
}
