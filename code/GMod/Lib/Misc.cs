using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Miku.Lua;
using Sandbox;

namespace Miku.GMod.Lib
{
	class Misc
	{

		public Misc( GModMachine machine )
		{
			// TIMING:
			machine.Env.DefineFunc( "CurTime", ( Executor ex ) =>
			{
				return Time.Now;
			} );

			machine.Env.DefineFunc( "FrameTime", ( Executor ex ) =>
			{
				// is properly affected by host_timescale
				return Time.Delta;
			} );
		}
	}
}
