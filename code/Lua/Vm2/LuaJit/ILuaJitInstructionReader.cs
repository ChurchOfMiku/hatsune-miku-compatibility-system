namespace Miku.Lua.Vm2
{
	interface ILuaJitInstructionReader
	{
		int Length { get; }
		int Position { get; set; }

		bool TryPeek( out LuaJitInstruction instruction );
		bool TryRead( out LuaJitInstruction instruction );
	}
}