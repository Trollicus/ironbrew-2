using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpVarArg : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.VarArg && instruction.B != 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local B=Inst[OP_B];for Idx=A,A+B-1 do Stk[Idx]=Vararg[Idx-A];end;";
	}
	
	public class OpVarArgB0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.VarArg && instruction.B == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];Top=A+Varargsz-1;for Idx=A,Top do local VA=Vararg[Idx-A];Stk[Idx]=VA;end;";
	}
}