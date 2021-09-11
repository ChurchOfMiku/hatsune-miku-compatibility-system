#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Miku.Lua;
using Miku.Lua.Objects;
using Sandbox;

namespace Miku.GMod.Lib
{
	class TraceLib
	{
		public TraceLib( GModMachine machine )
		{
			var util_lib = machine.Env.DefineLib( "util" );
			util_lib.DefineFunc( "TraceLine", ( Executor ex ) => {
				// input supports:
				// start?
				// endpos?
				// filter? - Entity only

				var trace_input = ex.GetArg(0).CheckTable();

				var start = trace_input.Get( "start" ).TryGetUserData()?.CheckVector() ?? new Vector3();
				var endpos = trace_input.Get( "endpos" ).TryGetUserData()?.CheckVector() ?? new Vector3();

				var trace = Trace.Ray( start, endpos );

				var filter = trace_input.Get( "filter" ).TryGetUserData()?.CheckEntity();
				if ( filter != null )
				{
					trace.Ignore( filter.GetEntity() );
				}
				var res = trace.Run();

				Log.Info( "??? " + res );

				var lua_res = new Table();

				return lua_res;
			} );
		}
	}
}
