using System;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	/// <summary>
	/// A class implementing all the bits needed to have a minimal support of a platform.
	/// This does not support the 'io'/'file' modules and has partial support of the 'os' module.
	/// </summary>
	public class LimitedPlatformAccessor
	{
		/// <summary>
		/// Default handler for 'print' calls. Can be customized in ScriptOptions
		/// </summary>
		/// <param name="content">The content.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void DefaultPrint(string content)
		{
			Sandbox.Log.Info(content);
		}
	}
}
