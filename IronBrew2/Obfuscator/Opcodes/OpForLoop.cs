using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpForLoop : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.ForLoop;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Step=Stk[A+2];local Index=Stk[A]+Step;Stk[A]=Index;if Step>0 then if Index<=Stk[A+1] then InstrPoint=InstrPoint+Inst[OP_B];Stk[A+3]=Index;end;elseif Index>=Stk[A+1] then InstrPoint=InstrPoint+Inst[OP_B];Stk[A+3]=Index;end;";
	}
}