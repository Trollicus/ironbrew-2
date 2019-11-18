using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpTest : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Test && instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"if Stk[Inst[OP_A]] then InstrPoint=InstrPoint+1;else InstrPoint=InstrPoint+Inst[OP_B];end;";
		
		public override void Mutate(Instruction instruction)
		{
			instruction.B = instruction.Chunk.Instructions[instruction.PC + 1].B + 1;
			instruction.InstructionType = InstructionType.AsBxC;
		}
	}
	
	public class OpTestC : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Test && instruction.C != 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"if not Stk[Inst[OP_A]] then InstrPoint=InstrPoint+1;else InstrPoint=InstrPoint+Inst[OP_B];end;";

		public override void Mutate(Instruction instruction)
		{
			instruction.B = instruction.Chunk.Instructions[instruction.PC + 1].B + 1;
			instruction.InstructionType = InstructionType.AsBxC;
		}
	}
} 