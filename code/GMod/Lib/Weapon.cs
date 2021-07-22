using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Miku.Lua;
using Miku.GMod.Entities;
using Sandbox;

namespace Miku.GMod.Lib
{
    class Weapon
	{
		public Weapon( GModMachineBase machine )
		{
			var class_weapon = machine.DefineClass( "Weapon" );
			machine.Ents.ClassWeapon = class_weapon;

			Dictionary<int, string> activityTable = new Dictionary<int, string>();

			foreach (var pair in machine.Env.GetDictionary())
			{
				var key_str = pair.Key.TryGetString();
				if ( key_str != null && key_str.StartsWith("ACT_") )
				{
					var act_id = pair.Value.TryGetNumber();
					if (act_id.HasValue)
					{
						activityTable.Add( (int)act_id, key_str );
					}
				}
			}

			class_weapon.DefineFunc( "SendWeaponAnim", ( Executor ex ) =>
			{
				var ent_info = ex.GetArg( 0 ).CheckUserData().CheckEntity();
				var view_model = ((GmodWeapon)ent_info.Entity).ViewModelEntity;
				if (view_model != null)
				{
					int act_id = (int)ex.GetArg( 1 ).CheckNumber();
					var act_name = activityTable[act_id];

					view_model.Sequence = act_name;
				}
				return null;
			} );

			// TODO actually do the reload,
			// this is just pasta'd SendWeaponAnim at the moment.
			class_weapon.DefineFunc( "DefaultReload", ( Executor ex ) =>
			{
				var ent_info = ex.GetArg( 0 ).CheckUserData().CheckEntity();
				var view_model = ((GmodWeapon)ent_info.Entity).ViewModelEntity;
				if ( view_model != null )
				{
					int act_id = (int)ex.GetArg( 1 ).CheckNumber();
					var act_name = activityTable[act_id];

					view_model.Sequence = act_name;
				}
				return null;
			} );
		}
	}
}
