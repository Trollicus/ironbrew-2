using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpForPrep : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.ForPrep;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];Stk[A]=Stk[A]-Stk[A+2];InstrPoint=InstrPoint+Inst[OP_B];";
	}
}