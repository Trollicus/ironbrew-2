using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpSetGlobal : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetGlobal;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Env[Inst[OP_B]] = Stk[Inst[OP_A]];";

		public override void Mutate(Instruction instruction)
		{
			instruction.B++;
			instruction.ConstantMask |= InstructionConstantMask.RB;
		}
	}
}