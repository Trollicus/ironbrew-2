using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Opcodes
{
	public class OpCall : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B > 2 &&
			instruction.C > 2;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
local Results = { Stk[A](Unpack(Stk, A + 1, Inst[OP_B])) };
local Edx = 0;
for Idx = A, Inst[OP_C] do 
	Edx = Edx + 1;
	Stk[Idx] = Results[Edx];
end
";

		public override void Mutate(Instruction instruction)
		{
			instruction.B += instruction.A - 1;
			instruction.C += instruction.A - 2;
		}
	}
	
	public class OpCallB2 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 2 &&
			instruction.C > 2;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
local Results = { Stk[A](Stk[A + 1]) };
local Edx = 0;
for Idx = A, Inst[OP_C] do 
	Edx = Edx + 1;
	Stk[Idx] = Results[Edx];
end
";
		public override void Mutate(Instruction instruction)
		{
			instruction.C += instruction.A - 2;
		}
	}
	
	public class OpCallB0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 0 &&
			instruction.C > 2;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
local Results = { Stk[A](Unpack(Stk, A + 1, Top)) };
local Edx = 0;
for Idx = A, Inst[OP_C] do 
	Edx = Edx + 1;
	Stk[Idx] = Results[Edx];
end
";
		public override void Mutate(Instruction instruction)
		{
			instruction.C += instruction.A - 2;
		}
	}

	public class OpCallB1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 1 &&	
			instruction.C > 2;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
local Results = { Stk[A]() };
local Limit = Inst[OP_C];
local Edx = 0;
for Idx = A, Limit do 
	Edx = Edx + 1;
	Stk[Idx] = Results[Edx];
end
";
		public override void Mutate(Instruction instruction)
		{
			instruction.C += instruction.A - 2;
		}
	}
	
	public class OpCallC0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B > 2 &&
			instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
local Results, Limit = _R(Stk[A](Unpack(Stk, A + 1, Inst[OP_B])))
Top = Limit + A - 1
local Edx = 0;
for Idx = A, Top do 
	Edx = Edx + 1;
	Stk[Idx] = Results[Edx];
end;
";
		public override void Mutate(Instruction instruction)
		{
			instruction.B += instruction.A - 1;
		}
	}
	
	public class OpCallC0B2 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 2 &&
			instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
local Results, Limit = _R(Stk[A](Stk[A + 1]))
Top = Limit + A - 1
local Edx = 0;
for Idx = A, Top do 
	Edx = Edx + 1;
	Stk[Idx] = Results[Edx];
end;
";
		public override void Mutate(Instruction instruction)
		{
			instruction.B += instruction.A - 1;
		}
	}
	
	public class OpCallC1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B > 2 &&
			instruction.C == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
Stk[A](Unpack(Stk, A + 1, Inst[OP_B]))
";
		public override void Mutate(Instruction instruction)
		{
			instruction.B += instruction.A - 1;
		}
	}
	
	public class OpCallC1B2 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 2 &&
			instruction.C == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
Stk[A](Stk[A + 1])
";
	}
	
	public class OpCallB0C0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 0 &&
			instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
local Results, Limit = _R(Stk[A](Unpack(Stk, A + 1, Top)))
Top = Limit + A - 1
local Edx = 0;
for Idx = A, Top do 
	Edx = Edx + 1;
	Stk[Idx] = Results[Edx];
end;
";
	}
	
	public class OpCallB0C1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 0 &&
			instruction.C == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
Stk[A](Unpack(Stk, A + 1, Top))
";
	}
	
	public class OpCallB1C0 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 1 &&
			instruction.C == 0;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
local Results, Limit = _R(Stk[A]())
Top = Limit + A - 1
local Edx = 0;
for Idx = A, Top do 
	Edx = Edx + 1;
	Stk[Idx] = Results[Edx];
end;
";
	}
	
	public class OpCallB1C1 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 1 &&
			instruction.C == 1;

		public override string GetObfuscated(ObfuscationContext context) =>
			"Stk[Inst[OP_A]]();";
	}
	
	public class OpCallC2 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B > 2 &&
			instruction.C == 2;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
Stk[A] = Stk[A](Unpack(Stk, A + 1, Inst[OP_B])) 
";
		public override void Mutate(Instruction instruction)
		{
			instruction.B += instruction.A - 1;
		}
	}
	
	public class OpCallC2B2 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 2 &&
			instruction.C == 2;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
Stk[A] = Stk[A](Stk[A + 1]) 
";
	}
	
	public class OpCallB0C2 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 0 &&
			instruction.C == 2;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
Stk[A] = Stk[A](Unpack(Stk, A + 1, Top))
";
	}

	public class OpCallB1C2 : VOpcode
	{
		public override bool IsInstruction(Instruction instruction) =>
			instruction.OpCode == Opcode.Call && instruction.B == 1 &&	
			instruction.C == 2;

		public override string GetObfuscated(ObfuscationContext context) =>
			@"
local A = Inst[OP_A]
Stk[A] = Stk[A]()
";
	}
}