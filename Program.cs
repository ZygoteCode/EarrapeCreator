using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

public class Program
{
    public static void Main()
    {
        Console.Title = "EarrapeCreator | Made by https://github.com/ZygoteCode/";
        Console.WriteLine("EarrapeCreator | Made by https://github.com/ZygoteCode/");

        if (!Directory.Exists("inputs"))
        {
            Directory.CreateDirectory("inputs");
        }

        if (Directory.Exists("temp"))
        {
            Directory.Delete("temp", true);
        }

        if (Directory.Exists("outputs"))
        {
            Directory.Delete("outputs", true);
        }

        if (!Directory.Exists("temp"))
        {
            Directory.CreateDirectory("temp");
        }

        if (!Directory.Exists("outputs"))
        {
            Directory.CreateDirectory("outputs");
        }

        int bitDepth = -1;

        while (bitDepth <= 0)
        {
            Console.Write("Please, insert the bit depth value to apply to the audio files here (it must be between 1 and 24): ");
            string input = Console.ReadLine();
            int result = -1;
            int.TryParse(input, out result);
            
            if (result <= 0 || result > 24)
            {
                Console.WriteLine("Invalid number, please try again. It must be between 1 and 24.");
            }
            else
            {
                bitDepth = result;
            }
        }

        Console.WriteLine("Processing all audio files, please wait a while.");

        foreach (string file in Directory.GetFiles("inputs"))
        {
            new Thread(() =>
            {
                try
                {
                    RunFFMpeg($"-i \"{file}\" -ar 44100 -ac 2 -b:a 9000k -map a -vn \"{Path.GetFullPath("temp") + "\\" + Path.GetFileNameWithoutExtension(file) + ".wav"}\"");

                    using (AudioFileReader reader = new AudioFileReader(file))
                    {
                        var format = reader.WaveFormat;
                        int bytesPerSample = format.BitsPerSample / 8;
                        int bufferLength = 1024 * bytesPerSample;
                        byte[] buffer = new byte[bufferLength];
                        int bytesRead;

                        using (WaveFileWriter writer = new WaveFileWriter(Path.GetFullPath("outputs") + "\\" + Path.GetFileNameWithoutExtension(file) + ".wav", format))
                        {
                            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                float[] floatBuffer = new float[bytesRead / bytesPerSample];
                                Buffer.BlockCopy(buffer, 0, floatBuffer, 0, bytesRead);

                                for (int n = 0; n < floatBuffer.Length; n++)
                                {
                                    float originalSample = floatBuffer[n];
                                    float crushedSample = (float)Math.Floor(originalSample * (1 << bitDepth)) / (1 << bitDepth);
                                    floatBuffer[n] = crushedSample;
                                }

                                Buffer.BlockCopy(floatBuffer, 0, buffer, 0, bytesRead);
                                writer.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                catch
                {

                }
            }).Start();
        }

        Console.WriteLine("Succesfully processed all audio files.");
        Console.ReadLine();
    }


    private static void RunFFMpeg(string arguments)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg.exe",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        }).WaitForExit();
    }
}
