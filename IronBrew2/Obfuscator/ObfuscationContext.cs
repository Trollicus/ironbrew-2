using System;
using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;
using IronBrew2.Extensions;

namespace IronBrew2.Obfuscator
{
	public enum ChunkStep
	{
		ParameterCount,
		StringTable,
		Instructions,
		Constants,
		Functions,
		LineInfo,
		StepCount
	}

	public enum InstructionStep1
	{
		Type,
		A,
		B,
		C,
		StepCount
	}

	public enum InstructionStep2
	{
		Op,
		Bx,
		D,
		StepCount
	}
	
	public class ObfuscationContext
	{
		public Chunk HeadChunk;
		public ChunkStep[] ChunkSteps;
		public InstructionStep1[] InstructionSteps1;
		public InstructionStep2[] InstructionSteps2;
		public int[] ConstantMapping;

		public Dictionary<Opcode, VOpcode> InstructionMapping = new Dictionary<Opcode, VOpcode>();

		public int PrimaryXorKey;
			
		public int IXorKey1;
		public int IXorKey2;
		
		public ObfuscationContext(Chunk chunk)
		{
			HeadChunk = chunk;
			ChunkSteps = Enumerable.Range(0, (int) ChunkStep.StepCount).Select(i => (ChunkStep) i).ToArray();
			ChunkSteps.Shuffle();
			
			InstructionSteps1 = Enumerable.Range(0, (int) InstructionStep1.StepCount).Select(i => (InstructionStep1) i).ToArray();
			InstructionSteps1.Shuffle();
			
			InstructionSteps2 = Enumerable.Range(0, (int) InstructionStep2.StepCount).Select(i => (InstructionStep2) i).ToArray();
			InstructionSteps2.Shuffle();
			
			ConstantMapping = Enumerable.Range(0, 4).ToArray();
			ConstantMapping.Shuffle();

			Random rand = new Random();
			
			PrimaryXorKey = rand.Next(0, 256);
			IXorKey1 = rand.Next(0, 256);
			IXorKey2 = rand.Next(0, 256);
		}
	}
}