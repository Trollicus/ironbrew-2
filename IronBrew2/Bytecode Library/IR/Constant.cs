using System.Collections.Generic;
using System.Linq;

namespace IronBrew2.Bytecode_Library.IR
{
	public class Constant
	{
		public List<Instruction> BackReferences = new List<Instruction>();
		
		public ConstantType Type;
		public dynamic Data;

		public Constant() { }
		
		public Constant(Constant other)
		{
			Type = other.Type;
			Data = other.Data;
			BackReferences = other.BackReferences.ToList();
		}
	}
}