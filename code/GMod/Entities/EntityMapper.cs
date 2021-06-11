
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

		public Table ClassPlayer = null;

		public Table Get(Entity ent) {
			{
				if (Map.TryGetValue(ent, out Table result))
				{
					return result;
				}
			}

			{
				var result = new Table();
				result.UserData = ent;

				if (ent is Player)
				{
					result.MetaTable = ClassPlayer;
				}

				Map[ent] = result;
				return result;
			}
		}
	}
}
