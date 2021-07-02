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
    class MetaTables
	{
		public Table? MetaString = null;

		public Table? Get(ValueSlot value)
		{
			if (value.Kind == ValueKind.Table)
			{
				return value.CheckTable().MetaTable;
			}
			if (value.Kind == ValueKind.UserData)
			{
				return value.CheckUserData().MetaTable;
			}
			if (value.Kind == ValueKind.String)
			{
				return MetaString;
			}
			return null;
		}
	}
}
