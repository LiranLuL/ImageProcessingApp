using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessingApp
{
    public class ImageProcessor
    {
        // Основная функция для эквализации гистограммы с параметрами для управления
        public Bitmap ApplyHistogramEqualization(Bitmap image, bool equalizeRed, bool equalizeGreen, bool equalizeBlue)
        {
            Bitmap newBitmap = new Bitmap(image.Width, image.Height);

            // Применяем эквализацию для каждого канала, если флаг равен true
            if (equalizeRed)
                ApplyChannelEqualization(image, newBitmap, 0); // Красный канал
            if (equalizeGreen)
                ApplyChannelEqualization(image, newBitmap, 1); // Зеленый канал
            if (equalizeBlue)
                ApplyChannelEqualization(image, newBitmap, 2); // Синий канал

            return newBitmap;
        }

        // Функция для эквализации одного канала
        private void ApplyChannelEqualization(Bitmap image, Bitmap newBitmap, int channel)
        {
            // Строим гистограмму для заданного канала
            int[] histogram = new int[256];
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    // В зависимости от канала, увеличиваем соответствующее значение в гистограмме
                    if (channel == 0) histogram[pixelColor.R]++;
                    if (channel == 1) histogram[pixelColor.G]++;
                    if (channel == 2) histogram[pixelColor.B]++;
                }
            }

            // Рассчитываем кумулятивную распределенную функцию (CDF)
            int[] cdf = new int[256];
            cdf[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            // Нормализуем CDF
            int minCDF = cdf[0];
            int maxCDF = cdf[255];
            float scale = 255.0f / (image.Width * image.Height - minCDF);

            // Применяем эквализацию для каждого пикселя
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int newPixelValue = 0;

                    // В зависимости от канала, меняем значение пикселя
                    if (channel == 0)
                    {
                        newPixelValue = (int)((cdf[pixelColor.R] - minCDF) * scale);
                        newBitmap.SetPixel(x, y, Color.FromArgb(newPixelValue, pixelColor.G, pixelColor.B));
                    }
                    if (channel == 1)
                    {
                        newPixelValue = (int)((cdf[pixelColor.G] - minCDF) * scale);
                        newBitmap.SetPixel(x, y, Color.FromArgb(pixelColor.R, newPixelValue, pixelColor.B));
                    }
                    if (channel == 2)
                    {
                        newPixelValue = (int)((cdf[pixelColor.B] - minCDF) * scale);
                        newBitmap.SetPixel(x, y, Color.FromArgb(pixelColor.R, pixelColor.G, newPixelValue));
                    }
                }
            }
        }
    }
}
