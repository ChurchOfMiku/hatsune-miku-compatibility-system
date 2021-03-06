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
		public Entity GetEntity()
		{
			if (Entity == null)
			{
				throw new Exception("Attempt to use NULL entity.");
			}
			return Entity;
		}

		/// <summary>
		/// A subset of methods work on NULL entities. Use this on those.
		/// </summary>
		/// <returns></returns>
		public Entity? GetEntityOrNull()
		{
			return Entity;
		}

		private Entity? Entity;
		public UserData LuaValue;
		public Table? LuaTable;

		public EntityData(Entity? ent, UserData ud, Table? tab)
		{
			Entity = ent;
			LuaValue = ud;
			LuaTable = tab;
		}
	}

	class EntityRegistry
	{
		private Dictionary<Entity, EntityData> Map = new Dictionary<Entity, EntityData>();

		public Table? ClassEntity = null;
		public Table? ClassPlayer = null;
		public Table? ClassWeapon = null;

		private EntityData? Null = null;

		public EntityData GetNullEntity()
		{
			if (Null == null)
			{
				var null_ud = new UserData( TypeID.Entity, null, ClassEntity );
				Null = new EntityData( null, null_ud, null );
				null_ud.Reference = Null;
			}
			return Null;
		}

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

		public EntityData Get(Entity? ent, Table? table = null) {
			if (ent == null)
			{
				return GetNullEntity();
			}

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
