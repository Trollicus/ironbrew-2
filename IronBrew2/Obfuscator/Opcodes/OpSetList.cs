using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpSetList : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetList && instruction.B != 0 && instruction.C != 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A];
local T = Stk[A];
for Idx = A + 1, Inst[OP_B] do 
	Insert(T, Stk[Idx])
end;";

		public override void Mutate(Instruction instruction)
		{
			instruction.B += instruction.A;
		}
	}
	
	public class OpSetListB0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetList && instruction.B == 0 && instruction.C != 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A];
local T = Stk[A];
for Idx = A + 1, Top do 
	Insert(T, Stk[Idx])
end;";
	}
	
	public class OpSetListC0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetList && instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
InstrPoint = InstrPoint + 1
local A = Inst[OP_A];
local T = Stk[A];
for Idx = A + 1, Inst[OP_B] do 
	Insert(T, Stk[Idx])
end;";

		public override void Mutate(Instruction instruction)
		{
			instruction.B = instruction.A + instruction.Chunk.Instructions[instruction.PC + 1].Data;
			instruction.InstructionType = InstructionType.ABx;
		}
	}
}