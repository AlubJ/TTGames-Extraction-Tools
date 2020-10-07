using ExtractHelper;

namespace ExtractDx11MESH.TXGHs
{
	public class TXGH05 : TXGH04
	{
		public TXGH05(byte[] fileData, int iPos)
			: base(fileData, iPos)
		{
		}

		protected override void ReadTextureMeta()
		{
			iPos += 16;
			iPos += 4;
			iPos += 4;
			iPos += 4;
			iPos += 4;
			iPos += 4;
			iPos += 17;
			int num = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			iPos += num;
		}

		protected override void ReadCam()
		{
			iPos += 4;
			int num = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			iPos += num * 12;
		}
	}
}
