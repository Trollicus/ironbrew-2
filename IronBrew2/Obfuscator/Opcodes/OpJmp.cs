using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpJmp : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Jmp;

		public override string GetObfuscated(ObfuscationContext context) =>
			"InstrPoint=Inst[OP_B];";
		
		public override void Mutate(Instruction instruction)
		{
			instruction.B += instruction.PC + 1;
		}
	}
}