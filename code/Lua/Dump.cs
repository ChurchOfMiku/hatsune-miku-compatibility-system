#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sandbox;
using Miku.Lua.Objects;

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

			var header = reader.ReadBytes( 3 );
			var version = reader.ReadByte();
			var flags = reader.ReadByte();

			Assert.True( Enumerable.SequenceEqual( header, DUMP_HEADER ) );
			Assert.True( version == DUMP_VERSION );
			Assert.True( flags == DUMP_FLAGS );
			var chunkName = reader.ReadString();

			var protos = new Stack<ProtoFunction>();
			int proto_len;
			while ( (proto_len = reader.Read7BitEncodedInt()) != 0 ) {
				var proto = new ProtoFunction();
				proto.Flags = reader.ReadByte();
				proto.NumArgs = reader.ReadByte();
				proto.NumSlots = reader.ReadByte();
				proto.UpValues = new ushort[reader.ReadByte()];

				proto.ConstGC = new ValueSlot[reader.Read7BitEncodedInt()];
				proto.ConstNum = new double[reader.Read7BitEncodedInt()];
				proto.Code = new uint[reader.Read7BitEncodedInt()];

				var debugSize = reader.Read7BitEncodedInt();
				uint debugFirstLine = (uint)reader.Read7BitEncodedInt();
				var debugLineCount = reader.Read7BitEncodedInt();

				proto.DebugName = chunkName + ":" + debugFirstLine;
				proto.ChunkName = chunkName;

				for (int i=0;i<proto.Code.Length;i++)
				{
					proto.Code[i] = reader.ReadUInt32();
				}

				for ( int i = 0; i < proto.UpValues.Length; i++ )
				{
					proto.UpValues[i] = reader.ReadUInt16();
				}

				for ( int i = 0; i < proto.ConstGC.Length; i++ )
				{
					var const_type = reader.Read7BitEncodedInt();
					switch (const_type)
					{
						case 0:
							proto.ConstGC[i] = protos.Pop();
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
							proto.ConstGC[i] = table;
							break;
						default:
							if (const_type >= 5)
							{
								proto.ConstGC[i] = String.Intern( reader.ReadStringN( const_type - 5 ) );
							} else
							{
								throw new Exception( "handle const type " + const_type );
							}
							break;
					}
				}

				for (int i = 0; i < proto.ConstNum.Length; i++ )
				{
					var n = reader.Read7BitEncodedInt64();
					var is_double = (n & 1) == 1;
					n >>= 1;
					if (is_double)
					{
						var m = reader.Read7BitEncodedInt64();
						long x = (m << 32) | n;
						double d = BitConverter.Int64BitsToDouble(x);
						proto.ConstNum[i] = d;
					} else
					{
						proto.ConstNum[i] = (int)n;
					}
				}

				// skip debuginfo for now
				var debug_bytes = reader.ReadBytes(debugSize);
				{
					var debug_reader = new BinaryReader( new MemoryStream( debug_bytes ) );
					proto.LineInfo = new uint[proto.Code.Length];
					if (debugLineCount < 256)
					{
						for (int i=0;i< proto.LineInfo.Length; i++)
						{
							proto.LineInfo[i] = debug_reader.ReadByte() + debugFirstLine;
						}
					} else if (debugLineCount < 65536)
					{
						for (int i = 0; i < proto.LineInfo.Length; i++)
						{
							proto.LineInfo[i] = debug_reader.ReadUInt16() + debugFirstLine;
						}
					} else
					{
						for (int i = 0; i < proto.LineInfo.Length; i++)
						{
							proto.LineInfo[i] = debug_reader.ReadUInt32() + debugFirstLine;
						}
					}
				}

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
						return String.Intern( reader.ReadStringN( entry_type - 5 ) );
					}
					else
					{
						throw new Exception( "tab => " + entry_type );
					}
			}
		}
	}
}
