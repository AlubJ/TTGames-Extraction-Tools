using ExtractHelper;

namespace ExtractDx11MESH.IVL5
{
	public class IVL501
	{
		protected byte[] fileData;

		protected int iPos;

		public int version;

		public IVL501(byte[] fileData, int iPos)
		{
			this.fileData = fileData;
			this.iPos = iPos;
			version = BigEndianBitConverter.ToInt32(fileData, iPos);
			this.iPos += 4;
			ColoredConsole.WriteLineInfo("{0:x8} IVL5 Version 0x{1:x2}", iPos, version);
		}

		public virtual int Read(ref int referencecounter)
		{
			return iPos;
		}
	}
}
