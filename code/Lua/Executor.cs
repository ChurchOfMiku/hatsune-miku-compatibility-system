using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Miku.Lua
{
	class Executor
	{
		struct FrameInfo
		{
			public Function Func;
			public int PC;
			public int RetBase;
			public int RetCount;
		}

		private int pc = 0;
		private int MultiRes = 0;
		private int StackTop = 0; // last entry in the stack + 1
		private Function Func;

		private List<ValueSlot> ValueStack = new List<ValueSlot>();
		private Stack<FrameInfo> CallStack = new Stack<FrameInfo>();

		public Executor( Function func )
		{
			AddFrame( func, 0, 0 );
		}

		private void AddFrame( Function new_func, int ret_base, int ret_count )
		{
			// Push old function + PC to stack.
			if (Func != null)
			{
				CallStack.Push( new FrameInfo{
					Func = Func,
					PC = pc,
					RetBase = ret_base,
					RetCount = ret_count
				} );
			}
			this.pc = 0;
			this.Func = new_func;

			// Grow value stack.
			var nil = ValueSlot.Nil();
			StackTop += Func.prototype.numSlots;
			while ( ValueStack.Count < StackTop )
			{
				ValueStack.Add( nil );
			}
		}

		private void PopFrame(int ret_source_base, int ret_slots_available)
		{
			if (CallStack.Count == 0)
			{
				if ( ret_slots_available != 0)
				{
					throw new Exception( "return values to C#" );
				}
			} else
			{
				var frame_info = CallStack.Pop();
				int ret_dest_base = frame_info.RetBase;
				int ret_slots_to_fill = frame_info.RetCount;
				if ( ret_slots_to_fill == -1 )
				{
					ret_slots_to_fill = ret_slots_available;
					MultiRes = ret_slots_available;
				}

				var nil = ValueSlot.Nil();
				for ( int i = 0; i < ret_slots_to_fill; i++ )
				{
					if ( i < ret_slots_available )
					{
						ValueStack[ret_dest_base - i] = ValueStack[ret_source_base - i];
					}
					else
					{
						ValueStack[ret_dest_base - i] = nil;
					}
				}

				StackTop -= Func.prototype.numSlots;
				Func = frame_info.Func;
				pc = frame_info.PC;
			}
		}

		private int GetRealStackIndex( uint index )
		{
			return (int)(StackTop - 1 - index);
		}

		private void StackSet(uint index, ValueSlot x)
		{
			ValueStack[GetRealStackIndex(index)] = x;
		}

		private ValueSlot StackGet(uint index)
		{
			return ValueStack[GetRealStackIndex( index )];
		}

		public void Run()
		{
			int safety = 0;
			const int LIMIT = 1_000_000;
			while (pc < Func.prototype.code.Length)
			{
				try
				{
					Step();
				} catch (Exception e)
				{
					LogState();
					throw;
				}
				safety++;
				if (safety >= LIMIT )
				{
					throw new Exception( "hit safety" );
				}
			}
		}
		public void LogState()
		{
			Log.Info( "======= STACK =======" );
			int slot_i = 0;
			foreach (var level in CallStack)
			{
				var slot_count = level.Func.prototype.numSlots;
				for (int j = 0; j < slot_count; j++ )
				{
					Log.Info( $"> {slot_count - j - 1}: {ValueStack[slot_i]}" );
					slot_i++;
				}
				Log.Info( "----------" );
			}
			{
				var slot_count = Func.prototype.numSlots;
				for ( int j = 0; j < slot_count; j++ )
				{
					Log.Info( $"> {slot_count - j - 1}: {ValueStack[slot_i]}" );
					slot_i++;
				}
			}
			Log.Info( "======= CODE =======" );
			for (int i=0;i<Func.prototype.code.Length;i++ )
			{
				uint instr = Func.prototype.code[i];
				var OP = (OpCode)(instr & 0xFF);
				if (pc == i)
				{
					Log.Info( "vvvvvvvvvvvvvvvvvvvvvvv" );
				}
				Log.Info( $"> {i}: {OP}" );
			}
		}

		public void Step()
		{
			uint instr = Func.prototype.code[pc];
			var OP = (OpCode)(instr & 0xFF);
			var A = (instr >> 8) & 0xFF;
			var B = (instr >> 24) & 0xFF;
			var C = (instr >> 16) & 0xFF;
			var D = (instr >> 16) & 0xFFFF;
			switch (OP)
			{
				// Comparisons
				case OpCode.ISLT:
				case OpCode.ISGE:
				case OpCode.ISLE:
				case OpCode.ISGT:
				case OpCode.ISEQV:
				case OpCode.ISNEV:
					{
						double nA = StackGet( A ).GetNumber();
						double nD = StackGet( D ).GetNumber();
						bool skip = false;
						switch (OP)
						{
							case OpCode.ISLT: skip = !(nA < nD); break;
							case OpCode.ISGE: skip = !(nA >= nD); break;
							case OpCode.ISLE: skip = !(nA <= nD); break;
							case OpCode.ISGT: skip = !(nA > nD); break;
							case OpCode.ISEQV: skip = !(nA == nD); break;
							case OpCode.ISNEV: skip = !(nA != nD); break;
						}
						if (skip) { pc++; }
						break;
					}
				// Move and Unary Ops
				case OpCode.MOV:
					{
						StackSet( A, StackGet( D ) );
						break;
					}
				case OpCode.LEN:
					{
						var table = StackGet( D ).GetTable();
						int len = table.GetLength();
						StackSet( A, ValueSlot.Number(len) );
						break;
					}
				// Binary Ops
				case OpCode.ADDVN:
				case OpCode.SUBVN:
				case OpCode.MULVN:
				case OpCode.DIVVN:
				case OpCode.MODVN:
					{
						double nB = StackGet( B ).GetNumber();
						double nC = Func.prototype.GetConstNum( C );
						double result = 0;
						switch (OP)
						{
							case OpCode.ADDVN: result = nB + nC; break;
							case OpCode.SUBVN: result = nB - nC; break;
							case OpCode.MULVN: result = nB * nC; break;
							case OpCode.DIVVN: result = nB / nC; break;
							case OpCode.MODVN: result = nB % nC; break;
						}
						StackSet( A, ValueSlot.Number( result ) );
						break;
					}
				case OpCode.SUBNV:
				case OpCode.MULNV:
					{
						double nB = StackGet( B ).GetNumber();
						double nC = Func.prototype.GetConstNum( C );
						double result = 0;
						switch ( OP )
						{
							case OpCode.SUBNV: result = nC - nB; break;
							case OpCode.MULNV: result = nC * nB; break;
						}
						StackSet( A, ValueSlot.Number( result ) );
						break;
					}
				case OpCode.ADDVV:
				case OpCode.SUBVV:
				case OpCode.MULVV:
					{
						double nB = StackGet( B ).GetNumber();
						double nC = StackGet( C ).GetNumber();
						double result = 0;
						switch (OP)
						{
							case OpCode.ADDVV: result = nB + nC; break;
							case OpCode.SUBVV: result = nB - nC; break;
							case OpCode.MULVV: result = nB * nC; break;
						}
						StackSet( A, ValueSlot.Number( result ) );
						break;
					}
				case OpCode.CAT:
					{
						var builder = new StringBuilder();
						for (uint i = B; i<=C;i++ )
						{
							// incompatible: glua fails to concat nil, possibly others for whatever reason
							builder.Append( StackGet( i ).ToString() );
						}
						StackSet( A, ValueSlot.String( builder.ToString() ) );
						break;
					}
				// Constants
				case OpCode.KSTR:
					{
						var str = Func.prototype.GetConstGC( D );
						StackSet( A,  str );
						break;
					}
				case OpCode.KSHORT:
					{
						var num = (short)D; // TODO signed?
						StackSet( A, ValueSlot.Number( num ) );
						break;
					}
				case OpCode.KNUM:
					{
						var num = Func.prototype.GetConstNum( D );
						StackSet( A, ValueSlot.Number( num ) );
						break;
					}
				// Upvalues and Function Init
				case OpCode.FNEW:
					{
						var new_proto = Func.prototype.GetConstGC(D).GetProtoFunction();
						var new_func = new Function( Func.env, new_proto );
						StackSet( A, ValueSlot.Function( new_func ) );
						break;
					}
				// Tables
				case OpCode.TNEW:
					{
						var table = new Table();
						StackSet( A, ValueSlot.Table( table ) );
						break;
					}
				case OpCode.TDUP:
					{
						var table_proto = Func.prototype.GetConstGC( D ).GetTable();
						var table = table_proto.CloneProto();
						StackSet( A, ValueSlot.Table( table ) );
						break;
					}
				case OpCode.GGET:
					{
						var str = Func.prototype.GetConstGC( D );
						StackSet( A, Func.env.Get( str ) );
						break;
					}
				case OpCode.GSET:
					{
						var str = Func.prototype.GetConstGC( D );
						Func.env.Set(str, StackGet(A));
						break;
					}
				case OpCode.TGETV:
					{
						var table = StackGet( B ).GetTable();
						StackSet( A, table.Get(StackGet(C)) );
						break;
					}
				case OpCode.TGETS:
					{
						var table = StackGet( B ).GetTable();
						var str = Func.prototype.GetConstGC( C );
						StackSet( A, table.Get( str ) );
						break;
					}
				// TGETB

				// TSETV
				// TSETS
				case OpCode.TSETB:
					{
						var table = StackGet( B ).GetTable();
						table.Set( (int)C, StackGet( A ) );
						break;
					}
				// Calls and Iterators
				case OpCode.CALL:
					{
						int call_base = GetRealStackIndex(A);
						
						int arg_base = call_base - 1;
						int arg_count = (int)C - 1;

						int ret_base = call_base;
						int ret_count = (int)B - 1;

						var call_func = ValueStack[call_base]; // TODO, meta calls
						if (call_func.IsFunction())
						{
							AddFrame( call_func.GetFunction(), ret_base, ret_count );

							for (int i=0;i<arg_count;i++ )
							{
								StackSet( (uint)i, ValueStack[arg_base - i] );
							}

							return; // DO NOT INCREMENT PC
						} else
						{
							var user_func = call_func.GetUserFunction();
							var args = new ValueSlot[arg_count];

							for ( int i = 0; i < arg_count; i++ )
							{
								args[i] = ValueStack[arg_base - i];
							}
							var rets = user_func( args );
							var nil = ValueSlot.Nil();

							// return all
							if (ret_count == -1 && rets != null)
							{
								ret_count = rets.Length;
								MultiRes = ret_count;
							}

							for ( int i = 0; i < ret_count; i++ )
							{
								if (rets != null && i < rets.Length)
								{
									ValueStack[ret_base - i] = rets[i];
								} else
								{
									ValueStack[ret_base - i] = nil;
								}
							}

							break;
						}
					}

				// Returns
				case OpCode.RETM:
					{
						int ret_base = GetRealStackIndex( A );
						int ret_count = (int)D + MultiRes;

						PopFrame(ret_base, ret_count);
						break;
					}
				case OpCode.RET0:
					{
						PopFrame( 0, 0 );
						break;
					}
				// Loops and Branches
				case OpCode.LOOP:
					{
						// nop
						break;
					}
				case OpCode.FORI:
					{
						double stop = StackGet( A + 1 ).GetNumber();
						double step = StackGet( A + 2 ).GetNumber();

						// for loop init
						double counter = StackGet( A ).GetNumber();
						StackSet( A + 3, ValueSlot.Number( counter ) );

						if ( (step > 0 && counter > stop) || (step < 0 && counter < stop) )
						{
							// condition failed, go to end
							Jump( D );
						}
						break;
					}
				case OpCode.FORL:
					{
						double stop = StackGet( A + 1 ).GetNumber();
						double step = StackGet( A + 2 ).GetNumber();

						// for loop step
						double counter = StackGet( A + 3 ).GetNumber();
						counter += step;
						StackSet( A + 3, ValueSlot.Number( counter ) );

						if ( !((step > 0 && counter > stop) || (step < 0 && counter < stop)) )
						{
							// condition passed, loop to top
							Jump( D );
						}
					}
					break;
				case OpCode.JMP:
					{
						Jump( D );
						break;
					}
				default:
					throw new Exception( $"> {OP}" );
			}
			pc++;
		}

		private void Jump(uint D)
		{
			int jump_offset = (int)D - 0x8000;
			pc += jump_offset;
		}
	}
}
