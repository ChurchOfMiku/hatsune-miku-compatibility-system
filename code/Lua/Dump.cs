#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sandbox;

namespace Miku.Lua
{
	static class ReaderExt
	{
		public static string ReadStringN( this BinaryReader reader, int n )
		{
			var bytes = reader.ReadBytes( n );
			var str = Encoding.UTF8.GetString( bytes );
			return str;
		}
	}

	class Dump
	{
		private static readonly byte[] DUMP_HEADER = new byte[] { 27, 76, 74 };
		private static readonly byte   DUMP_VERSION = 2;
		private static readonly byte   DUMP_FLAGS = 0; // chunk must include debug info and be the correct endian-ness

		public static ProtoFunction Read(byte[] dump)
		{
			var stream = new MemoryStream( dump );
			var reader = new BinaryReader( stream );

			Assert.True( Enumerable.SequenceEqual( reader.ReadBytes(3), DUMP_HEADER ) );
			Assert.True( reader.ReadByte() == DUMP_VERSION );
			Assert.True( reader.Read7BitEncodedInt64() == DUMP_FLAGS );
			var chunkName = reader.ReadString();

			var protos = new Stack<ProtoFunction>();
			int proto_len;
			while ( (proto_len = reader.Read7BitEncodedInt()) != 0 ) {
				var proto = new ProtoFunction();
				proto.flags = reader.ReadByte();
				proto.numArgs = reader.ReadByte();
				proto.numSlots = reader.ReadByte();
				proto.UpValues = new ushort[reader.ReadByte()];

				proto.constGC = new ValueSlot[reader.Read7BitEncodedInt()];
				proto.constNum = new double[reader.Read7BitEncodedInt()];
				proto.code = new uint[reader.Read7BitEncodedInt()];

				var debugSize = reader.Read7BitEncodedInt();
				var debugFirstLine = reader.Read7BitEncodedInt();
				var debugLineCount = reader.Read7BitEncodedInt();

				proto.DebugName = chunkName + ":" + debugFirstLine;

				for (int i=0;i<proto.code.Length;i++)
				{
					proto.code[i] = reader.ReadUInt32();
				}

				for ( int i = 0; i < proto.UpValues.Length; i++ )
				{
					proto.UpValues[i] = reader.ReadUInt16();
				}

				for ( int i = 0; i < proto.constGC.Length; i++ )
				{
					var const_type = reader.Read7BitEncodedInt();
					switch (const_type)
					{
						case 0:
							proto.constGC[i] = protos.Pop();
							break;
						case 1:
							var size_array = reader.Read7BitEncodedInt();
							var size_hash = reader.Read7BitEncodedInt();
							var table = new Table();
							for (int j=0;j < size_array; j++ )
							{
								var entry = ReadTableEntry( reader );
								if (j == 0)
								{
									// Skip the zeroth entry unless it is actually provided
									if ( entry.Kind != ValueKind.Nil )
									{
										table.Set( 0, entry );
									}
								}
								else
								{
									table.PushVal( entry );
								}
							}
							for (int j=0;j < size_hash; j++ )
							{
								var key = ReadTableEntry( reader );
								var val = ReadTableEntry( reader );
								table.Set( key, val );
							}
							proto.constGC[i] = table;
							break;
						default:
							if (const_type >= 5)
							{
								proto.constGC[i] = reader.ReadStringN( const_type - 5 );
							} else
							{
								throw new Exception( "handle const type " + const_type );
							}
							break;
					}
				}

				for (int i = 0; i < proto.constNum.Length; i++ )
				{
					var n = reader.Read7BitEncodedInt64();
					var is_double = (n & 1) == 1;
					n >>= 1;
					if (is_double)
					{
						var m = reader.Read7BitEncodedInt64();
						long x = (m << 32) | n;
						double d = BitConverter.Int64BitsToDouble(x);
						proto.constNum[i] = d;
					} else
					{
						proto.constNum[i] = (int)n;
					}
				}

				// skip debuginfo for now
				reader.ReadBytes(debugSize);

				protos.Push(proto);
			}

			var result = protos.Pop();
			Assert.NotNull(result);
			return result;
		}

		private static ValueSlot ReadTableEntry( BinaryReader reader)
		{
			var entry_type = reader.Read7BitEncodedInt();
			switch ( entry_type )
			{
				case 0:
					return ValueSlot.NIL;
				case 1:
					return ValueSlot.FALSE;
				case 2:
					return ValueSlot.TRUE;
				case 3:
					return reader.Read7BitEncodedInt();
				case 4:
					long low = reader.Read7BitEncodedInt64();
					long high = reader.Read7BitEncodedInt64();
					double res = BitConverter.Int64BitsToDouble( low | (high << 32) );
					return res;
				default:
					if ( entry_type >= 5 )
					{
						return reader.ReadStringN( entry_type - 5 );
					}
					else
					{
						throw new Exception( "tab => " + entry_type );
					}
			}
		}
	}
}
