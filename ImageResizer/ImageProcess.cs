using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }


        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            foreach (var filePath in allFiles)
            {
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);

                string destFile = Path.Combine(destPath, imgName + ".jpg");
                processedImage.Save(destFile, ImageFormat.Jpeg);
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業Task
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImagesTask(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            Task[] allTasks = new Task[allFiles.Count()];
            for (var index = 0; index < allFiles.Count; index++)
            {
                var filePath = allFiles[index];
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                allTasks[index] = Task.Run(() =>
                {

                    Bitmap processedImage = processBitmap((Bitmap) imgPhoto,
                        sourceWidth, sourceHeight,
                        destionatonWidth, destionatonHeight);

                    string destFile = Path.Combine(destPath, imgName + ".jpg");
                    processedImage.Save(destFile, ImageFormat.Jpeg);
                });
            }

            Task.WaitAll(allTasks);
        }

        /// <summary>
        /// 進行圖片的縮放作業Async
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            List<ImageInfo> imageInfos = new List<ImageInfo>(allFiles.Count());
            foreach (var filePath in allFiles)
            {
                ImageInfo imgInfo = GetImageInfo(scale, filePath);
                imageInfos.Add(imgInfo);
            }

            //allFiles.AsParallel().ForAll(filePath =>
            //{
            //    ImageInfo imgInfo = GetImageInfo(scale, filePath);
            //    imageInfos.Add(imgInfo);
            //});

            Task[] allTasks = new Task[allFiles.Count()];
            allTasks = imageInfos.Select(item => Task.Run(async () =>
            {
                //Console.WriteLine($"{item.ImgName}執行緒的 ID={Thread.CurrentThread.ManagedThreadId}");
                var resizeItem = await ProcessBitmapAsync(item);
                string destFile = Path.Combine(destPath, resizeItem.ImgName + ".jpg");
                resizeItem.ImgPhoto.Save(destFile, ImageFormat.Jpeg);
            }))
            .ToArray();

            try
            {
                await Task.WhenAll(allTasks);
            }
            catch (Exception e)
            {
                // 當所有等候工作都執行結束後，可以檢查是否有執行失敗的工作
                foreach (Task faulted in allTasks.Where(t => t.IsFaulted))
                {
                    Console.WriteLine(faulted.Exception.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業Parallel
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImagesParallel(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            allFiles.AsParallel().ForAll(filePath =>
            {
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);

                string destFile = Path.Combine(destPath, imgName + ".jpg");
                processedImage.Save(destFile, ImageFormat.Jpeg);
            });
        }

        /// <summary>
        /// 進行圖片的縮放作業Parallel.ForEach
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public async Task ResizeImagesParallelForEach(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            List<Task> Tasks = new List<Task>();
            await Task.Run(() => Parallel.ForEach(allFiles, async filePath =>
            {
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                     sourceWidth, sourceHeight,
                     destionatonWidth, destionatonHeight);

                string destFile = Path.Combine(destPath, imgName + ".jpg");
                processedImage.Save(destFile, ImageFormat.Jpeg);
            }));
        }

        /// <summary>
        /// 進行圖片的縮放作業Parallel.ForEach
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public async Task ResizeImagesParallelForEachNest(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            List<Task> Tasks = new List<Task>();
            await Task.Run(() => Parallel.ForEach(allFiles, async filePath =>
            {
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                //Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                //     sourceWidth, sourceHeight,
                //     destionatonWidth, destionatonHeight);

                //string destFile = Path.Combine(destPath, imgName + ".jpg");
                //processedImage.Save(destFile, ImageFormat.Jpeg);

                Tasks.Add(Task.Run(() => processBitmap((Bitmap)imgPhoto, sourceWidth, sourceHeight,
                         destionatonWidth, destionatonHeight))
                    .ContinueWith(x =>
                    {
                        string destFile = Path.Combine(destPath, imgName + ".jpg");
                        x.Result.Save(destFile, ImageFormat.Jpeg);
                    }));

            }));

            await Task.WhenAll(Tasks);
        }

        /// <summary>
        /// 取得圖檔資訊
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public ImageInfo GetImageInfo(double scale, string filePath)
        {
            ImageInfo imgInfo = new ImageInfo();
            imgInfo.ImgPhoto = (Bitmap)Image.FromFile(filePath);
            imgInfo.ImgName = Path.GetFileNameWithoutExtension(filePath);

            imgInfo.SrcWidth = imgInfo.ImgPhoto.Width;
            imgInfo.SrcHeight = imgInfo.ImgPhoto.Height;

            imgInfo.NewWidth = (int) (imgInfo.SrcWidth * scale);
            imgInfo.NewHeight = (int) (imgInfo.SrcHeight * scale);
            return imgInfo;
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap processBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業Async
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        async Task<Bitmap> processBitmapAsync(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            return await Task.Run(() =>
            {
                Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
                Graphics g = Graphics.FromImage(resizedbitmap);
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(img,
                    new Rectangle(0, 0, newWidth, newHeight),
                    new Rectangle(0, 0, srcWidth, srcHeight),
                    GraphicsUnit.Pixel);
                return resizedbitmap;
            });
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業Async
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        async Task<ImageVM> ProcessBitmapAsync(ImageInfo imgInfo)
        {
            return await Task.Run(() =>
            {
                Bitmap resizedbitmap = new Bitmap(imgInfo.NewWidth, imgInfo.NewHeight);
                Graphics g = Graphics.FromImage(resizedbitmap);
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(imgInfo.ImgPhoto,
                    new Rectangle(0, 0, imgInfo.NewWidth, imgInfo.NewHeight),
                    new Rectangle(0, 0, imgInfo.SrcWidth, imgInfo.SrcHeight),
                    GraphicsUnit.Pixel);
                ImageVM result = new ImageVM();
                result.ImgPhoto = resizedbitmap;
                result.ImgName = imgInfo.ImgName;
                return result;
            });
        }
    }
}
