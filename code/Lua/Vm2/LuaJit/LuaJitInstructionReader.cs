using System;

#nullable enable

namespace Miku.Lua.Vm2
{
	class LuaJitInstructionReader : ILuaJitInstructionReader
	{
		private readonly ReadOnlyMemory<uint> _buffer;
		private int _position;

		public LuaJitInstructionReader( ReadOnlyMemory<uint> buffer )
		{
			_buffer = buffer;
			_position = 0;
		}

		public LuaJitInstructionReader( ReadOnlyMemory<uint> buffer, int position ) : this( buffer )
		{
			if ( _position < 0 || _position > buffer.Length )
			{
				throw new ArgumentOutOfRangeException( nameof( position ), "Position must be inside the buffer." );
			}

			_position = position;
		}

		public int Position
		{
			get => _position;
			set
			{
				if ( value != _position )
				{
					if ( value < 0 || value > _buffer.Length )
					{
						throw new ArgumentOutOfRangeException( nameof( value ), "Position must be inside the buffer." );
					}

					_position = value;
				}
			}
		}

		public int Length => _buffer.Length;

		public bool TryPeek( out LuaJitInstruction instruction )
		{
			if ( Position >= _buffer.Length )
			{
				instruction = default;
				return false;
			}

			instruction = LuaJitInstruction.Decode( _buffer.Span[_position] );
			return true;
		}

		public bool TryRead( out LuaJitInstruction instruction )
		{
			if ( TryPeek( out instruction ) )
			{
				Position++;
				return true;
			}

			return false;
		}
	}
}
