using System;
using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Extensions;

namespace IronBrew2.Obfuscator.Control_Flow.Types
{
	public static class TestPreserve
	{
		private static List<int> used = new List<int>();

		private static int NIntND(int min, int max)
		{
			var x = Enumerable.Range(min, max - min).ToList();
			x.RemoveAll(y => used.Contains(y));
			x.Shuffle();
			int n = x[0];
			used.Add(n);
			return n;
		}

		public static void DoInstructions(Chunk chunk, List<Instruction> instructions)
		{
			for (int idx = 0; idx < instructions.Count; idx++)
			{
				used.Clear();
				Instruction i = instructions[idx];
				switch (i.OpCode)
				{
					case Opcode.Lt:
					case Opcode.Le:
					case Opcode.Eq:
					{
						int mReg1 = 250;
						int mReg2 = 251;

						Instruction ma, mb;

						if (i.RefOperands[0] is Constant c1)
						{
							ma = new Instruction(chunk, Opcode.LoadConst, c1);
							ma.A = mReg1;
						}
						else
						{
							ma = new Instruction(chunk, Opcode.Move);
							ma.A = mReg1;
							ma.B = i.B;
						}

						if (i.RefOperands[1] is Constant c2)
						{
							mb = new Instruction(chunk, Opcode.LoadConst, c2);
							mb.A = mReg2;
						}
						else
						{
							mb = new Instruction(chunk, Opcode.Move);
							mb.A = mReg2;
							mb.B = i.C;
						}

						Instruction loadbool1 = new Instruction(chunk, Opcode.LoadBool);
						loadbool1.A = mReg1;
						loadbool1.B = 0;

						Instruction loadbool2 = new Instruction(chunk, Opcode.LoadBool);
						loadbool2.A = mReg2;
						loadbool2.B = 0;

						i.B = mReg1;
						i.C = mReg2;
						
						i.SetupRefs();

						chunk.Instructions.InsertRange(chunk.InstructionMap[i] + 2,new[]{new Instruction(loadbool1), new Instruction(loadbool2)}); //yed
						chunk.Instructions.InsertRange(chunk.InstructionMap[i], new[] {ma, mb});
						chunk.UpdateMappings();
						
						chunk.Instructions.InsertRange(chunk.InstructionMap[(Instruction)chunk.Instructions[chunk.InstructionMap[i] + 1].RefOperands[0]], new[]{loadbool1, loadbool2}); // 10/10
						chunk.Instructions[chunk.InstructionMap[i] + 1].RefOperands[0] = loadbool1;
						chunk.UpdateMappings();

						foreach (Instruction ins in i.BackReferences)
							ins.RefOperands[0] = ma;
							
						break;
					}

					case Opcode.Test:
					case Opcode.TestSet:
					{
						int rReg = NIntND(0, 128);
						int pReg = NIntND(257, 512);
						
						Instruction m1 = new Instruction(chunk, Opcode.Move);
						m1.A = pReg;
						m1.B = rReg;
						
						Instruction m2 = new Instruction(chunk, Opcode.Move);
						m2.A = rReg;
						m2.B = i.A;	
						
						Instruction lb = new Instruction(chunk, Opcode.LoadBool);
						lb.A = pReg;
						lb.B = 0;
						
						Instruction m3 = new Instruction(chunk, Opcode.Move);
						m3.A = rReg;
						m3.B = pReg;
						
						chunk.Instructions.InsertRange(chunk.InstructionMap[i] + 2,new[] {new Instruction(m3), new Instruction(lb) });
						chunk.Instructions.InsertRange(chunk.InstructionMap[i], new[] {m1, m2});
						chunk.UpdateMappings();
						
						chunk.Instructions.InsertRange(chunk.InstructionMap[(Instruction)chunk.Instructions[chunk.InstructionMap[i] + 1].RefOperands[0]], new[]{m3, lb}); // 10/10
						chunk.Instructions[chunk.InstructionMap[i] + 1].RefOperands[0] = m3;
						chunk.UpdateMappings();
						
						foreach (Instruction ins in i.BackReferences)
							ins.RefOperands[0] = m1;
						
						break;
					}
				}
			}
		}
	}
}