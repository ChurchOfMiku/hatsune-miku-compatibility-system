using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.GMod.Assets
{
    class AssetUtil
	{
		public static string FixModelName(string name)
		{
			return name.Replace( ".mdl", ".vmdl" );
		}
	}
}
