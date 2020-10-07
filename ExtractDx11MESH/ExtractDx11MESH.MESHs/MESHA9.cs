using ExtractHelper;

namespace ExtractDx11MESH.MESHs
{
	public class MESHA9 : MESH30
	{
		public MESHA9(byte[] fileData, int iPos)
			: base(fileData, iPos)
		{
		}

		protected override VertexList ReadVertexList(int numberofvertices)
		{
			VertexList vertexList = new VertexList();
			iPos += 4;
			iPos += 4;
			int num = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}           Number of Vertex Definitions: {1:x8}", iPos, num);
			iPos += 4;
			for (int i = 0; i < num; i++)
			{
				vertexList.VertexDefinitions.Add(ReadVertexDefinition());
			}
			iPos += 6;
			ColoredConsole.WriteLine("{0:x8}           Number of Vertices: {1:x8}", iPos, numberofvertices);
			for (int i = 0; i < numberofvertices; i++)
			{
				vertexList.Vertices.Add(ReadVertex(vertexList.VertexDefinitions));
			}
			return vertexList;
		}
	}
}
