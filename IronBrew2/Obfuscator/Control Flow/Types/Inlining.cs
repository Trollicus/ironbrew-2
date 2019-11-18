using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Control_Flow.Types
{
	public class Inlining
	{
		private Chunk _head;

		public Inlining(Chunk chunk) =>
			_head = chunk;

		public bool ShouldInline(Chunk target, Chunk inlined, out List<Instruction> calls, out List<Instruction> closures, out bool inlineAll)
		{
			calls = new List<Instruction>();
			closures = new List<Instruction>();
			inlineAll = false;
			
			if (inlined.Instructions.Count < 3)
				return false;

			if (inlined.Instructions[0].OpCode != Opcode.GetGlobal || inlined.Instructions[1].OpCode != Opcode.LoadBool || inlined.Instructions[2].OpCode != Opcode.Call)
				return false;
			
			if (((Constant) inlined.Instructions[0].RefOperands[0]).Data.ToString() != "IB_INLINING_START")
				return false;

			inlineAll = inlined.Instructions[1].B == 1;
			
			inlined.Instructions[0].OpCode = Opcode.Move;
			inlined.Instructions[0].A = 0;
			inlined.Instructions[0].B = 0;
			
			inlined.Instructions[1].OpCode = Opcode.Move;
			inlined.Instructions[1].A = 0;
			inlined.Instructions[1].B = 0;
			
			inlined.Instructions[2].OpCode = Opcode.Move;
			inlined.Instructions[2].A = 0;
			inlined.Instructions[2].B = 0;

			inlined.Constants.Remove((Constant) inlined.Instructions[0].RefOperands[0]);
			
			if (target.StackSize + inlined.StackSize + 1 > 255)
				return false;

			if (inlined.Instructions.Any(i => i.OpCode == Opcode.GetUpval || i.OpCode == Opcode.SetUpval))
				return false;
			
			bool[] registers = new bool[256];
			bool res = false;
			
			for (var i = 0; i < target.Instructions.Count; i++)
			{
				var instr = target.Instructions[i];
				switch (instr.OpCode)
				{
					case Opcode.Move:
					{
						registers[instr.A] = registers[instr.B];
						break;
					}
					case Opcode.LoadNil:
					case Opcode.Unm:
					case Opcode.Not:
					case Opcode.Len:
					case Opcode.TestSet:
					{
						registers[instr.A] = false;
						registers[instr.B] = false;
						break;
					}					
					case Opcode.LoadConst:
					case Opcode.LoadBool:
					case Opcode.GetGlobal:
					case Opcode.SetGlobal:
					case Opcode.Return:
					case Opcode.VarArg:
					case Opcode.Test:
					case Opcode.ForPrep:
					case Opcode.ForLoop:
					case Opcode.TForLoop:
					case Opcode.NewTable:
					case Opcode.SetList:
					case Opcode.Close:
					case Opcode.GetTable:
					case Opcode.SetTable:
					case Opcode.Add:
					case Opcode.Sub:
					case Opcode.Mul:
					case Opcode.Div:
					case Opcode.Mod:
					case Opcode.Pow:
					case Opcode.Concat:
					case Opcode.Self:
					{
						registers[instr.A] = false;
						break;
					}
					case Opcode.Closure:
						if (instr.RefOperands[0] == inlined)
						{
							closures.Add(instr);
							registers[instr.A] = true;
						}

						break;
					case Opcode.Call:
					case Opcode.TailCall:
						int limit = instr.A + instr.C - 1;
						
						if (instr.C == 0)
							limit = target.StackSize;
							
						if (registers[instr.A])
						{
							calls.Add(instr);
							res = true;	
						}

						for (int c = instr.A; c <= limit; c++)
							registers[c] = false;
						
						break;
				}
			}

			return res;
		}

		public void DoChunk(Chunk chunk)
		{
			foreach (Chunk sub in chunk.Functions.ToList())
			{	
				DoChunk(sub);
				if (ShouldInline(chunk, sub, out var locations, out var closures, out bool inlineAll))
				{
					if (inlineAll)
						chunk.Functions.Remove(sub);
					
					foreach (var loc in locations)
					{
						int target = loc.A + loc.B + 1;
						if (loc.B == 0)
							target = chunk.StackSize + 1;

						sub.Rebase(target, target);
						
						List<Instruction> modified = new List<Instruction>();

						int idx = chunk.Instructions.IndexOf(loc);
						chunk.Instructions.Remove(loc);
						
						foreach (var bRef in loc.BackReferences)
							bRef.SetupRefs();
						
						chunk.UpdateMappings();
						
						Instruction next = chunk.Instructions[idx];

						int lim = sub.ParameterCount - 1;
						if (loc.B == 0)
							lim = chunk.StackSize - loc.A;
						
						for (int i = 0; i <= lim; i++)
						{
							chunk.Instructions.Insert(idx++, new Instruction(chunk, Opcode.Move)
							{
								A = target + i,
								B = loc.A   + i + 1
							});
						}
						
						Dictionary<Instruction, Instruction> map = new Dictionary<Instruction, Instruction>();

						bool done = false;
						for (var i = 0; i < sub.Instructions.Count; i++)
						{
							var instr = new Instruction(sub.Instructions[i]);
							instr.Chunk = chunk;

							map.Add(sub.Instructions[i], instr);
							switch (instr.OpCode)
							{
								case Opcode.Return:
								{
									int callLimit = loc.C - 1;

									if (callLimit == -1)
										callLimit = instr.B - 2;

									if (callLimit <= -1)
										callLimit = sub.StackSize;

									List<Instruction> t = new List<Instruction>();

									for (int j = 0; j <= callLimit; j++)
										t.Add(new Instruction(chunk, Opcode.Move)
										      {
											      A = loc.A   + j,
											      B = instr.A + j,
										      });

									Instruction setTop = new Instruction(chunk, Opcode.SetTop);
									setTop.A = loc.A + callLimit;
									
									t.Add(setTop);
									t.Add(new Instruction(chunk, Opcode.Jmp, next));
	
									map[sub.Instructions[i]] = t.First();
									modified.AddRange(t);
									done = true;
									break;
								}
								case Opcode.TailCall:
								{
									int callLimit = loc.C - 1;

									if (callLimit == -1)
										callLimit = instr.B - 1;

									if (callLimit == -1)
										callLimit = chunk.StackSize - loc.A + 1;

									List<Instruction> t = new List<Instruction>();
									
									for (int j = 0; j <= callLimit; j++)
										t.Add(new Instruction(chunk, Opcode.Move)
										{
											A = loc.A   + j,
											B = instr.A + j,
										});

									instr.OpCode = Opcode.Call;
									instr.A = loc.A;	

									t.Add(instr);
									t.Add(new Instruction(chunk, Opcode.Jmp, next));
	
									map[sub.Instructions[i]] = t.First();
									modified.AddRange(t);
									done = true;
									break;
								}
								default:
									modified.Add(instr);
									break;
							}

							if (done)
								break;
						}
						
						chunk.Instructions.InsertRange(idx, modified);
						
						foreach (Instruction k in map.Keys)
						{
							map[k].BackReferences.Clear();
							for (int i = 0; i < k.BackReferences.Count; i++)
								map[k].BackReferences.Add(map[k.BackReferences[i]]);

							if (k.RefOperands[0] is Instruction i2)
								map[k].RefOperands[0] = map[i2];
						}
					}
					
					chunk.UpdateMappings();

					foreach (var clos in closures)
					{
						chunk.Instructions.RemoveRange(chunk.InstructionMap[clos], ((Chunk)clos.RefOperands[0]).UpvalueCount + 1);
						foreach (var bRef in clos.BackReferences)
							bRef.SetupRefs();
					}
					
					foreach (Constant c in sub.Constants)
					{
						var nc = chunk.Constants.FirstOrDefault(c2 => c2.Type == c.Type && c2.Data == c.Data);
						if (nc == null)
						{
							nc = new Constant(c);
							chunk.Constants.Add(nc);
						}

						foreach (var inst in chunk.Instructions)
						{
							if (inst.RefOperands[0] is Constant c2 && c == c2)
								inst.RefOperands[0] = nc;
							
							
							if (inst.RefOperands[1] is Constant c3 && c == c3)
								inst.RefOperands[1] = nc;
						}
					}
					
					foreach (Chunk c in sub.Functions)
						chunk.Functions.Add(c);
					
					chunk.UpdateMappings();
					foreach (var _ins in chunk.Instructions)
						_ins.UpdateRegisters();
				}
			}
		}

		public void DoChunks()
		{
			DoChunk(_head);
			//File.WriteAllBytes("asd.luac", new VanillaSerializer(_head).Serialize());
		}
	}
}