using System;
using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Extensions;

namespace IronBrew2.Obfuscator.Control_Flow.Types
{
	public static class TestSpam
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

		public static void DoInstructions(Chunk chunk, List<Instruction> Instructions)
		{
			Instructions = Instructions.ToList();
			CFGenerator cg = new CFGenerator();
			Random r = new Random();
			
			for (int i =  Instructions.Count - 1; i >= 0; i--)
			{
				used.Clear();
				Instruction instr = Instructions[i];
				
				List<Instruction> newInstructions = new List<Instruction>();

				switch (instr.OpCode)
				{
					case Opcode.Eq:
					case Opcode.Lt:
					case Opcode.Le:
					{
						Instruction[] AddTVGroup(Instruction test)
						{
							Instruction cmp1, cmp2;
							{
								cmp1 = new Instruction(test);
								Instruction target = chunk.Instructions[chunk.InstructionMap[test] + 1];
								
								Instruction jmpCorrect = cg.NextJMP(chunk, (Instruction) target.RefOperands[0]);
								Instruction jmpJunk = cg.NextJMP(chunk, Instructions[r.Next(0, i - 2)]);

								target.RefOperands[0] = cmp1;
								chunk.Instructions.AddRange(new[] {cmp1, jmpCorrect, jmpJunk});
							}
							
							{
								cmp2 = new Instruction(test);
								
								Instruction target = chunk.Instructions[chunk.InstructionMap[test] + 2];

								Instruction jmpCorrect = cg.NextJMP(chunk, target);
								Instruction jmpJunk = cg.NextJMP(chunk, Instructions[r.Next(0, i - 2)]);
								Instruction jmpStart = cg.NextJMP(chunk, cmp2);

								chunk.Instructions.Insert(chunk.InstructionMap[target], jmpStart);
								chunk.Instructions.AddRange(new[] {cmp2, jmpJunk, jmpCorrect});
							}

							chunk.UpdateMappings();
							return new[] {cmp1, cmp2};
						}


						List<Instruction> tv1 = AddTVGroup(instr).ToList();
						for (int j = 0; j < 3; j++)
						{
							List<Instruction> tv2 = new List<Instruction>();
							foreach (Instruction ins in tv1)
								tv2.AddRange(AddTVGroup(ins));
							tv1 = tv2;
						}
						break;
					}

					case Opcode.Test:
					case Opcode.TestSet:
					{
						Instruction[] AddTVGroup(Instruction test)
						{
							Instruction test1, test2;
							{
								test1 = new Instruction(test);
								
								Instruction target = chunk.Instructions[chunk.InstructionMap[test] + 1];

								Instruction jmpCorrect = cg.NextJMP(chunk, (Instruction) target.RefOperands[0]);
								Instruction jmpJunk = cg.NextJMP(chunk, Instructions[r.Next(0, i - 2)]);
								
								target.RefOperands[0] = test1;
								chunk.Instructions.AddRange(new[] {test1, jmpCorrect, jmpJunk});
							}
							
							{
								test2 = new Instruction(test);

								Instruction target = chunk.Instructions[chunk.InstructionMap[test] + 2];

								Instruction jmpCorrect = cg.NextJMP(chunk, target);
								Instruction jmpJunk = cg.NextJMP(chunk, Instructions[r.Next(0, i - 2)]);
								Instruction jmpStart = cg.NextJMP(chunk, test2);

								chunk.Instructions.Insert(chunk.InstructionMap[target], jmpStart);
								chunk.Instructions.AddRange(new[] {test2, jmpJunk, jmpCorrect});
							}
							
							chunk.UpdateMappings();
							return new[] {test1, test2};
						}

						List<Instruction> tv1 = AddTVGroup(instr).ToList();
						for (int j = 0; j < 3; j++)
						{
							int x = tv1.Count;
							for (var index = 0; index < x; index++)
							{
								Instruction ins = tv1[index];
								tv1.AddRange(AddTVGroup(ins));
							}
						}
						break;
					}
				}
			}
			chunk.UpdateMappings();
		}
	}
}