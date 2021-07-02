
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;
using Miku.Lua;

namespace Miku.GMod.Entities
{
	public enum ScriptedEntityKind
	{
		Entity,
		Weapon
	}

	class EntityRegistry
	{

		//private Dictionary<string, (ScriptedEntityKind, Table)> ScriptedClasses = new Dictionary<string, (ScriptedEntityKind, Table)>();

		private Dictionary<Entity, UserData> Map = new Dictionary<Entity, UserData>();

		public Table ClassPlayer = null;
		public Table ClassWeapon = null;

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

		public UserData Get(Entity ent, Table class_table = null) {
			{
				if (Map.TryGetValue(ent, out UserData result ))
				{
					return result;
				}
			}

			{
				if ( class_table == null)
				{
					if (ent is Player)
					{
						class_table = ClassPlayer;
					} else
					{
						throw new Exception( "can not handle " + ent );
					}
				}

				var result = new UserData((int)TypeID.Entity, ent, class_table );
				Map[ent] = result;
				return result;
			}
		}
	}
}
