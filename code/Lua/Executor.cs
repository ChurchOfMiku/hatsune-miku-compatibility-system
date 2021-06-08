﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Miku.Lua
{
	class SilentExecException : Exception
	{
		private Exception Wrapped;
		public SilentExecException(Exception wrapped)
		{
			Wrapped = wrapped;
		}
		public override string Message => Wrapped.Message;
	}
	class Executor
	{
		struct FrameInfo
		{
			public Function Func;
			public int PC;
			public int RetBase;
			public int RetCount;
		}

		public class UpValueBox
		{
			public (Executor, int)? StackSlot;
			public ValueSlot Value;

			public ValueSlot Get()
			{
				if ( StackSlot != null )
				{
					var ss = StackSlot.Value;
					return ss.Item1.ValueStack[ss.Item2];
				}
				else
				{
					return Value;
				}
			}

			public void Set(ValueSlot val)
			{
				if ( StackSlot != null )
				{
					var ss = StackSlot.Value;
					ss.Item1.ValueStack[ss.Item2] = val;
				}
				else
				{
					Value = val;
				}
			}
		}

		private int pc = 0;
		private int MultiRes = 0;
		private int StackTop = 0; // last entry in the stack + 1
		private Function Func;

		private List<ValueSlot> ValueStack = new List<ValueSlot>();
		private Stack<FrameInfo> CallStack = new Stack<FrameInfo>();
		private SortedDictionary<int, UpValueBox> OpenUpValues = new SortedDictionary<int, UpValueBox>();

		public ValueSlot[]? Results;

		public Executor( Function func, ValueSlot[] args )
		{
			AddFrame( func, 0, 0 );
			for (int i=0;i<args.Length;i++ )
			{
				ValueStack[i] = args[i];
			}
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
				Results = new ValueSlot[ret_slots_available];
				for ( int i = 0; i < ret_slots_available; i++ )
				{
					Results[i] = ValueStack[ret_source_base - i];
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
					if (!(e is SilentExecException))
					{
						LogState();
					}
					throw new SilentExecException(e);
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
				Log.Info( "^^^--- " + level.Func.prototype.DebugName );
			}
			{
				var slot_count = Func.prototype.numSlots;
				for ( int j = 0; j < slot_count; j++ )
				{
					Log.Info( $"> {slot_count - j - 1}: {ValueStack[slot_i]}" );
					slot_i++;
				}
				Log.Info( "^^^--- " + Func.prototype.DebugName );
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
							case OpCode.ISEQV: skip = !(nA == nD); break; // TODO BAD
							case OpCode.ISNEV: skip = !(nA != nD); break; // TODO BAD
						}
						if (skip) { pc++; }
						break;
					}
				case OpCode.ISEQS:
					{
						var vB = StackGet( B );
						var vC = Func.prototype.GetConstGC( C );
						bool skip = !vB.Equals(vC);
						if ( skip ) { pc++; }
						break;
					}
				// ISEQS
				case OpCode.ISNES:
					{
						var vB = StackGet( B );
						var vC = Func.prototype.GetConstGC( C );
						bool skip = vB.Equals( vC );
						if ( skip ) { pc++; }
						break;
					}
				// ISEQN
				// ISNEN
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
				// KCDATA
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
				case OpCode.KPRI:
					{
						ValueSlot val;
						if (D == 1)
						{
							val = ValueSlot.Bool(false);
						} else if (D == 2)
						{
							val = ValueSlot.Bool(true);
						} else
						{
							val = ValueSlot.Nil();
						}
						StackSet( A, val );
						break;
					}
				case OpCode.KNIL:
					{
						var nil = ValueSlot.Nil();
						for (uint i=A;i<=D;i++)
						{
							StackSet( i, nil );
						}
						break;
					}
				// Upvalues and Function Init
				case OpCode.UGET:
					{
						var val = Func.UpValues[D].Get();
						StackSet( A, val );
						break;
					}
				case OpCode.USETV:
					{
						Func.UpValues[A].Set(StackGet(D));
						break;
					}
				case OpCode.UCLO:
					{
						int upval_close_base = GetRealStackIndex( A );
						for (int i= upval_close_base; i<StackTop;i++)
						{
							UpValueBox uv;
							if (OpenUpValues.TryGetValue( i, out uv ))
							{
								uv.Value = ValueStack[uv.StackSlot.Value.Item2];
								OpenUpValues.Remove( i );
								uv.StackSlot = null;
							}
						}
						Jump( D );
						break;
					}
				case OpCode.FNEW:
					{
						var new_proto = Func.prototype.GetConstGC(D).GetProtoFunction();
						var upvals = new UpValueBox[new_proto.UpValues.Length];
						for (int i=0;i< upvals.Length; i++ )
						{
							var uv_code = new_proto.UpValues[i];
							if ( (uv_code & 0x8000) == 0 )
							{
								var uv_slot = uv_code & 0x3FFF;
								var uv_box = Func.UpValues[uv_slot];
								Assert.NotNull(uv_box);
								upvals[i] = uv_box;
							} else
							{
								var uv_slot = uv_code & 0x3FFF;
								int real_stack_index = GetRealStackIndex( (uint)uv_slot );
								UpValueBox uv_box;
								if (!OpenUpValues.TryGetValue( real_stack_index, out uv_box) )
								{
									uv_box = new UpValueBox()
									{
										StackSlot = (this, real_stack_index)
									};
									OpenUpValues.Add( real_stack_index, uv_box );
								}
								upvals[i] = uv_box;
							}
						}
						var new_func = new Function( Func.env, new_proto, upvals );
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
				case OpCode.TSETS:
					{
						var table = StackGet( B ).GetTable();
						var str = Func.prototype.GetConstGC( C );
						table.Set( str, StackGet( A ) );
						break;
					}
				case OpCode.TSETB:
					{
						var table = StackGet( B ).GetTable();
						table.Set( (int)C, StackGet( A ) );
						break;
					}
				// Calls and Iterators
				case OpCode.CALL:
				case OpCode.CALLT:
					{
						int call_base = GetRealStackIndex(A);
						
						int arg_base = call_base - 1;
						int arg_count = (int)C - 1;

						int ret_base = call_base;
						int ret_count = (int)B - 1;

						var call_func = ValueStack[call_base]; // TODO, meta calls
						if (call_func.IsFunction())
						{
							if (OP == OpCode.CALLT)
							{
								throw new Exception( "todo tailcall for lua funcs" );
							}
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
							var rets = user_func( args, Func.env );
							var nil = ValueSlot.Nil();

							// tail calls return everything
							if (OP == OpCode.CALLT)
							{
								ret_count = -1;
							}

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

							if (OP == OpCode.CALLT)
							{
								PopFrame( ret_base, ret_count );
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
				case OpCode.RET1:
					{
						int ret_base = GetRealStackIndex( A );
						PopFrame( ret_base, 1 );
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
