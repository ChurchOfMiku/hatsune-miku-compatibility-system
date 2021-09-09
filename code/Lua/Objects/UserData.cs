using Miku.GMod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Lua.Objects
{
	class UserData
	{
		public GMod.TypeID TypeID; // Used for GLua type IDs.
		public object Reference;
		public Table MetaTable;

		public UserData( GMod.TypeID type_id, object reference, Table meta )
		{
			TypeID = type_id;
			Reference = reference;
			MetaTable = meta;
		}

		public EntityData CheckEntity()
		{
			return (EntityData)Reference;
		}

		public Vector3 CheckVector()
		{
			return (Vector3)Reference;
		}
	}
}
