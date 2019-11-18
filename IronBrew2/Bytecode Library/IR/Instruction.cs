using System;
using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Obfuscator;

namespace IronBrew2.Bytecode_Library.IR
{
	public class Instruction
	{
		public object[] RefOperands = {null, null, null};
		public List<Instruction> BackReferences = new List<Instruction>();

		public Chunk Chunk;
		public Opcode OpCode;
		public InstructionType InstructionType;

		public int A;
		public int B;
		public int C;

		public int Data;
		public int PC;
		public int Line;

		public CustomInstructionData CustomData;

		public Instruction(Instruction other)
		{
			RefOperands = other.RefOperands.ToArray();
			BackReferences = other.BackReferences.ToList();
			Chunk = other.Chunk;
			OpCode = other.OpCode;
			InstructionType = other.InstructionType;
			A = other.A;
			B = other.B;
			C = other.C;
			Data = other.Data;
			PC = other.PC;
			Line = other.Line;
		}

		public Instruction(Chunk chunk, Opcode code, params object[] refOperands)
		{
			A = 0;
			B = 0;
			C = 0;
			Data = 0;
			
			Chunk = chunk;
			OpCode = code;

			if (Deserializer.InstructionMappings.TryGetValue(code, out InstructionType type))
				InstructionType = type;
			else
				InstructionType = InstructionType.ABC;

			for (int i = 0; i < refOperands.Length; i++)
			{
				var op =  refOperands[i];
				RefOperands[i] = op;
				
				if (op is Instruction ins)
					ins.BackReferences.Add(this);
			}
		}
		
		public void UpdateRegisters()
		{
			if (InstructionType == InstructionType.Data)
				return;
			
			PC = Chunk.InstructionMap[this];
			switch (OpCode)
			{
				case Opcode.LoadConst:
				case Opcode.GetGlobal:
				case Opcode.SetGlobal:
					B = Chunk.ConstantMap[(Constant)RefOperands[0]];
					break;
				case Opcode.Jmp:
				case Opcode.ForLoop:
				case Opcode.ForPrep:
					B = Chunk.InstructionMap[(Instruction)RefOperands[0]] - PC - 1;
					break;
				case Opcode.Closure:
					B = Chunk.FunctionMap[(Chunk)RefOperands[0]];
					break;
				case Opcode.GetTable:
				case Opcode.SetTable:
				case Opcode.Add:
				case Opcode.Sub:
				case Opcode.Mul:
				case Opcode.Div:
				case Opcode.Mod:
				case Opcode.Pow:
				case Opcode.Eq:
				case Opcode.Lt:
				case Opcode.Le:
				case Opcode.Self:
					if (RefOperands[0] is Constant cB)
						B = Chunk.ConstantMap[cB] + 256;

					if (RefOperands[1] is Constant cC)
						C = Chunk.ConstantMap[cC] + 256;
					break;
			}
		}

		public void SetupRefs()
		{
			RefOperands = new object[] {null, null, null};
			switch (OpCode)
			{
				case Opcode.LoadConst:
				case Opcode.GetGlobal:
				case Opcode.SetGlobal:
					RefOperands[0] = Chunk.Constants[B];
					((Constant)RefOperands[0]).BackReferences.Add(this);
					break;
				case Opcode.Jmp:
				case Opcode.ForLoop:
				case Opcode.ForPrep:
					RefOperands[0] = Chunk.Instructions[Chunk.InstructionMap[this] + B + 1];
					((Instruction) RefOperands[0]).BackReferences.Add(this);
					break;
				case Opcode.Closure:
					RefOperands[0] = Chunk.Functions[B];
					break;
				case Opcode.GetTable:
				case Opcode.SetTable:
				case Opcode.Add:
				case Opcode.Sub:
				case Opcode.Mul:
				case Opcode.Div:
				case Opcode.Mod:
				case Opcode.Pow:
				case Opcode.Eq:
				case Opcode.Lt:
				case Opcode.Le:
				case Opcode.Self:
					if (B > 255)
						RefOperands[0] = Chunk.Constants[B - 256];

					if (C > 255)
						RefOperands[1] = Chunk.Constants[C - 256];
					break;
			}
		}
	}
}