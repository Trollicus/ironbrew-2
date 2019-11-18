using System.Collections.Generic;
using IronBrew2.Bytecode_Library.Bytecode;

namespace IronBrew2.Bytecode_Library.IR
{
	public class Chunk
	{
		public string                       Name;
		public int                          Line;
		public int                          LastLine;
		public byte                         UpvalueCount;
		public byte                         ParameterCount;
		public byte                         VarargFlag;
		public byte                         StackSize;
		public int                          CurrentOffset  = 0;
		public int                          CurrentParamOffset = 0;
		public List<Instruction>            Instructions;
		public Dictionary<Instruction, int> InstructionMap = new Dictionary<Instruction, int>();
		public List<Constant>               Constants;
		public Dictionary<Constant, int>    ConstantMap = new Dictionary<Constant, int>();
		public List<Chunk>                  Functions;
		public Dictionary<Chunk, int>       FunctionMap = new Dictionary<Chunk, int>();
		public List<string>                 Upvalues;
		
		public void UpdateMappings()
		{
			InstructionMap.Clear();
			ConstantMap.Clear();
			FunctionMap.Clear();

			for (int i = 0; i < Instructions.Count; i++)
				InstructionMap.Add(Instructions[i], i);

			for (int i = 0; i < Constants.Count; i++)
				ConstantMap.Add(Constants[i], i);

			for (int i = 0; i < Functions.Count; i++)
				FunctionMap.Add(Functions[i], i);
		}

		public int Rebase(int offset, int paramOffset = 0)
		{
			offset      -= CurrentOffset;
			paramOffset -= CurrentParamOffset;

			CurrentOffset += offset;
			CurrentParamOffset += paramOffset;

			StackSize = (byte) (StackSize + offset);

			//thanks lua for not distinguishing parameters and regular stack values!
			var Params = ParameterCount - 1;
			for (var i = 0; i < Instructions.Count; i++)
			{
				var instr = Instructions[i];

				switch (instr.OpCode)
				{
					case Opcode.Move:
					case Opcode.LoadNil:
					case Opcode.Unm:
					case Opcode.Not:
					case Opcode.Len:
					case Opcode.TestSet:
					{
						if (instr.A > Params)
							instr.A += offset;
						else
							instr.A += paramOffset;

						if (instr.B > Params)
							instr.B += offset;
						else
							instr.B += paramOffset;
						break;
					}
					case Opcode.LoadConst:
					case Opcode.LoadBool:
					case Opcode.GetGlobal:
					case Opcode.SetGlobal:
					case Opcode.GetUpval:
					case Opcode.SetUpval:
					case Opcode.Call:
					case Opcode.TailCall:
					case Opcode.Return:
					case Opcode.VarArg:
					case Opcode.Test:
					case Opcode.ForPrep:
					case Opcode.ForLoop:
					case Opcode.TForLoop:
					case Opcode.NewTable:
					case Opcode.SetList:
					case Opcode.Close:
					{
						if (instr.A > Params)
							instr.A += offset;
						else
							instr.A += paramOffset;
						break;
					}
					case Opcode.GetTable:
					case Opcode.SetTable:
					{
						if (instr.A > Params)
							instr.A += offset;
						else
							instr.A += paramOffset;

						if (instr.B < 255)
						{
							if (instr.B > Params)
								instr.B += offset;
							else
								instr.B += paramOffset;
						}

						if (instr.C > Params)
							instr.C += offset;
						else
							instr.C += paramOffset;

						break;
					}
					case Opcode.Add:
					case Opcode.Sub:
					case Opcode.Mul:
					case Opcode.Div:
					case Opcode.Mod:
					case Opcode.Pow:
					{
						if (instr.A > Params)
							instr.A += offset;
						else
							instr.A += paramOffset;

						if (instr.B < 255)
						{
							if (instr.B > Params)
								instr.B += offset;
							else
								instr.B += paramOffset;
						}

						if (instr.C < 255)
						{
							if (instr.C > Params)
								instr.C += offset;
							else
								instr.C += paramOffset;
						}

						break;
					}
					case Opcode.Concat:
					{
						if (instr.A > Params)
							instr.A += offset;
						else
							instr.A += paramOffset;

						if (instr.B > Params)
							instr.B += offset;
						else
							instr.B += paramOffset;

						if (instr.C > Params)
							instr.C += offset;
						else
							instr.C += paramOffset;

						break;
					}
					case Opcode.Self:
					{
						if (instr.A > Params)
							instr.A += offset;
						else
							instr.A += paramOffset;

						if (instr.B > Params)
							instr.B += offset;
						else
							instr.B += paramOffset;

						if (instr.C < 255)
						{
							if (instr.C > Params)
								instr.C += offset;
							else
								instr.C += paramOffset;
						}

						break;
					}
					case Opcode.Eq:
					case Opcode.Lt:
					case Opcode.Le:
					{
						if (instr.B < 255)
						{
							if (instr.B > Params)
								instr.B += offset;
							else
								instr.B += paramOffset;
						}

						if (instr.C < 255)
						{
							if (instr.C > Params)
								instr.C += offset;
							else
								instr.C += paramOffset;
						}

						break;
					}
					case Opcode.Closure:
					{
						if (instr.A > Params)
							instr.A += offset;
						else
							instr.A += paramOffset;

						var nProto = Functions[instr.B];

						//fuck you lua
						for (var i2 = 0; i2 < nProto.UpvalueCount; i2++)
						{
							var cInst = Instructions[i + i2 + 1];

							if (cInst.OpCode != Opcode.Move)
								continue;

							if (cInst.B > Params)
								cInst.B += offset;
							else
								cInst.B += paramOffset;
						}

						i += nProto.UpvalueCount;
						break;
					}
				}
			}

			return ParameterCount;
		}
	}
}