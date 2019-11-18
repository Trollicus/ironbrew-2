using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using IronBrew2.Bytecode_Library.Bytecode;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Obfuscator.Control_Flow.Types
{
    public static class EQMutate
    {
        public static Random      Random      = new Random();
        public static CFGenerator CFGenerator = new CFGenerator();

        public static void DoInstructions(Chunk chunk, List<Instruction> instructions)
        {
            chunk.UpdateMappings();
            foreach (Instruction l in instructions)
            {
                if (l.OpCode != Opcode.Eq)
                    continue;

                Instruction target = (Instruction) chunk.Instructions[chunk.InstructionMap[l] + 1].RefOperands[0];
                Instruction target2 = chunk.Instructions[chunk.InstructionMap[l] + 2];

                Instruction newLt = new Instruction(l);
                newLt.OpCode = Opcode.Lt;
                newLt.A = l.A;

                Instruction newLe = new Instruction(l);
                newLe.OpCode = Opcode.Le;
                newLe.A = l.A == 0 ? 1 : 0;
                
                int idx = chunk.InstructionMap[l];

                Instruction j1 = CFGenerator.NextJMP(chunk, target2);
                Instruction j2 = CFGenerator.NextJMP(chunk, target2);
                Instruction j3 = CFGenerator.NextJMP(chunk, target);

                chunk.Instructions.InsertRange(idx, new[] {newLt, j1, newLe, j2, j3});

                chunk.UpdateMappings();
                foreach (Instruction i in chunk.Instructions)
                    i.UpdateRegisters();

                Instruction j = chunk.Instructions[chunk.InstructionMap[l] + 1];

                chunk.Instructions.Remove(l);
                chunk.Instructions.Remove(j);
                
                foreach (Instruction br in l.BackReferences)
                    br.RefOperands[0] = newLt;

                foreach (Instruction br in j.BackReferences)
                    br.RefOperands[0] = newLt;
                chunk.UpdateMappings();
            }
        }
    }
}