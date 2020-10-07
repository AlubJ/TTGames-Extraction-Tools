using ExtractHelper;

namespace ExtractDx11MESH.TXGHs
{
	public class TXGH08 : TXGH07
	{
		public TXGH08(byte[] fileData, int iPos)
			: base(fileData, iPos)
		{
		}

		public override int Read(ref int referencecounter)
		{
			iPos += 4;
			int num = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			ColoredConsole.WriteLine("{0:x8}   Number of Unknown: 0x{1:x2}", iPos, num);
			iPos += 4 * num;
			iPos += 4;
			int num2 = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			ColoredConsole.WriteLine("{0:x8}   Number of Textures: 0x{1:x2}", iPos, num2);
			for (int i = 0; i < num2; i++)
			{
				ReadTextureMeta();
				referencecounter++;
			}
			iPos += 4;
			num = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			ColoredConsole.WriteLine("{0:x8}   Number of Unknown: 0x{1:x2}", iPos, num);
			iPos += 4 * num;
			iPos += 4;
			int num3 = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			ColoredConsole.WriteLine("{0:x8}   Number of Cameras: 0x{1:x2}", iPos, num3);
			for (int i = 0; i < num3; i++)
			{
				ReadCam();
			}
			iPos += 4;
			num = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			ColoredConsole.WriteLine("{0:x8}   Number of Unknown: 0x{1:x2}", iPos, num);
			if (num != 0)
			{
				referencecounter++;
			}
			iPos += 2 * num;
			return iPos;
		}
	}
}
