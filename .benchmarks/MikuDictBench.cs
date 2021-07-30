using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Miku.Lua;
using Miku.Lua.Objects.Internal;

namespace Miku.Benchmarks
{
	[SimpleJob( RuntimeMoniker.Net60 )]
	[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
	[CategoriesColumn]
	[MemoryDiagnoser]
	[PlainExporter, HtmlExporter, MarkdownExporterAttribute.GitHub]
	public class MikuDictBench
	{
		private ImmutableArray<ValueSlot> _keys, _values;
		// we can assign null since we set them in setup
		private MikuDict _readMikuDict = null!;
		private Dictionary<ValueSlot, ValueSlot> _readNetDict = null!;

		[Params( 62 )]
		public int Size { get; set; }

		[GlobalSetup]
		public void GlobalSetup()
		{
			_readMikuDict = new MikuDict();
			_readNetDict = new Dictionary<ValueSlot, ValueSlot>();
			var keys = ImmutableArray.CreateBuilder<ValueSlot>( Size );
			var values = ImmutableArray.CreateBuilder<ValueSlot>( Size );
			for ( var n = 0; n < Size; n++ )
			{
				keys.Add( "key" + n );
				values.Add( "value" + n );
				_readMikuDict.Set( keys[n], values[n] );
				_readNetDict[keys[n]] = values[n];
			}
			_keys = keys.MoveToImmutable();
			_values = values.MoveToImmutable();
		}

		[Benchmark( Baseline = true )]
		[BenchmarkCategory( "Write" )]
		public void NetWrite()
		{
			var dict = new Dictionary<ValueSlot, ValueSlot>();
			var keys = _keys;
			var values = _values;
			for ( var n = 0; n < Size; n++ )
			{
				dict[keys[n]] = values[n];
			}
		}

		[Benchmark]
		[BenchmarkCategory( "Write" )]
		public void MikuWrite()
		{
			var dict = new MikuDict();
			var keys = _keys;
			var values = _values;
			for ( var n = 0; n < Size; n++ )
			{
				dict.Set( keys[n], values[n] );
			}
		}

		[Benchmark( Baseline = true )]
		[BenchmarkCategory( "Read" )]
		public void NetRead()
		{
			var dict = _readNetDict;
			var keys = _keys;
			for ( var n = 0; n < Size; n++ )
			{
				_ = dict[keys[n]];
			}
		}

		[Benchmark]
		[BenchmarkCategory( "Read" )]
		public void MikuRead()
		{
			var dict = _readMikuDict;
			var keys = _keys;
			for ( var n = 0; n < Size; n++ )
			{
				_ = dict.Get( keys[n] );
			}
		}
	}
}
