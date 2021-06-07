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
		private int pc = 0;
		private int MultiRes = 0;
		private Function Func;

		private List<ValueSlot> ValueStack = new List<ValueSlot>();
		private List<(Function, int)> CallStack = new List<(Function, int)>();

		public Executor( Function func )
		{
			AddFrame( func );
		}

		private void AddFrame( Function new_func )
		{
			// Push old function + PC to stack.
			if (Func != null)
			{
				CallStack.Add( (Func, pc) );
			}
			this.pc = 0;
			this.Func = new_func;
			// Grow value stack.
			var nil = ValueSlot.Nil();
			for ( int i = 0; i < Func.prototype.numSlots; i++ )
			{
				ValueStack.Add( nil );
			}
		}

		private int GetRealStackIndex( uint index )
		{
			return (int)(ValueStack.Count - 1 - index);
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
			while (pc < Func.prototype.code.Length && safety < 1000)
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
			}
		}
		public void LogState()
		{
			int slot_i = 0;
			foreach (var level in CallStack)
			{
				var slot_count = level.Item1.prototype.numSlots;
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
				case OpCode.ISGT:
					{
						double nA = StackGet( A ).GetNumber();
						double nD = StackGet( D ).GetNumber();
						Log.Info( $"-> {nA} {nD}" );
						if (!(nA > nD))
						{
							pc++;
						}
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
				case OpCode.SUBVN:
				case OpCode.MULVN:
					{
						double nB = StackGet( B ).GetNumber();
						double nC = Func.prototype.GetConstNum( C );
						double result = 0;
						switch (OP)
						{
							case OpCode.SUBVN: result = nB - nC; break;
							case OpCode.MULVN: result = nB * nC; break;
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
				// Constants
				case OpCode.KSHORT:
					{
						var num = (short)D; // TODO signed?
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
							AddFrame( call_func.GetFunction() );

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

						for (int i=0;i< ret_count;i++ )
						{
							Log.Info( "ret " + ValueStack[ret_base - i] );
						}
						throw new Exception( $"return" );
						break;
					}
				// Loops and Branches
				case OpCode.LOOP:
					{
						// nop
						break;
					}
				case OpCode.FORI:
				case OpCode.FORL:
					Log.Info( "for-loop" );
					double stop = StackGet( A + 1 ).GetNumber();
					double step = StackGet( A + 2 ).GetNumber();

					double counter;
					if (OP == OpCode.FORI)
					{
						// for loop init
						double start = StackGet( A ).GetNumber();
						counter = start;
					} else
					{
						// for loop step
						counter = StackGet( A + 3 ).GetNumber();
						counter += step;
					}
					StackSet( A + 3, ValueSlot.Number( counter ) );

					if (step < 0 && counter < stop)
					{
						throw new Exception( $"fori {stop} {step} {counter}" );
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
			Log.Info( $"jump to {  (OpCode)(Func.prototype.code[pc + 1] & 0xFF) }" );
		}
	}
}
