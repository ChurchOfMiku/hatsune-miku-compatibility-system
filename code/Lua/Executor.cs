#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public bool Debug = false;

		struct FrameInfo
		{
			public Function Func;
			public int PC;
			public int RetBase;
			public int RetCount;
			public int FrameSize;
			public ValueSlot[]? VarArgs;
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

			public override string ToString()
			{
				return "upval "+StackSlot+" "+Value;
			}
		}

		// We allocate this much extra space on the stack.
		// multirets can take up more space than the frame
		private const int STACK_MARGIN = 100;

		private int pc = 0;
		private int MultiRes = 0;
		private int StackTop = 0; // last entry in the stack + 1
		private int FrameSize = 0;
		private Function Func;

		private List<ValueSlot> ValueStack = new List<ValueSlot>();
		private Stack<FrameInfo> CallStack = new Stack<FrameInfo>();
		private SortedDictionary<int, UpValueBox> OpenUpValues = new SortedDictionary<int, UpValueBox>();

		private ValueSlot[]? VarArgs;

		public ValueSlot[]? Results;

		public LuaMachine? Machine;

		public Executor( Function func, ValueSlot[] args )
		{
			AddFrame( func, 0, 0, args.Length );
			Func = func; // used to suppress null warning
			for (uint i=0;i<args.Length;i++ )
			{
				StackSet(i, args[i]);
			}
			StoreVarArgs( args.Length );
		}

		public Function? GetFunctionAtLevel(int level)
		{
			if (level <= 0)
			{
				return null;
			}
			if (level == 1)
			{
				return Func;
			}
			level--;
			foreach (var entry in CallStack )
			{
				level--;
				if (level == 0)
				{
					return entry.Func;
				}
			}
			return null;
		}

		private void AddFrame( Function new_func, int ret_base, int ret_count, int arg_count )
		{
			// Push old function + PC to stack.
			if (Func != null)
			{
				CallStack.Push( new FrameInfo{
					Func = Func,
					PC = pc,
					VarArgs = VarArgs,
					FrameSize = FrameSize,
					RetBase = ret_base,
					RetCount = ret_count
				} );
			}
			this.pc = 0;
			this.Func = new_func;

			// Grow value stack.
			FrameSize = Math.Max( Func.Prototype.numSlots , arg_count);
			StackTop += FrameSize;
			while ( ValueStack.Count < StackTop + STACK_MARGIN )
			{
				ValueStack.Add( ValueSlot.NIL );
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

				for ( int i = 0; i < ret_slots_to_fill; i++ )
				{
					if ( i < ret_slots_available )
					{
						ValueStack[ret_dest_base - i] = ValueStack[ret_source_base - i];
					}
					else
					{
						ValueStack[ret_dest_base - i] = ValueSlot.NIL;
					}
				}

				StackTop -= FrameSize;
				Func = frame_info.Func;
				pc = frame_info.PC;
				VarArgs = frame_info.VarArgs;
				FrameSize = frame_info.FrameSize;
			}
		}

		private void StoreVarArgs(int args_in)
		{
			if ( args_in < Func.Prototype.numArgs )
			{
				// we need to clear arguments we're not using
				for (uint i=(uint)args_in;i<Func.Prototype.numArgs;i++ )
				{
					StackSet( i, ValueSlot.NIL );
				}
			}

			if (Func.Prototype.IsVarArg())
			{
				if ( args_in > Func.Prototype.numArgs )
				{
					uint varg_base = (uint)Func.Prototype.numArgs;
					int varg_count = args_in - Func.Prototype.numArgs;
					VarArgs = new ValueSlot[varg_count];
					for (uint i=0;i<varg_count;i++ )
					{
						VarArgs[i] = StackGet(varg_base + i);
					}
				} else
				{
					VarArgs = new ValueSlot[0];
				}
			} else
			{
				VarArgs = null;
			}
		}

		private int GetRealStackIndex( uint index )
		{
			return (int)(StackTop - 1 - index);
		}

		private void StackSet(uint index, ValueSlot x)
		{
			if (index >= FrameSize)
			{
				throw new Exception( "stack oob" );
			}
			ValueStack[GetRealStackIndex(index)] = x;
		}

		private ValueSlot StackGet(uint index)
		{
			return ValueStack[GetRealStackIndex( index )];
		}

		public void Run()
		{
			int safety = 0;
			int LIMIT = 5_000_000;
			if (Debug)
			{
				//LIMIT = 1000;
			}
			while (pc < Func.Prototype.code.Length && Results == null)
			{
				try
				{
					Step();
				} catch (Exception e)
				{
					if (!(e is SilentExecException))
					{
						Log.Error( e.Message );
						Log.Error( e.StackTrace );
						LogStack();
						//LogState();
					}
					throw new SilentExecException(e);
				}
				safety++;
				if (safety >= LIMIT )
				{
					//this.LogState();
					throw new Exception( "hit safety" );
				}
			}
		}

		public void LogStack()
		{
			Log.Info( " at " + Func.Prototype.DebugName );
			foreach ( var level in CallStack )
			{
				Log.Info( " at " + level.Func.Prototype.DebugName );
			}
		}

		public void LogState()
		{
			Log.Info( "======= STACK =======" );
			int slot_i = 0;
			foreach (var level in CallStack.Reverse() )
			{
				var slot_count = level.FrameSize;
				for (int j = 0; j < slot_count; j++ )
				{
					Log.Info( $"({slot_i}) {slot_count - j - 1}: {ValueStack[slot_i]}" );
					slot_i++;
				}
				Log.Info( "^^^--- " + level.Func.Prototype.DebugName );
			}
			{
				var slot_count = FrameSize;
				for ( int j = 0; j < slot_count; j++ )
				{
					Log.Info( $"({slot_i}) {slot_count - j - 1}: {ValueStack[slot_i]}" );
					slot_i++;
				}
				Log.Info( "^^^--- " + Func.Prototype.DebugName );
			}
			Log.Info( "======= CODE =======" );
			for (int i=0;i<Func.Prototype.code.Length;i++ )
			{
				uint instr = Func.Prototype.code[i];
				var OP = (OpCode)(instr & 0xFF);
				var A = (instr >> 8) & 0xFF;
				var B = (instr >> 24) & 0xFF;
				var C = (instr >> 16) & 0xFF;
				var D = (instr >> 16) & 0xFFFF;
				if (pc == i)
				{
					Log.Info( "vvvvvvvvvvvvvvvvvvvvvvv" );
				}
				Log.Info( $"> {i}: {OP} {A} {B} {C} {D}" ); 
			}
		}

		public void Step()
		{
			uint instr = Func.Prototype.code[pc];
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
					{
						double nA = StackGet( A ).CheckNumber();
						double nD = StackGet( D ).CheckNumber();
						bool skip = false;
						switch (OP)
						{
							case OpCode.ISLT: skip = !(nA < nD); break;
							case OpCode.ISGE: skip = !(nA >= nD); break;
							case OpCode.ISLE: skip = !(nA <= nD); break;
							case OpCode.ISGT: skip = !(nA > nD); break;
						}
						if (skip) { pc++; }
						break;
					}
				case OpCode.ISEQV:
					{
						var vA = StackGet( A );
						var vD = StackGet( D );
						bool skip = !vA.Equals( vD );
						if ( skip ) { pc++; }
						break;
					}
				case OpCode.ISNEV:
					{
						var vA = StackGet( A );
						var vD = StackGet( D );
						bool skip = vA.Equals( vD );
						if ( skip ) { pc++; }
						break;
					}
				case OpCode.ISEQS:
					{
						var vA = StackGet( A );
						var vD = Func.Prototype.GetConstGC( D );
						bool skip = !vA.Equals(vD);
						if ( skip ) { pc++; }
						break;
					}
				case OpCode.ISNES:
					{
						var vA = StackGet( A );
						var vD = Func.Prototype.GetConstGC( D );
						bool skip = vA.Equals( vD );
						if ( skip ) { pc++; }
						break;
					}
				case OpCode.ISEQN:
					{
						var vA = StackGet( A );
						var vD = ValueSlot.Number( Func.Prototype.GetConstNum( D ) );
						bool skip = !vA.Equals( vD );
						if ( skip ) { pc++; }
						break;
					}
				case OpCode.ISNEN:
					{
						var vA = StackGet( A );
						var vD = ValueSlot.Number( Func.Prototype.GetConstNum( D ) );
						bool skip = vA.Equals( vD );
						if ( skip ) { pc++; }
						break;
					}
				case OpCode.ISEQP:
					{
						var vA = StackGet( A );
						var vD = ValueSlot.Prim( D );
						bool skip = !vA.Equals( vD );
						if ( skip ) { pc++; }
						break;
					}
				case OpCode.ISNEP:
					{
						var vA = StackGet( A );
						var vD = ValueSlot.Prim( D );
						bool skip = vA.Equals( vD );
						if ( skip ) { pc++; }
						break;
					}
				// Test and Copy
				case OpCode.ISTC:
					{
						var val = StackGet( D );
						if (val.IsTruthy())
						{
							StackSet( A, val );
						} else
						{
							pc++;
						}
						break;
					}
				case OpCode.ISFC:
					{
						var val = StackGet( D );
						if ( !val.IsTruthy() )
						{
							StackSet( A, val );
						}
						else
						{
							pc++;
						}
						break;
					}
				case OpCode.IST:
					if ( !StackGet( D ).IsTruthy() )
					{
						pc++;
					}
					break;
				case OpCode.ISF:
					if ( StackGet( D ).IsTruthy() )
					{
						pc++;
					}
					break;
				// ISEQN
				// ISNEN
				// Move and Unary Ops
				case OpCode.MOV:
					{
						StackSet( A, StackGet( D ) );
						break;
					}
				case OpCode.NOT:
					{
						bool result = !StackGet( D ).IsTruthy();
						StackSet( A, ValueSlot.Bool( result ) );
						break;
					}
				case OpCode.UNM:
					{
						double num = StackGet( D ).CheckNumber();
						StackSet( A, ValueSlot.Number(-num) );
						break;
					}
				case OpCode.LEN:
					{
						StackSet( A, ValueOperations.Len( StackGet( D ) ) );
						break;
					}
				// Binary Ops
				case OpCode.ADDVN:
				case OpCode.SUBVN:
				case OpCode.MULVN:
				case OpCode.DIVVN:
				case OpCode.MODVN:
					{
						double nB = StackGet( B ).CheckNumber();
						double nC = Func.Prototype.GetConstNum( C );
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
						double nB = StackGet( B ).CheckNumber();
						double nC = Func.Prototype.GetConstNum( C );
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
						double nB = StackGet( B ).CheckNumber();
						double nC = StackGet( C ).CheckNumber();
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
						var str = Func.Prototype.GetConstGC( D );
						StackSet( A,  str );
						break;
					}
				// KCDATA: don't care
				case OpCode.KSHORT:
					{
						var num = ((int)D << 16) >> 16; // TODO check!
						StackSet( A, ValueSlot.Number( num ) );
						break;
					}
				case OpCode.KNUM:
					{
						var num = Func.Prototype.GetConstNum( D );
						StackSet( A, ValueSlot.Number( num ) );
						break;
					}
				case OpCode.KPRI:
					{
						StackSet( A, ValueSlot.Prim(D) );
						break;
					}
				case OpCode.KNIL:
					{
						for (uint i=A;i<=D;i++)
						{
							StackSet( i, ValueSlot.NIL );
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
					Func.UpValues[A].Set(StackGet(D));
					break;
				case OpCode.USETN:
					Func.UpValues[A].Set( ValueSlot.Number(Func.Prototype.GetConstNum(D)) );
					break;
				case OpCode.UCLO:
					{
						int upval_close_base = GetRealStackIndex( A );
						int frame_top = GetRealStackIndex((uint)(FrameSize - 1));
						for (int i= upval_close_base; i>= frame_top; i--)
						{
							UpValueBox uv;
							if (OpenUpValues.TryGetValue( i, out uv! ))
							{
								uv.Value = ValueStack[i];
								OpenUpValues.Remove( i );
								uv.StackSlot = null;
							}
						}
						Jump( D );
						break;
					}
				case OpCode.FNEW:
					{
						var new_proto = Func.Prototype.GetConstGC(D).CheckProtoFunction();
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
								if (!OpenUpValues.TryGetValue( real_stack_index, out uv_box!) )
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
						var new_func = new Function( new_proto, Func.Env, Func.PrimitiveMT, upvals );
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
						var table_proto = Func.Prototype.GetConstGC( D ).CheckTable();
						var table = table_proto.CloneProto();
						StackSet( A, ValueSlot.Table( table ) );
						break;
					}
				case OpCode.GGET:
					{
						var str = Func.Prototype.GetConstGC( D );
						StackSet( A, ValueOperations.Get( ValueSlot.Table(Func.Env), str, Func.PrimitiveMT ) );
						break;
					}
				case OpCode.GSET:
					{
						var str = Func.Prototype.GetConstGC( D );
						Func.Env.Set(str, StackGet(A));
						break;
					}
				case OpCode.TGETV:
					{
						StackSet( A, ValueOperations.Get( StackGet( B ), StackGet( C ), Func.PrimitiveMT ) );
						break;
					}
				case OpCode.TGETS:
					{
						var str = Func.Prototype.GetConstGC( C );
						StackSet( A, ValueOperations.Get( StackGet( B ), str, Func.PrimitiveMT ) );
						break;
					}
				case OpCode.TGETB:
					{
						StackSet( A, ValueOperations.Get( StackGet( B ), ValueSlot.Number(C), Func.PrimitiveMT ) );
						break;
					}
				case OpCode.TSETV:
					{
						var table = StackGet( B ).CheckTable();
						table.Set( StackGet(C), StackGet( A ) );
						break;
					}
				case OpCode.TSETS:
					{
						var table = StackGet( B ).CheckTable();
						var str = Func.Prototype.GetConstGC( C );
						table.Set( str, StackGet( A ) );
						break;
					}
				case OpCode.TSETB:
					{
						var table = StackGet( B ).CheckTable();
						table.Set( (int)C, StackGet( A ) );
						break;
					}
				case OpCode.TSETM:
					{
						var table = StackGet( A - 1 ).CheckTable();
						var num = Func.Prototype.GetConstNum( D );
						var start_index = (int)BitConverter.DoubleToInt64Bits( num );
						for (int i=0;i<MultiRes;i++ )
						{
							table.Set( start_index + i, StackGet(A + (uint)i) );
						}
						break;
					}
				// Calls and Iterators
				case OpCode.CALL:
				case OpCode.CALLM:
				case OpCode.CALLT:
				case OpCode.CALLMT:
				case OpCode.ITERC:
					{
						bool is_tailcall = (OP == OpCode.CALLT || OP == OpCode.CALLMT);
						bool is_multires = (OP == OpCode.CALLM || OP == OpCode.CALLMT);

						int call_base = GetRealStackIndex(A);
						
						int arg_base = call_base - 1;
						int arg_count = (int)C - 1;
						if (is_multires)
						{
							arg_count += 1 + MultiRes;
						}

						int ret_base = call_base;
						int ret_count = (int)B - 1;

						if (OP == OpCode.ITERC)
						{
							// A, A+1, A+2 = A-3, A-2, A-1;
							for (uint i=0;i<3;i++ )
							{
								StackSet( A + i, StackGet( A - 3 + i ) );
							}
						}

						var call_func = ValueStack[call_base]; // TODO, meta calls
						if (call_func.Kind == ValueKind.Function)
						{
							if ( is_tailcall )
							{
								// Save arguments.
								var args = new ValueSlot[arg_count];
								for ( int i = 0; i < arg_count; i++ )
								{
									args[i] = ValueStack[arg_base - i];
								}

								// Clear current function and reset stack.
								StackTop -= FrameSize;
								Func = null!;

								AddFrame( call_func.CheckFunction(), 0, 0, arg_count );

								for ( int i = 0; i < arg_count; i++ )
								{
									StackSet( (uint)i, args[i] );
								}

								StoreVarArgs( arg_count );
							} else
							{
								AddFrame( call_func.CheckFunction(), ret_base, ret_count, arg_count );

								for (int i=0;i<arg_count;i++ )
								{
									var arg_val = ValueStack[arg_base - i];
									StackSet( (uint)i, arg_val );
								}

								StoreVarArgs( arg_count );
							}

							return; // DO NOT INCREMENT PC
						} else
						{
							var user_func = call_func.CheckUserFunction();
							var args = new ValueSlot[arg_count];

							for ( int i = 0; i < arg_count; i++ )
							{
								args[i] = ValueStack[arg_base - i];
							}
							var rets = user_func( args, this );

							// tail calls return everything
							if ( is_tailcall )
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
									ValueStack[ret_base - i] = ValueSlot.NIL;
								}
							}

							if (is_tailcall)
							{
								PopFrame( ret_base, ret_count );
							}
							break;
						}
					}
				case OpCode.VARG:
					{
						if (VarArgs == null)
						{
							throw new Exception( "Attempt to use varargs in non-vararg function." );
						}

						// Same as call result handling:
						int ret_base = GetRealStackIndex( A );
						int ret_count = (int)B - 1;
						if ( ret_count == -1 )
						{
							ret_count = VarArgs.Length;
							MultiRes = ret_count;
						}

						for ( int i = 0; i < ret_count; i++ )
						{
							if ( i < VarArgs.Length )
							{
								ValueStack[ret_base - i] = VarArgs[i];
							}
							else
							{
								ValueStack[ret_base - i] = ValueSlot.NIL;
							}
						}
						break;
					}
				// Returns
				case OpCode.RETM:
					{
						int ret_base = GetRealStackIndex( A );
						int ret_count = (int)D + MultiRes;
						PopFrame(ret_base, ret_count);
						break;
					}
				case OpCode.RET:
					{
						int ret_base = GetRealStackIndex( A );
						int ret_count = (int)D - 1;
						PopFrame( ret_base, ret_count );
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
						double stop = StackGet( A + 1 ).CheckNumber();
						double step = StackGet( A + 2 ).CheckNumber();

						// for loop init
						double counter = StackGet( A ).CheckNumber();
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
						double stop = StackGet( A + 1 ).CheckNumber();
						double step = StackGet( A + 2 ).CheckNumber();

						// for loop step
						double counter = StackGet( A + 3 ).CheckNumber();
						counter += step;
						StackSet( A + 3, ValueSlot.Number( counter ) );

						if ( !((step > 0 && counter > stop) || (step < 0 && counter < stop)) )
						{
							// condition passed, loop to top
							Jump( D );
						}
					}
					break;
				case OpCode.ITERL:
					{
						var iter_res = StackGet( A );
						if (iter_res.Kind != ValueKind.Nil)
						{
							StackSet( A - 1, iter_res );
							Jump( D );
						}
						break;
					}
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

		public string? GetDirectory()
		{
			var debug_name = Func.Prototype.DebugName;
			if (debug_name.StartsWith("@lua/"))
			{
				var no_prefix = debug_name.Substring( 5 );
				var no_filename = no_prefix.Substring(0, no_prefix.LastIndexOf( '/' )+1);
				return no_filename;
			}
			return null;
		}
	}
}
