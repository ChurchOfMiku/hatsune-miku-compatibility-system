#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;
using Miku.Lua.Objects;

namespace Miku.GMod.Entities
{
	class EntityData
	{
		public Entity Entity;
		public UserData LuaValue;
		public Table LuaTable;

		public EntityData(Entity ent, UserData ud, Table tab)
		{
			Entity = ent;
			LuaValue = ud;
			LuaTable = tab ?? new Table();
		}
	}

	class EntityRegistry
	{
		private Dictionary<Entity, EntityData> Map = new Dictionary<Entity, EntityData>();

		public Table? ClassPlayer = null;
		public Table? ClassWeapon = null;

		/*public void RegisterClass(string name, ScriptedEntityKind kind, Table table)
		{
			table.Set( "__index", ValueSlot.Table( table ) );
			if (kind == ScriptedEntityKind.Weapon)
			{
				table.MetaTable = ClassWeapon;
			} else
			{
				throw new Exception( "Can not register SENTs." );
			}

			ScriptedClasses[name] = (kind, table);
		}*/

		/*public Entity Create(string name)
		{
			if (!ScriptedClasses.TryGetValue(name, out var pair))
			{
				throw new Exception( "Class is not registered: "+name );
			}

			if (pair.Item1 == ScriptedEntityKind.Weapon)
			{
				return new GmodWeapon(pair.Item2);
			} else
			{
				throw new Exception( "Can not create SENTs." );
			}
		}*/

		public EntityData Get(Entity ent, Table? table = null) {
			{
				if (Map.TryGetValue(ent, out EntityData? result))
				{
					if (table != null)
					{
						result.LuaTable = table;
					}
					return result;
				}
			}

			Table? class_table = null;
			if (ent is Player)
			{
				class_table = ClassPlayer;
			} else if (ent is BaseWeapon)
			{
				class_table = ClassWeapon;
			}

			if (class_table == null)
			{
				throw new Exception( "no class table for " + ent );
			}

			{
				// There's a gross cycle here:
				var ud = new UserData(TypeID.Entity, null, class_table );
				var ent_data = new EntityData(ent, ud, table ?? new Table());
				ud.Reference = ent_data;

				Map[ent] = ent_data;
				return ent_data;
			}
		}
	}
}
