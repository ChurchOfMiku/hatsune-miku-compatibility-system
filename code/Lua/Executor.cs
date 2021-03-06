#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sandbox;
using System.Runtime.CompilerServices;

using Miku.Lua.Objects;

namespace Miku.Lua
{
	class SilentExecException : Exception
	{
		public SilentExecException(Exception inner) : base(inner.Message, inner)
		{
		}
	}

	partial class Executor
	{
		struct FrameInfo
		{
			public Function Func;
			public int PC;
			public int RetBase;
			public int RetCount;
			public int FrameBase;
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
		private const int STACK_MARGIN = 260;

		private int PC = 0; // signed because it can be set to -1 at the start of a call
		private int MultiRes = 0;
		private Function Func = null!; // our current function

		/// <summary>
		/// Stack of values.
		/// ALL assignments to the value stack need to use StackSet. Otherwise we risk breaking the stack frame.
		/// </summary>
		private List<ValueSlot> ValueStack = new List<ValueSlot>();

		/// <summary>
		/// Last entry in the stack + 1.
		/// </summary>
		private int FrameTop = 0;

		/// <summary>
		/// Zeroth entry in the stack.
		/// </summary>
		private int FrameBase = 0;

		private Stack<FrameInfo> CallStack = new Stack<FrameInfo>();
		private SortedDictionary<int, UpValueBox> OpenUpValues = new SortedDictionary<int, UpValueBox>();

		/// <summary>
		/// Not to be confused with MultiRes, stores the arguments from `...`.
		/// </summary>
		private ValueSlot[]? VarArgs;

		/// <summary>
		///  Used for results of an execution.
		/// </summary>
		private int ResultCount = -1;

		public int GetResultCount()
		{
			return ResultCount;
		}

		public ValueSlot GetResult(int i)
		{
			if (i<0 || i>= ResultCount )
			{
				return ValueSlot.NIL;
			}
			return ValueStack[i];
		}

		/// <summary>
		/// Used for UserFunction arguments.
		/// </summary>
		private int ArgCount = -1;

		public int GetArgCount()
		{
			return ArgCount;
		}

		public ValueSlot GetArg(int i)
		{
			if ( i < 0 || i >= ArgCount )
			{
				return ValueSlot.NIL;
			}
			return StackGet( i );
		}

		private int RetCount = -1;
		public void Return(ValueSlot val)
		{
			StackSet( ArgCount + RetCount, val );
			RetCount++;
		}

		public readonly LuaMachine Machine;

		public Executor( Function func, ValueSlot[] args, LuaMachine machine )
		{
			Machine = machine;

			CallPrepare( func, 0, 0 );
			for (int i=0;i<args.Length;i++ )
			{
				StackSet(i, args[i]);
			}
			CallArgsReady( args.Length );

			// Need to force PC to zero since it defaults to -1.
			PC = 0;
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

		/// <summary>
		/// Get the function at the bottom of the call stack
		/// </summary>
		/// <returns></returns>
		public Function GetBaseFunction()
		{
			if (CallStack.Count > 0)
			{
				var entry = CallStack.Reverse().First();
				return entry.Func;
			} else
			{
				return Func;
			}
		}

		// TODO consider pulling function from stack, might simplify calls
		// NOTE ret_base is now a LOCAL stack offset
		private void CallPrepare( ValueSlot call_value, int ret_base = 0, int ret_count = 0, bool replace = false )
		{
			// Check function HERE to avoid messing up the stack in the case that it fails.
			var new_func = call_value.CheckFunction();

			// Push old function + PC to stack.
			if ( Func != null && !replace )
			{
				CallStack.Push( new FrameInfo
				{
					Func = Func,
					PC = PC,
					VarArgs = VarArgs,
					FrameBase = FrameBase,
					RetBase = ret_base,
					RetCount = ret_count
				} );
			}

			Func = new_func;
			PC = -1;
			VarArgs = null;

			if (!replace)
			{
				FrameBase = FrameTop;
			}

			// Make sure the stack has plenty of slots.
			while ( ValueStack.Count < FrameTop + STACK_MARGIN )
			{
				ValueStack.Add( ValueSlot.NIL );
			}
		}

		/// <summary>
		/// This ensures unprovided args slots are clear, and stores extra args in VarArgs if applicable.
		/// </summary>
		private void CallArgsReady( int args_in )
		{
			// If a UserFunc, we handle the invocation here.
			if (Func.Prototype.UserFunc != null)
			{
				Profiler.UpdateUserFunc( Func.Prototype.DebugName );

				int old_arg_count = ArgCount;
				int old_ret_count = RetCount;
				ArgCount = args_in;
				RetCount = 0;

				var ret_val = Func.Prototype.UserFunc( this );
				if (ret_val != null)
				{
					Return( ret_val.Value );
				}

				int ret_base = ArgCount;
				int ret_count = RetCount;

				ArgCount = old_arg_count;
				RetCount = old_ret_count;

				ReturnInternal( ret_base, ret_count );
				return;
			}

			if ( args_in < Func.Prototype.NumArgs )
			{
				// we need to clear arguments we're not using
				for ( int i = args_in; i < Func.Prototype.NumArgs; i++ )
				{
					StackSet( i, ValueSlot.NIL );
				}
			}

			if ( Func.Prototype.IsVarArg() )
			{
				if ( args_in > Func.Prototype.NumArgs )
				{
					int varg_base = Func.Prototype.NumArgs;
					int varg_count = args_in - Func.Prototype.NumArgs;

					VarArgs = new ValueSlot[varg_count];
					for ( int i = 0; i < varg_count; i++ )
					{
						VarArgs[i] = StackGet( varg_base + i );
					}
				}
				else
				{
					VarArgs = new ValueSlot[0];
				}
			}
		}

		// NOTE ret_source_base is now a LOCAL stack offset
		private void ReturnInternal(int ret_source_base, int ret_slots_available)
		{
			// TODO clear stack slots that are no-longer used, to prevent leaking references.
			if (CallStack.Count == 0)
			{
				ResultCount = ret_slots_available;
				for ( int i = 0; i < ret_slots_available; i++ )
				{
					ValueStack[i] = StackGet(ret_source_base + i);
				}
			} else
			{
				// Need to get this stack offset before messing with the exec state.
				int real_source_base = GetRealStackIndex( ret_source_base );

				var frame_info = CallStack.Pop();
				int ret_dest_base = frame_info.RetBase;
				int ret_slots_to_fill = frame_info.RetCount;
				if ( ret_slots_to_fill == -1 )
				{
					ret_slots_to_fill = ret_slots_available;
					MultiRes = ret_slots_available;
				}

				Func = frame_info.Func;
				PC = frame_info.PC;
				VarArgs = frame_info.VarArgs;
				FrameTop = FrameBase;
				FrameBase = frame_info.FrameBase;

				for ( int i = 0; i < ret_slots_to_fill; i++ )
				{
					if ( i < ret_slots_available )
					{
						StackSet( ret_dest_base + i, ValueStack[real_source_base + i] );
					}
					else
					{
						StackSet( ret_dest_base + i, ValueSlot.NIL );
					}
				}
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private int GetRealStackIndex( int index )
		{
			return FrameBase + index;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void StackSet(int index, ValueSlot x)
		{
			int real_index = GetRealStackIndex( index );
			if ( real_index >= FrameTop)
			{
				FrameTop = real_index + 1;
			}
			ValueStack[real_index] = x;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private ValueSlot StackGet(int index)
		{
			return ValueStack[GetRealStackIndex( index )];
		}

		public void Run()
		{
			int safety = 0;
			int LIMIT = 10_000_000;

			try
			{
				while (ResultCount == -1)
				{
					// This was formerly its own method, but due to .NET's *VERY GOOD* compiler tech,
					// we can save a substantial amount of time by inlining it manually.
					// MethodImplOptions.AggressiveInlining did not help.
					#region Stepper
					{
						uint instr = Func.Prototype.Code[PC];
						var OP = (OpCode)(instr & 0xFF);

						Profiler.Update( OP, Func.Prototype.DebugName );

						int A = (int)((instr >> 8) & 0xFF);
						int B = (int)((instr >> 24) & 0xFF);
						int C = (int)((instr >> 16) & 0xFF);
						int D = (int)((instr >> 16) & 0xFFFF);

						switch ( OP )
						{
							#region Comparisons
							case OpCode.ISLT:
								{
									var lhs = StackGet( A ).CheckNumber();
									var rhs = StackGet( D ).CheckNumber();
									if ( !(lhs < rhs) )
										PC++;
									break;
								}

							case OpCode.ISGE:
								{
									var lhs = StackGet( A ).CheckNumber();
									var rhs = StackGet( D ).CheckNumber();
									if ( !(lhs >= rhs) )
										PC++;
									break;
								}

							case OpCode.ISLE:
								{
									var lhs = StackGet( A ).CheckNumber();
									var rhs = StackGet( D ).CheckNumber();
									if ( !(lhs <= rhs) )
										PC++;
									break;
								}

							case OpCode.ISGT:
								{
									double lhs = StackGet( A ).CheckNumber();
									double rhs = StackGet( D ).CheckNumber();
									if ( !(lhs > rhs) )
										PC++;
									break;
								}

							case OpCode.ISEQV:
								{
									var vA = StackGet( A );
									var vD = StackGet( D );
									if ( !vA.Equals( vD ) )
										PC++;
									break;
								}

							case OpCode.ISNEV:
								{
									var vA = StackGet( A );
									var vD = StackGet( D );
									if ( vA.Equals( vD ) )
										PC++;
									break;
								}

							case OpCode.ISEQS:
								{
									var vA = StackGet( A );
									var vD = Func.Prototype.GetConstGC( D );
									if ( !vA.Equals( vD ) )
										PC++;
									break;
								}

							case OpCode.ISNES:
								{
									var vA = StackGet( A );
									var vD = Func.Prototype.GetConstGC( D );
									if ( vA.Equals( vD ) )
										PC++;
									break;
								}

							case OpCode.ISEQN:
								{
									ValueSlot lhs = StackGet( A );
									ValueSlot rhs = Func.Prototype.GetConstNum( D );
									if ( !lhs.Equals( rhs ) )
										PC++;
									break;
								}

							case OpCode.ISNEN:
								{
									ValueSlot lhs = StackGet( A );
									ValueSlot rhs = Func.Prototype.GetConstNum( D );
									if ( lhs.Equals( rhs ) )
										PC++;
									break;
								}

							case OpCode.ISEQP:
								{
									var lhs = StackGet( A );
									var rhs = ValueSlot.Prim( D );
									if ( !lhs.Equals( rhs ) )
										PC++;
									break;
								}

							case OpCode.ISNEP:
								{
									var lhs = StackGet( A );
									var rhs = ValueSlot.Prim( D );
									if ( lhs.Equals( rhs ) )
										PC++;
									break;
								}
							#endregion

							#region Test, Test and Copy
							case OpCode.ISTC:
								{
									var val = StackGet( D );
									if ( val.IsTruthy() )
										StackSet( A, val );
									else
										PC++;
									break;
								}

							case OpCode.ISFC:
								{
									var val = StackGet( D );
									if ( !val.IsTruthy() )
										StackSet( A, val );
									else
										PC++;
									break;
								}

							case OpCode.IST:
								if ( !StackGet( D ).IsTruthy() )
									PC++;
								break;

							case OpCode.ISF:
								if ( StackGet( D ).IsTruthy() )
									PC++;
								break;

							// ISEQN
							// ISNEN
							#endregion

							#region Move and Unary Ops
							case OpCode.MOV:
								StackSet( A, StackGet( D ) );
								break;

							case OpCode.NOT:
								{
									bool result = !StackGet( D ).IsTruthy();
									StackSet( A, result );
									break;
								}

							case OpCode.UNM:
								{
									double num = StackGet( D ).CheckNumber();
									StackSet( A, -num );
									break;
								}

							case OpCode.LEN:
								{
									StackSet( A, ValueOperations.Len( StackGet( D ) ) );
									break;
								}
							#endregion

							// Binary Ops
							#region Arithmetic - *NV
							case OpCode.ADDVN:
								ValueOperations.Add( this, A, StackGet( B ), Func.Prototype.GetConstNum( C ) );
								break;
							case OpCode.SUBVN:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = Func.Prototype.GetConstNum( C );
									StackSet( A, nB - nC );
									break;
								}

							case OpCode.MULVN:
								ValueOperations.Mul( this, A, StackGet( B ), Func.Prototype.GetConstNum( C ) );
								break;
							case OpCode.DIVVN:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = Func.Prototype.GetConstNum( C );
									StackSet( A, nB / nC );
									break;
								}

							case OpCode.MODVN:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = Func.Prototype.GetConstNum( C );
									StackSet( A, nB % nC );
									break;
								}
							#endregion Arithmetic - *NV

							#region Arithmetic - *NV
							case OpCode.ADDNV:
								ValueOperations.Add( this, A, Func.Prototype.GetConstNum( C ), StackGet( B ) );
								break;
							case OpCode.SUBNV:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = Func.Prototype.GetConstNum( C );
									StackSet( A, nC - nB );
									break;
								}

							case OpCode.MULNV:
								ValueOperations.Mul( this, A, Func.Prototype.GetConstNum( C ), StackGet( B ) );
								break;
							case OpCode.DIVNV:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = Func.Prototype.GetConstNum( C );
									StackSet( A, nC / nB );
									break;
								}

							case OpCode.MODNV:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = Func.Prototype.GetConstNum( C );
									StackSet( A, nC % nB );
									break;
								}
							#endregion Arithmetic - *NV

							#region Arithmetic - *VV
							case OpCode.ADDVV:
								ValueOperations.Add( this, A, StackGet( B ), StackGet( C ) );
								break;
							case OpCode.SUBVV:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = StackGet( C ).CheckNumber();
									StackSet( A, nB - nC );
									break;
								}

							case OpCode.MULVV:
								ValueOperations.Mul( this, A, StackGet( B ), StackGet( C ) );
								break;
							case OpCode.DIVVV:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = StackGet( C ).CheckNumber();
									StackSet( A, nB / nC );
									break;
								}

							case OpCode.MODVV:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = StackGet( C ).CheckNumber();
									StackSet( A, nB % nC );
									break;
								}

							case OpCode.POW:
								{
									double nB = StackGet( B ).CheckNumber();
									double nC = StackGet( C ).CheckNumber();
									StackSet( A, Math.Pow( nB, nC ) );
									break;
								}
							#endregion Arithmetic - *VV

							case OpCode.CAT:
								{
									var builder = new StringBuilder();
									for ( int i = B; i <= C; i++ )
									{
										// incompatible: glua fails to concat nil, possibly others for whatever reason
										builder.Append( StackGet( i ).ToString() );
									}
									StackSet( A, builder.ToString() );
									break;
								}

							#region Constants
							case OpCode.KSTR:
								{
									var str = Func.Prototype.GetConstGC( D );
									StackSet( A, str );
									break;
								}

							// KCDATA: don't care

							case OpCode.KSHORT:
								{
									// TODO check!
									StackSet( A, (short)D );
									break;
								}

							case OpCode.KNUM:
								{
									var num = Func.Prototype.GetConstNum( D );
									StackSet( A, num );
									break;
								}

							case OpCode.KPRI:
								{
									StackSet( A, ValueSlot.Prim( D ) );
									break;
								}

							case OpCode.KNIL:
								{
									for ( int i = A; i <= D; i++ )
									{
										StackSet( i, ValueSlot.NIL );
									}
									break;
								}
							#endregion

							#region Upvalues and Function Init
							case OpCode.UGET:
								{
									var val = Func.UpValues[D].Get();
									StackSet( A, val );
									break;
								}
							case OpCode.USETV:
								Func.UpValues[A].Set( StackGet( D ) );
								break;
							case OpCode.USETN:
								Func.UpValues[A].Set( Func.Prototype.GetConstNum( D ) );
								break;
							case OpCode.USETP:
								Func.UpValues[A].Set( ValueSlot.Prim( D ) );
								break;
							case OpCode.UCLO:
								{
									int upval_close_base = GetRealStackIndex( A );
									for ( int i = upval_close_base; i < FrameTop; i++ )
									{
										UpValueBox uv;
										if ( OpenUpValues.TryGetValue( i, out uv! ) )
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
									var new_proto = Func.Prototype.GetConstGC( D ).CheckProtoFunction();
									var upvals = new UpValueBox[new_proto.UpValues.Length];
									for ( int i = 0; i < upvals.Length; i++ )
									{
										var uv_code = new_proto.UpValues[i];
										if ( (uv_code & 0x8000) == 0 )
										{
											var uv_slot = uv_code & 0x3FFF;
											var uv_box = Func.UpValues[uv_slot];
											Assert.NotNull( uv_box );
											upvals[i] = uv_box;
										}
										else
										{
											var uv_slot = uv_code & 0x3FFF;
											int real_stack_index = GetRealStackIndex( uv_slot );
											UpValueBox uv_box;
											if ( !OpenUpValues.TryGetValue( real_stack_index, out uv_box! ) )
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
									var new_func = new Function( new_proto, Func.Env, upvals );
									StackSet( A, new_func );
									break;
								}
							#endregion

							#region Table Ops
							case OpCode.TNEW:
								{
									var table = new Table();
									StackSet( A, table );
									break;
								}
							case OpCode.TDUP:
								{
									var table_proto = Func.Prototype.GetConstGC( D ).CheckTable();
									var table = table_proto.CloneProto();
									StackSet( A, table );
									break;
								}
							case OpCode.GGET:
								{
									var str = Func.Prototype.GetConstGC( D );
									ValueOperations.Get( this, A, Func.Env, str );
									break;
								}
							case OpCode.GSET:
								{
									var str = Func.Prototype.GetConstGC( D );
									ValueOperations.Set( this, Func.Env, str, StackGet( A ) );
									break;
								}
							case OpCode.TGETV:
								ValueOperations.Get( this, A, StackGet( B ), StackGet( C ) );
								break;
							case OpCode.TGETS:
								{
									var str = Func.Prototype.GetConstGC( C );
									ValueOperations.Get( this, A, StackGet( B ), str );
									break;
								}
							case OpCode.TGETB:
								ValueOperations.Get( this, A, StackGet( B ), C );
								break;
							case OpCode.TSETV:
								{
									ValueOperations.Set( this, StackGet( B ), StackGet( C ), StackGet( A ) );
									break;
								}
							case OpCode.TSETS:
								{
									var str = Func.Prototype.GetConstGC( C );
									ValueOperations.Set( this, StackGet( B ), str, StackGet( A ) );
									break;
								}
							case OpCode.TSETB:
								{
									ValueOperations.Set( this, StackGet( B ), C, StackGet( A ) );
									break;
								}
							case OpCode.TSETM:
								{
									var table = StackGet( A - 1 );
									var num = Func.Prototype.GetConstNum( D );
									var start_index = (int)BitConverter.DoubleToInt64Bits( num );
									for ( int i = 0; i < MultiRes; i++ )
									{
										ValueOperations.Set( this, table, start_index + i, StackGet( A + i ) );
									}
									break;
								}
							#endregion

							#region Calls, Iterators, and VARG
							case OpCode.CALL:
							case OpCode.CALLM:
							case OpCode.CALLT:
							case OpCode.CALLMT:
							case OpCode.ITERC:
								{
									bool is_tailcall = (OP == OpCode.CALLT || OP == OpCode.CALLMT);
									bool is_multires = (OP == OpCode.CALLM || OP == OpCode.CALLMT);

									int call_base = GetRealStackIndex( A );

									int arg_base = call_base + 1;
									int arg_count = C - 1;
									if ( is_multires )
									{
										arg_count += 1 + MultiRes;
									}

									int ret_count = B - 1;

									if ( OP == OpCode.ITERC )
									{
										// A, A+1, A+2 = A-3, A-2, A-1;
										for ( int i = 0; i < 3; i++ )
										{
											StackSet( A + i, StackGet( A - 3 + i ) );
										}
									}

									var call_func = StackGet( A );

									CallPrepare( call_func, A, ret_count, is_tailcall );
									for ( int i = 0; i < arg_count; i++ )
									{
										var arg_val = ValueStack[arg_base + i];
										StackSet( i, arg_val );
									}
									CallArgsReady( arg_count );

									break;
								}
							case OpCode.VARG:
								{
									if ( VarArgs == null )
									{
										throw new Exception( "Attempt to use varargs in non-vararg function." );
									}

									// Same as call result handling:
									int ret_count = B - 1;
									if ( ret_count == -1 )
									{
										ret_count = VarArgs.Length;
										MultiRes = ret_count;
									}

									for ( int i = 0; i < ret_count; i++ )
									{
										if ( i < VarArgs.Length )
										{
											StackSet( A + i, VarArgs[i] );
										}
										else
										{
											StackSet( A + i, ValueSlot.NIL );
										}
									}
									break;
								}
							#endregion

							#region Returns
							case OpCode.RETM:
								ReturnInternal( A, D + MultiRes );
								break;
							case OpCode.RET:
								ReturnInternal( A, D - 1 );
								break;
							case OpCode.RET0:
								ReturnInternal( 0, 0 );
								break;
							case OpCode.RET1:
								ReturnInternal( A, 1 );
								break;
							#endregion

							#region Loops and Branches
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
									StackSet( A + 3, counter );

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
									StackSet( A + 3, counter );

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
									if ( iter_res.Kind != ValueKind.Nil )
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
							#endregion
							default:
								throw new Exception( $"> {OP}" );
						}
						PC++;
					}

					Profiler.Stop();
					#endregion

					safety++;
					if ( safety >= LIMIT )
					{
						//this.LogState();
						throw new Exception( "hit safety" );
					}
				}
			} catch (Exception e)
			{
				// TODO throw an exception that contains exectuor info, don't log anything here.
				if (!(e is SilentExecException))
				{
					Log.Error( "Lua Error: " + ErrorHelper.Check( Func, PC ) );
					Log.Error(StackTrace());
				}
				throw new SilentExecException(e);
			}
		}

		public string StackTrace()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine( "    at " + Func.Prototype.GetDebugLine(PC) );
			foreach ( var level in CallStack )
			{
				builder.AppendLine( "    at " + level.Func.Prototype.GetDebugLine(level.PC) );
			}
			return builder.ToString();
		}

		public void LogState()
		{
			Log.Info( "======= FRAME =======" );
			for ( int i = FrameBase; i < FrameTop; i++ )
			{
				Log.Info( $"> {i}: {ValueStack[i]}" );
			}
			Log.Info( "======= CODE =======" );
			for ( int i = 0; i < Func.Prototype.Code.Length; i++ )
			{
				uint instr = Func.Prototype.Code[i];
				var OP = (OpCode)(instr & 0xFF);
				var A = (instr >> 8) & 0xFF;
				var B = (instr >> 24) & 0xFF;
				var C = (instr >> 16) & 0xFF;
				var D = (instr >> 16) & 0xFFFF;
				string arrow = "";
				if ( PC == i )
				{
					arrow = "<--------------";
				}
				Log.Info( $"> {i}: {OP} {A} {B} {C} {D} {arrow}" );
			}
		}

		private void Jump( int D )
		{
			int jump_offset = D - 0x8000;
			PC += jump_offset;
		}

		public string? GetDirectory( int level )
		{
			Function? func;
			if ( level == -1 )
			{
				func = GetBaseFunction();
			}
			else
			{
				func = GetFunctionAtLevel( level );
			}
			if ( func == null )
			{
				return null;
			}
			var debug_name = func.Prototype.DebugName;
			if ( debug_name.StartsWith( "@lua/" ) )
			{
				var no_prefix = debug_name.Substring( 5 );
				var no_filename = no_prefix.Substring( 0, no_prefix.LastIndexOf( '/' ) + 1 );
				return no_filename;
			}
			return null;
		}
	}
}
