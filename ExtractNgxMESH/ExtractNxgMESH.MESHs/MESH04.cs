using System;
using System.Collections.Generic;
using ExtractHelper;
using ExtractHelper.VariableTypes;

namespace ExtractNxgMESH.MESHs
{
	public class MESH04
	{
		protected float[] lookUp;

		public Dictionary<int, VertexList> Vertexlistsdictionary = new Dictionary<int, VertexList>();

		public Dictionary<int, List<ushort>> Indexlistsdictionary = new Dictionary<int, List<ushort>>();

		public List<Part> Parts = new List<Part>();

		protected byte[] fileData;

		protected int iPos;

		public int version;

		protected float[] LookUp
		{
			get
			{
				if (lookUp == null)
				{
					double num = 0.007874015748031496;
					lookUp = new float[256];
					lookUp[0] = -1f;
					for (int i = 1; i < 256; i++)
					{
						lookUp[i] = (float)((double)lookUp[i - 1] + num);
					}
					lookUp[127] = 0f;
					lookUp[255] = 1f;
				}
				return lookUp;
			}
		}

		public MESH04(byte[] fileData, int iPos)
		{
			this.fileData = fileData;
			this.iPos = iPos;
		}

		public virtual int Read(ref int referencecounter)
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

		protected virtual Part ReadPart(ref int referencecounter)
		{
			Part part = new Part();
			int num = BigEndianBitConverter.ToInt32(fileData, iPos);
			ColoredConsole.WriteLine("{0:x8}     Number of Vertex Lists: 0x{1:x8}", iPos, num);
			iPos += 4;
			for (int i = 0; i < num; i++)
			{
				ColoredConsole.WriteLine("{0:x8}       Vertex List 0x{1:x8}", iPos, i);
				part.VertexListReferences1.Add(GetVertexListReference(ref referencecounter, out var _));
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
			referencecounter++;
			byte lastByte = 0;
			iPos += 4;
			int num2 = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			if (num2 > 0)
			{
				ColoredConsole.Write("{0:x8}     ", iPos);
				for (int i = 0; i < num2; i++)
				{
					ColoredConsole.Write("{0:x2} ", fileData[iPos]);
					lastByte = fileData[iPos];
					iPos++;
				}
				ColoredConsole.WriteLine();
				referencecounter++;
			}
			int num3 = BigEndianBitConverter.ToInt32(fileData, iPos);
			iPos += 4;
			if (num3 != 0)
			{
				int num4 = ReadRelativePositionList(lastByte);
				referencecounter += num4;
			}
			return part;
		}

		protected virtual int ReadRelativePositionList(byte lastByte)
		{
			int num = BigEndianBitConverter.ToInt32(fileData, iPos);
			if (num != 0)
			{
				ColoredConsole.WriteLineError("{0:x8}       Relative Position Lists iUnk1 = {1:x8} (!= 0)", iPos, num);
			}
			iPos += 4;
			int num2 = 1;
			int num3 = 0;
			while (BigEndianBitConverter.ToInt32(fileData, iPos) != 0)
			{
				int num4 = BigEndianBitConverter.ToInt32(fileData, iPos);
				int num5 = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
				ColoredConsole.WriteLine("{0:x8}       Count 4 Number of Relative Position Lists: 0x{1:x8} 0x{2:x8}", iPos, num4, num5);
				iPos += 8;
				num2++;
			}
			ColoredConsole.WriteLine("{0:x8}       Number of Relative Position Lists: 0x{1:x8}", iPos, num2);
			num = BigEndianBitConverter.ToInt32(fileData, iPos);
			if (num != 0)
			{
				ColoredConsole.WriteLineError("{0:x8}       Relative Position Lists iUnk2 = {1:x8} (!= 0)", iPos, num);
			}
			iPos += 4;
			for (int i = 0; i < num2; i++)
			{
				int num6 = BigEndianBitConverter.ToInt32(fileData, iPos);
				iPos += 4;
				if (num6 != 0)
				{
					ColoredConsole.WriteLine("{0:x8}       Number of Relative Positions: 0x{1:x8}", iPos, num6);
					iPos += num6 * 12;
				}
				else
				{
					ColoredConsole.WriteLineError("{0:x8}       Number of Relative Positions unknown: 0x{1:x8}", iPos, 0);
				}
				if (lastByte == byte.MaxValue)
				{
					iPos += 5;
					num3++;
				}
				else
				{
					num = BigEndianBitConverter.ToInt16(fileData, iPos);
					if (num != 0)
					{
						ColoredConsole.WriteLineError("{0:x8}       Relative Position Lists iUnk3a = {1:x4} (!= 0)", iPos, num);
					}
					iPos += 2;
					num = BigEndianBitConverter.ToInt32(fileData, iPos);
					if (num != 1)
					{
						ColoredConsole.WriteLineError("{0:x8}       Relative Position Lists iUnk3b = {1:x8} (!= 1)", iPos, num);
					}
					else
					{
						num3++;
					}
					iPos += 4;
					int num7 = BigEndianBitConverter.ToInt32(fileData, iPos);
					ColoredConsole.WriteLine("{0:x8}       Number of Relative Position Tupels: 0x{1:x8}", iPos, num7);
					iPos += 4;
					iPos += 4 * num7;
					num3++;
				}
				num3++;
			}
			return num3;
		}

		protected virtual int GetIndexListReference(ref int referencecounter)
		{
			int num = -1;
			if (fileData[iPos] == 192)
			{
				num = BigEndianBitConverter.ToInt16(fileData, iPos + 2);
				iPos += 4;
				ColoredConsole.WriteLine("{0:x8}     Index List Reference to 0x{1:x4}", iPos, num);
				iPos += 4;
			}
			else
			{
				ColoredConsole.WriteLine("{0:x8}         New Index List 0x{1:x4}", iPos, referencecounter);
				iPos += 4;
				iPos += 4;
				int num2 = BigEndianBitConverter.ToInt32(fileData, iPos);
				ColoredConsole.WriteLine("{0:x8}           Number of Indices: {1:x8}", iPos, num2);
				iPos += 4;
				iPos += 4;
				List<ushort> list = new List<ushort>();
				for (int i = 0; i < num2; i++)
				{
					list.Add(BigEndianBitConverter.ToUInt16(fileData, iPos));
					iPos += 2;
				}
				Indexlistsdictionary.Add(referencecounter, list);
				num = referencecounter++;
			}
			return num;
		}

		protected virtual VertexListReference GetVertexListReference(ref int referencecounter, out int offset)
		{
			int num = -1;
			if (fileData[iPos] == 192)
			{
				num = BigEndianBitConverter.ToInt16(fileData, iPos + 2);
				ColoredConsole.WriteLineWarn("{0:x8}         Vertex List Reference to 0x{1:x4}", iPos, num);
				iPos += 4;
				int num2 = BigEndianBitConverter.ToInt32(fileData, iPos);
				ColoredConsole.WriteLine("{0:x8}           Unknown 0x{1:x8}", iPos, num2);
				iPos += 4;
				offset = BigEndianBitConverter.ToInt32(fileData, iPos);
				ColoredConsole.WriteLine("{0:x8}           Offset 0x{1:x8}", iPos, offset);
				iPos += 4;
			}
			else
			{
				ColoredConsole.WriteLineWarn("{0:x8}         New Vertex List 0x{1:x4}", iPos, referencecounter);
				int num2 = BigEndianBitConverter.ToInt32(fileData, iPos);
				ColoredConsole.WriteLine("{0:x8}           Unknown 0x{1:x8}", iPos, num2);
				iPos += 4;
				num2 = BigEndianBitConverter.ToInt32(fileData, iPos);
				ColoredConsole.WriteLine("{0:x8}           Unknown 0x{1:x8}", iPos, num2);
				iPos += 4;
				int numberofvertices = BigEndianBitConverter.ToInt32(fileData, iPos);
				iPos += 4;
				VertexList value = ReadVertexList(numberofvertices);
				offset = BigEndianBitConverter.ToInt32(fileData, iPos);
				ColoredConsole.WriteLine("{0:x8}           Offset 0x{1:x8}", iPos, offset);
				iPos += 4;
				Vertexlistsdictionary.Add(referencecounter, value);
				num = referencecounter++;
			}
			VertexListReference vertexListReference = new VertexListReference();
			vertexListReference.GlobalOffset = 0;
			vertexListReference.Reference = num;
			return vertexListReference;
		}

		protected virtual VertexList ReadVertexList(int numberofvertices)
		{
			VertexList vertexList = new VertexList();
			//Console.WriteLine("1: {0:x8}", iPos);
			//Console.WriteLine("fileData: {0} at {1}", fileData[iPos], iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.position;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			//Console.WriteLine("2: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.normal;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			//Console.WriteLine("3: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.colorSet0;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			//Console.WriteLine("4: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.tangent;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			//Console.WriteLine("5: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.colorSet1;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			//Console.WriteLine("6: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.uvSet01;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			iPos += 2;
			//Console.WriteLine("7: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.uvSet2;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			iPos += 2;
			//Console.WriteLine("8: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.blendIndices0;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			//Console.WriteLine("9: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.blendWeight0;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			//Console.WriteLine("10: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.lightDirSet;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			//Console.WriteLine("11: {0:x8}", iPos);
			if (fileData[iPos] != 0)
			{
				VertexDefinition vertexDefinition = new VertexDefinition();
				vertexDefinition.Variable = VertexDefinition.VariableEnum.lightColSet;
				vertexDefinition.VariableType = (VertexDefinition.VariableTypeEnum)fileData[iPos];
				vertexList.VertexDefinitions.Add(vertexDefinition);
				ColoredConsole.WriteLine("{0:x8}             {1} {2}", iPos, vertexDefinition.VariableType.ToString(), vertexDefinition.Variable.ToString());
			}
			iPos += 2;
			iPos += 6;
			//Console.WriteLine("12: {0:x8}", iPos);
			ColoredConsole.WriteLine("{0:x8}           Number of Vertices: {1:x8}", iPos, numberofvertices);
			for (int i = 0; i < numberofvertices; i++)
			{
				vertexList.Vertices.Add(ReadVertex(vertexList.VertexDefinitions));
			}
			return vertexList;
		}

		protected virtual Vertex ReadVertex(List<VertexDefinition> vertexdefinitions)
		{
			Vertex vertex = new Vertex();
			foreach (VertexDefinition vertexdefinition in vertexdefinitions)
			{
				switch (vertexdefinition.Variable)
				{
				case VertexDefinition.VariableEnum.position:
					vertex.Position = (Vector3)ReadVariableValue(vertexdefinition.VariableType);
					break;
				case VertexDefinition.VariableEnum.normal:
					vertex.Normal = (Vector3)ReadVariableValue(vertexdefinition.VariableType);
					break;
				case VertexDefinition.VariableEnum.colorSet0:
					vertex.ColorSet0 = (Color4)ReadVariableValue(vertexdefinition.VariableType);
					break;
				case VertexDefinition.VariableEnum.colorSet1:
					vertex.ColorSet1 = (Color4)ReadVariableValue(vertexdefinition.VariableType);
					break;
				case VertexDefinition.VariableEnum.uvSet01:
					vertex.UVSet0 = (Vector2)ReadVariableValue(vertexdefinition.VariableType);
					break;
				case VertexDefinition.VariableEnum.tangent:
				case VertexDefinition.VariableEnum.unknown6:
				case VertexDefinition.VariableEnum.uvSet2:
				case VertexDefinition.VariableEnum.unknown8:
				case VertexDefinition.VariableEnum.blendIndices0:
				case VertexDefinition.VariableEnum.blendWeight0:
				case VertexDefinition.VariableEnum.unknown11:
				case VertexDefinition.VariableEnum.lightDirSet:
				case VertexDefinition.VariableEnum.lightColSet:
					ReadVariableValue(vertexdefinition.VariableType);
					break;
				default:
					throw new NotSupportedException(vertexdefinition.Variable.ToString());
				}
			}
			return vertex;
		}

		protected virtual object ReadVariableValue(VertexDefinition.VariableTypeEnum variabletype)
		{
			switch (variabletype)
			{
			case VertexDefinition.VariableTypeEnum.vec2float:
			{
				Vector2 vector6 = new Vector2();
				vector6.X = BigEndianBitConverter.ToSingle(fileData, iPos);
				vector6.Y = BigEndianBitConverter.ToSingle(fileData, iPos + 4);
				Vector2 result7 = vector6;
				iPos += 8;
				return result7;
			}
			case VertexDefinition.VariableTypeEnum.vec3float:
			{
				Vector3 vector5 = new Vector3();
				vector5.X = BigEndianBitConverter.ToSingle(fileData, iPos);
				vector5.Y = BigEndianBitConverter.ToSingle(fileData, iPos + 4);
				vector5.Z = BigEndianBitConverter.ToSingle(fileData, iPos + 8);
				Console.WriteLine("{0:x8}        X | Y | Z:         | {1} | {2} | {3}", iPos, vector5.X, vector5.Y, vector5.Z);
				Vector3 result6 = vector5;
				iPos += 12;
				return result6;
			}
			case VertexDefinition.VariableTypeEnum.vec4float:
			{
				Vector4 vector4 = new Vector4();
				vector4.X = BigEndianBitConverter.ToSingle(fileData, iPos);
				vector4.Y = BigEndianBitConverter.ToSingle(fileData, iPos + 4);
				vector4.Z = BigEndianBitConverter.ToSingle(fileData, iPos + 8);
				vector4.W = BigEndianBitConverter.ToHalf(fileData, iPos + 12);
				Vector4 result5 = vector4;
				iPos += 16;
				return result5;
			}
			case VertexDefinition.VariableTypeEnum.vec2half:
			{
				Vector2 vector3 = new Vector2();
				vector3.X = BigEndianBitConverter.ToHalf(fileData, iPos);
				vector3.Y = BigEndianBitConverter.ToHalf(fileData, iPos + 2);
				//Console.WriteLine("{0:x8}        X | Y:         | {1} | {2}", iPos, vector3.X, vector3.Y);
				Vector2 result4 = vector3;
				iPos += 4;
				return result4;
			}
			case VertexDefinition.VariableTypeEnum.vec4half:
			{
				Vector4 vector2 = new Vector4();
				vector2.X = BigEndianBitConverter.ToHalf(fileData, iPos);
				vector2.Y = BigEndianBitConverter.ToHalf(fileData, iPos + 2);
				vector2.Z = BigEndianBitConverter.ToHalf(fileData, iPos + 4);
				vector2.W = BigEndianBitConverter.ToHalf(fileData, iPos + 6);
				Vector4 result3 = vector2;
				iPos += 8;
				return result3;
			}
			case VertexDefinition.VariableTypeEnum.vec4char:
				iPos += 4;
				return 1;
			case VertexDefinition.VariableTypeEnum.vec4mini:
			{
				Vector4 vector = new Vector4();
				vector.X = LookUp[fileData[iPos]];
				//Console.WriteLine(iPos);
				//Console.WriteLine(fileData[iPos]);
				vector.Y = LookUp[fileData[iPos + 1]];
				vector.Z = LookUp[fileData[iPos + 2]];
				vector.W = LookUp[fileData[iPos + 3]];
				//Console.WriteLine("{0:x8}        X | Y | Z | W:         | {1} | {2} | {3} | {4}", iPos, vector.X, vector.Y, vector.Z, vector.W);
				Vector4 result2 = vector;
				iPos += 4;
				return result2;
			}
			case VertexDefinition.VariableTypeEnum.color4char:
			{
				Color4 color = new Color4();
				color.R = fileData[iPos];
				color.G = fileData[iPos + 1];
				color.B = fileData[iPos + 2];
				color.A = fileData[iPos + 3];
				//Console.WriteLine("{0:x8}        R | G | B | A:         | {1} | {2} | {3} | {4}", iPos, color.R, color.G, color.B, color.A);
				Color4 result = color;
				iPos += 4;
				return result;
			}
			default:
				throw new NotImplementedException(variabletype.ToString());
			}
		}
	}
}
