using System.Collections.Immutable;

namespace Miku.Tests.TestData
{
	internal sealed class LuaJitFunction
	{
		public string Name { get; }
		public ImmutableArray<double> NumberConstants { get; }
		public ImmutableArray<Constant> Constants { get; }
		public ImmutableArray<Instruction> Instructions { get; }

		public LuaJitFunction(
			string name,
			ImmutableArray<double> numberConstants,
			ImmutableArray<Constant> constants,
			ImmutableArray<Instruction> instructions )
		{
			Name = name;
			NumberConstants = numberConstants;
			Constants = constants;
			Instructions = instructions;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
