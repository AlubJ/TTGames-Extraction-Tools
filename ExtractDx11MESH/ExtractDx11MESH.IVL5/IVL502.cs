using ExtractHelper;

namespace ExtractDx11MESH.IVL5
{
	public class IVL502 : IVL501
	{
		public IVL502(byte[] fileData, int iPos)
			: base(fileData, iPos)
		{
		}

		public override int Read(ref int referencecounter)
		{
			iPos += 4;
			iPos += 4;
			iPos += 4;
			int num = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}   Number of HGOL: 0x{1:x8}", iPos, num);
			return iPos;
		}
	}
}
