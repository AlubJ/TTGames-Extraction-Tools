using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExtractHelper;
using ExtractHelper.VariableTypes;

namespace ExtractPcGhg
{
	internal class ExtractPcGhg
	{
		internal class ImageMeta
		{
			public int Width;

			public int Height;

			public int Size;
		}

		private string directoryname;

		private string extention;

		private string filename;

		private string filenamewithoutextension;

		private string fullPath;

		private int pos = 0;

		private byte[] fileData;

		private bool extractMesh = false;

		private int absOffsetGSNH;

		private int absOffsetPNTR;

		private List<byte[]> vertexLists = new List<byte[]>();

		private List<byte[]> indexLists = new List<byte[]>();

		private List<Part> parts = new List<Part>();

		protected float[] lookUp;

		private bool nu20first = false;

		private StreamWriter SW;

		public int ExportFormat;

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
				string text = args[i];
				if (text != null && text == "-x")
				{
					extractMesh = true;
				}
			}

			SW = new StreamWriter(File.OpenWrite(filenamewithoutextension + "_OUTPUT.TXT"));

			ExportFormat = 0;
		}

		public void Extract()
		{
			FileInfo fileInfo = new FileInfo(fullPath);
			directoryname = fileInfo.DirectoryName;
			FileStream fileStream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			fileData = new byte[(int)fileInfo.Length];
			fileStream.Read(fileData, 0, (int)fileInfo.Length);
			fileStream.Close();
			if (fileData[0] == 78 && fileData[1] == 85 && fileData[2] == 50 && fileData[3] == 48)
			{
				nu20first = true;
			}
			else
			{
				int num = BitConverter.ToInt32(fileData, pos);
				pos += 4;
				short num2 = BitConverter.ToInt16(fileData, pos);
				pos += 2;
				string text = directoryname + "\\" + filenamewithoutextension + ".mtl";
				ColoredConsole.WriteLineDebug("CreateFile: {0}", text);

				SW.WriteLine("CreateFile: {0}", text);

				StreamWriter streamWriter = new StreamWriter(text);
				streamWriter.WriteLine("# " + filenamewithoutextension);
				for (int i = 0; i < num2; i++)
				{
					string path = ReadDDS(i);
					streamWriter.WriteLine("newmtl " + Path.GetFileNameWithoutExtension(path));
					streamWriter.WriteLine("    Ka 0.99609375 0.99609375 0.99609375");
					streamWriter.WriteLine("    Kd 0.99609375 0.99609375 0.99609375");
					streamWriter.WriteLine("    map_Ka " + Path.GetFullPath(path));
					streamWriter.WriteLine("    map_Kd " + Path.GetFullPath(path));
					streamWriter.WriteLine();
				}
				streamWriter.Close();
				short num3 = BitConverter.ToInt16(fileData, pos);
				pos += 2;
				for (int i = 0; i < num3; i++)
				{
					//ColoredConsole.WriteLineDebug("current pos {0}", pos);
					vertexLists.Add(ReadVertexList(ref pos));
				}
				short num4 = BitConverter.ToInt16(fileData, pos);
				pos += 2;
				for (int i = 0; i < num4; i++)
				{
					indexLists.Add(ReadIndexList(ref pos));
				}
				pos = num + 4;
			}
			ReadNU20();
			CreateObjFile();
			for (int i = 0; i < parts.Count; i++)
			{
				CreateObjFiles(parts[i], i);
			}
		}

		private void CreateObjFile()
		{
			switch (ExportFormat)
            {
				case 0:
					string OBJ = directoryname + "\\" + filenamewithoutextension + ".obj";
					ColoredConsole.WriteLineDebug("CreateFile: {0}", OBJ);

					SW.WriteLine("CreateFile: {0}", OBJ);

					StreamWriter OBJ_W = new StreamWriter(OBJ);
					OBJ_W.WriteLine("# " + filenamewithoutextension);
					for (int i = 0; i < parts.Count; i++)
					{
						WriteObjPart(OBJ_W, parts[i]);
					}
					OBJ_W.Close();
					break;
				case 1:
					string PLY = directoryname + "\\" + filenamewithoutextension + ".dae";
					ColoredConsole.WriteLineDebug("CreateFile: {0}", PLY);

					SW.WriteLine("CreateFile: {0}", PLY);

					StreamWriter PLY_W = new StreamWriter(PLY);
					PLY_W.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
					PLY_W.WriteLine("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
					PLY_W.WriteLine("  <asset>");
					PLY_W.WriteLine("    <contributor>");
					PLY_W.WriteLine("      <author>ExtractPCGHG</author>");
					PLY_W.WriteLine("      <authoring_tool>ExtractPCGHG v1.6</authoring_tool>");
					PLY_W.WriteLine("    </contributor>");
					PLY_W.WriteLine("    <created>{0}</created>", DateTime.Now.ToString());
					PLY_W.WriteLine("    <modified>{0}</modified>", DateTime.Now.ToString());
					PLY_W.WriteLine("    <unit name=\"meter\" meter=\"1\"/>");
					PLY_W.WriteLine("    <up_axis>Y_UP</up_axis>");
					PLY_W.WriteLine("  </asset>");
					PLY_W.WriteLine("  <library_geometries>");
					for (int i = 0; i < parts.Count; i++)
					{
						string MeshID = "Part" + i.ToString() + "-mesh";
						string MeshName = "Part" + i.ToString();
						PLY_W.WriteLine("    <geometry id=\"{0}\" name=\"{1}\">", MeshID, MeshName);
						PLY_W.WriteLine("      <mesh>");
						WritePLYPart(PLY_W, parts[i], MeshID, MeshName);
						PLY_W.WriteLine("      </mesh>");
						PLY_W.WriteLine("    </geometry>");
						
					}
					PLY_W.WriteLine("  </library_geometries>");
					PLY_W.WriteLine("  <library_visual_scenes>");
					PLY_W.WriteLine("    <visual_scene id=\"Scene\" name=\"Scene\">");
					for (int i = 0; i < parts.Count; i++)
					{
						string MeshID = "Part" + i.ToString();
						string MeshName = "Part" + i.ToString();
						PLY_W.WriteLine("      <node id=\"{0}\" name=\"{1}\" type=\"NODE\">", MeshID, MeshName);
						PLY_W.WriteLine("        <matrix sid=\"transform\">1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</matrix>");
						PLY_W.WriteLine("        <instance_geometry url=\"#{0}\" name=\"{1}\"/>", MeshID + "-mesh", MeshName);
						PLY_W.WriteLine("      </node>");

					}
					PLY_W.WriteLine("    </visual_scene>");
					PLY_W.WriteLine("  </library_visual_scenes>");
					PLY_W.WriteLine("  <scene>");
					PLY_W.WriteLine("    <instance_visual_scene url=\"#Scene\"/>");
					PLY_W.WriteLine("  </scene>");
					PLY_W.WriteLine("</COLLADA>");
					PLY_W.Close();
					break;
			}
		}

		private void WritePLYPart(StreamWriter streamwriter, Part part, string MeshID, string MeshName)
		{
			bool flag = true;
			bool flag2 = true;
			byte[] array = vertexLists[part.vertexList];
			byte[] value = indexLists[part.indexList];
			streamwriter.WriteLine("        <source id=\"{0}\">", MeshID + "-positions");
			streamwriter.Write("          <float_array id=\"{0}\" count=\"{1}\">", MeshID + "-positions-array", (part.numberOfVertices * 3).ToString());
			for (int i = part.offsetVertices; i < part.offsetVertices + part.numberOfVertices; i++)
			{
				Vector3 vector = new Vector3();
				vector.X = BitConverter.ToSingle(array, i * part.vertexSize);
				vector.Y = BitConverter.ToSingle(array, i * part.vertexSize + 4);
				vector.Z = BitConverter.ToSingle(array, i * part.vertexSize + 8);
				//ColoredConsole.WriteLineDebug("VX: {0}", i * part.vertexSize);
				//ColoredConsole.WriteLineDebug("VX: {0}", i * part.vertexSize + 4);
				//ColoredConsole.WriteLineDebug("VX: {0}", i * part.vertexSize + 8);
				streamwriter.Write($"{vector.X:0.000000} {vector.Y:0.000000} {vector.Z:0.000000} ".Replace(',', '.'));
			}
			streamwriter.Write("</float_array>\n");
			streamwriter.WriteLine("          <technique_common>");
			streamwriter.WriteLine("            <accessor source=\"#{0}\" count=\"{1}\" stride=\"3\">", MeshID + "-positions-array", part.numberOfVertices.ToString());
			streamwriter.WriteLine("              <param name=\"X\" type=\"float\"/>");
			streamwriter.WriteLine("              <param name=\"Y\" type=\"float\"/>");
			streamwriter.WriteLine("              <param name=\"Z\" type=\"float\"/>");
			streamwriter.WriteLine("            </accessor>");
			streamwriter.WriteLine("          </technique_common>");
			streamwriter.WriteLine("        </source>");
			streamwriter.Write("");

			streamwriter.WriteLine("        <source id=\"{0}\">", MeshID + "-normals");
			streamwriter.Write("          <float_array id=\"{0}\" count=\"{1}\">", MeshID + "-normals-array", (part.numberOfVertices * 3).ToString());

			for (int i = part.offsetVertices; i < part.offsetVertices + part.numberOfVertices; i++)
			{
				Vector3 vector2 = new Vector3();
				vector2.X = LookUp[array[i * part.vertexSize + 12]];
				vector2.Y = LookUp[array[i * part.vertexSize + 13]];
				vector2.Z = LookUp[array[i * part.vertexSize + 14]];
				streamwriter.Write($"{vector2.X:0.000000} {vector2.Y:0.000000} {vector2.Z:0.000000} ".Replace(',', '.'));
			}
			streamwriter.Write("</float_array>\n");
			streamwriter.WriteLine("          <technique_common>");
			streamwriter.WriteLine("            <accessor source=\"#{0}\" count=\"{1}\" stride=\"3\">", MeshID + "-normals-array", part.numberOfVertices.ToString());
			streamwriter.WriteLine("              <param name=\"X\" type=\"float\"/>");
			streamwriter.WriteLine("              <param name=\"Y\" type=\"float\"/>");
			streamwriter.WriteLine("              <param name=\"Z\" type=\"float\"/>");
			streamwriter.WriteLine("            </accessor>");
			streamwriter.WriteLine("          </technique_common>");
			streamwriter.WriteLine("        </source>");


			streamwriter.WriteLine("        <source id=\"{0}\">", MeshID + "-map-0");
			streamwriter.Write("          <float_array id=\"{0}\" count=\"{1}\">", MeshID + "-map-0-array", (part.numberOfVertices * 2).ToString());

			if (part.vertexSize != 24)
			{
				for (int i = part.offsetVertices; i < part.offsetVertices + part.numberOfVertices; i++)
				{
					Vector2 vector3 = new Vector2();
					if (part.vertexSize == 28)
					{
						vector3.X = BitConverter.ToSingle(array, i * part.vertexSize + 20);
						vector3.Y = BitConverter.ToSingle(array, i * part.vertexSize + 24);
						streamwriter.Write($"{vector3.X:0.000000} {vector3.Y:0.000000} ".Replace(',', '.'));
					}
					if (part.vertexSize == 36 || part.vertexSize == 44)
					{
						vector3.X = BitConverter.ToSingle(array, i * part.vertexSize + 28);
						vector3.Y = BitConverter.ToSingle(array, i * part.vertexSize + 32);
						streamwriter.Write($"{vector3.X:0.000000} {vector3.Y:0.000000} ".Replace(',', '.'));
					}
					else if (part.vertexSize == 32 || part.vertexSize == 40)
					{
						vector3.X = BitConverter.ToSingle(array, i * part.vertexSize + 24);
						vector3.Y = BitConverter.ToSingle(array, i * part.vertexSize + 28);
						streamwriter.Write($"{vector3.X:0.000000} {vector3.Y:0.000000} ".Replace(',', '.'));
					}
				}
			}
			streamwriter.Write("</float_array>\n");
			streamwriter.WriteLine("          <technique_common>");
			streamwriter.WriteLine("            <accessor source=\"#{0}\" count=\"{1}\" stride=\"2\">", MeshID + "-map-0-array", part.numberOfVertices.ToString());
			streamwriter.WriteLine("              <param name=\"S\" type=\"float\"/>");
			streamwriter.WriteLine("              <param name=\"T\" type=\"float\"/>");
			streamwriter.WriteLine("            </accessor>");
			streamwriter.WriteLine("          </technique_common>");
			streamwriter.WriteLine("        </source>");
			streamwriter.WriteLine("        <vertices id=\"{0}\">", MeshID + "-vertices");
			streamwriter.WriteLine("          <input semantic=\"POSITION\" source=\"#{0}\"/>", MeshID + "-positions");
			streamwriter.WriteLine("        </vertices>");
			streamwriter.WriteLine("        <triangles count=\"{0}\">", (part.numberOfIndices / 3).ToString());
			streamwriter.WriteLine("          <input semantic=\"VERTEX\" source=\"#{0}\" offset=\"0\"/>", MeshID + "-vertices");
			streamwriter.WriteLine("          <input semantic=\"NORMAL\" source=\"#{0}\" offset=\"1\"/>", MeshID + "-normals");
			streamwriter.WriteLine("          <input semantic=\"TEXCOORD\" source=\"#{0}\" offset=\"2\" set=\"0\"/>", MeshID + "-map-0");
			streamwriter.Write("          <p>");


			string format = "{0} {1} {2} ";
			for (int i = part.offsetIndices; i < part.offsetIndices + part.numberOfIndices; i++)
			{
				short num = BitConverter.ToInt16(value, i * 2);
				short num2 = BitConverter.ToInt16(value, i * 2 + 2);
				short num3 = BitConverter.ToInt16(value, i * 2 + 4);
				streamwriter.Write(string.Format(format, num, num2, num3));
				if (num != num2 && num2 != num3 && num3 != num)
				{
					//streamwriter.Write(string.Format(format, -(num - part.numberOfVertices), -(num2 - part.numberOfVertices), -(num3 - part.numberOfVertices)));
					//streamwriter.Write(string.Format(format, num, num2, num3));
				}
			}
			streamwriter.Write("</p>\n");
			streamwriter.WriteLine("        </triangles>");
		}

		private void WriteObjPart(StreamWriter streamwriter, Part part)
		{
			bool flag = true;
			bool flag2 = true;
			byte[] array = vertexLists[part.vertexList];
			byte[] value = indexLists[part.indexList];
			for (int i = part.offsetVertices; i < part.offsetVertices + part.numberOfVertices; i++)
			{
				Vector3 vector = new Vector3();
				vector.X = BitConverter.ToSingle(array, i * part.vertexSize);
				vector.Y = BitConverter.ToSingle(array, i * part.vertexSize + 4);
				vector.Z = BitConverter.ToSingle(array, i * part.vertexSize + 8);
				//ColoredConsole.WriteLineDebug("VX: {0}", i * part.vertexSize);
				//ColoredConsole.WriteLineDebug("VX: {0}", i * part.vertexSize + 4);
				//ColoredConsole.WriteLineDebug("VX: {0}", i * part.vertexSize + 8);
				streamwriter.WriteLine($"v {vector.X:0.000000} {vector.Y:0.000000} {vector.Z:0.000000} ".Replace(',', '.'));
			}
			for (int i = part.offsetVertices; i < part.offsetVertices + part.numberOfVertices; i++)
			{
				Vector3 vector2 = new Vector3();
				vector2.X = LookUp[array[i * part.vertexSize + 12]];
				vector2.Y = LookUp[array[i * part.vertexSize + 13]];
				vector2.Z = LookUp[array[i * part.vertexSize + 14]];
				streamwriter.WriteLine($"vn {vector2.X:0.000000} {vector2.Y:0.000000} {vector2.Z:0.000000} ".Replace(',', '.'));
			}
			if (part.vertexSize != 24)
			{
				for (int i = part.offsetVertices; i < part.offsetVertices + part.numberOfVertices; i++)
				{
					Vector2 vector3 = new Vector2();
					if (part.vertexSize == 28)
					{
						vector3.X = BitConverter.ToSingle(array, i * part.vertexSize + 20);
						vector3.Y = BitConverter.ToSingle(array, i * part.vertexSize + 24);
						streamwriter.WriteLine($"vt {vector3.X:0.000000} {vector3.Y:0.000000} ".Replace(',', '.'));
					}
					if (part.vertexSize == 36 || part.vertexSize == 44)
					{
						vector3.X = BitConverter.ToSingle(array, i * part.vertexSize + 28);
						vector3.Y = BitConverter.ToSingle(array, i * part.vertexSize + 32);
						streamwriter.WriteLine($"vt {vector3.X:0.000000} {vector3.Y:0.000000} ".Replace(',', '.'));
					}
					else if (part.vertexSize == 32 || part.vertexSize == 40)
					{
						vector3.X = BitConverter.ToSingle(array, i * part.vertexSize + 24);
						vector3.Y = BitConverter.ToSingle(array, i * part.vertexSize + 28);
						streamwriter.WriteLine($"vt {vector3.X:0.000000} {vector3.Y:0.000000} ".Replace(',', '.'));
					}
				}
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
			for (int i = part.offsetIndices; i < part.offsetIndices + part.numberOfIndices; i++)
			{
				short num = BitConverter.ToInt16(value, i * 2);
				short num2 = BitConverter.ToInt16(value, i * 2 + 2);
				short num3 = BitConverter.ToInt16(value, i * 2 + 4);
				if (num != num2 && num2 != num3 && num3 != num)
				{
					if (i % 2 == part.offsetIndices % 2)
					{
						streamwriter.WriteLine(string.Format(format, num - part.numberOfVertices, num2 - part.numberOfVertices, num3 - part.numberOfVertices));
					}
					else
					{
						streamwriter.WriteLine(string.Format(format, num3 - part.numberOfVertices, num2 - part.numberOfVertices, num - part.numberOfVertices));
					}
				}
			}
		}

		private void CreateObjFiles(Part part, int partnumber)
		{
			switch (ExportFormat)
			{
				case 0:
					string OBJ = directoryname + "\\" + filenamewithoutextension + $"{partnumber:0000}" + ".obj";
					ColoredConsole.WriteLineDebug("CreateFile: {0}", OBJ);

					SW.WriteLine("CreateFile: {0}", OBJ);

					StreamWriter OBJ_W = new StreamWriter(OBJ);
					OBJ_W.WriteLine("# " + filenamewithoutextension);
					WriteObjPart(OBJ_W, part);
					OBJ_W.Close();
					break;
				case 1:
					string PLY = directoryname + "\\" + filenamewithoutextension + $"{partnumber:0000}" + ".ply";
					ColoredConsole.WriteLineDebug("CreateFile: {0}", PLY);

					SW.WriteLine("CreateFile: {0}", PLY);

					StreamWriter PLY_W = new StreamWriter(PLY);
					PLY_W.WriteLine("ply");
					PLY_W.WriteLine("format ascii 1.0");
					PLY_W.WriteLine("comment " + filenamewithoutextension);
					PLY_W.WriteLine("element vertex " + part.numberOfVertices);
					PLY_W.WriteLine("property float x");
					PLY_W.WriteLine("property float y");
					PLY_W.WriteLine("property float z");
					PLY_W.WriteLine("property float nx");
					PLY_W.WriteLine("property float ny");
					PLY_W.WriteLine("property float nz");
					PLY_W.WriteLine("property float s");
					PLY_W.WriteLine("property float t");
					PLY_W.WriteLine("element face " + part.numberOfIndices);
					PLY_W.WriteLine("property list uchar uint vertex_indices");
					PLY_W.WriteLine("end_header");

					//WritePLYPart(PLY_W, part);
					PLY_W.Close();
					break;
			}
		}

		private void ReadNU20()
		{
			ColoredConsole.WriteLineInfo("{0:x8} NU20", pos);

			SW.WriteLine("{0:x8} NU20", pos);

			pos += 4;
			int num = -BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}   Size: {1}", pos, num);

			SW.WriteLine("{0:x8}   Size: {1}", pos, num);

			pos += 4;
			pos += 4;
			pos += 4;
			ReadHEAD();
			ReadNTBL();
			ReadMS00();
			ReadGSNH();
		}

		private void ReadGSNH()
		{
			pos = absOffsetGSNH;
			ColoredConsole.WriteLineInfo("{0:x8}   GSNH", pos);

			SW.WriteLine("{0:x8}   GSNH", pos);

			pos += 4;
			int num = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Size: {1}", pos, num);

			SW.WriteLine("{0:x8}     Size: {1}", pos, num);

			pos += 4;
			pos += 4;
			int num2 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     NumberOfImages: {1}", pos, num2);

			SW.WriteLine("{0:x8}     NumberOfImages: {1}", pos, num2);

			pos += 4;
			int num3 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     relOffsetImagesMeta: {1}", pos, num3);

			SW.WriteLine("{0:x8}     relOffsetImagesMeta: {1}", pos, num3);

			pos += 4;
			if (nu20first)
			{
				List<ImageMeta> list = readImageMetas(num3 + pos - 4, num2);
				absOffsetPNTR += 4;
				int num4 = BitConverter.ToInt32(fileData, absOffsetPNTR);
				absOffsetPNTR += num4;
				ColoredConsole.WriteLineDebug("{0:x8}     endPNTR: {1:x8}", pos, absOffsetPNTR);

				SW.WriteLine("{0:x8}     endPNTR: {1:x8}", pos, absOffsetPNTR);

				string text = directoryname + "\\" + filenamewithoutextension + ".mtl";
				ColoredConsole.WriteLineDebug("CreateFile: {0}", text);

				SW.WriteLine("CreateFile: {0}", text);

				StreamWriter streamWriter = new StreamWriter(text);
				streamWriter.WriteLine("# " + filenamewithoutextension);
				int num5 = 0;
				foreach (ImageMeta item in list)
				{
					string text2 = directoryname + "\\" + filenamewithoutextension + $"{num5++:0000}" + ".dds";
					ColoredConsole.WriteLineDebug("CreateFile: {0}", text2);

					SW.WriteLine("CreateFile: {0}", text2);

					FileStream fileStream = File.OpenWrite(text2);
					fileStream.Write(fileData, absOffsetPNTR, item.Size);
					fileStream.Close();
					absOffsetPNTR += item.Size;
					streamWriter.WriteLine("newmtl " + Path.GetFileNameWithoutExtension(text2));
					streamWriter.WriteLine("    Ka 0.99609375 0.99609375 0.99609375");
					streamWriter.WriteLine("    Kd 0.99609375 0.99609375 0.99609375");
					streamWriter.WriteLine("    map_Ka " + Path.GetFullPath(text2));
					streamWriter.WriteLine("    map_Kd " + Path.GetFullPath(text2));
					streamWriter.WriteLine();
				}
				streamWriter.Close();
				short num6 = BitConverter.ToInt16(fileData, absOffsetPNTR);
				ColoredConsole.WriteLineDebug("{0:x8}     NumberOfVertexLists: {1}", absOffsetPNTR, num6);

				SW.WriteLine("{0:x8}     NumberOfVertexLists: {1}", absOffsetPNTR, num6);

				absOffsetPNTR += 2;
				for (int i = 0; i < num6; i++)
				{
					vertexLists.Add(ReadVertexList(ref absOffsetPNTR));
				}
				short num7 = BitConverter.ToInt16(fileData, absOffsetPNTR);
				ColoredConsole.WriteLineDebug("{0:x8}     NumberOfIndexLists: {1}", absOffsetPNTR, num7);

				SW.WriteLine("{0:x8}     NumberOfIndexLists: {1}", absOffsetPNTR, num7);

				absOffsetPNTR += 2;
				for (int i = 0; i < num7; i++)
				{
					indexLists.Add(ReadIndexList(ref absOffsetPNTR));
				}
			}
			pos += 452;
			int num8 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     RelOffsetMeshMeta: {1:x8}", pos, num8);

			SW.WriteLine("{0:x8}     RelOffsetMeshMeta: {1:x8}", pos, num8);

			pos += num8;
			int num9 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     NumberOfVertexLists: {1:x8}", pos, num9);

			SW.WriteLine("{0:x8}     NumberOfVertexLists: {1:x8}", pos, num9);

			pos += 4;
			pos += 4;
			int num10 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     NumberOfIndexLists: {1:x8}", pos, num10);

			SW.WriteLine("{0:x8}     NumberOfIndexLists: {1:x8}", pos, num10);

			pos += 4;
			pos += 4;
			pos += 4;
			int num11 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     NumberOfParts: {1}", pos, num11);

			SW.WriteLine("{0:x8}     NumberOfParts: {1}", pos, num11);

			pos += 4;
			pos += 8;
			for (int i = 0; i < num11; i++)
			{
				parts.Add(ReadPart(pos + BitConverter.ToInt32(fileData, pos)));
				pos += 4;
			}
			for (int i = 0; i < num11; i++)
			{
			}
		}

		private List<ImageMeta> readImageMetas(int relPos, int numberOfImages)
		{
			List<ImageMeta> list = new List<ImageMeta>();
			for (int i = 0; i < numberOfImages; i++)
			{
				int num = BitConverter.ToInt32(fileData, relPos);
				ColoredConsole.WriteLine("{0:x8}       ImageMetaOffset = {1}", relPos, num);

				SW.WriteLine("{0:x8}       ImageMetaOffset = {1}", relPos, num);

				relPos += 4;
				list.Add(readImageMeta(relPos + num - 4));
			}
			return list;
		}

		private ImageMeta readImageMeta(int relPos)
		{
			int num = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteLine("{0:x8}       Width = {1}", relPos, num);

			SW.WriteLine("{0:x8}       Width = {1}", relPos, num);

			relPos += 4;
			int num2 = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteLine("{0:x8}       Height = {1}", relPos, num2);

			SW.WriteLine("{0:x8}       Height = {1}", relPos, num2);

			relPos += 4;
			relPos += 16;
			relPos += 44;
			int num3 = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteLine("{0:x8}       Size = {1}", relPos, num3);

			SW.WriteLine("{0:x8}       Size = {1}", relPos, num3);

			relPos += 4;
			ImageMeta imageMeta = new ImageMeta();
			imageMeta.Width = num;
			imageMeta.Height = num2;
			imageMeta.Size = num3;
			return imageMeta;
		}

		int PartNum = 0;
		private Part ReadPart(int relPos)
		{
			ColoredConsole.Write("{0:x8}   Part {1}: \n", relPos, PartNum);

			SW.WriteLine("{0:x8}   Part {1}: ", relPos, PartNum);


			if (BitConverter.ToInt32(fileData, relPos) == 6)
			{
				ColoredConsole.Write("{0:x8}       Part:               Enabled  ({1})\n", relPos, BitConverter.ToInt32(fileData, relPos)); // 6 or 0
				SW.WriteLine("{0:x8}       Part:               Enabled  ({1})", relPos, BitConverter.ToInt32(fileData, relPos));
			} else
            {
				ColoredConsole.Write("{0:x8}       Part:               Disabled ({1})\n", relPos, BitConverter.ToInt32(fileData, relPos)); // 6 or 0
				SW.WriteLine("{0:x8}       Part:               Disabled ({1})", relPos, BitConverter.ToInt32(fileData, relPos));
			}

			relPos += 4;
			int num = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteInfo("{0:x8}       Number of Indices:  {1}\n", relPos, num);

			SW.WriteLine("{0:x8}       Number of Indices:  {1}", relPos, num);

			relPos += 4;
			short num2 = BitConverter.ToInt16(fileData, relPos);
			ColoredConsole.WriteWarn("{0:x8}       Vertex Size:        {1}\n", relPos, num2);

			SW.WriteLine("{0:x8}       Vertex Size:        {1}", relPos, num2);

			ColoredConsole.WriteWarn("{0:x8}       ", relPos);

			SW.Write("{0:x8}       Attached To Bone:   ", relPos);

			relPos += 2;
			ColoredConsole.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			SW.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			relPos++;
			ColoredConsole.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			SW.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			relPos++;
			ColoredConsole.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			SW.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			relPos++;
			ColoredConsole.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			SW.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			relPos++;
			ColoredConsole.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			SW.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			relPos++;
			ColoredConsole.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			SW.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			relPos++;
			ColoredConsole.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			SW.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			relPos++;
			ColoredConsole.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			SW.Write("{0:000} ", (byte)BitConverter.ToChar(fileData, relPos));

			relPos++;
			ColoredConsole.Write("{0} \n", BitConverter.ToInt16(fileData, relPos));

			SW.Write("{0} \n", BitConverter.ToInt16(fileData, relPos));

			relPos += 2;
			int num3 = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteInfo("{0:x8}       Offset Vertices:    {1}\n", relPos, num3);

			SW.WriteLine("{0:x8}       Offset Vertices:    {1}", relPos, num3);

			relPos += 4;
			int num4 = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteInfo("{0:x8}       Number of Vertices: {1}\n", relPos, num4);

			SW.WriteLine("{0:x8}       Number of Vertices: {1}", relPos, num4);

			relPos += 4;
			int num5 = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteInfo("{0:x8}       Offset Indices:     {1}\n", relPos, num5);

			SW.WriteLine("{0:x8}       Offset Indices:     {1}", relPos, num5);

			relPos += 4;
			int num6 = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.Write("{0:x8}       Index List:         {1}\n", relPos, num6);

			SW.WriteLine("{0:x8}       Index List:         {1}", relPos, num6);

			relPos += 4;
			int num7 = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteInfo("{0:x8}       Vertex List:        {1}\n", relPos, num7);

			SW.WriteLine("{0:x8}       Vertex List:        {1}", relPos, num7);

			relPos += 4;
			ColoredConsole.WriteInfo("{0:x8}       ", relPos);

			SW.Write("{0:x8}       Unknown:            ", relPos);

			for (int i = 0; i < 4; i++)
			{
				ColoredConsole.Write("{0} ", BitConverter.ToInt32(fileData, relPos));

				SW.Write("{0} ", BitConverter.ToInt32(fileData, relPos));

				relPos += 4;
			}

			SW.Write("\n");
			ColoredConsole.WriteLine();
			Part part = new Part();
			part.vertexSize = num2;
			part.numberOfIndices = num;
			part.offsetIndices = num5;
			part.numberOfVertices = num4;
			part.offsetVertices = num3;
			part.vertexList = num7;
			part.indexList = num6;

			PartNum++;

			return part;
		}

		private void ReadMS00()
		{
			ColoredConsole.WriteLineInfo("{0:x8}   MS00", pos);

			SW.WriteLine("{0:x8}   MS00", pos);

			pos += 4;
			int num = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Size: {1}", pos, num);

			SW.WriteLine("{0:x8}     Size: {1}", pos, num);

			pos += 4;
			int num2 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     NumberOfMaterials: {1}", pos, num2);

			SW.WriteLine("{0:x8}     NumberOfMaterials: {1}", pos, num2);

			pos += 4;
			pos += num - 12;
		}

		private void ReadHEAD()
		{
			ColoredConsole.WriteLineInfo("{0:x8}   HEAD", pos);

			SW.WriteLine("{0:x8}   HEAD", pos);

			pos += 4;
			int num = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Size: {1}", pos, num);

			SW.WriteLine("{0:x8}     Size: {1}", pos, num);

			pos += 4;
			int num2 = BitConverter.ToInt32(fileData, pos);
			absOffsetPNTR = pos + num2 - 8;
			ColoredConsole.WriteLineDebug("{0:x8}     RelativeOffsetPNTR: {1:x8} --> AbsoluteOffsetPNTR: {2:x8}", pos, num2, absOffsetPNTR);

			SW.WriteLine("{0:x8}     RelativeOffsetPNTR: {1:x8} --> AbsoluteOffsetPNTR: {2:x8}", pos, num2, absOffsetPNTR);

			pos += 4;
			int num3 = BitConverter.ToInt32(fileData, pos);
			absOffsetGSNH = pos + num3 - 8;
			ColoredConsole.WriteLineDebug("{0:x8}     RelativeOffsetGSNH: {1:x8} --> AbsoluteOffsetGSNH: {2:x8}", pos, num3, absOffsetGSNH);

			SW.WriteLine("{0:x8}     RelativeOffsetGSNH: {1:x8} --> AbsoluteOffsetGSNH: {2:x8}", pos, num3, absOffsetGSNH);

			pos += 4;
		}

		private void ReadNTBL()
		{
			ColoredConsole.WriteLineInfo("{0:x8}   NTBL", pos);

			SW.WriteLine("{0:x8}   NTBL", pos);

			pos += 4;
			int num = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Size: {1}", pos, num);

			SW.WriteLine("{0:x8}     Size: {1}", pos, num);

			pos += 4;
			pos += num - 8;
		}

		private byte[] ReadIndexList(ref int relPos)
		{
			ColoredConsole.WriteLineInfo("{0:x8}   new IndexList", relPos);

			SW.WriteLine("{0:x8}   new IndexList", relPos);

			int num = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteLineDebug("{0:x8}     Size: {1}", relPos, num);

			SW.WriteLine("{0:x8}     Size: {1}", relPos, num);

			relPos += 4;
			byte[] array = new byte[num];
			Array.Copy(fileData, relPos, array, 0, num);
			relPos += num;
			return array;
		}

		private byte[] ReadVertexList(ref int relPos)
		{
			ColoredConsole.WriteLineInfo("{0:x8}   new VertexList", relPos);

			SW.WriteLine("{0:x8}   new VertexList", relPos);

			int num = BitConverter.ToInt32(fileData, relPos);
			ColoredConsole.WriteLineDebug("{0:x8}     Size: {1}", relPos, num);

			SW.WriteLine("{0:x8}     Size: {1}", relPos, num);

			relPos += 4;
			byte[] array = new byte[num];
			Array.Copy(fileData, relPos, array, 0, num);
			relPos += num;
			return array;
		}

		private string ReadDDS(int i)
		{
			ColoredConsole.WriteLineInfo("{0:x8}   new Image", pos);

			SW.WriteLine("{0:x8}   new Image", pos);

			int num = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Height: {1}", pos, num);

			SW.WriteLine("{0:x8}     Height: {1}", pos, num);

			pos += 4;
			int num2 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Width: {1}", pos, num2);

			SW.WriteLine("{0:x8}     Width: {1}", pos, num2);

			pos += 4;
			int num3 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Meta: {1}", pos, num3);

			SW.WriteLine("{0:x8}     Meta: {1}", pos, num3);

			pos += 4;
			int num4 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Meta: {1}", pos, num4);

			SW.WriteLine("{0:x8}     Meta: {1}", pos, num4);

			pos += 4;
			int num5 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Meta: {1}", pos, num5);

			SW.WriteLine("{0:x8}     Meta: {1}", pos, num5);

			pos += 4;
			int num6 = BitConverter.ToInt32(fileData, pos);
			ColoredConsole.WriteLineDebug("{0:x8}     Size: {1}", pos, num6);

			SW.WriteLine("{0:x8}     Size: {1}", pos, num6);

			pos += 4;
			string text = directoryname + "\\" + filenamewithoutextension + $"{i:0000}" + ".dds";
			ColoredConsole.WriteLineDebug("CreateFile: {0}", text);


			SW.WriteLine("CreateFile: {0}", text);

			FileStream fileStream = File.OpenWrite(text);
			fileStream.Write(fileData, pos, num6);
			fileStream.Close();
			pos += num6;
			return text;
		}
	}
}
