using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;
using Miku.Lua;

namespace Miku.GMod.Entities
{
	class EntityMapper
	{
		Dictionary<Entity, Table> Map = new Dictionary<Entity, Table>();
		public Table Get(Entity ent) {
			throw new Exception( "niy" );
		}
	}
}
