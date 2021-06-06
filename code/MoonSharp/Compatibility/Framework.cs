using MoonSharp.Interpreter.Compatibility.Frameworks;

namespace MoonSharp.Interpreter.Compatibility
{
	public static class Framework
	{
		static FrameworkImpl s_FrameworkCurrent = new FrameworkImpl();

		public static FrameworkImpl Do { get { return s_FrameworkCurrent; } }
	}
}
