using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpTestSet : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.TestSet && instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local B=Stk[Inst[OP_B]];if B then InstrPoint=InstrPoint+1;else Stk[Inst[OP_A]]=B;InstrPoint=InstrPoint+Instr[InstrPoint+1][OP_B]+1;end;";
	}
	
	public class OpTestSetC : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.TestSet && instruction.C != 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local B=Stk[Inst[OP_B]];if not B then InstrPoint=InstrPoint+1;else Stk[Inst[OP_A]]=B;InstrPoint=InstrPoint+Instr[InstrPoint+1][OP_B]+1;end;";
		
	}
}