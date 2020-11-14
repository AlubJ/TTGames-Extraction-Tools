using System;

namespace AMF
{
    class Program
    {
        static Wavefront obj = new Wavefront();

        static AlubsModelFormat amf = new AlubsModelFormat();

        static void Main(string[] args)
        {
            // Get obj file from arguements
            string[] arguments = Environment.GetCommandLineArgs();
            obj.ReadWavefront(arguments[1]);
            amf.CreateModel(obj.vertex, obj.vertexTextures, obj.vertexNormals, obj.vertexFaces);
            Console.ReadKey();
        }
    }
}
