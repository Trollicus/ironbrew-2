using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpSetList : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetList && instruction.B != 0 &&
			instruction.C > 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Offset=(Inst[OP_C]-1)*50;local T=Stk[A];local B=Inst[OP_B];for Idx=1,B do T[Offset+Idx]=Stk[A+Idx] end;";
	}

	public class OpSetListC1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetList && instruction.B != 0 &&
			instruction.C == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local T=Stk[A];local B=Inst[OP_B];for Idx=1,B do T[Idx]=Stk[A+Idx] end;";
	}
	
	public class OpSetListB0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetList && instruction.B == 0 &&
			instruction.C != 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Offset=(Inst[OP_C]-1)*50;local T=Stk[A];local X=Top-A;for Idx=1,X do T[Offset+Idx]=Stk[A+Idx] end;";
	}
	
	public class OpSetListC0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetList && instruction.B != 0 &&
			instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];InstrPoint=InstrPoint+1;local Offset=(Instr[InstrPoint][5]-1)*50;local T=Stk[A];local B=Inst[OP_B];for Idx=1,B do T[Offset+Idx]=Stk[A+Idx] end;";
	}
	
	public class OpSetListB0C0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetList && instruction.B == 0 &&
			instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];InstrPoint=InstrPoint+1;local Offset=(Instr[InstrPoint][5]-1)*50;local T=Stk[A];local X=Top-A;for Idx=1,X do T[Offset+Idx]=Stk[A+Idx] end;";
	}

	public class OpSetListB0C1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.SetList && instruction.B == 0 &&
			instruction.C == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local T=Stk[A];local X=Top-A;for Idx=1,X do T[Idx]=Stk[A+Idx] end;";
	}
}