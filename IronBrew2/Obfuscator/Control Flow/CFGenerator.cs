using System;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Control_Flow
{
	public class CFGenerator
	{
		public Random Random = new Random();

		public Instruction NextJMP(Chunk lc, Instruction Reference) =>
			new Instruction(lc, Opcode.Jmp, Reference);

		public Instruction BelievableRandom(Chunk lc)
		{
			Instruction ins = new Instruction(lc, (Opcode)Random.Next(0, 37));

			ins.A = Random.Next(0, 128);
			ins.B = Random.Next(0, 128);
			ins.C = Random.Next(0, 128);

			while (true)
			{
				switch (ins.OpCode)
				{
					case Opcode.LoadConst:
					case Opcode.GetGlobal:
					case Opcode.SetGlobal:
					case Opcode.Jmp:
					case Opcode.ForLoop:
					case Opcode.TForLoop:
					case Opcode.ForPrep:
					case Opcode.Closure:
					case Opcode.GetTable:
					case Opcode.SetTable:
					case Opcode.Add:
					case Opcode.Sub:
					case Opcode.Mul:
					case Opcode.Div:
					case Opcode.Mod:
					case Opcode.Pow:
					case Opcode.Test:
					case Opcode.TestSet:
					case Opcode.Eq:
					case Opcode.Lt:
					case Opcode.Le:
					case Opcode.Self:
						ins.OpCode = (Opcode) Random.Next(0, 37);
						continue;

					default:
						return ins;
				}
			}
		}
		
		public Constant GetOrAddConstant(Chunk chunk, ConstantType type, dynamic constant, out int constantIndex)
		{
			var current =
				chunk.Constants.FirstOrDefault(c => c.Type == type &&
				                                    c.Data == constant); // type checking to prevent errors i guess
			if (current != null)
			{
				constantIndex = chunk.Constants.IndexOf(current);
				return current;
			}

			Constant newConst = new Constant
			                    {
				                    Type = type,
				                    Data = constant
			                    };


			constantIndex = chunk.Constants.Count;

			chunk.Constants.Add(newConst);
			chunk.ConstantMap.Add(newConst, constantIndex);

			return newConst;
		}
	}
}