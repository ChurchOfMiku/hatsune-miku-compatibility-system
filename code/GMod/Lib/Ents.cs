using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Miku.Lua;

namespace Miku.GMod.Lib
{
	class Ents
	{
		public Ents( GModMachine machine )
		{
			var lib_ents = machine.Env.DefineLib( "ents" );
			lib_ents.DefineFunc( "GetByIndex", ( Executor ex ) =>
			{
				int id = (int)ex.GetArg( 0 ).CheckNumber();
				var ent = Sandbox.Entity.FindByIndex( id );
				return machine.Ents.Get(ent).LuaValue;
			} );

		}
	}
}
