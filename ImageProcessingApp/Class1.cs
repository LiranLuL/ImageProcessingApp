using System;
using System.Collections.Generic;
using System.Drawing;  // Для работы с пикселями и цветами
using System.Windows.Media.Imaging;  // Для работы с изображениями WPF
using System.Windows;

namespace ImageProcessingApp
{

    public class ConnectedComponentInfo
    {
        public WriteableBitmap AreaImage { get; set; }  // Изображение области
        public int X { get; set; }  // Координата X верхнего левого угла области
        public int Y { get; set; }  // Координата Y верхнего левого угла области
        public int Width { get; set; }  // Ширина области
        public int Height { get; set; }  // Высота области

        public ConnectedComponentInfo(WriteableBitmap areaImage, int x, int y, int width, int height)
        {
            AreaImage = areaImage;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public class ConnectedComponents
    {
        public static List<ConnectedComponentInfo> FindConnectedComponents(Bitmap bitmap)
        {
            // Загружаем изображение с помощью WPF
            Bitmap img = new Bitmap(bitmap);  // Замените на путь к вашему изображению
            int width = img.Width;
            int height = img.Height;

            // Создаем массив меток для хранения результатов разметки
            int[,] labels = new int[height, width];
            int label = 1;

            // Разметка связных областей
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    if (IsForeground(img.GetPixel(j, i)) && labels[i, j] == 0)
                    {
                        LabelConnectedComponents(labels, img, i, j, label);
                        label++;
                    }
                }
            }

            // Список для хранения изображений каждой области
            List<ConnectedComponentInfo> connectedComponents = new List<ConnectedComponentInfo>();

            // Для каждой области создаем объект ConnectedComponentInfo с координатами и изображением
            for (int currentLabel = 1; currentLabel < label; currentLabel++)
            {
                // Получаем информацию о компоненте (изображение + координаты)
                ConnectedComponentInfo componentInfo = CreateComponentInfo(labels, img, width, height, currentLabel);
                connectedComponents.Add(componentInfo);
            }

            return connectedComponents;
        }

        // Метод для создания объекта ConnectedComponentInfo с изображением и координатами
        static ConnectedComponentInfo CreateComponentInfo(int[,] labels, Bitmap img, int width, int height, int currentLabel)
        {
            int minX = width, minY = height, maxX = 0, maxY = 0;

            // Проходим по меткам и находим минимальные и максимальные координаты для текущей области
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    if (labels[i, j] == currentLabel)
                    {
                        if (i < minY) minY = i;
                        if (i > maxY) maxY = i;
                        if (j < minX) minX = j;
                        if (j > maxX) maxX = j;
                    }
                }
            }

            // Вычисляем размеры области
            int areaWidth = maxX - minX + 1;
            int areaHeight = maxY - minY + 1;

            // Создаем изображение области
            WriteableBitmap areaImage = CreateAreaImage(labels, img, width, height, currentLabel);

            // Возвращаем объект ConnectedComponentInfo с изображением и координатами
            return new ConnectedComponentInfo(areaImage, minX, minY, areaWidth, areaHeight);
        }


        // Метод для выполнения поиска связных областей и возврата списка изображений (WriteableBitmap)
        public static List<WriteableBitmap> FindConnectedComponents(string imagePath)
        {
            // Загружаем изображение с помощью WPF
            Bitmap img = new Bitmap(imagePath);  // Замените на путь к вашему изображению
            int width = img.Width;
            int height = img.Height;

            // Создаем массив меток для хранения результатов разметки
            int[,] labels = new int[height, width];
            int label = 1;

            // Разметка связных областей
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    if (IsForeground(img.GetPixel(j, i)) && labels[i, j] == 0)
                    {
                        LabelConnectedComponents(labels, img, i, j, label);
                        label++;
                    }
                }
            }

            // Список для хранения изображений каждой области
            List<WriteableBitmap> separatedAreas = new List<WriteableBitmap>();

            // Для каждой области создаем отдельное изображение
            for (int currentLabel = 1; currentLabel < label; currentLabel++)
            {
                WriteableBitmap areaImage = CreateAreaImage(labels, img, width, height, currentLabel);
                separatedAreas.Add(areaImage);
            }

            return separatedAreas;
        }

        // Проверка, является ли пиксель "фоном" или "объектом"
        static bool IsForeground(Color pixelColor)
        {
            return pixelColor.R == 0 && pixelColor.G == 0 && pixelColor.B == 0; // Черный пиксель — это объект
        }

        // Метод для разметки связных областей
        static void LabelConnectedComponents(int[,] labels, Bitmap img, int startRow, int startCol, int currentLabel)
        {
            Stack<Tuple<int, int>> stack = new Stack<Tuple<int, int>>();
            stack.Push(Tuple.Create(startRow, startCol));

            while (stack.Count > 0)
            {
                var point = stack.Pop();
                int i = point.Item1;
                int j = point.Item2;

                if (i >= 0 && i < img.Height && j >= 0 && j < img.Width && IsForeground(img.GetPixel(j, i)) && labels[i, j] == 0)
                {
                    labels[i, j] = currentLabel;

                    if (i > 0 && labels[i - 1, j] == 0) stack.Push(Tuple.Create(i - 1, j));
                    if (i < img.Height - 1 && labels[i + 1, j] == 0) stack.Push(Tuple.Create(i + 1, j));
                    if (j > 0 && labels[i, j - 1] == 0) stack.Push(Tuple.Create(i, j - 1));
                    if (j < img.Width - 1 && labels[i, j + 1] == 0) stack.Push(Tuple.Create(i, j + 1));
                }
            }
        }

        // Метод для создания изображения области в виде WriteableBitmap
        static WriteableBitmap CreateAreaImage(int[,] labels, Bitmap img, int width, int height, int currentLabel)
        {
            // Находим границы текущей области (минимальные и максимальные координаты)
            int minX = width, minY = height, maxX = 0, maxY = 0;

            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    if (labels[i, j] == currentLabel)
                    {
                        if (i < minY) minY = i;
                        if (i > maxY) maxY = i;
                        if (j < minX) minX = j;
                        if (j > maxX) maxX = j;
                    }
                }
            }

            // Вычисляем размеры области
            int areaWidth = maxX - minX + 1;
            int areaHeight = maxY - minY + 1;

            // Создаем WriteableBitmap для области
            WriteableBitmap areaImage = new WriteableBitmap(areaWidth, areaHeight, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);

            // Массив для пикселей области
            byte[] pixelData = new byte[areaWidth * areaHeight * 4];

            // Проходим по меткам области и создаем изображение
            for (int i = minY; i <= maxY; ++i)
            {
                for (int j = minX; j <= maxX; ++j)
                {
                    // Индекс пикселя в массиве
                    int index = ((i - minY) * areaWidth + (j - minX)) * 4;

                    if (labels[i, j] == currentLabel)
                    {
                        // Черный цвет для пикселей области
                        pixelData[index] = 0;    // Blue
                        pixelData[index + 1] = 0; // Green
                        pixelData[index + 2] = 0; // Red
                        pixelData[index + 3] = 255; // Alpha
                    }
                    else
                    {
                        // Белый цвет для фона
                        pixelData[index] = 255;    // Blue
                        pixelData[index + 1] = 255; // Green
                        pixelData[index + 2] = 255; // Red
                        pixelData[index + 3] = 255; // Alpha
                    }
                }
            }

            // Записываем пиксели в WriteableBitmap
            areaImage.WritePixels(new Int32Rect(0, 0, areaWidth, areaHeight), pixelData, areaWidth * 4, 0);

            return areaImage;
        }
    }
}
