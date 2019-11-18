using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using IronBrew2.Bytecode_Library.IR;

namespace IronBrew2.Bytecode_Library.Bytecode
{
	public class Deserializer
	{
		private MemoryStream _stream;
		
		private bool _bigEndian;
		private byte _sizeNumber;
		private byte _sizeSizeT;
		private Encoding _fuckingLua = Encoding.GetEncoding(28591);

		private bool _expectingSetlistData;

		public static Dictionary<Opcode, InstructionType> InstructionMappings = new Dictionary<Opcode, InstructionType>()
		{
			{ Opcode.Move, InstructionType.ABC },
			{ Opcode.LoadConst, InstructionType.ABx },
			{ Opcode.LoadBool, InstructionType.ABC },
			{ Opcode.LoadNil, InstructionType.ABC },
			{ Opcode.GetUpval, InstructionType.ABC },
			{ Opcode.GetGlobal, InstructionType.ABx },
			{ Opcode.GetTable, InstructionType.ABC },
			{ Opcode.SetGlobal, InstructionType.ABx },
			{ Opcode.SetUpval, InstructionType.ABC },
			{ Opcode.SetTable, InstructionType.ABC },
			{ Opcode.NewTable, InstructionType.ABC },
			{ Opcode.Self, InstructionType.ABC },
			{ Opcode.Add, InstructionType.ABC },
			{ Opcode.Sub, InstructionType.ABC },
			{ Opcode.Mul, InstructionType.ABC },
			{ Opcode.Div, InstructionType.ABC },
			{ Opcode.Mod, InstructionType.ABC },
			{ Opcode.Pow, InstructionType.ABC },
			{ Opcode.Unm, InstructionType.ABC },
			{ Opcode.Not, InstructionType.ABC },
			{ Opcode.Len, InstructionType.ABC },
			{ Opcode.Concat, InstructionType.ABC },
			{ Opcode.Jmp, InstructionType.AsBx },
			{ Opcode.Eq, InstructionType.ABC },
			{ Opcode.Lt, InstructionType.ABC },
			{ Opcode.Le, InstructionType.ABC },
			{ Opcode.Test, InstructionType.ABC },
			{ Opcode.TestSet, InstructionType.ABC },
			{ Opcode.Call, InstructionType.ABC },
			{ Opcode.TailCall, InstructionType.ABC },
			{ Opcode.Return, InstructionType.ABC },
			{ Opcode.ForLoop, InstructionType.AsBx },
			{ Opcode.ForPrep, InstructionType.AsBx },
			{ Opcode.TForLoop, InstructionType.ABC },
			{ Opcode.SetList, InstructionType.ABC },
			{ Opcode.Close, InstructionType.ABC },
			{ Opcode.Closure, InstructionType.ABx },
			{ Opcode.VarArg, InstructionType.ABC }
		};
		
		public Deserializer(byte[] input) =>
			_stream = new MemoryStream(input);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] Read(int size, bool factorEndianness = true)
		{
			byte[] bytes = new byte[size];
			_stream.Read(bytes, 0, size);

			if (factorEndianness && (_bigEndian == BitConverter.IsLittleEndian)) //if factor in endianness AND endianness differs between the two versions
				bytes = bytes.Reverse().ToArray();	
			
			return bytes;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ReadSizeT() => 
			_sizeSizeT == 4 ? ReadInt32() : ReadInt64();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ReadInt64() =>
			BitConverter.ToInt64(Read(8), 0);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadInt32(bool factorEndianness = true) =>
			BitConverter.ToInt32(Read(4, factorEndianness), 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ReadByte() =>
			Read(1)[0];
		
		public string ReadString()
		{
			long c = ReadSizeT();
			int count = (int) c;
			
			if (count == 0)
				return "";

			byte[] val = Read(count, false);
			return _fuckingLua.GetString(val, 0, count - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double ReadDouble() =>
			BitConverter.ToDouble(Read(_sizeNumber), 0);

		public Instruction DecodeInstruction(Chunk chunk, int index)
		{
			int code = ReadInt32();
			Instruction i = new Instruction(chunk, (Opcode) (code & 0x3F));
			
			i.Data = code;
			
			if (_expectingSetlistData)
			{
				_expectingSetlistData = false;
				
				i.InstructionType = InstructionType.Data;
				return i;
			}
			
			i.A = (code >> 6) & 0xFF;

			switch (i.InstructionType)
			{
				//WHAT THE FUCK LUA
				case InstructionType.ABC:
					i.B = (code >> 6 + 8 + 9) & 0x1FF;
					i.C = (code >> 6 + 8) & 0x1FF;
				break;
				
				case InstructionType.ABx:
					i.B = (code >> 6 + 8) & 0x3FFFF;
					i.C = -1;
				break;
				
				case InstructionType.AsBx:
					i.B = ((code >> 6 + 8) & 0x3FFFF) - 131071;
					i.C = -1;
				break;
			}

			if (i.OpCode == Opcode.SetList && i.C == 0)
				_expectingSetlistData = true;
			
			return i;
		}

		public List<Instruction> DecodeInstructions(Chunk chunk)
		{
			List<Instruction> instructions = new List<Instruction>();
			
			int Count = ReadInt32();
			
			for (int i = 0; i < Count; i++)
				instructions.Add(DecodeInstruction(chunk, i));
			
			return instructions;
		}

		public Constant DecodeConstant()
		{
			Constant c = new Constant();
			byte Type = ReadByte();
			//ALSO WHAT THE FUCK LUA
			switch (Type)
			{
				case 0:
					c.Type = ConstantType.Nil;
					c.Data = null;
				break;
				
				case 1:
					c.Type = ConstantType.Boolean;
					c.Data = ReadByte() != 0;
				break;
				
				case 3:
					c.Type = ConstantType.Number;
					c.Data = ReadDouble();
				break;
				
				case 4:
					c.Type = ConstantType.String;
					c.Data = ReadString();
				break;
			}

			return c;
		}
		
		public List<Constant> DecodeConstants()
		{
			List<Constant> constants = new List<Constant>();
			
			int Count = ReadInt32();
			
			for (int i = 0; i < Count; i++)
				constants.Add(DecodeConstant());

			return constants;
		}

		public Chunk DecodeChunk()
		{
			Chunk c = new Chunk
			          {
				          Name           = ReadString(),
				          Line           = ReadInt32(),
				          LastLine       = ReadInt32(),
				          UpvalueCount   = ReadByte(),
				          ParameterCount = ReadByte(),
				          VarargFlag     = ReadByte(),
				          StackSize      = ReadByte(),
				          Upvalues       = new List<string>()
			          };
			
			c.Instructions = DecodeInstructions(c);
			c.Constants = DecodeConstants();
			c.Functions = DecodeChunks();
			
			c.UpdateMappings();
			
			foreach (var inst in c.Instructions)
				inst.SetupRefs();
			
			int count = ReadInt32();
			for (int i = 0; i < count; i++) // source line pos list
				c.Instructions[i].Line = ReadInt32();

			//skip other debug info cus fuckit.wav

			count = ReadInt32();
			for (int i = 0; i < count; i++) // local list
			{
				ReadString();
				ReadInt32();
				ReadInt32();
			}
			count = ReadInt32();
			for (int i = 0; i < count; i++) // upvalues
				c.Upvalues.Add(ReadString());
			
			return c;
		}

		public List<Chunk> DecodeChunks()
		{
			List<Chunk> Chunks = new List<Chunk>();
			
			int count = ReadInt32();
			
			for (int i = 0; i < count; i++)
				Chunks.Add(DecodeChunk());

			return Chunks;
		}

		public Chunk DecodeFile()
		{
			int header = ReadInt32();
			
			if (header != 0x1B4C7561 && header != 0x61754C1B)
				throw new Exception("Invalid luac file.");
				
			if (ReadByte() != 0x51)
				throw new Exception("Only Lua 5.1 is supported.");

			ReadByte(); //format official shit wtf
			
			_bigEndian = ReadByte() == 0;
			
			ReadByte(); //size of int (assume 4 fuck off)
			
			_sizeSizeT = ReadByte();
			
			ReadByte(); //size of instruction (fuck it not supporting anything else than default)
			
			_sizeNumber = ReadByte();
			
			ReadByte(); //not supporting integer number bullshit fuck off

			Chunk c = DecodeChunk();
			return c;
		}
	}
}