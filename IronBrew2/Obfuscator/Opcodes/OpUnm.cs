using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpUnm : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Unm;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[Inst[OP_A]]=-Stk[Inst[OP_B]];";
	}
}