using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpCall : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B > 1 &&
			instruction.C > 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Args={};local Edx=0;local Limit=A+Inst[OP_B]-1;for Idx=A+1,Limit do Edx=Edx+1;Args[Edx]=Stk[Idx];end;local Results={Stk[A](Unpack(Args,1,Limit-A))};local Limit=A+Inst[OP_C]-2;Edx=0;for Idx=A,Limit do Edx=Edx+1;Stk[Idx]=Results[Edx];end;Top=Limit;";
	}
	
	public class OpCallB0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 0 &&
			instruction.C > 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Args={};local Edx=0;local Limit=Top;for Idx=A+1,Limit do Edx=Edx+1;Args[Edx]=Stk[Idx];end;local Results={Stk[A](Unpack(Args,1,Limit-A))};local Limit=A+Inst[OP_C]-2;Edx=0;for Idx=A,Limit do Edx=Edx+1;Stk[Idx]=Results[Edx];end;Top=Limit;";
	}
	
	public class OpCallB1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 1 &&	
			instruction.C > 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Results,Limit={Stk[A]()};local Limit=A+Inst[OP_C]-2;local Edx=0;for Idx=A,Limit do Edx=Edx+1;Stk[Idx]=Results[Edx];end;Top=Limit;";
	}
	
	public class OpCallC0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B > 1 &&
			instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Args={};local Edx=0;local Limit=A+Inst[OP_B]-1;for Idx=A+1,Limit do Edx=Edx+1;Args[Edx]=Stk[Idx];end;local Results,Limit=_R(Stk[A](Unpack(Args,1,Limit-A)));Limit=Limit+A-1;Edx=0;for Idx=A,Limit do Edx=Edx+1;Stk[Idx]=Results[Edx];end;Top=Limit;";
	}
	
	public class OpCallC1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B > 1 &&
			instruction.C == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Args={};local Edx=0;local Limit=A+Inst[OP_B]-1;for Idx=A+1,Limit do Edx=Edx+1;Args[Edx]=Stk[Idx];end;Stk[A](Unpack(Args,1,Limit-A));Top=A;";
	}
	
	public class OpCallB0C0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 0 &&
			instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Args={};local Edx=0;local Limit=Top;for Idx=A+1,Limit do Edx=Edx+1;Args[Edx]=Stk[Idx];end;local Results,Limit=_R(Stk[A](Unpack(Args,1,Limit-A)));Limit=Limit+A-1;Edx=0;for Idx=A,Limit do Edx=Edx+1;Stk[Idx]=Results[Edx];end;Top=Limit;";
	}
	
	public class OpCallB0C1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 0 &&
			instruction.C == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Args={};local Edx=0;local Limit=Top;for Idx=A+1,Limit do Edx=Edx+1;Args[Edx]=Stk[Idx];end;Stk[A](Unpack(Args,1,Limit-A));Top=A;";
	}
	
	public class OpCallB1C0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 1 &&
			instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			"local A=Inst[OP_A];local Results,Limit=_R(Stk[A]());Top=A-1;Limit=Limit+A-1;local Edx=0;for Idx=A,Limit do Edx=Edx+1;Stk[Idx]=Results[Edx];end;Top=Limit;";
	}
	
	public class OpCallB1C1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 1 &&
			instruction.C == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[Inst[OP_A]]();Top=A;";
	}
}