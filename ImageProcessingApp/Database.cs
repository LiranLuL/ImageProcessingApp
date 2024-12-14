using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImageProcessingApp
{

    public class ImageDatabase
    {
        public List<ImageRecord> _records;
        private int _nextId;

        public ImageDatabase()
        {
            _records = new List<ImageRecord>();
            _nextId = 1;
        }

        // Добавление новой записи в базу
        public void AddRecord(ulong perceptualHash, Bitmap image)
        {
            var newRecord = new ImageRecord(_nextId++, perceptualHash, image);
            _records.Add(newRecord);
        }

        // Поиск записи по хэшу с учетом процента совпадения
        public List<(ImageRecord, double)> SearchByHash(ulong perceptualHash, double threshold)
        {
            var results = new List<(ImageRecord, double)>();

            foreach (var record in _records)
            {
                double similarity = GetHammingDistance(record.PerceptualHash, perceptualHash);
                if (similarity >= threshold)
                {
                    results.Add((record, similarity));
                }
            }

            return results.OrderByDescending(r => r.Item2).ToList();
        }

        // Удаление записи по ID
        public void DeleteRecord(int id)
        {
            var recordToDelete = _records.FirstOrDefault(r => r.Id == id);
            if (recordToDelete != null)
            {
                _records.Remove(recordToDelete);
            }
        }

        // Рассчитываем Хеммингтонское расстояние (различие битов)
        private double GetHammingDistance(ulong hash1, ulong hash2)
        {
            ulong diff = hash1 ^ hash2;
            int dist = 0;

            while (diff != 0)
            {
                dist += (int)(diff & 1); // Если младший бит 1, увеличиваем счетчик
                diff >>= 1; // Сдвигаем на 1 бит влево
            }

            return (64 - dist) / 64.0; // Процент совпадения
        }

        // Получение всех записей
        public List<ImageRecord> GetAllRecords()
        {
            return _records;
        }
    }
}