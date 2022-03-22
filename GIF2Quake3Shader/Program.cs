using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;

namespace GIF2Quake3Shader
{
    class Program
    {

        class Shader
        {
            public string name;
            public float frequency;
            public List<string> images = new List<string>();
            public List<int> durations = new List<int>();
        }




        static void Main(string[] args)
        {
            bool readSuccessful;
            Image image = null;
            List<Shader> shaders = new List<Shader>();
            foreach (string file in args)
            {

                Shader shaderHere = new Shader();
                
                readSuccessful = true;
                try
                {
                    image = Image.FromFile(file);
                }
                catch (Exception e)
                {
                    readSuccessful = false;
                }
                if (!readSuccessful)
                {
                    Console.WriteLine("File " + file + " could not be read.");
                    continue;
                }
                else
                {
                    Console.WriteLine("File " + file + " read.");
                }

                //scaledImage = new Bitmap(image,new Size(1920,1080),);

                FrameDimension dim = new FrameDimension(image.FrameDimensionsList[0]);
                int frameCount = image.GetFrameCount(dim);

                int totalDelay = 0;

                for (int i = 0; i < frameCount; i++)
                {
                    image.SelectActiveFrame(dim, i);
                    var delayPropertyBytes = image.GetPropertyItem(20736).Value;
                    int frameDelay = BitConverter.ToInt32(delayPropertyBytes, i * 4) * 10;

                    //scaledImage = scaleImage(image, theTargetWidth, theTargetHeight);
                    string output = Path.GetFileNameWithoutExtension(file) + $"_{i}.png";
                    using (Bitmap resized = new Bitmap(image, 512, 512))
                    {
                        resized.Save(output);
                        shaderHere.images.Add(output);
                        shaderHere.durations.Add(frameDelay);
                        totalDelay += frameDelay;
                        Console.WriteLine($"{i}: {frameDelay}ms");
                    }
                }
                image.Dispose();

                float averageDelay = (float)totalDelay / (float)frameCount;
                float frequency = 1000.0f / averageDelay;

                Console.WriteLine($"Average Delay: {averageDelay}ms, frequency: {frequency}");
                shaderHere.frequency = frequency;
                shaderHere.name = Path.GetFileNameWithoutExtension(file);

                shaders.Add(shaderHere);
            }



            StringBuilder sb = new StringBuilder();

            string path = "PLACEHOLDER";
            string blendFunc = "GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA";
            foreach (Shader shader in shaders)
            {
                sb.AppendLine($"{path}/{shader.name}");
                sb.AppendLine("{");
                sb.AppendLine("\tcull\tdisable");
                sb.AppendLine("\t{");
                int index = 0;
                foreach (string frame in shader.images)
                {
                    if(index++ == 0)
                    {
                        sb.Append($"\t\tanimMap {shader.frequency} {path}/{frame}");
                    } else
                    {
                        sb.Append($" {path}/{frame}");
                    }
                }
                sb.AppendLine($"");
                sb.AppendLine($"\t\tblendFunc {blendFunc}");
                sb.AppendLine($"\t\trgbGen vertex");
                sb.AppendLine($"\t\talphaGen vertex");
                sb.AppendLine("\t}");
                sb.AppendLine("}");
                sb.AppendLine("");
            }
            sb.AppendLine("");

            File.WriteAllText("PLACEHOLDER.shader",sb.ToString());

            Console.ReadKey();
        }
    }
}
