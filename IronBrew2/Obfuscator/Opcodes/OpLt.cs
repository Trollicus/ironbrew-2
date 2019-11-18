using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpLt : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Lt && instruction.A == 0 && instruction.B <= 255 && instruction.C <= 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"if(Stk[Inst[OP_A]]<Stk[Inst[OP_C]])then InstrPoint=InstrPoint+1;else InstrPoint=InstrPoint+Inst[OP_B];end;";
		
		public override void Mutate(Instruction instruction)
		{
			instruction.A = instruction.B;
			
			instruction.B = instruction.Chunk.Instructions[instruction.PC + 1].B + 1;
			instruction.InstructionType = InstructionType.AsBxC;
		}
	}
	
	public class OpLtB : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Lt && instruction.A == 0 && instruction.B > 255 && instruction.C <= 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"if(Const[Inst[OP_A]]<Stk[Inst[OP_C]])then InstrPoint=InstrPoint+1;else InstrPoint=InstrPoint+Inst[OP_B];end;";

		public override void Mutate(Instruction instruction)
		{
			instruction.A = instruction.B - 255;
			
			instruction.B = instruction.Chunk.Instructions[instruction.PC + 1].B + 1;
			instruction.InstructionType = InstructionType.AsBxC;
		}
	}
	
	public class OpLtC : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Lt && instruction.A == 0 && instruction.B <= 255 && instruction.C > 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"if(Stk[Inst[OP_A]]<Const[Inst[OP_C]])then InstrPoint=InstrPoint+1;else InstrPoint=InstrPoint+Inst[OP_B];end;";

		public override void Mutate(Instruction instruction)
		{
			instruction.A = instruction.B;
			instruction.C -= 255;

			instruction.B = instruction.Chunk.Instructions[instruction.PC + 1].B + 1;
			instruction.InstructionType = InstructionType.AsBxC;
		}
	}
	
	public class OpLtBC : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Lt && instruction.A == 0 && instruction.B > 255 && instruction.C > 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"if(Const[Inst[OP_A]]<Const[Inst[OP_C]])then InstrPoint=InstrPoint+1;else InstrPoint=InstrPoint+Inst[OP_B];end;";

		public override void Mutate(Instruction instruction)
		{
			instruction.A = instruction.B - 255;
			instruction.C -= 255;

			instruction.B = instruction.Chunk.Instructions[instruction.PC + 1].B + 1;
			instruction.InstructionType = InstructionType.AsBxC;
		}
	}
}