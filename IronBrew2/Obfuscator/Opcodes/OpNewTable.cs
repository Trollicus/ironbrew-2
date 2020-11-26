using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpNewTableB0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) => instruction.OpCode == Opcode.NewTable;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[Inst[OP_A]]={};";
	}
}