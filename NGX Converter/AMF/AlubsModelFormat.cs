using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AMF
{
    class AlubsModelFormat
    {
        public bool CreateModel(List<float[]> vertex, List<float[]> vertexTextures, List<float[]> vertexNormals, List<string> vertexFaces)
        {
            FileStream fs = new FileStream("vertexList_Vertices.dat", FileMode.Create, FileAccess.Write);
            var i = 1;
            // New Vertex
            foreach (var face in vertexFaces)
            {
                string[] part = face.Split('/');

                int vert = Convert.ToInt32(part[0]) - 1;
                byte[] x = BitConverter.GetBytes(vertex[vert][0]);
                byte[] y = BitConverter.GetBytes(vertex[vert][1]);
                byte[] z = BitConverter.GetBytes(vertex[vert][2]);
                Array.Reverse(x);
                Array.Reverse(y);
                Array.Reverse(z);
                fs.Write(x, 0, 4);
                fs.Write(y, 0, 4);
                fs.Write(z, 0, 4);
                i++;
                /*
                int text = Convert.ToInt32(part[1]) - 1;
                bin.Write(vertexTextures[text][0]);
                bin.Write(vertexTextures[text][1]);

                int norm = Convert.ToInt32(part[2]) - 1;
                bin.Write(vertexNormals[norm][0]);
                bin.Write(vertexNormals[norm][1]);
                bin.Write(vertexNormals[norm][2]);
                */
            }
            Console.WriteLine("Number of vertices: {0}", i);
            fs.Close();

            fs = new FileStream("vertexList_Other.dat", FileMode.Create, FileAccess.Write);
            i = 1;
            // New Vertex
            foreach (var face in vertexFaces)
            {
                string[] part = face.Split('/');

                int norm = Convert.ToInt32(part[2]) - 1;
                byte[] nx = { 0x00 };//BitConverter.GetBytes(vertex[norm][0]);
                byte[] ny = { 0x00 };//BitConverter.GetBytes(vertex[norm][1]);
                byte[] nz = { 0x00 };//BitConverter.GetBytes(vertex[norm][2]);
                byte[] nw = { 0x00 };//BitConverter.GetBytes(vertex[norm][2]);
                Array.Reverse(nx);
                Array.Reverse(ny);
                Array.Reverse(nz);
                Array.Reverse(nw);
                fs.Write(nx);
                fs.Write(ny);
                fs.Write(nz);
                fs.Write(nw);

                byte[] r = { 0xFE };//BitConverter.GetBytes(vertex[norm][0]);
                byte[] g = { 0x7F };//BitConverter.GetBytes(vertex[norm][1]);
                byte[] b = { 0x7F };//BitConverter.GetBytes(vertex[norm][2]);
                byte[] a = { 0x7F };//BitConverter.GetBytes(vertex[norm][2]);
                Array.Reverse(r);
                Array.Reverse(g);
                Array.Reverse(b);
                Array.Reverse(a);
                fs.Write(r);
                fs.Write(g);
                fs.Write(b);
                fs.Write(a);

                int text = Convert.ToInt32(part[1]) - 1;
                float[] j = vertexTextures[text];
                short encoded = (short)123.45;

                byte[] u = BitConverter.GetBytes(encoded);
                byte[] v = { };

                Console.WriteLine(u[0]);

                Array.Reverse(u);
                Array.Reverse(v);
                fs.Write(u);
                fs.Write(v);

                i++;
            }
            Console.WriteLine("Number of vertices: {0}", i);
            fs.Close();

            return false;
        }
    }
}
