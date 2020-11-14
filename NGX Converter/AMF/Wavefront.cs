using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AMF
{
    public class Wavefront
    {
        // Vertex position
        public List<float[]> vertex = new List<float[]>();

        // UV Coordinates
        public List<float[]> vertexTextures = new List<float[]>();

        // Vertex Normals
        public List<float[]> vertexNormals = new List<float[]>();

        // Vertex Faces
        public List<string> vertexFaces = new List<string>();


        public bool ReadWavefront (string FileLocation = "")
        {
            string[] lines = File.ReadAllLines(FileLocation);
            
            foreach(var line in lines)
            {
                string[] keyword = line.Split(" ");

                switch (keyword[0])
                {
                    case "v":
                        float[] v = { float.Parse(keyword[1]), float.Parse(keyword[2]), float.Parse(keyword[3]) };
                        vertex.Add(v);
                        break;
                    case "vt":
                        float[] vt = { float.Parse(keyword[1]), float.Parse(keyword[2]) };
                        vertexTextures.Add(vt);
                        break;
                    case "vn":
                        float[] vn = { float.Parse(keyword[1]), float.Parse(keyword[2]), float.Parse(keyword[3]) };
                        vertexNormals.Add(vn);
                        break;
                    case "f":
                        for (int i = 1; i < keyword.Length; i++)
                        {

                            vertexFaces.Add(keyword[i]);
                        }
                        break;
                }
            }

            return true;
        }
    }
}