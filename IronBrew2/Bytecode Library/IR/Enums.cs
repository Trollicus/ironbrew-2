using System;

namespace IronBrew2.Bytecode_Library.IR
{
	public enum ConstantType
	{
		Nil,
		Boolean,
		Number,
		String
	}
	
	public enum InstructionType
	{
		ABC,
		ABx,
		AsBx,
		AsBxC,
		Data
	}

	[Flags]
	public enum InstructionConstantMask
	{
		NK = 0,
		RA = 1,
		RB = 2,
		RC = 4
	}
}