using System.Collections.Immutable;
using System.Reflection;
using Miku.Lua;

namespace Miku.Tests.TestData
{
	internal static class LuaJitFunctions
	{
		static LuaJitFunctions()
		{
			#region FizzBuzz
			FizzBuzz = new(
				nameof( FizzBuzz ),
				ImmutableArray.Create(
					3d,
					0d,
					5d ),
				ImmutableArray.Create(
					new Constant( "print" ),
					new Constant( "fizzbuzz" ),
					new Constant( "fizz" ),
					new Constant( "buzz" ) ),
				ImmutableArray.Create(
					new Instruction( "0001    KSHORT   0   1", 0x00010029u, OpCode.KSHORT, 0, 1 ),
					new Instruction( "0002    KSHORT   1 100", 0x00640129u, OpCode.KSHORT, 1, 100 ),
					new Instruction( "0003    KSHORT   2   1", 0x00010229u, OpCode.KSHORT, 2, 1 ),
					new Instruction( "0004    FORI     0 => 0033", 0x801C004Du, OpCode.FORI, 0, 32796 ),
					new Instruction( "0005 => MODVN    4   3   0  ; 3", 0x0300041Au, OpCode.MODVN, 4, 768 ),
					new Instruction( "0006    ISNEN    4   1      ; 0", 0x00010409u, OpCode.ISNEN, 4, 1 ),
					new Instruction( "0007    JMP      4 => 0015", 0x80070458u, OpCode.JMP, 4, 32775 ),
					new Instruction( "0008    MODVN    4   3   2  ; 5", 0x0302041Au, OpCode.MODVN, 4, 770 ),
					new Instruction( "0009    ISNEN    4   1      ; 0", 0x00010409u, OpCode.ISNEN, 4, 1 ),
					new Instruction( "0010    JMP      4 => 0015", 0x80040458u, OpCode.JMP, 4, 32772 ),
					new Instruction( "0011    GGET     4   0      ; \"print\"", 0x00000436u, OpCode.GGET, 4, 0 ),
					new Instruction( "0012    KSTR     6   1      ; \"fizzbuzz\"", 0x00010627u, OpCode.KSTR, 6, 1 ),
					new Instruction( "0013    CALL     4   1   2", 0x01020442u, OpCode.CALL, 4, 258 ),
					new Instruction( "0014    JMP      4 => 0032", 0x80110458u, OpCode.JMP, 4, 32785 ),
					new Instruction( "0015 => MODVN    4   3   0  ; 3", 0x0300041Au, OpCode.MODVN, 4, 768 ),
					new Instruction( "0016    ISNEN    4   1      ; 0", 0x00010409u, OpCode.ISNEN, 4, 1 ),
					new Instruction( "0017    JMP      4 => 0022", 0x80040458u, OpCode.JMP, 4, 32772 ),
					new Instruction( "0018    GGET     4   0      ; \"print\"", 0x00000436u, OpCode.GGET, 4, 0 ),
					new Instruction( "0019    KSTR     6   2      ; \"fizz\"", 0x00020627u, OpCode.KSTR, 6, 2 ),
					new Instruction( "0020    CALL     4   1   2", 0x01020442u, OpCode.CALL, 4, 258 ),
					new Instruction( "0021    JMP      4 => 0032", 0x800A0458u, OpCode.JMP, 4, 32778 ),
					new Instruction( "0022 => MODVN    4   3   2  ; 5", 0x0302041Au, OpCode.MODVN, 4, 770 ),
					new Instruction( "0023    ISNEN    4   1      ; 0", 0x00010409u, OpCode.ISNEN, 4, 1 ),
					new Instruction( "0024    JMP      4 => 0029", 0x80040458u, OpCode.JMP, 4, 32772 ),
					new Instruction( "0025    GGET     4   0      ; \"print\"", 0x00000436u, OpCode.GGET, 4, 0 ),
					new Instruction( "0026    KSTR     6   3      ; \"buzz\"", 0x00030627u, OpCode.KSTR, 6, 3 ),
					new Instruction( "0027    CALL     4   1   2", 0x01020442u, OpCode.CALL, 4, 258 ),
					new Instruction( "0028    JMP      4 => 0032", 0x80030458u, OpCode.JMP, 4, 32771 ),
					new Instruction( "0029 => GGET     4   0      ; \"print\"", 0x00000436u, OpCode.GGET, 4, 0 ),
					new Instruction( "0030    MOV      6   3", 0x00030612u, OpCode.MOV, 6, 3 ),
					new Instruction( "0031    CALL     4   1   2", 0x01020442u, OpCode.CALL, 4, 258 ),
					new Instruction( "0032 => FORL     0 => 0005", 0x7FE4004Fu, OpCode.FORL, 0, 32740 ),
					new Instruction( "0033 => RET0     0   1", 0x0001004Bu, OpCode.RET0, 0, 1 ) ) );
			#endregion FizzBuzz

			#region Factorial
			Factorial__fact = new(
				nameof( Factorial__fact ),
				ImmutableArray.Create(
					1d ),
				ImmutableArray.Create(
					new Constant( "fact" ) ),
				ImmutableArray.Create(
					new Instruction( "0001    KSHORT   1   0", 0x00000129u, OpCode.KSHORT, 1, 0 ),
					new Instruction( "0002    ISGT     0   1", 0x00010003u, OpCode.ISGT, 0, 1 ),
					new Instruction( "0003    JMP      1 => 0007", 0x80030158u, OpCode.JMP, 1, 32771 ),
					new Instruction( "0004    KSHORT   1   1", 0x00010129u, OpCode.KSHORT, 1, 1 ),
					new Instruction( "0005    RET1     1   2", 0x0002014Cu, OpCode.RET1, 1, 2 ),
					new Instruction( "0006    JMP      1 => 0012", 0x80050158u, OpCode.JMP, 1, 32773 ),
					new Instruction( "0007 => GGET     1   0      ; \"fact\"", 0x00000136u, OpCode.GGET, 1, 0 ),
					new Instruction( "0008    SUBVN    3   0   0  ; 1", 0x00000317u, OpCode.SUBVN, 3, 0 ),
					new Instruction( "0009    CALL     1   2   2", 0x02020142u, OpCode.CALL, 1, 514 ),
					new Instruction( "0010    MULVV    1   0   1", 0x00010122u, OpCode.MULVV, 1, 1 ),
					new Instruction( "0011    RET1     1   2", 0x0002014Cu, OpCode.RET1, 1, 2 ),
					new Instruction( "0012 => RET0     0   1", 0x0001004Bu, OpCode.RET0, 0, 1 ) ) );
			Factorial__root = new(
				nameof( Factorial__root ),
				ImmutableArray<double>.Empty,
				ImmutableArray.Create(
					new Constant( Factorial__fact ),
					new Constant( "fact" ),
					new Constant( "print" ) ),
				ImmutableArray.Create(
					new Instruction( "0001    FNEW     0   0      ; factorial.lua:1", 0x00000033u, OpCode.FNEW, 0, 0 ),
					new Instruction( "0002    GSET     0   1      ; \"fact\"", 0x00010037u, OpCode.GSET, 0, 1 ),
					new Instruction( "0003    GGET     0   2      ; \"print\"", 0x00020036u, OpCode.GGET, 0, 2 ),
					new Instruction( "0004    GGET     2   1      ; \"fact\"", 0x00010236u, OpCode.GGET, 2, 1 ),
					new Instruction( "0005    KSHORT   4  10", 0x000A0429u, OpCode.KSHORT, 4, 10 ),
					new Instruction( "0006    CALL     2   0   2", 0x00020242u, OpCode.CALL, 2, 2 ),
					new Instruction( "0007    CALLM    0   1   0", 0x01000041u, OpCode.CALLM, 0, 256 ),
					new Instruction( "0008    RET0     0   1", 0x0001004Bu, OpCode.RET0, 0, 1 ) ) );
			#endregion Factorial

			#region Mandelbrot
			Mandelbrot = new(
				nameof( Mandelbrot ),
				ImmutableArray.Create(
					-1.3d,
					1.3d,
					0.1d,
					-2.1d,
					1.1d,
					0.04d,
					2d,
					1d,
					10d ),
				ImmutableArray.Create(
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
					new Constant( "print" ) ),
				ImmutableArray.Create(
					new Instruction( "0001    TDUP     0   0", 0x00000035u, OpCode.TDUP, 0, 0 ),
					new Instruction( "0002    KNUM     1   0      ; -1.3", 0x0000012Au, OpCode.KNUM, 1, 0 ),
					new Instruction( "0003    KNUM     2   1      ; 1.3", 0x0001022Au, OpCode.KNUM, 2, 1 ),
					new Instruction( "0004    KNUM     3   2      ; 0.1", 0x0002032Au, OpCode.KNUM, 3, 2 ),
					new Instruction( "0005    FORI     1 => 0045", 0x8027014Du, OpCode.FORI, 1, 32807 ),
					new Instruction( "0006 => KSTR     5   1      ; \"\"", 0x00010527u, OpCode.KSTR, 5, 1 ),
					new Instruction( "0007    KNUM     6   3      ; -2.1", 0x0003062Au, OpCode.KNUM, 6, 3 ),
					new Instruction( "0008    KNUM     7   4      ; 1.1", 0x0004072Au, OpCode.KNUM, 7, 4 ),
					new Instruction( "0009    KNUM     8   5      ; 0.04", 0x0005082Au, OpCode.KNUM, 8, 5 ),
					new Instruction( "0010    FORI     6 => 0041", 0x801E064Du, OpCode.FORI, 6, 32798 ),
					new Instruction( "0011 => KSHORT  10   0", 0x00000A29u, OpCode.KSHORT, 10, 0 ),
					new Instruction( "0012    KSHORT  11   0", 0x00000B29u, OpCode.KSHORT, 11, 0 ),
					new Instruction( "0013    KSHORT  12   0", 0x00000C29u, OpCode.KSHORT, 12, 0 ),
					new Instruction( "0014 => KSHORT  13 100", 0x00640D29u, OpCode.KSHORT, 13, 100 ),
					new Instruction( "0015    ISGE    12  13", 0x000D0C01u, OpCode.ISGE, 12, 13 ),
					new Instruction( "0016    JMP     13 => 0036", 0x80130D58u, OpCode.JMP, 13, 32787 ),
					new Instruction( "0017    LOOP    13 => 0036", 0x80120D55u, OpCode.LOOP, 13, 32786 ),
					new Instruction( "0018    MULVV   13  10  10", 0x0A0A0D22u, OpCode.MULVV, 13, 2570 ),
					new Instruction( "0019    MULVV   14  11  11", 0x0B0B0E22u, OpCode.MULVV, 14, 2827 ),
					new Instruction( "0020    ADDVV   13  13  14", 0x0D0E0D20u, OpCode.ADDVV, 13, 3342 ),
					new Instruction( "0021    KSHORT  14   4", 0x00040E29u, OpCode.KSHORT, 14, 4 ),
					new Instruction( "0022    ISGT    14  13", 0x000D0E03u, OpCode.ISGT, 14, 13 ),
					new Instruction( "0023    JMP     13 => 0025", 0x80010D58u, OpCode.JMP, 13, 32769 ),
					new Instruction( "0024    JMP     13 => 0036", 0x800B0D58u, OpCode.JMP, 13, 32779 ),
					new Instruction( "0025 => MULVV   13  11  11", 0x0B0B0D22u, OpCode.MULVV, 13, 2827 ),
					new Instruction( "0026    MULVV   14  10  10", 0x0A0A0E22u, OpCode.MULVV, 14, 2570 ),
					new Instruction( "0027    SUBVV   13  13  14", 0x0D0E0D21u, OpCode.SUBVV, 13, 3342 ),
					new Instruction( "0028    ADDVV   13  13   9", 0x0D090D20u, OpCode.ADDVV, 13, 3337 ),
					new Instruction( "0029    MULNV   14  11   6  ; 2", 0x0B060E1Du, OpCode.MULNV, 14, 2822 ),
					new Instruction( "0030    MULVV   14  14  10", 0x0E0A0E22u, OpCode.MULVV, 14, 3594 ),
					new Instruction( "0031    ADDVV   14  14   4", 0x0E040E20u, OpCode.ADDVV, 14, 3588 ),
					new Instruction( "0032    ADDVN   12  12   7  ; 1", 0x0C070C16u, OpCode.ADDVN, 12, 3079 ),
					new Instruction( "0033    MOV     10  14", 0x000E0A12u, OpCode.MOV, 10, 14 ),
					new Instruction( "0034    MOV     11  13", 0x000D0B12u, OpCode.MOV, 11, 13 ),
					new Instruction( "0035    JMP     13 => 0014", 0x7FEA0D58u, OpCode.JMP, 13, 32746 ),
					new Instruction( "0036 => MOV     13   5", 0x00050D12u, OpCode.MOV, 13, 5 ),
					new Instruction( "0037    MODVN   14  12   8  ; 10", 0x0C080E1Au, OpCode.MODVN, 14, 3080 ),
					new Instruction( "0038    TGETV   14   0  14", 0x000E0E38u, OpCode.TGETV, 14, 14 ),
					new Instruction( "0039    CAT      5  13  14", 0x0D0E0526u, OpCode.CAT, 5, 3342 ),
					new Instruction( "0040    FORL     6 => 0011", 0x7FE2064Fu, OpCode.FORL, 6, 32738 ),
					new Instruction( "0041 => GGET     6   2      ; \"print\"", 0x00020636u, OpCode.GGET, 6, 2 ),
					new Instruction( "0042    MOV      8   5", 0x00050812u, OpCode.MOV, 8, 5 ),
					new Instruction( "0043    CALL     6   1   2", 0x01020642u, OpCode.CALL, 6, 258 ),
					new Instruction( "0044    FORL     1 => 0006", 0x7FD9014Fu, OpCode.FORL, 1, 32729 ),
					new Instruction( "0045 => RET0     0   1", 0x0001004Bu, OpCode.RET0, 0, 1 ) ) );
			#endregion Mandelbrot

			#region Speedtest
			Speedtest = new(
				nameof( Speedtest ),
				ImmutableArray.Create(
					2d,
					1d,
					7d,
					1.5d ),
				ImmutableArray.Create(
					new Constant( "print" ),
					new Constant( "-- The Computer Language Shootout\n-- http://shootout.alioth.debian.org/\n-- contributed by Mike Pall" ),
					new Constant( "P4\n" ),
					new Constant( " " ),
					new Constant( "os" ),
					new Constant( "clock" ) ),
				ImmutableArray.Create(
					new Instruction( "0001    GGET     0   0      ; \"print\"", 0x00000036u, OpCode.GGET, 0, 0 ),
					new Instruction( "0002    KSTR     2   1      ; \"-- The Computer Language Shootout\n-- ht\"~", 0x00010227u, OpCode.KSTR, 2, 1 ),
					new Instruction( "0003    CALL     0   1   2", 0x01020042u, OpCode.CALL, 0, 258 ),
					new Instruction( "0004    KSHORT   0 2000", 0x07D00029u, OpCode.KSHORT, 0, 2000 ),
					new Instruction( "0005    MOV      1   0", 0x00000112u, OpCode.MOV, 1, 0 ),
					new Instruction( "0006    DIVNV    2   0   0  ; 2", 0x0000021Eu, OpCode.DIVNV, 2, 0 ),
					new Instruction( "0007    KSHORT   3  50", 0x00320329u, OpCode.KSHORT, 3, 50 ),
					new Instruction( "0008    KSHORT   4   4", 0x00040429u, OpCode.KSHORT, 4, 4 ),
					new Instruction( "0009    KSHORT   5   0", 0x00000529u, OpCode.KSHORT, 5, 0 ),
					new Instruction( "0010    GGET     6   0      ; \"print\"", 0x00000636u, OpCode.GGET, 6, 0 ),
					new Instruction( "0011    KSTR     8   2      ; \"P4\n\"", 0x00020827u, OpCode.KSTR, 8, 2 ),
					new Instruction( "0012    MOV      9   0", 0x00000912u, OpCode.MOV, 9, 0 ),
					new Instruction( "0013    KSTR    10   3      ; \" \"", 0x00030A27u, OpCode.KSTR, 10, 3 ),
					new Instruction( "0014    MOV     11   1", 0x00010B12u, OpCode.MOV, 11, 1 ),
					new Instruction( "0015    CALL     6   1   5", 0x01050642u, OpCode.CALL, 6, 261 ),
					new Instruction( "0016    GGET     6   4      ; \"os\"", 0x00040636u, OpCode.GGET, 6, 4 ),
					new Instruction( "0017    TGETS    6   6   5  ; \"clock\"", 0x06050639u, OpCode.TGETS, 6, 1541 ),
					new Instruction( "0018    CALL     6   2   1", 0x02010642u, OpCode.CALL, 6, 513 ),
					new Instruction( "0019    KSHORT   7   0", 0x00000729u, OpCode.KSHORT, 7, 0 ),
					new Instruction( "0020    SUBVN    8   1   1  ; 1", 0x01010817u, OpCode.SUBVN, 8, 257 ),
					new Instruction( "0021    KSHORT   9   1", 0x00010929u, OpCode.KSHORT, 9, 1 ),
					new Instruction( "0022    FORI     7 => 0080", 0x8039074Du, OpCode.FORI, 7, 32825 ),
					new Instruction( "0023 => MULNV   11  10   0  ; 2", 0x0A000B1Du, OpCode.MULNV, 11, 2560 ),
					new Instruction( "0024    DIVVV   11  11   1", 0x0B010B23u, OpCode.DIVVV, 11, 2817 ),
					new Instruction( "0025    SUBVN   11  11   1  ; 1", 0x0B010B17u, OpCode.SUBVN, 11, 2817 ),
					new Instruction( "0026    KSHORT  12   0", 0x00000C29u, OpCode.KSHORT, 12, 0 ),
					new Instruction( "0027    SUBVN   13   0   1  ; 1", 0x00010D17u, OpCode.SUBVN, 13, 1 ),
					new Instruction( "0028    KSHORT  14   8", 0x00080E29u, OpCode.KSHORT, 14, 8 ),
					new Instruction( "0029    FORI    12 => 0079", 0x80310C4Du, OpCode.FORI, 12, 32817 ),
					new Instruction( "0030 => KSHORT  16   0", 0x00001029u, OpCode.KSHORT, 16, 0 ),
					new Instruction( "0031    ADDVN   17  15   2  ; 7", 0x0F021116u, OpCode.ADDVN, 17, 3842 ),
					new Instruction( "0032    KPRI    18   0", 0x0000122Bu, OpCode.KPRI, 18, 0 ),
					new Instruction( "0033    ISGE    17   0", 0x00001101u, OpCode.ISGE, 17, 0 ),
					new Instruction( "0034    JMP     19 => 0037", 0x80021358u, OpCode.JMP, 19, 32770 ),
					new Instruction( "0035    MOV     18  17", 0x00111212u, OpCode.MOV, 18, 17 ),
					new Instruction( "0036    JMP     19 => 0038", 0x80011358u, OpCode.JMP, 19, 32769 ),
					new Instruction( "0037 => SUBVN   18   0   1  ; 1", 0x00011217u, OpCode.SUBVN, 18, 1 ),
					new Instruction( "0038 => MOV     19  15", 0x000F1312u, OpCode.MOV, 19, 15 ),
					new Instruction( "0039    MOV     20  18", 0x00121412u, OpCode.MOV, 20, 18 ),
					new Instruction( "0040    KSHORT  21   1", 0x00011529u, OpCode.KSHORT, 21, 1 ),
					new Instruction( "0041    FORI    19 => 0068", 0x801A134Du, OpCode.FORI, 19, 32794 ),
					new Instruction( "0042 => ADDVV   16  16  16", 0x10101020u, OpCode.ADDVV, 16, 4112 ),
					new Instruction( "0043    KSHORT  23   0", 0x00001729u, OpCode.KSHORT, 23, 0 ),
					new Instruction( "0044    KSHORT  24   0", 0x00001829u, OpCode.KSHORT, 24, 0 ),
					new Instruction( "0045    KSHORT  25   0", 0x00001929u, OpCode.KSHORT, 25, 0 ),
					new Instruction( "0046    KSHORT  26   0", 0x00001A29u, OpCode.KSHORT, 26, 0 ),
					new Instruction( "0047    MULVV   27  22   2", 0x16021B22u, OpCode.MULVV, 27, 5634 ),
					new Instruction( "0048    SUBVN   27  27   3  ; 1.5", 0x1B031B17u, OpCode.SUBVN, 27, 6915 ),
					new Instruction( "0049    KSHORT  28   1", 0x00011C29u, OpCode.KSHORT, 28, 1 ),
					new Instruction( "0050    MOV     29   3", 0x00031D12u, OpCode.MOV, 29, 3 ),
					new Instruction( "0051    KSHORT  30   1", 0x00011E29u, OpCode.KSHORT, 30, 1 ),
					new Instruction( "0052    FORI    28 => 0067", 0x800E1C4Du, OpCode.FORI, 28, 32782 ),
					new Instruction( "0053 => MULVV   32  23  24", 0x17182022u, OpCode.MULVV, 32, 5912 ),
					new Instruction( "0054    SUBVV   33  25  26", 0x191A2121u, OpCode.SUBVV, 33, 6426 ),
					new Instruction( "0055    ADDVV   23  33  27", 0x211B1720u, OpCode.ADDVV, 23, 8475 ),
					new Instruction( "0056    ADDVV   33  32  32", 0x20202120u, OpCode.ADDVV, 33, 8224 ),
					new Instruction( "0057    ADDVV   24  33  11", 0x210B1820u, OpCode.ADDVV, 24, 8459 ),
					new Instruction( "0058    MULVV   25  23  23", 0x17171922u, OpCode.MULVV, 25, 5911 ),
					new Instruction( "0059    MULVV   26  24  24", 0x18181A22u, OpCode.MULVV, 26, 6168 ),
					new Instruction( "0060    ADDVN    5   5   1  ; 1", 0x05010516u, OpCode.ADDVN, 5, 1281 ),
					new Instruction( "0061    ADDVV   33  25  26", 0x191A2120u, OpCode.ADDVV, 33, 6426 ),
					new Instruction( "0062    ISGE     4  33", 0x00210401u, OpCode.ISGE, 4, 33 ),
					new Instruction( "0063    JMP     33 => 0066", 0x80022158u, OpCode.JMP, 33, 32770 ),
					new Instruction( "0064    ADDVN   16  16   1  ; 1", 0x10011016u, OpCode.ADDVN, 16, 4097 ),
					new Instruction( "0065    JMP     28 => 0067", 0x80011C58u, OpCode.JMP, 28, 32769 ),
					new Instruction( "0066 => FORL    28 => 0053", 0x7FF21C4Fu, OpCode.FORL, 28, 32754 ),
					new Instruction( "0067 => FORL    19 => 0042", 0x7FE6134Fu, OpCode.FORL, 19, 32742 ),
					new Instruction( "0068 => ISGT     0  17", 0x00110003u, OpCode.ISGT, 0, 17 ),
					new Instruction( "0069    JMP     19 => 0078", 0x80081358u, OpCode.JMP, 19, 32776 ),
					new Instruction( "0070    MOV     19   0", 0x00001312u, OpCode.MOV, 19, 0 ),
					new Instruction( "0071    MOV     20  17", 0x00111412u, OpCode.MOV, 20, 17 ),
					new Instruction( "0072    KSHORT  21   1", 0x00011529u, OpCode.KSHORT, 21, 1 ),
					new Instruction( "0073    FORI    19 => 0078", 0x8004134Du, OpCode.FORI, 19, 32772 ),
					new Instruction( "0074 => ADDVV   23  16  16", 0x10101720u, OpCode.ADDVV, 23, 4112 ),
					new Instruction( "0075    ADDVN   16  23   1  ; 1", 0x17011016u, OpCode.ADDVN, 16, 5889 ),
					new Instruction( "0076    ADDVN    5   5   1  ; 1", 0x05010516u, OpCode.ADDVN, 5, 1281 ),
					new Instruction( "0077    FORL    19 => 0074", 0x7FFC134Fu, OpCode.FORL, 19, 32764 ),
					new Instruction( "0078 => FORL    12 => 0030", 0x7FCF0C4Fu, OpCode.FORL, 12, 32719 ),
					new Instruction( "0079 => FORL     7 => 0023", 0x7FC7074Fu, OpCode.FORL, 7, 32711 ),
					new Instruction( "0080 => GGET     7   4      ; \"os\"", 0x00040736u, OpCode.GGET, 7, 4 ),
					new Instruction( "0081    TGETS    7   7   5  ; \"clock\"", 0x07050739u, OpCode.TGETS, 7, 1797 ),
					new Instruction( "0082    CALL     7   2   1", 0x02010742u, OpCode.CALL, 7, 513 ),
					new Instruction( "0083    GGET     8   0      ; \"print\"", 0x00000836u, OpCode.GGET, 8, 0 ),
					new Instruction( "0084    SUBVV   10   7   6", 0x07060A21u, OpCode.SUBVV, 10, 1798 ),
					new Instruction( "0085    CALL     8   1   2", 0x01020842u, OpCode.CALL, 8, 258 ),
					new Instruction( "0086    GGET     8   0      ; \"print\"", 0x00000836u, OpCode.GGET, 8, 0 ),
					new Instruction( "0087    MOV     10   5", 0x00050A12u, OpCode.MOV, 10, 5 ),
					new Instruction( "0088    CALL     8   1   2", 0x01020842u, OpCode.CALL, 8, 258 ),
					new Instruction( "0089    RET0     0   1", 0x0001004Bu, OpCode.RET0, 0, 1 ) ) );
			#endregion Speedtest
		}

		public static LuaJitFunction FizzBuzz { get; }

		public static LuaJitFunction Factorial__root { get; }

		public static LuaJitFunction Factorial__fact { get; }

		public static LuaJitFunction Mandelbrot { get; }

		public static LuaJitFunction Speedtest { get; }

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
