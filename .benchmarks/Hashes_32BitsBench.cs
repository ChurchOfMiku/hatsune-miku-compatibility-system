using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Miku.Lua.Vm2;

namespace Miku.Benchmarks
{
	[SimpleJob( RuntimeMoniker.Net60 )]
	[MemoryDiagnoser]
	[PlainExporter, HtmlExporter, MarkdownExporterAttribute.GitHub]
	public class Hashes_32BitsBench
	{
		[Params( 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 )]
		public int Length { get; set; }

		private byte[] _data = Array.Empty<byte>();

		[GlobalSetup]
		public void Setup()
		{
			_data = new byte[Length];
			new Random( 123 ).NextBytes( _data );
		}

		[Benchmark( Baseline = true, Description = "FNV-1a" )]
		public int FNV1aGetHashCode() =>
			Hash.GetFNV1aHashCode( _data.AsSpan() );

		[Benchmark( Description = "xxHash" )]
		public uint XXH32GetHashCode() =>
			Hash.GetXXHash32HashCode( _data.AsSpan() );
	}
}
