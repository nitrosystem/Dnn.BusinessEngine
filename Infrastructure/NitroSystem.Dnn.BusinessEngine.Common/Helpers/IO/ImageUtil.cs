using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Utilities
{
    public static class ImageUtil
    {
        public static Byte[] GetImageThumbnailBytes(string path, int width, int height)
        {
            var image = Image.FromFile(path);
            using (Image thumbnail = image.GetThumbnailImage(width, height, () => false, IntPtr.Zero))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    thumbnail.Save(memoryStream, ImageFormat.Png);
                    Byte[] bytes = new Byte[memoryStream.Length];
                    memoryStream.Position = 0;
                    memoryStream.Read(bytes, 0, (int)bytes.Length);

                    return bytes;
                }
            }
        }

        public static Image ResizeImage(Stream stream, string newImagePath, int width, int height)
        {
            Image result = null;

            try
            {
                Bitmap srcBmp = new Bitmap(stream);
                float ratio = srcBmp.Width / srcBmp.Height;
                SizeF newSize = new SizeF(width, height != 0 ? height : (srcBmp.Height * width / srcBmp.Width));
                Bitmap target = new Bitmap((int)newSize.Width, (int)newSize.Height);
                using (Graphics graphics = Graphics.FromImage(target))
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(srcBmp, 0, 0, newSize.Width, newSize.Height);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        target.Save(memoryStream, GetImageFormat(newImagePath));
                        result = Image.FromStream(memoryStream);
                        result.Save(newImagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        public static bool IsImageContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType) && contentType.ToLower().StartsWith("image/");
        }

        public static byte[] GetResizedImage(string path, string path2, int width, int height)
        {
            Bitmap imgIn = new Bitmap(path);
            double y = imgIn.Height;
            double x = imgIn.Width;
            double factor = 1;
            if (width > 0)
            {
                factor = width / x;
            }
            else if (height > 0)
            {
                factor = height / y;
            }
            System.IO.MemoryStream outStream = new System.IO.MemoryStream();
            Bitmap imgOut = new Bitmap((int)(x * factor), (int)(y * factor));
            Graphics g = Graphics.FromImage(imgOut);
            g.Clear(Color.White);
            g.DrawImage(imgIn, new Rectangle(0, 0, (int)(factor * x), (int)(factor * y)), new Rectangle(0, 0, (int)x, (int)y), GraphicsUnit.Pixel);
            imgOut.Save(path);

            Bitmap imgOut2 = new Bitmap(width, height);
            Graphics g2 = Graphics.FromImage(imgOut2);
            g2.Clear(Color.White);
            g2.DrawImage(imgOut, new RectangleF((width - imgOut.Width) / 2, (height - imgOut.Height) / 2, (int)(factor * x), (int)(factor * y)), new RectangleF(0, 0, (int)(factor * x), (int)(factor * y)), GraphicsUnit.Pixel);
            imgOut2.Save(path2);

            return outStream.ToArray();
        }

        public static ImageFormat GetImageFormat(string filePath)
        {
            switch (Path.GetExtension(filePath))
            {
                case ".bmp": return ImageFormat.Bmp;
                case ".gif": return ImageFormat.Gif;
                case ".jpg": return ImageFormat.Jpeg;
                case ".png": return ImageFormat.Png;
                default: return ImageFormat.Jpeg;
            }
        }

        public static string GetContentType(string path)
        {
            switch (Path.GetExtension(path))
            {
                case ".bmp": return "Image/bmp";
                case ".gif": return "Image/gif";
                case ".jpg": return "Image/jpeg";
                case ".png": return "Image/png";
                default: break;
            }
            return "";
        }

        public static Stream ToStream(this Image image)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;
            return stream;
        }
    }
}


