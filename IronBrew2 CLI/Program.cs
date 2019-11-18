using System;
using System.IO;
using System.Text;
using IronBrew2;
using IronBrew2.Obfuscator;

namespace IronBrew2_CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			Directory.CreateDirectory("temp");
			if (!IB2.Obfuscate("temp",  args[0], new ObfuscationSettings(), out string err))
			{
				Console.WriteLine("ERR: " + err);
				return;
			}

			File.Delete("out.lua");
			File.Move("temp/out.lua", "out.lua");
			Directory.Delete("temp", true);
			Console.WriteLine("Done!");
		}
	}
}