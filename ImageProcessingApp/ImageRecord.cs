using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing; // Для хранения изображений
using System.Windows.Media.Imaging; // Для отображения изображений в WPF
using System.Windows.Media;
using System.IO;

namespace ImageProcessingApp
{
    public class ImageRecordViewModel
    {
        public int Id { get; set; }
        public ImageSource ImageSource { get; set; } // Преобразованный Bitmap в ImageSource для отображения
        public string PerceptualHash { get; set; } // Перцептивный хеш


    }
    public class ImageRecord
    {
        public ulong PerceptualHash { get; set; }
        public Bitmap Image { get; set; } // Храним изображение для отображения
        public int Id { get; set; }
        public ImageSource ImageSource { get; set; } // Преобразованный Bitmap в ImageSource для отображения
        private BitmapImage ConvertToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public ImageRecord(int id, ulong perceptualHash, Bitmap image)
        {
            Id = id;
            PerceptualHash = perceptualHash;
            Image = image;
            ImageSource = ConvertToBitmapImage(image);
        }
    }

}
