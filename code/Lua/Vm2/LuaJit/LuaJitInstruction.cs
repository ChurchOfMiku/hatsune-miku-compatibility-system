using System.Runtime.InteropServices;

#nullable enable

namespace Miku.Lua.Vm2
{
	/// <summary>
	/// Represents a LuaJIT instruction.
	/// </summary>
	/// <remarks>
	/// LuaJIT instructions have two formats:
	/// <list type="table">
	/// <item>
	/// <term>B</term>
	/// <term>C</term>
	/// <term>A</term>
	/// <term>OP</term>
	/// </item>
	/// <item>
	/// <term>D</term>
	/// <term>A</term>
	/// <term>OP</term>
	/// </item>
	/// </list>
	/// For more information on which type an OP uses, check
	/// <a href="http://wiki.luajit.org/Bytecode-2.0">http://wiki.luajit.org/Bytecode-2.0</a>
	/// </remarks>
	[StructLayout( LayoutKind.Explicit )]
	internal readonly struct LuaJitInstruction : IEquatable<LuaJitInstruction>
	{
		[FieldOffset( 0 )]
		private readonly uint _raw;

		#region Base

		[FieldOffset( 0 )]
		private readonly OpCode _opCode;
		public OpCode OpCode => _opCode;

		[FieldOffset( 1 )]
		private readonly byte _a;
		public byte A => _a;

		#endregion Base

		#region Instr Type A

		[FieldOffset( 2 )]
		private readonly ushort _d;
		public ushort D => _d;

		#endregion Instr Type A

		#region Instr Type B

		[FieldOffset( 2 )]
		private readonly byte _c;
		public byte C => _c;

		[FieldOffset( 3 )]
		private readonly byte _b;
		public byte B => _b;

		#endregion Instr Type B

		// Don't use this, use Decode.
		private LuaJitInstruction( uint raw ) : this()
		{
			_raw = raw;
		}

		public LuaJitInstruction( OpCode opCode, byte a, ushort d ) : this()
		{
			_opCode = opCode;
			_a = a;
			_d = d;
		}

		public LuaJitInstruction( OpCode opCode, byte a, byte c, byte b ) : this()
		{
			_opCode = opCode;
			_a = a;
			_c = c;
			_b = b;
		}

		/// <summary>
		/// Encodes this instruction.
		/// </summary>
		/// <returns></returns>
		public uint Encode() => _raw;

		/// <summary>
		/// Decodes an instruction without validating it.
		/// <see cref="Decode(uint)"/> is recommended as it does validation.
		/// </summary>
		/// <param name="raw">The instruction to decode.</param>
		/// <returns></returns>
		public static LuaJitInstruction UnsafeDecode( uint raw ) => new( raw );

		/// <summary>
		/// Decodes an instruction and validates it.
		/// </summary>
		/// <param name="raw"></param>
		/// <returns></returns>
		/// <remarks>
		/// Currently only validates the OpCode.
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the instruction has an invalid opcode.
		/// </exception>
		public static LuaJitInstruction Decode( uint raw )
		{
			LuaJitInstruction instr = new( raw );
			if ( instr.OpCode is < OpCode.First or > OpCode.Last )
			{
				throw new InvalidOperationException( $"Instruction has an invalid opcode." );
			}
			return instr;
		}

		/// <inheritdoc/>
		public override string ToString() => $"{{ Op = {OpCode}, A = 0x{A:X2}, B = 0x{B:X2}, C = 0x{C:X2}, D = 0x{D:X4} }}";

		/// <inheritdoc/>
		public override bool Equals( object? obj ) => obj is LuaJitInstruction instruction && Equals( instruction );

		/// <inheritdoc/>
		public bool Equals( LuaJitInstruction other ) => _raw == other._raw;

		/// <inheritdoc/>
		public override int GetHashCode() => HashCode.Combine( _raw );

		/// <summary>
		/// Checks whether an instruction is equal to another.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>Whether both instructions are equal.</returns>
		public static bool operator ==( LuaJitInstruction left, LuaJitInstruction right ) => left.Equals( right );

		/// <summary>
		/// Checks whether an instruction is not equal to another.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>Whether both instructions are not equal.</returns>
		public static bool operator !=( LuaJitInstruction left, LuaJitInstruction right ) => !(left == right);
	}
}
