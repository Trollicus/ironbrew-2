using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpReturn : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Return && instruction.B > 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Limit=A+Inst[OP_B]-2;local Output={};local Edx=0;for Idx=A,Limit do Edx=Edx+1;Output[Edx]=Stk[Idx];end; do return Unpack(Output,1,Edx) end;";
	}
	
	public class OpReturnB0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Return && instruction.B == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Limit=Top;local Output={};local Edx=0;for Idx=A,Limit do Edx=Edx+1;Output[Edx]=Stk[Idx];end;do return Unpack(Output,1,Edx) end;";
	}
	
	public class OpReturnB1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Return && instruction.B == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"do return end;";
	}
}