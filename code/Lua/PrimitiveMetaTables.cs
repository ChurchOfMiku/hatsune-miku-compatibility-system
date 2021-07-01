#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Lua
{
	/// <summary>
	/// Used to get metatables for values, and store information about primitive metatables.
	/// </summary>
    class PrimitiveMetaTables
	{
		public Table? MetaString = null;

		public Table? Get(ValueSlot value)
		{
			if (value.Kind == ValueKind.Table)
			{
				var tab = value.CheckTable();
				return tab.MetaTable;
			}
			if (value.Kind == ValueKind.String)
			{
				return MetaString;
			}
			return null;
		}
	}
}
