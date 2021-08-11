using System.Collections.Immutable;
using System.Reflection;
using Miku.Lua;

namespace Miku.Tests.TestData
{
	internal static class LuaJitFunctions
	{
		static LuaJitFunctions()
		{
			FizzBuzz = new(
				nameof( FizzBuzz ),
				ImmutableArray.Create( new double[]
				{
					2,
					0,
				} ),
				ImmutableArray.Create( new[]
				{
					new Constant( "fizz" ),
					new Constant( "buzz" ),
				} ),
				ImmutableArray.Create( new[]
				{
					new Instruction( "0001    TNEW     0   0", 0x00000034u, OpCode.TNEW  , 0, 0 ),
					new Instruction( "0002    KSHORT   1   1", 0x00010129u, OpCode.KSHORT, 1, 1 ),
					new Instruction( "0003    KSHORT   2 100", 0x00640229u, OpCode.KSHORT, 2, 100 ),
					new Instruction( "0004    KSHORT   3   1", 0x00010329u, OpCode.KSHORT, 3, 1 ),
					new Instruction( "0005    FORI     1 => 0014", 0x8008014Du, OpCode.FORI  , 1, 32776 ),
					new Instruction( "0006 => MODVN    5   4   0  ; 2", 0x0400051Au, OpCode.MODVN , 5, 1024 ),
					new Instruction( "0007    ISNEN    5   1      ; 0", 0x00010509u, OpCode.ISNEN , 5, 1 ),
					new Instruction( "0008    JMP      5 => 0011", 0x80020558u, OpCode.JMP   , 5, 32770 ),
					new Instruction( "0009    KSTR     5   0      ; \"fizz\"", 0x00000527u, OpCode.KSTR  , 5, 0 ),
					new Instruction( "0010    JMP      6 => 0012", 0x80010658u, OpCode.JMP   , 6, 32769 ),
					new Instruction( "0011 => KSTR     5   1      ; \"buzz\"", 0x00010527u, OpCode.KSTR  , 5, 1 ),
					new Instruction( "0012 => TSETV    5   0   4", 0x0004053Cu, OpCode.TSETV , 5, 4 ),
					new Instruction( "0013    FORL     1 => 0006", 0x7FF8014Fu, OpCode.FORL  , 1, 32760 ),
					new Instruction( "0014 => RET0     0   1", 0x0001004Bu, OpCode.RET0  , 0, 1 ),
				} ) );

			Factorial = new(
				nameof( Factorial ),
				ImmutableArray.Create( new double[]
				{
					1,
				} ),
				ImmutableArray.Create( new[]
				{
					new Constant( "fact" ),
				} ),
				ImmutableArray.Create( new[]
				{
					new Instruction( "0001    KSHORT   1   0", 0x00000129u, OpCode.KSHORT, 1, 0 ),
					new Instruction( "0002    ISGT     0   1", 0x00010003u, OpCode.ISGT  , 0, 1 ),
					new Instruction( "0003    JMP      1 => 0007", 0x80030158u, OpCode.JMP   , 1, 32771 ),
					new Instruction( "0004    KSHORT   1   1", 0x00010129u, OpCode.KSHORT, 1, 1 ),
					new Instruction( "0005    RET1     1   2", 0x0002014Cu, OpCode.RET1  , 1, 2 ),
					new Instruction( "0006    JMP      1 => 0012", 0x80050158u, OpCode.JMP   , 1, 32773 ),
					new Instruction( "0007 => GGET     1   0      ; \"fact\"", 0x00000136u, OpCode.GGET  , 1, 0 ),
					new Instruction( "0008    SUBVN    3   0   0  ; 1", 0x00000317u, OpCode.SUBVN , 3, 0 ),
					new Instruction( "0009    CALL     1   2   2", 0x02020142u, OpCode.CALL  , 1, 514 ),
					new Instruction( "0010    MULVV    1   0   1", 0x00010122u, OpCode.MULVV , 1, 1 ),
					new Instruction( "0011    RET1     1   2", 0x0002014Cu, OpCode.RET1  , 1, 2 ),
					new Instruction( "0012 => RET0     0   1", 0x0001004Bu, OpCode.RET0  , 0, 1 ),
				} ) );

			Mandelbrot = new(
				nameof( Mandelbrot ),
				ImmutableArray.Create( new double[]
				{
					-1.3,
					1.3,
					0.1,
					-2.1,
					1.1,
					0.04,
					2,
					1,
					10,
				} ),
				ImmutableArray.Create( new[]
				{
					new Constant( new Dictionary<Constant, Constant>
					{
						[new Constant( 0 )] = new Constant( " " ),
						[new Constant( 1 )] = new Constant( "." ),
						[new Constant( 2 )] = new Constant( ":" ),
						[new Constant( 3 )] = new Constant( "-" ),
						[new Constant( 4 )] = new Constant( "=" ),
						[new Constant( 5 )] = new Constant( "+" ),
						[new Constant( 6 )] = new Constant( "*" ),
						[new Constant( 7 )] = new Constant( "#" ),
						[new Constant( 8 )] = new Constant( "%" ),
						[new Constant( 9 )] = new Constant( "@" ),
					} ),
					new Constant( "" ),
					new Constant( "print" ),
				} ),
				ImmutableArray.Create( new[]
				{
					new Instruction( "0001    TDUP     0   0", 0x00000035u, OpCode.TDUP  , 0, 0 ),
					new Instruction( "0002    KNUM     1   0      ; -1.3", 0x0000012Au, OpCode.KNUM  , 1, 0 ),
					new Instruction( "0003    KNUM     2   1      ; 1.3", 0x0001022Au, OpCode.KNUM  , 2, 1 ),
					new Instruction( "0004    KNUM     3   2      ; 0.1", 0x0002032Au, OpCode.KNUM  , 3, 2 ),
					new Instruction( "0005    FORI     1 => 0045", 0x8027014Du, OpCode.FORI  , 1, 32807 ),
					new Instruction( "0006 => KSTR     5   1      ; \"\"", 0x00010527u, OpCode.KSTR  , 5, 1 ),
					new Instruction( "0007    KNUM     6   3      ; -2.1", 0x0003062Au, OpCode.KNUM  , 6, 3 ),
					new Instruction( "0008    KNUM     7   4      ; 1.1", 0x0004072Au, OpCode.KNUM  , 7, 4 ),
					new Instruction( "0009    KNUM     8   5      ; 0.04", 0x0005082Au, OpCode.KNUM  , 8, 5 ),
					new Instruction( "0010    FORI     6 => 0041", 0x801E064Du, OpCode.FORI  , 6, 32798 ),
					new Instruction( "0011 => KSHORT  10   0", 0x00000A29u, OpCode.KSHORT, 10, 0 ),
					new Instruction( "0012    KSHORT  11   0", 0x00000B29u, OpCode.KSHORT, 11, 0 ),
					new Instruction( "0013    KSHORT  12   0", 0x00000C29u, OpCode.KSHORT, 12, 0 ),
					new Instruction( "0014 => KSHORT  13 100", 0x00640D29u, OpCode.KSHORT, 13, 100 ),
					new Instruction( "0015    ISGE    12  13", 0x000D0C01u, OpCode.ISGE  , 12, 13 ),
					new Instruction( "0016    JMP     13 => 0036", 0x80130D58u, OpCode.JMP   , 13, 32787 ),
					new Instruction( "0017    LOOP    13 => 0036", 0x80120D55u, OpCode.LOOP  , 13, 32786 ),
					new Instruction( "0018    MULVV   13  10  10", 0x0A0A0D22u, OpCode.MULVV , 13, 2570 ),
					new Instruction( "0019    MULVV   14  11  11", 0x0B0B0E22u, OpCode.MULVV , 14, 2827 ),
					new Instruction( "0020    ADDVV   13  13  14", 0x0D0E0D20u, OpCode.ADDVV , 13, 3342 ),
					new Instruction( "0021    KSHORT  14   4", 0x00040E29u, OpCode.KSHORT, 14, 4 ),
					new Instruction( "0022    ISGT    14  13", 0x000D0E03u, OpCode.ISGT  , 14, 13 ),
					new Instruction( "0023    JMP     13 => 0025", 0x80010D58u, OpCode.JMP   , 13, 32769 ),
					new Instruction( "0024    JMP     13 => 0036", 0x800B0D58u, OpCode.JMP   , 13, 32779 ),
					new Instruction( "0025 => MULVV   13  11  11", 0x0B0B0D22u, OpCode.MULVV , 13, 2827 ),
					new Instruction( "0026    MULVV   14  10  10", 0x0A0A0E22u, OpCode.MULVV , 14, 2570 ),
					new Instruction( "0027    SUBVV   13  13  14", 0x0D0E0D21u, OpCode.SUBVV , 13, 3342 ),
					new Instruction( "0028    ADDVV   13  13   9", 0x0D090D20u, OpCode.ADDVV , 13, 3337 ),
					new Instruction( "0029    MULNV   14  11   6  ; 2", 0x0B060E1Du, OpCode.MULNV , 14, 2822 ),
					new Instruction( "0030    MULVV   14  14  10", 0x0E0A0E22u, OpCode.MULVV , 14, 3594 ),
					new Instruction( "0031    ADDVV   14  14   4", 0x0E040E20u, OpCode.ADDVV , 14, 3588 ),
					new Instruction( "0032    ADDVN   12  12   7  ; 1", 0x0C070C16u, OpCode.ADDVN , 12, 3079 ),
					new Instruction( "0033    MOV     10  14", 0x000E0A12u, OpCode.MOV   , 10, 14 ),
					new Instruction( "0034    MOV     11  13", 0x000D0B12u, OpCode.MOV   , 11, 13 ),
					new Instruction( "0035    JMP     13 => 0014", 0x7FEA0D58u, OpCode.JMP   , 13, 32746 ),
					new Instruction( "0036 => MOV     13   5", 0x00050D12u, OpCode.MOV   , 13, 5 ),
					new Instruction( "0037    MODVN   14  12   8  ; 10", 0x0C080E1Au, OpCode.MODVN , 14, 3080 ),
					new Instruction( "0038    TGETV   14   0  14", 0x000E0E38u, OpCode.TGETV , 14, 14 ),
					new Instruction( "0039    CAT      5  13  14", 0x0D0E0526u, OpCode.CAT   , 5, 3342 ),
					new Instruction( "0040    FORL     6 => 0011", 0x7FE2064Fu, OpCode.FORL  , 6, 32738 ),
					new Instruction( "0041 => GGET     6   2      ; \"print\"", 0x00020636u, OpCode.GGET  , 6, 2 ),
					new Instruction( "0042    MOV      8   5", 0x00050812u, OpCode.MOV   , 8, 5 ),
					new Instruction( "0043    CALL     6   1   2", 0x01020642u, OpCode.CALL  , 6, 258 ),
					new Instruction( "0044    FORL     1 => 0006", 0x7FD9014Fu, OpCode.FORL  , 1, 32729 ),
					new Instruction( "0045 => RET0     0   1", 0x0001004Bu, OpCode.RET0  , 0, 1 ),
				} ) );
		}

		public static LuaJitFunction FizzBuzz { get; }

		public static LuaJitFunction Factorial { get; }

		public static LuaJitFunction Mandelbrot { get; }

		public static IEnumerable<LuaJitFunction> All
		{
			get
			{
				IEnumerable<PropertyInfo> props = typeof( LuaJitFunctions )
					.GetProperties( BindingFlags.Public | BindingFlags.Static )
					.Where( prop => prop.PropertyType == typeof( LuaJitFunction ) );

				foreach ( PropertyInfo? prop in props )
				{
					yield return (LuaJitFunction)prop.GetValue( null )!;
				}
			}
		}
	}
}
