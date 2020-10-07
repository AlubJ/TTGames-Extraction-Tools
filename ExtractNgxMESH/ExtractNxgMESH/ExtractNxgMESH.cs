using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExtractHelper;
using ExtractHelper.LDraw;
using ExtractHelper.VariableTypes;
using ExtractNxgMESH.DISPs;
using ExtractNxgMESH.IVL5;
using ExtractNxgMESH.MESHs;
using ExtractNxgMESH.TXGHs;

namespace ExtractNxgMESH
{
	public class ExtractNxgMESH
	{
		private string directoryname;

		private string extention;

		private string filename;

		private string filenamewithoutextension;

		private string fullPath;

		private int iPos = 0;

		private byte[] fileData;

		private int referencecounter = 7;

		private MESH04 mesh;

		private TXGH01 txgh;

		private IVL501 ivl5;

		private DISP15 disp;

		private bool extractMesh = true;

		private bool onlyInfo = false;

		public void ParseArgs(string[] args)
		{
			if (args.Count() < 1)
			{
				throw new ArgumentException("No argument handed over!");
			}
			if (!File.Exists(args[0]))
			{
				throw new ArgumentException($"File {args[0]} does not exist!");
			}
			directoryname = Path.GetDirectoryName(args[0]);
			extention = Path.GetExtension(args[0]);
			filename = Path.GetFileName(args[0]);
			filenamewithoutextension = Path.GetFileNameWithoutExtension(args[0]);
			fullPath = Path.GetFullPath(args[0]);
			if (!(extention.ToLower() == ".ghg") && !(extention.ToLower() == ".gsc"))
			{
				throw new ArgumentException("File extention != .ghg and != .gsc");
			}
			for (int i = 1; i < args.Length; i++)
			{
				switch (args[i])
				{
				case "-x":
					extractMesh = true;
					break;
				case "-i":
					onlyInfo = true;
					break;
				}
			}
		}

		public void Extract()
		{
			ColoredConsole.WriteLineInfo("{0:x8} {1}", iPos, fullPath);
			FileInfo fileInfo = new FileInfo(fullPath);
			directoryname = fileInfo.DirectoryName;
			FileStream fileStream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			fileData = new byte[(int)fileInfo.Length];
			fileStream.Read(fileData, 0, (int)fileInfo.Length);
			fileStream.Close();
			while (iPos < fileData.Length)
			{
				if (fileData[iPos] == 48 && fileData[iPos + 1] == 50 && fileData[iPos + 2] == 85 && fileData[iPos + 3] == 78)
				{
					int num = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
					ColoredConsole.WriteLineInfo("{0:x8} NU20 Version 0x{1:x2}", iPos, num);
					iPos += 4;
					iPos += 4;
				}
				else if (fileData[iPos] == 79 && fileData[iPos + 1] == 70 && fileData[iPos + 2] == 78 && fileData[iPos + 3] == 73)
				{
					int num = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
					ColoredConsole.WriteLineInfo("{0:x8} INFO Version 0x{1:x2}", iPos, num);
					iPos += 4;
					iPos += 4;
				}
				else if (fileData[iPos] == 76 && fileData[iPos + 1] == 66 && fileData[iPos + 2] == 84 && fileData[iPos + 3] == 78)
				{
					int num = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
					ColoredConsole.WriteLineInfo("{0:x8} NTBL Version 0x{1:x2}", iPos, num);
					iPos += 4;
					iPos += 4;
				}
				else if (fileData[iPos] == 83 && fileData[iPos + 1] == 68 && fileData[iPos + 2] == 78 && fileData[iPos + 3] == 66)
				{
					int num = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
					ColoredConsole.WriteLineInfo("{0:x8} BNDS Version 0x{1:x2}", iPos, num);
					iPos += 4;
					iPos += 4;
				}
				else if (fileData[iPos] == 72 && fileData[iPos + 1] == 83 && fileData[iPos + 2] == 69 && fileData[iPos + 3] == 77)
				{
					int num = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
					ColoredConsole.WriteLineInfo("{0:x8} MESH Version 0x{1:x2}", iPos, num);
					iPos += 4;
					iPos += 4;
					if (!onlyInfo)
					{
						readMESH(num);
					}
				}
				else if (fileData[iPos] == 72 && fileData[iPos + 1] == 71 && fileData[iPos + 2] == 88 && fileData[iPos + 3] == 84)
				{
					int num = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
					ColoredConsole.WriteLineInfo("{0:x8} TXGH Version 0x{1:x2}", iPos, num);
					iPos += 4;
					iPos += 4;
					if (!onlyInfo)
					{
						readTXGH(num);
					}
				}
				else if (fileData[iPos] == 53 && fileData[iPos + 1] == 76 && fileData[iPos + 2] == 86 && fileData[iPos + 3] == 73)
				{
					int num = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
					ColoredConsole.WriteLineInfo("{0:x8} IVL5 Version 0x{1:x2}", iPos, num);
					iPos += 4;
					iPos += 4;
				}
				else if (fileData[iPos] == 76 && fileData[iPos + 1] == 79 && fileData[iPos + 2] == 71 && fileData[iPos + 3] == 72)
				{
					int num = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
					ColoredConsole.WriteLineInfo("{0:x8} HGOL Version 0x{1:x2}", iPos, num);
					iPos += 4;
					iPos += 4;
				}
				else if (fileData[iPos] == 80 && fileData[iPos + 1] == 83 && fileData[iPos + 2] == 73 && fileData[iPos + 3] == 68)
				{
					int num = BigEndianBitConverter.ToInt32(fileData, iPos + 4);
					ColoredConsole.WriteLineInfo("{0:x8} DISP Version 0x{1:x2}", iPos, num);
					iPos += 4;
					iPos += 4;
					if (!onlyInfo)
					{
						readDISP(num);
					}
				}
				else
				{
					iPos++;
				}
			}
		}

		private void readIVL5(int version)
		{
			if (version == 1)
			{
				ivl5 = new IVL501(fileData, iPos);
				iPos = ivl5.Read();
			}
			else
			{
				ColoredConsole.WriteLineError("Not Yet Supported: IVL5 Version {0:x2}", version);
			}
		}

		private void readDISP(int version)
		{
			switch (version)
			{
			case 21:
				disp = new DISP15(fileData, iPos);
				break;
			case 23:
				disp = new DISP17(fileData, iPos);
				break;
			case 33:
				disp = new DISP21(fileData, iPos);
				break;
			default:
				ColoredConsole.WriteLineError("Not Yet Supported: DISP Version {0:x2}", version);
				return;
			}
			iPos = disp.Read();
		}

		private void readMESH(int version)
		{
			switch (version)
			{
			case 4:
				mesh = new MESH04(fileData, iPos);
				break;
			case 5:
				mesh = new MESH05(fileData, iPos);
				break;
			case 46:
				mesh = new MESH2E(fileData, iPos);
				break;
			case 47:
				mesh = new MESH2F(fileData, iPos);
				break;
			case 48:
				mesh = new MESH30(fileData, iPos);
				break;
			case 169:
				mesh = new MESHA9(fileData, iPos);
				break;
			case 170:
				mesh = new MESHAA(fileData, iPos);
				referencecounter = 5;
				break;
			case 175:
				mesh = new MESHAF(fileData, iPos);
				referencecounter = 5;
				break;
			default:
				ColoredConsole.WriteLineError("Not Yet Supported: MESH Version {0:x2}", version);
				return;
			}
			iPos = mesh.Read(ref referencecounter);
			int num = 0;
			bool flag = true;
			foreach (Part part in mesh.Parts)
			{
				if (extractMesh)
				{
					CreateObjFile(part, num++);
				}
				else
				{
					CheckData(part);
				}
			}
		}

		private void readTXGH(int version)
		{
			switch (version)
			{
			case 1:
				referencecounter = 9;
				txgh = new TXGH01(fileData, iPos);
				break;
			case 3:
				referencecounter = 9;
				txgh = new TXGH03(fileData, iPos);
				break;
			case 4:
				referencecounter = 9;
				txgh = new TXGH04(fileData, iPos);
				break;
			case 5:
				referencecounter = 9;
				txgh = new TXGH05(fileData, iPos);
				break;
			case 6:
				referencecounter = 9;
				txgh = new TXGH06(fileData, iPos);
				break;
			case 7:
				referencecounter = 9;
				txgh = new TXGH07(fileData, iPos);
				break;
			case 8:
				referencecounter = 7;
				txgh = new TXGH08(fileData, iPos);
				break;
			case 9:
				referencecounter = 7;
				txgh = new TXGH09(fileData, iPos);
				break;
			case 10:
				txgh = new TXGH0A(fileData, iPos);
				break;
			case 12:
				txgh = new TXGH0C(fileData, iPos);
				break;
			default:
				ColoredConsole.WriteLineError("Not Yet Supported: TXGH Version {0:x2}", version);
				return;
			}
			iPos = txgh.Read(ref referencecounter);
		}

		private void CheckData(Part part)
		{
			bool flag = false;
			bool flag2 = false;
			List<Vertex> list = null;
			VertexList vertexList = mesh.Vertexlistsdictionary[part.VertexListReferences1[0].Reference];
			VertexList vertexList2 = null;
			if (part.VertexListReferences1.Count > 1)
			{
				vertexList2 = mesh.Vertexlistsdictionary[part.VertexListReferences1[1].Reference];
			}
			List<ushort> list2 = mesh.Indexlistsdictionary[part.IndexListReference1];
			if (vertexList.Vertices[0].Position != null)
			{
				for (int i = part.OffsetVertices; i < part.OffsetVertices + part.NumberVertices; i++)
				{
					Vector3 position = vertexList.Vertices[i].Position;
				}
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].Position != null)
			{
				for (int i = part.OffsetVertices2; i < part.OffsetVertices2 + part.NumberVertices; i++)
				{
					Vector3 position = vertexList2.Vertices[i].Position;
				}
			}
			if (vertexList.Vertices[0].UVSet0 != null)
			{
				flag2 = true;
				for (int i = part.OffsetVertices; i < part.OffsetVertices + part.NumberVertices; i++)
				{
					Vector2 uVSet = vertexList.Vertices[i].UVSet0;
				}
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].UVSet0 != null)
			{
				flag2 = true;
				for (int i = part.OffsetVertices2; i < part.OffsetVertices2 + part.NumberVertices; i++)
				{
					Vector2 uVSet = vertexList2.Vertices[i].UVSet0;
				}
			}
			if (vertexList.Vertices[0].Normal != null)
			{
				flag = true;
				for (int i = part.OffsetVertices; i < part.OffsetVertices + part.NumberVertices; i++)
				{
					Vector3 normal = vertexList.Vertices[i].Normal;
				}
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].Normal != null)
			{
				flag = true;
				for (int i = part.OffsetVertices2; i < part.OffsetVertices2 + part.NumberVertices; i++)
				{
					Vector3 normal = vertexList2.Vertices[i].Normal;
				}
			}
			if (vertexList.Vertices[0].ColorSet0 != null)
			{
				list = vertexList.Vertices;
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].ColorSet0 != null)
			{
				list = vertexList2.Vertices;
			}
		}

		private void CreateDatFile(Part part, int partnumber)
		{
			float scale = 262f;
			string newfilename = directoryname + "\\" + filenamewithoutextension + $"{partnumber:0000}" + ".dat";
			VertexList vertexList = mesh.Vertexlistsdictionary[part.VertexListReferences1[0].Reference];
			int offsetN = part.OffsetVertices;
			if (vertexList.Vertices[0].Normal == null && part.VertexListReferences1.Count > 1)
			{
				vertexList = mesh.Vertexlistsdictionary[part.VertexListReferences1[1].Reference];
				offsetN = part.OffsetVertices2;
			}
			List<ushort> indexList = mesh.Indexlistsdictionary[part.IndexListReference1];
			VertexList vertexList2 = mesh.Vertexlistsdictionary[part.VertexListReferences1[0].Reference];
			int offsetP = part.OffsetVertices;
			if (vertexList2.Vertices[0].Position == null && part.VertexListReferences1.Count > 1)
			{
				vertexList2 = mesh.Vertexlistsdictionary[part.VertexListReferences1[1].Reference];
				offsetP = part.OffsetVertices2;
			}
			if (vertexList2.Vertices[0].Position == null && part.VertexListReferences11 != null)
			{
				for (int i = 0; i < part.VertexListReferences11.Count; i++)
				{
					newfilename = directoryname + "\\" + filenamewithoutextension + $"{partnumber:0000}.{i:000}" + ".dat";
					vertexList2 = mesh.Vertexlistsdictionary[part.VertexListReferences11[i].Reference];
					WriteDatFile(part, vertexList2, vertexList, newfilename, indexList, scale, offsetP, offsetN);
				}
			}
			else
			{
				WriteDatFile(part, vertexList2, vertexList, newfilename, indexList, scale, offsetP, offsetN);
			}
		}

		private void WriteDatFile(Part part, VertexList vertexListP, VertexList vertexListN, string newfilename, List<ushort> IndexList, float scale, int offsetP, int offsetN)
		{
			OptionalLines optionalLines = new OptionalLines();
			StreamWriter streamWriter = new StreamWriter(newfilename);
			DateTime now = DateTime.Now;
			string str = $"{now.Year:0000}-{now.Month:00}-{now.Day:00}";
			streamWriter.WriteLine("0 " + Path.GetFileName(newfilename) + " (Needs Work)");
			streamWriter.WriteLine("0 Name: " + Path.GetFileNameWithoutExtension(newfilename));
			streamWriter.WriteLine("0 Author: <AuthorRealName> [<AuthorLDrawName>]");
			streamWriter.WriteLine("0 !LDRAW_ORG Unofficial_Part");
			streamWriter.WriteLine("0 !LICENSE Redistributable under CCAL version 2.0 : see CAreadme.txt");
			streamWriter.WriteLine();
			streamWriter.WriteLine("0 BFC CERTIFY CCW");
			streamWriter.WriteLine();
			streamWriter.WriteLine("0 !HISTORY " + str + " {TtGames} Original part shape");
			streamWriter.WriteLine("0 !HISTORY " + str + " [<AuthorLDrawName>] File preparation for LDraw Parts Tracker");
			streamWriter.WriteLine();
			streamWriter.WriteLine("0 // Needs Work: Clean necessary");
			streamWriter.WriteLine("0 // Scale and Origin are not right");
			streamWriter.WriteLine();
			if (vertexListP.Vertices[0].Position != null)
			{
				Console.WriteLine("OffsetVertices: {0:x8}, OffsetNormals: {1:x8}, NumberIndices: {2:x8}", offsetP, offsetN, part.NumberIndices);
				for (int i = 0; i < part.NumberIndices; i += 3)
				{
					streamWriter.WriteLine("3 16 " + vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i]].Position.ToString(scale) + " " + vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 1]].Position.ToString(scale) + " " + vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 2]].Position.ToString(scale));
					streamWriter.WriteLine("2 24 " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i]].Position.ToString(scale) + " " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i]].Position.X * scale + vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i]].Normal.X) + " " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i]].Position.Y * scale + vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i]].Normal.Y) + " " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i]].Position.Z * scale + vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i]].Normal.Z)).Replace(',', '.'));
					streamWriter.WriteLine("2 24 " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 1]].Position.ToString(scale) + " " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 1]].Position.X * scale + vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 1]].Normal.X) + " " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 1]].Position.Y * scale + vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 1]].Normal.Y) + " " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 1]].Position.Z * scale + vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 1]].Normal.Z)).Replace(',', '.'));
					streamWriter.WriteLine("2 24 " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 2]].Position.ToString(scale) + " " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 2]].Position.X * scale + vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 2]].Normal.X) + " " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 2]].Position.Y * scale + vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 2]].Normal.Y) + " " + (vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 2]].Position.Z * scale + vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 2]].Normal.Z)).Replace(',', '.'));
					optionalLines.Add(new OptionalLine(vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i]].Position, vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 1]].Position, vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 2]].Position, vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i]].Normal, vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 1]].Normal));
					optionalLines.Add(new OptionalLine(vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 1]].Position, vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 2]].Position, vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i]].Position, vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 1]].Normal, vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 2]].Normal));
					optionalLines.Add(new OptionalLine(vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 2]].Position, vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i]].Position, vertexListP.Vertices[offsetP + IndexList[part.OffsetIndices + i + 1]].Position, vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i + 2]].Normal, vertexListN.Vertices[offsetN + IndexList[part.OffsetIndices + i]].Normal));
				}
			}
			foreach (OptionalLine item in optionalLines)
			{
				if (item.B == null)
				{
					streamWriter.Write("2 " + 24 + " ");
					streamWriter.Write(item.X.ToString(scale));
					streamWriter.Write(item.Y.ToString(scale));
					streamWriter.WriteLine();
				}
				else
				{
					streamWriter.Write("5 " + 24 + " ");
					streamWriter.Write(item.X.ToString(scale));
					streamWriter.Write(item.Y.ToString(scale));
					streamWriter.Write(item.A.ToString(scale));
					streamWriter.Write(item.B.ToString(scale));
					streamWriter.WriteLine();
				}
			}
			streamWriter.Close();
		}

		private void CreateObjFile(Part part, int partnumber)
		{
			VertexList vertexList = mesh.Vertexlistsdictionary[part.VertexListReferences1[0].Reference];
			VertexList vertexList2 = null;
			if (part.VertexListReferences1.Count > 1)
			{
				vertexList2 = mesh.Vertexlistsdictionary[part.VertexListReferences1[1].Reference];
			}
			List<ushort> indexList = mesh.Indexlistsdictionary[part.IndexListReference1];
			StringBuilder stringBuilder = CreateObjFileSub(part, vertexList, vertexList2, indexList, partnumber);
			if (vertexList.Vertices[0].Position != null)
			{
				Console.WriteLine("part.OffsetVertices: {0:x8}", part.OffsetVertices);
				string path = directoryname + "\\" + filenamewithoutextension + $"{partnumber:0000}" + ".obj";
				StreamWriter streamWriter = new StreamWriter(path);
				streamWriter.WriteLine("# " + filenamewithoutextension);
				for (int i = part.OffsetVertices; i < part.OffsetVertices + part.NumberVertices; i++)
				{
					Vector3 position = vertexList.Vertices[i].Position;
					streamWriter.WriteLine($"v {position.X:0.000000} {position.Y:0.000000} {position.Z:0.000000} ".Replace(',', '.'));
				}
				streamWriter.Write(stringBuilder.ToString());
				streamWriter.Close();
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].Position != null)
			{
				Console.WriteLine("part.OffsetVertices2: {0:x8}", part.OffsetVertices2);
				string path = directoryname + "\\" + filenamewithoutextension + $"{partnumber:0000}" + ".obj";
				StreamWriter streamWriter = new StreamWriter(path);
				streamWriter.WriteLine("# " + filenamewithoutextension);
				for (int i = part.OffsetVertices2; i < part.OffsetVertices2 + part.NumberVertices; i++)
				{
					Vector3 position = vertexList2.Vertices[i].Position;
					streamWriter.WriteLine($"v {position.X:0.000000} {position.Y:0.000000} {position.Z:0.000000} ".Replace(',', '.'));
				}
				streamWriter.Write(stringBuilder.ToString());
				streamWriter.Close();
			}
			else
			{
				if (part.VertexListReferences11 == null)
				{
					return;
				}
				for (int j = 0; j < part.VertexListReferences11.Count; j++)
				{
					Console.WriteLine("part.OffsetVertices11: {0:x8}", j);
					string path = directoryname + "\\" + filenamewithoutextension + $"{partnumber:0000}.{j:000}" + ".obj";
					StreamWriter streamWriter = new StreamWriter(path);
					streamWriter.WriteLine("# " + filenamewithoutextension);
					VertexList vertexList3 = mesh.Vertexlistsdictionary[part.VertexListReferences11[j].Reference];
					for (int i = 0; i < vertexList3.Vertices.Count; i++)
					{
						Vector3 position = vertexList3.Vertices[i].Position;
						streamWriter.WriteLine($"v {position.X:0.000000} {position.Y:0.000000} {position.Z:0.000000} ".Replace(',', '.'));
					}
					streamWriter.Write(stringBuilder.ToString());
					streamWriter.Close();
				}
			}
		}

		private StringBuilder CreateObjFileSub(Part part, VertexList vertexList1, VertexList vertexList2, List<ushort> IndexList, int partnumber)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			bool flag2 = false;
			List<Vertex> list = null;
			if (vertexList1.Vertices[0].UVSet0 != null)
			{
				flag2 = true;
				for (int i = part.OffsetVertices; i < part.OffsetVertices + part.NumberVertices; i++)
				{
					Vector2 uVSet = vertexList1.Vertices[i].UVSet0;
					stringBuilder.AppendLine($"vt {uVSet.X:0.000000} {uVSet.Y:0.000000} ".Replace(',', '.'));
				}
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].UVSet0 != null)
			{
				flag2 = true;
				for (int i = part.OffsetVertices2; i < part.OffsetVertices2 + part.NumberVertices; i++)
				{
					Vector2 uVSet = vertexList2.Vertices[i].UVSet0;
					stringBuilder.AppendLine($"vt {uVSet.X:0.000000} {uVSet.Y:0.000000} ".Replace(',', '.'));
				}
			}
			if (vertexList1.Vertices[0].Normal != null)
			{
				flag = true;
				for (int i = part.OffsetVertices; i < part.OffsetVertices + part.NumberVertices; i++)
				{
					Vector3 normal = vertexList1.Vertices[i].Normal;
					stringBuilder.AppendLine($"vn {normal.X:0.000000} {normal.Y:0.000000} {normal.Z:0.000000} ".Replace(',', '.'));
				}
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].Normal != null)
			{
				flag = true;
				for (int i = part.OffsetVertices2; i < part.OffsetVertices2 + part.NumberVertices; i++)
				{
					Vector3 normal = vertexList2.Vertices[i].Normal;
					stringBuilder.AppendLine($"vn {normal.X:0.000000} {normal.Y:0.000000} {normal.Z:0.000000} ".Replace(',', '.'));
				}
			}
			if (vertexList1.Vertices[0].ColorSet0 != null)
			{
				list = vertexList1.Vertices;
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].ColorSet0 != null)
			{
				list = vertexList2.Vertices;
			}
			string format = "f {0} {1} {2}";
			if (flag2)
			{
				format = ((!flag) ? "f {0}/{0} {1}/{1} {2}/{2}" : "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}");
			}
			else if (flag)
			{
				format = "f {0}//{0} {1}//{1} {2}//{2}";
			}
			stringBuilder.AppendLine("mtllib " + filenamewithoutextension + $"{partnumber:0000}" + ".mtl");
			string path = directoryname + "\\" + filenamewithoutextension + $"{partnumber:0000}" + ".mtl";
			StreamWriter streamWriter = new StreamWriter(path);
			string a = "";
			List<string> list2 = new List<string>();
			for (int i = part.OffsetIndices; i < part.OffsetIndices + part.NumberIndices; i += 3)
			{
				if (list != null)
				{
					Color4 colorSet = list[part.OffsetVertices + IndexList[i]].ColorSet0;
					string text = $"{colorSet.R}_{colorSet.G}_{colorSet.B}_{colorSet.A}";
					if (a != text)
					{
						if (!list2.Contains(text))
						{
							streamWriter.WriteLine("newmtl mtl_" + text);
							string str = $"{(double)colorSet.R / 256.0} {(double)colorSet.G / 256.0} {(double)colorSet.B / 256.0}".Replace(',', '.');
							streamWriter.WriteLine("   Ka " + str);
							streamWriter.WriteLine("   Kd " + str);
							streamWriter.WriteLine();
							list2.Add(text);
						}
						stringBuilder.AppendLine("usemtl mtl_" + text);
						a = text;
					}
				}
				stringBuilder.AppendLine(string.Format(format, IndexList[i] - part.NumberVertices, IndexList[i + 1] - part.NumberVertices, IndexList[i + 2] - part.NumberVertices));
			}
			streamWriter.Close();
			return stringBuilder;
		}

		private void WriteIntoObjFile(Part part, StreamWriter streamwriter)
		{
			bool flag = false;
			bool flag2 = false;
			List<Vertex> list = null;
			VertexList vertexList = mesh.Vertexlistsdictionary[part.VertexListReferences1[0].Reference];
			VertexList vertexList2 = null;
			if (part.VertexListReferences1.Count > 1)
			{
				vertexList2 = mesh.Vertexlistsdictionary[part.VertexListReferences1[1].Reference];
			}
			List<ushort> list2 = mesh.Indexlistsdictionary[part.IndexListReference1];
			if (vertexList.Vertices[0].Position != null)
			{
				for (int i = part.OffsetVertices; i < part.OffsetVertices + part.NumberVertices; i++)
				{
					Vector3 position = vertexList.Vertices[i].Position;
					streamwriter.WriteLine($"v {position.X:0.000000} {position.Y:0.000000} {position.Z:0.000000} ".Replace(',', '.'));
				}
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].Position != null)
			{
				for (int i = part.OffsetVertices2; i < part.OffsetVertices2 + part.NumberVertices; i++)
				{
					Vector3 position = vertexList2.Vertices[i].Position;
					streamwriter.WriteLine($"v {position.X:0.000000} {position.Y:0.000000} {position.Z:0.000000} ".Replace(',', '.'));
				}
			}
			if (vertexList.Vertices[0].UVSet0 != null)
			{
				flag2 = true;
				for (int i = part.OffsetVertices; i < part.OffsetVertices + part.NumberVertices; i++)
				{
					Vector2 uVSet = vertexList.Vertices[i].UVSet0;
					streamwriter.WriteLine($"vt {uVSet.X:0.000000} {uVSet.Y:0.000000} ".Replace(',', '.'));
				}
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].UVSet0 != null)
			{
				flag2 = true;
				for (int i = part.OffsetVertices2; i < part.OffsetVertices2 + part.NumberVertices; i++)
				{
					Vector2 uVSet = vertexList2.Vertices[i].UVSet0;
					streamwriter.WriteLine($"vt {uVSet.X:0.000000} {uVSet.Y:0.000000} ".Replace(',', '.'));
				}
			}
			if (vertexList.Vertices[0].Normal != null)
			{
				flag = true;
				for (int i = part.OffsetVertices; i < part.OffsetVertices + part.NumberVertices; i++)
				{
					Vector3 normal = vertexList.Vertices[i].Normal;
					streamwriter.WriteLine($"vn {normal.X:0.000000} {normal.Y:0.000000} {normal.Z:0.000000} ".Replace(',', '.'));
				}
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].Normal != null)
			{
				flag = true;
				for (int i = part.OffsetVertices2; i < part.OffsetVertices2 + part.NumberVertices; i++)
				{
					Vector3 normal = vertexList2.Vertices[i].Normal;
					streamwriter.WriteLine($"vn {normal.X:0.000000} {normal.Y:0.000000} {normal.Z:0.000000} ".Replace(',', '.'));
				}
			}
			if (vertexList.Vertices[0].ColorSet0 != null)
			{
				list = vertexList.Vertices;
			}
			else if (vertexList2 != null && vertexList2.Vertices[0].ColorSet0 != null)
			{
				list = vertexList2.Vertices;
			}
			string format = "f {0} {1} {2}";
			if (flag2)
			{
				format = ((!flag) ? "f {0}/{0} {1}/{1} {2}/{2}" : "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}");
			}
			else if (flag)
			{
				format = "f {0}//{0} {1}//{1} {2}//{2}";
			}
			for (int i = part.OffsetIndices; i < part.OffsetIndices + part.NumberIndices; i += 3)
			{
				streamwriter.WriteLine(string.Format(format, list2[i] - part.NumberVertices, list2[i + 1] - part.NumberVertices, list2[i + 2] - part.NumberVertices));
			}
		}
	}
}
