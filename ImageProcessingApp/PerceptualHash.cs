using System;
using System.Drawing;

public class PerceptualHash
{
    // Метод для вычисления перцептивного хэша
    public static ulong GetImageHash(Bitmap image, int size = 8)
    {
        // Уменьшаем изображение до N x N (например, 8x8)
        Bitmap resizedImage = new Bitmap(image, new Size(size, size));

        // Получаем значения яркости пикселей и вычисляем хэш
        ulong hash = 0;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Получаем цвет пикселя (который уже черно-белый)
                Color pixelColor = resizedImage.GetPixel(x, y);
                int brightness = pixelColor.R; // Все каналы R, G, B одинаковые, т.к. изображение черно-белое

                // Преобразуем в бит (0 или 1) — порог 128, если яркость больше 128, то 1, иначе 0
                if (brightness > 128)
                {
                    hash |= (1UL << (y * size + x));
                }
            }
        }
        return hash;
    }

    // Метод для вычисления расстояния Хемминга между двумя хэшами
    public static int HammingDistance(ulong hash1, ulong hash2)
    {
        // Побитово вычисляем разницу между двумя хэшами
        ulong xorResult = hash1 ^ hash2;

        // Считаем количество единичных битов (1) в результате операции XOR
        int distance = 0;
        while (xorResult != 0)
        {
            // Инкрементируем количество различий (единичных битов)
            distance += (int)(xorResult & 1);
            xorResult >>= 1; // Сдвигаем в право, чтобы проверить следующий бит
        }

        return distance;
    }
}
