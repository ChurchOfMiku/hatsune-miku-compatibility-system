using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Lua
{
    class UserData
	{
		public int TypeID; // Used for GLua type IDs.
		public object Reference;
		public Table MetaTable;

		public UserData(int type_id, object reference, Table meta)
		{
			TypeID = type_id;
			Reference = reference;
			MetaTable = meta;
		}
	}
}
