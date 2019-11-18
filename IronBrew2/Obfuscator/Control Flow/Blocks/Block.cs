using System.Collections.Generic;
using System.Linq;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Control_Flow.Blocks
{
	public class Block
	{
		public Chunk Chunk;
		public List<Instruction> Body = new List<Instruction>();
		public Block Successor = null;

		public Block(Chunk c) =>
			Chunk = c;
	}
}