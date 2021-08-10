using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Miku.Lua;

namespace Miku.Benchmarks
{
	[SimpleJob( RuntimeMoniker.Net60 )]
	[MemoryDiagnoser]
	[PlainExporter, HtmlExporter, MarkdownExporterAttribute.GitHub]
	public class LuajitInstructionDecodingBench
	{
		public static uint[] Instructions => new[]
		{
			0x7ff8014fu,
		};

		public LuajitInstructionDecodingBench()
		{
		}

		[ParamsSource( nameof( Instructions ) )]
		public uint Instruction { get; set; }

		private static DecodedInstruction ManualDecode( uint raw )
		{
			OpCode op = (OpCode)(byte)raw;
			byte a = (byte)(raw >> 8);
			ushort d = (ushort)(raw >> 16);
			byte c = (byte)d;
			byte b = (byte)(d >> 8);
			return new DecodedInstruction( op, a, c, b, d );
		}

		[StructLayout( LayoutKind.Explicit )]
		private readonly struct Union
		{
			[FieldOffset( 0 )]
			public readonly uint Raw;

			#region Base

			[FieldOffset( 0 )]
			public readonly OpCode OpCode;

			[FieldOffset( 1 )]
			public readonly byte A;

			#endregion Base

			[FieldOffset( 2 )]
			public readonly ushort D;

			[FieldOffset( 2 )]
			public readonly byte C;

			[FieldOffset( 3 )]
			public readonly byte B;

			public Union( uint raw ) : this()
			{
				Raw = raw;
			}
		}

		private static DecodedInstruction UnionDecode( uint raw )
		{
			Union dec = new( raw );
			return new( dec.OpCode, dec.A, dec.C, dec.B, dec.D );
		}

		[Benchmark( Baseline = true )]
		public void ManualDecode() =>
			_ = ManualDecode( Instruction );

		[Benchmark]
		public void UnionDecode() =>
			_ = UnionDecode( Instruction );
	}

	internal record struct DecodedInstruction( OpCode OpCode, byte A, byte C, byte B, ushort D )
	{
		public DecodedInstruction( OpCode opCode, byte a, ushort d )
			: this( opCode, a, (byte)d, (byte)(d >> 8), d )
		{
		}
	}
}
