﻿using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Controls;


namespace ImageProcessingApp
{

   
    public static class BitmapExtensions
    {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
   

    public partial class MainWindow : Window
    {
        private ImageDatabase _imageDatabase;

        private Bitmap currentImage;
        public Bitmap BlackWhiteImage;
        public MainWindow()
        {
            InitializeComponent();
            _imageDatabase = new ImageDatabase();
            LoadDatabaseUI();
        }
        // Загрузка всех записей в UI
        private void LoadDatabaseUI()
        {
            // Преобразуем каждый элемент в список с привязанными изображениями (Bitmap -> ImageSource)
            var recordsWithImages = _imageDatabase.GetAllRecords()
                                                   .Select(record => new ImageRecordViewModel
                                                   {
                                                       Id = record.Id,
                                                       PerceptualHash = record.PerceptualHash.ToString(),
                                                       ImageSource = record.Image.ToBitmapImage() // Преобразуем в ImageSource
                                                   }).ToList();
            // Привязываем к ListBox
            BaseListBox.ItemsSource = recordsWithImages;
        }
        // Обработчик добавления записи
        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            Bitmap smallImage = null;
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                smallImage = new Bitmap(openFileDialog.FileName);
            }
           
            ulong perceptualHash = PerceptualHash.GetImageHash(smallImage, 8);
            _imageDatabase.AddRecord(perceptualHash, smallImage);

            LoadDatabaseUI();
        }
        // Обработчик удаления записи
        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecord = (ImageRecordViewModel)BaseListBox.SelectedItem;
            if (selectedRecord != null)
            {
                _imageDatabase.DeleteRecord(selectedRecord.Id);
                LoadDatabaseUI();
            }
        }
        // Обработчик поискав
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ulong perceptualHash = Convert.ToUInt64(SearchHashTextBox.Text);
            double threshold = SimilaritySlider.Value;

            var results = _imageDatabase.SearchByHash(perceptualHash, threshold);
            
            SearchResultsListBox.ItemsSource = results.Select(r => r.Item1);
        }
        // Загрузка изображения
        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                currentImage = new Bitmap(openFileDialog.FileName);
                DisplayImage(currentImage);
            }
        }
        private void LoadImage2_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                BlackWhiteImage = new Bitmap(openFileDialog.FileName);
            }
        }

        // Отображение изображения на WPF
        private void DisplayImage(Bitmap image)
        {
            var hBitmap = image.GetHbitmap();
            var wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            ImageControl.Source = wpfBitmap;
        }

        // Преобразование полноцветного изображения с контрастированием
        private void ApplyContrast_Click(object sender, RoutedEventArgs e)
        {
            var contrast = contrastSlider.Value;
            Bitmap contrastedImage = ApplyColorImageContrast(currentImage, (float)contrast);
            currentImage = contrastedImage;
            DisplayImage(currentImage);
        }

        private Bitmap ApplyColorImageContrast(Bitmap image, float contrast)
        {
            Bitmap newBitmap = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);

                    // Применяем контраст к каждому каналу изображения
                    int r = (int)((pixelColor.R - 128) * contrast + 128);
                    int g = (int)((pixelColor.G - 128) * contrast + 128);
                    int b = (int)((pixelColor.B - 128) * contrast + 128);

                    // Ограничиваем значения пикселей в диапазоне от 0 до 255
                    r = Math.Min(255, Math.Max(0, r));
                    g = Math.Min(255, Math.Max(0, g));
                    b = Math.Min(255, Math.Max(0, b));

                    newBitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return newBitmap;
        }


        // Преобразование в полутоновое изображение
        private void ApplyGrayscale_Click(object sender, RoutedEventArgs e)
        {
            Bitmap grayscaleImage = ConvertToGrayscale(currentImage);
            int maskSize = (int)medianSlider.Value;

            grayscaleImage = ApplyMedianFilter(grayscaleImage, maskSize);
            currentImage = grayscaleImage;
            DisplayImage(grayscaleImage);
        }

        private Bitmap ConvertToGrayscale(Bitmap image)
        {
            Bitmap newBitmap = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);

                    // Взвешенное среднее для преобразования в оттенки серого
                    int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);

                    newBitmap.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }

            return newBitmap;
        }


        // Применение медианного фильтра для сглаживания
        private void ApplyMedianFilter_Click(object sender, RoutedEventArgs e)
        {
            Bitmap grayscaleImage = ConvertToGrayscale(currentImage);
            int maskSize = (int)medianSlider.Value;

            grayscaleImage = ApplyMedianFilter(grayscaleImage, maskSize);
            DisplayImage(grayscaleImage);
        }

        private Bitmap ApplyMedianFilter(Bitmap image, int maskSize)
        {
            Bitmap newBitmap = new Bitmap(image.Width, image.Height);
            int offset = maskSize / 2;

            for (int x = offset; x < image.Width - offset; x++)
            {
                for (int y = offset; y < image.Height - offset; y++)
                {
                    List<int> rValues = new List<int>();
                    List<int> gValues = new List<int>();
                    List<int> bValues = new List<int>();

                    // Собираем значения пикселей в маске
                    for (int dx = -offset; dx <= offset; dx++)
                    {
                        for (int dy = -offset; dy <= offset; dy++)
                        {
                            Color pixelColor = image.GetPixel(x + dx, y + dy);
                            rValues.Add(pixelColor.R);
                            gValues.Add(pixelColor.G);
                            bValues.Add(pixelColor.B);
                        }
                    }

                    // Сортируем и находим медиану
                    rValues.Sort();
                    gValues.Sort();
                    bValues.Sort();

                    int rMedian = rValues[rValues.Count / 2];
                    int gMedian = gValues[gValues.Count / 2];
                    int bMedian = bValues[bValues.Count / 2];

                    // Применяем медиану к изображению
                    newBitmap.SetPixel(x, y, Color.FromArgb(rMedian, gMedian, bMedian));
                }
            }

            return newBitmap;
        }


        // Преобразование в монохромное изображение (черно-белое)
        private void ApplyDilation_Click(object sender, RoutedEventArgs e)
        {
            int maskSize = (int)dilationSlider.Value;
            int bl = (int)blackLevel.Value;
            currentImage = ConvertToBlackAndWhite(currentImage, bl);
            currentImage = ApplyDilation(currentImage, maskSize);
            DisplayImage(currentImage);
        }

        private Bitmap ConvertToBlackAndWhite(Bitmap image, int threshold)
        {
            Bitmap newBitmap = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    // Получаем цвет пикселя
                    Color pixelColor = image.GetPixel(x, y);

                    // Вычисляем яркость пикселя с использованием формулы для серого цвета
                    int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);

                    // Применяем пороговую обработку: если яркость больше порога, делаем пиксель белым
                    if (grayValue > threshold)
                    {
                        newBitmap.SetPixel(x, y, Color.White); // Белый цвет
                    }
                    else
                    {
                        newBitmap.SetPixel(x, y, Color.Black); // Черный цвет
                    }
                }
            }

            return newBitmap;
        }

        private Bitmap ApplyDilation(Bitmap image, int maskSize)
        {
            Bitmap newBitmap = new Bitmap(image.Width, image.Height);

            // Определяем смещение для маски
            int offset = maskSize / 2;

            // Пройдем по всем пикселям изображения
            for (int x = offset; x < image.Width - offset; x++)
            {
                for (int y = offset; y < image.Height - offset; y++)
                {
                    bool isDilated = false;

                    // Применяем маску (выбираем область для дилатации)
                    for (int dx = -offset; dx <= offset; dx++)
                    {
                        for (int dy = -offset; dy <= offset; dy++)
                        {
                            // Проверяем, является ли текущий пиксель белым (255) в области маски
                            if (image.GetPixel(x + dx, y + dy).R == 255) // Только белые пиксели
                            {
                                isDilated = true;
                                break;
                            }
                        }
                        if (isDilated) break;
                    }

                    // Если хотя бы один пиксель в маске белый, ставим текущий пиксель белым
                    if (isDilated)
                        newBitmap.SetPixel(x, y, Color.White); // Белый пиксель
                    else
                        newBitmap.SetPixel(x, y, Color.Black); // Черный пиксель
                }
            }

            return newBitmap;
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалоговое окно для выбора местоположения и имени файла
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (saveFileDialog.ShowDialog() == true)
            {
                // Получаем путь для сохранения изображения
                string filePath = saveFileDialog.FileName;

                // Сохраняем изображение
                SaveImage(currentImage, filePath);
            }
        }
        private void FindConnectedComponents_Click(object sender, RoutedEventArgs e)
        {
            List<WriteableBitmap> areas = ConnectedComponents.FindConnectedComponents(BlackWhiteImage);

            // Очищаем StackPanel перед добавлением новых элементов
            ImageStackPanel.Children.Clear();

            // Устанавливаем горизонтальную ориентацию для StackPanel, чтобы изображения выстраивались в строку
            ImageStackPanel.Orientation = Orientation.Horizontal;

            // Перебираем все найденные области
            for (int i = 0; i < areas.Count; i++)
            {
                // Создаем контейнер для изображения и его индекса
                StackPanel imageContainer = new StackPanel
                {
                    Orientation = Orientation.Vertical, // Ориентация вертикальная для картинки и индекса
                    Margin = new Thickness(10)
                };

                // Создаем элемент Image
                System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image
                {
                    Source = areas[i],
                    Width = 50, // Устанавливаем фиксированный размер (можно изменить)
                    Height = 50, // Устанавливаем фиксированный размер (можно изменить)
                    Margin = new Thickness(10)
                };

                // Создаем элемент TextBlock для отображения индекса
                TextBlock indexText = new TextBlock
                {
                    Text = $"Область {i + 1}", // Индекс области
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0) // Отступ сверху
                };

                // Добавляем изображение и текстовый блок в контейнер
                imageContainer.Children.Add(imageControl);
                imageContainer.Children.Add(indexText);

                // Добавляем контейнер в основной StackPanel (с горизонтальной ориентацией)
                ImageStackPanel.Children.Add(imageContainer);
            }
        }

        private void SaveImage(Bitmap image, string filePath)
        {
            // Сохраняем изображение в указанном формате (JPG, PNG, BMP и т.д.)
            try
            {
                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png); // Можно изменить на .Jpeg или .Bmp в зависимости от выбранного формата
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изображения: {ex.Message}");
            }
        }
    }
}