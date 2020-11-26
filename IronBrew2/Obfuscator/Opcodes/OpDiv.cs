using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpDiv : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Div && instruction.B <= 255 && instruction.C <= 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[Inst[OP_A]]=Stk[Inst[OP_B]] / Stk[Inst[OP_C]];";
	}
	
	public class OpDivB : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Div && instruction.B > 255 && instruction.C <= 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[Inst[OP_A]] = Inst[OP_B] / Stk[Inst[OP_C]];";

		public override void Mutate(Instruction instruction)
		{
			instruction.B -= 255;
			instruction.ConstantMask |= InstructionConstantMask.RB;
		}
	}
	
	public class OpDivC : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Div && instruction.B <= 255 && instruction.C > 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[Inst[OP_A]] = Stk[Inst[OP_B]] / Inst[OP_C];";

		public override void Mutate(Instruction instruction)
		{
			instruction.C -= 255;
			instruction.ConstantMask |= InstructionConstantMask.RC;
		}
	}
	
	public class OpDivBC : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Div && instruction.B > 255 && instruction.C > 255;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[Inst[OP_A]] =  Inst[OP_B] / Inst[OP_C];";

		public override void Mutate(Instruction instruction)
		{
			instruction.B -= 255;
			instruction.C -= 255;
			instruction.ConstantMask |= InstructionConstantMask.RB | InstructionConstantMask.RC;
		}
	}
}