using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace ZipCompressor
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				Console.WriteLine("Args required!");
				Console.ReadLine();
				return;
			}

			string srcPath = null, dstFile = null;
			foreach (var arg in args)
			{
				var values = arg.Split('=');
				if (values.Length != 2)
				{
					Console.WriteLine("Invalid arg: " + arg);
					Console.ReadLine();
					return;
				}

				switch (values[0])
				{
					case "src": srcPath = values[1]; break;
					case "dst": dstFile = values[1]; break;
					default:
						Console.WriteLine("Invalid arg type: " + values[0]);
						Console.ReadLine();
						return;
				}
			}

			if (string.IsNullOrEmpty(srcPath))
			{
				Console.WriteLine("Src path not set!");
				Console.ReadLine();
				return;
			}

			if (string.IsNullOrEmpty(dstFile))
			{
				Console.WriteLine("Dst file not set!");
				Console.ReadLine();
				return;
			}

			try
			{
				if (File.Exists(dstFile)) File.Delete(dstFile);
				ZipFile.CreateFromDirectory(srcPath, dstFile);
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to save file: " + e.Message);
				Console.ReadLine();
				return;
			}

			Console.WriteLine("Succeeded!");
		}
	}
}
