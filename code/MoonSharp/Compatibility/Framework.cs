using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.Compatibility.Frameworks;

namespace MoonSharp.Interpreter.Compatibility
{
	public static class Framework
	{
		static FrameworkStubs s_FrameworkCurrent = new FrameworkStubs();

		public static FrameworkBase Do { get { return s_FrameworkCurrent; } }
	}
}
