using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.GMod.Lib
{
    class Weapon
	{
		public Weapon(GModMachineBase machine)
		{
			machine.RunFile( "glib/weapon.lua" );
			var class_weapon = machine.Env.Get( "_CLASS_WEAPON" ).CheckTable();
			machine.Ents.ClassWeapon = class_weapon;
		}
	}
}
