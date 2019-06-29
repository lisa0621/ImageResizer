using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            ImageProcess imageProcess = new ImageProcess();

            imageProcess.Clean(destinationPath);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);
            Console.WriteLine($"原始花費時間: {sw.ElapsedMilliseconds} ms");
            sw.Stop();

            imageProcess.Clean(destinationPath);
            sw.Restart();
            imageProcess.ResizeImagesTask(sourcePath, destinationPath, 2.0);
            sw.Stop();
            Console.WriteLine($"Task: {sw.ElapsedMilliseconds} ms");

            imageProcess.Clean(destinationPath);
            sw.Restart();
            await imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0);
            sw.Stop();
            Console.WriteLine($"Async花費時間: {sw.ElapsedMilliseconds} ms");

            imageProcess.Clean(destinationPath);
            sw.Restart();
            imageProcess.ResizeImagesParallel(sourcePath, destinationPath, 2.0);
            sw.Stop();

            Console.WriteLine($"Parallel花費時間: {sw.ElapsedMilliseconds} ms");

            imageProcess.Clean(destinationPath);
            sw.Restart();
            await imageProcess.ResizeImagesParallelForEach(sourcePath, destinationPath, 2.0);
            sw.Stop();

            Console.WriteLine($"ParallelForEach花費時間: {sw.ElapsedMilliseconds} ms");

            imageProcess.Clean(destinationPath);
            sw.Restart();
            await imageProcess.ResizeImagesParallelForEachNest(sourcePath, destinationPath, 2.0);
            sw.Stop();

            Console.WriteLine($"ResizeImagesParallelForEachNest花費時間: {sw.ElapsedMilliseconds} ms");
        }
    }
}
