using ExtractHelper;

namespace ExtractNxgMESH.TXGHs
{
	public class TXGH07 : TXGH06
	{
		public TXGH07(byte[] fileData, int iPos)
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
			iPos += 10;
		}
	}
}
