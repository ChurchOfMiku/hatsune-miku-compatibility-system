#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Lua
{
    class PrimitiveMetaTables
	{
		public Table? MetaString = null;

		public Table? Get(ValueSlot value)
		{
			if (value.Kind == ValueKind.String)
			{
				return MetaString;
			}
			return null;
		}
	}
}
