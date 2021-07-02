using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.GMod
{
    enum TypeID
	{
		Invalid = -1,
		Nil = 0,
		Bool = 1,
		LightUserData = 2,
		Number = 3,
		String = 4,
		Table = 5,
		Function = 6,
		UserData = 7,
		Thread = 8,
		Entity = 9,
		Vector = 10,
		Angle = 11
	}
}
