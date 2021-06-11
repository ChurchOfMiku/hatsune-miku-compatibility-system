using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Miku.GMod.Entities
{
	class GmodWeapon : BaseWeapon
	{
		public override void AttackPrimary()
		{
			base.AttackPrimary();
			var dmg = new DamageInfo();
			dmg.Damage = 10;
			Owner.TakeDamage( dmg );
		}
	}
}
