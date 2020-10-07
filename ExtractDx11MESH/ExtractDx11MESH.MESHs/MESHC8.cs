using System;
using ExtractHelper;

namespace ExtractDx11MESH.MESHs
{
	public class MESHC8 : MESHAF
	{
		public MESHC8(byte[] fileData, int iPos)
			: base(fileData, iPos)
		{
		}

		public override int Read(ref int referencecounter)
		{
			int num = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}   Number of Parts: 0x{1:x8}", iPos, num);
			iPos += 4;
			for (int i = 0; i < num; i++)
			{
				ColoredConsole.WriteLine("{0:x8}   Part 0x{1:x8}", iPos, i);
				Parts.Add(ReadPart(ref referencecounter));
			}
			return iPos;
		}

		protected override Part ReadPart(ref int referencecounter)
		{
			int num = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}   Number of RNMS: 0x{1:x8}", iPos, num);
			iPos += 4;
			iPos += 4;
			int num2 = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLineInfo("{0:x8}   RNMS Version 0x{1:x2}", iPos, num2);
			iPos += 4;
			Part part = new Part();
			int num3 = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}     Number of Vertex Lists: 0x{1:x8}", iPos, num3);
			iPos += 4;
			for (int i = 0; i < num3; i++)
			{
				ColoredConsole.WriteLine("{0:x8}       Vertex List 0x{1:x8}", iPos, i);
				part.VertexListReferences1.Add(GetVertexListReference(ref referencecounter));
			}
			iPos += 4;
			part.IndexListReference1 = GetIndexListReference(ref referencecounter);
			part.OffsetIndices = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}     Offset Indices: 0x{1:x8}", iPos, part.OffsetIndices);
			iPos += 4;
			part.NumberIndices = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}     Number Indices: 0x{1:x8}", iPos, part.NumberIndices);
			iPos += 4;
			part.OffsetVertices = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}     Offset Vertices: 0x{1:x8}", iPos, part.OffsetVertices);
			iPos += 4;
			if (BigEndianBitConverter.ToInt16(fileData, iPos) != 0)
			{
				throw new NotSupportedException("ReadPart Offset Vertices + 4");
			}
			iPos += 2;
			part.NumberVertices = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}     Number Vertices: 0x{1:x8}", iPos, part.NumberVertices);
			iPos += 4;
			iPos += 4;
			int num4 = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			if (num4 > 0)
			{
				ColoredConsole.Write("{0:x8}     ", iPos);
				for (int i = 0; i < num4; i++)
				{
					ColoredConsole.Write("{0:x2} ", fileData[iPos]);
					iPos++;
				}
				ColoredConsole.WriteLine();
				referencecounter++;
			}
			int num5 = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			if (num5 != 0)
			{
				int num6 = ReadRelativePositionList();
				referencecounter += num6;
			}
			else
			{
				iPos += 4;
			}
			iPos += 36;
			referencecounter++;
			referencecounter++;
			return part;
		}
	}
}
