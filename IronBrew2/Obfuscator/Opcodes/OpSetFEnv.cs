using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpSetFEnv : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetFenv;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Env = Stk[Inst[OP_A]]";
	}
}