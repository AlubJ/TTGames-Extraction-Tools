using System;
using ExtractHelper;

namespace ExtractNxgMESH
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				ExtractNxgMESH extractNxgMESH = new ExtractNxgMESH();
				string[] a = { @"C:\Users\alunj\source\repos\LEGO TOOLS\ExtractNgxMESH\obj\Debug\net20\HAT_INDY_REDDISHBROWN_NXG.GSC" };
				extractNxgMESH.ParseArgs(a);
				extractNxgMESH.Extract();
			}
			catch (NotSupportedException ex)
			{
				ColoredConsole.WriteLineError("Not yet surported: " + ex.Message);
			}
			catch (NotImplementedException ex2)
			{
				ColoredConsole.WriteLineError("Not yet implemented: " + ex2.Message);
			}
			catch (Exception ex3)
			{
				ColoredConsole.WriteLineError(ex3.Message);
				ColoredConsole.WriteLineError(ex3.StackTrace);
			}

			Console.WriteLine("Press enter to close...");
			Console.ReadLine();
		}
	}
}
