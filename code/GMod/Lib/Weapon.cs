using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Miku.Lua;

namespace Miku.GMod.Lib
{
    class Weapon
	{
		public Weapon(GModMachineBase machine)
		{
			var class_weapon = new Table();
			machine.Ents.ClassWeapon = class_weapon;
		}
	}
}
