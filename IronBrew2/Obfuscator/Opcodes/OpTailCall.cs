using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpTailCall : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.TailCall && instruction.B > 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Args={};local Limit=A+Inst[OP_B]-1;for Idx=A+1,Limit do Args[#Args+1]=Stk[Idx];end;do return Stk[A](Unpack(Args,1,Limit-A)) end;";
	}
	
	public class OpTailCallB0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.TailCall && instruction.B == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Args={};local Limit=Top;for Idx=A+1,Limit do Args[#Args+1]=Stk[Idx];end;do return Stk[A](Unpack(Args,1,Limit-A)) end;";
	}
	
	public class OpTailCallB1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.TailCall && instruction.B == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];do return Stk[A](); end;";
	}
}