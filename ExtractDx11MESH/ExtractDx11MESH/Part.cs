using System.Collections.Generic;

namespace ExtractDx11MESH
{
	public class Part
	{
		public List<int> VertexListReferences1 = new List<int>();

		public List<int> VertexListReferences2 = new List<int>();

		public int IndexListReference1;

		public int IndexListReference2;

		public int OffsetIndices;

		public int NumberIndices;

		public int OffsetVertices;

		public int NumberVertices;

		public int OffsetIndices2;

		public int OffsetVertices2;

		public int NumberVertices2;
	}
}
