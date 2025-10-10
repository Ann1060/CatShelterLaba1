using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;

namespace Model
{
    public class CatDataService
    {
        private static CatDataService _instance;

        // Путь к нашему существующему файлу
        private string _dataFilePath;

        private List<Cat> _cats = new List<Cat>();
        private int _nextId = 1;
        private readonly List<ICatDataObserver> _observers = new List<ICatDataObserver>();

        public static CatDataService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CatDataService();
                }
                return _instance;
            }
        }

        private CatDataService()
        {
            // Путь к нашему SharedCatsData.json в корне решения
            string solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            _dataFilePath = Path.Combine(solutionDirectory, "SharedCatsData.json");

            // Создаем директорию, если не существует
            var directory = Path.GetDirectoryName(_dataFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Console.WriteLine($"CatDataService загружает из: {_dataFilePath}");
            LoadDataFromFile();
        }

        private void LoadDataFromFile()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    string json = File.ReadAllText(_dataFilePath);
                    var cats = JsonConvert.DeserializeObject<List<Cat>>(json);
                    if (cats != null)
                    {
                        _cats = cats;
                        _nextId = _cats.Count > 0 ? _cats.Max(c => c.Id) + 1 : 1;
                        Console.WriteLine($"Загружено {_cats.Count} котов, следующий ID: {_nextId}");
                    }
                }
                else
                {
                    Console.WriteLine($"Файл не существует, будет создан при первом сохранении: {_dataFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
                _cats = new List<Cat>();
                _nextId = 1;
            }
        }

        private void SaveDataToFile()
        {
            try
            {
                Console.WriteLine($"CatDataService сохраняет в: {_dataFilePath}");
                System.Diagnostics.Debug.WriteLine($"CatDataService сохраняет в: {_dataFilePath}");

                // СОХРАНЯЕМ данные в файл
                string json = JsonConvert.SerializeObject(_cats, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_dataFilePath, json);

                Console.WriteLine($"Файл существует: {File.Exists(_dataFilePath)}");
                Console.WriteLine($"Успешно сохранено {_cats.Count} котов");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения данных: {ex.Message}");
            }
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers.ToList())
            {
                observer.OnCatDataChanged();
            }
        }

        public void RegisterObserver(ICatDataObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void UnregisterObserver(ICatDataObserver observer)
        {
            _observers.Remove(observer);
        }


        public void AddCat(Cat cat)
        {
            cat.Id = _nextId++;
            _cats.Add(cat);
            SaveDataToFile();
            NotifyObservers();
        }

        public List<Cat> GetAllCats() => new List<Cat>(_cats);

        public Cat GetCatById(int id) => _cats.FirstOrDefault(c => c.Id == id);

        public void UpdateCat(Cat updatedCat)
        {
            var cat = GetCatById(updatedCat.Id);
            if (cat != null)
            {
                cat.Name = updatedCat.Name;
                cat.Breed = updatedCat.Breed;
                cat.Age = updatedCat.Age;
                SaveDataToFile();
                NotifyObservers();
            }
        }

        public void DeleteCat(int id)
        {
            var cat = GetCatById(id);
            if (cat != null)
            {
                _cats.Remove(cat);
                SaveDataToFile();
                NotifyObservers();
            }
        }

        public Dictionary<string, int> GetCatsByBreedGrouped()
        {
            return _cats.GroupBy(c => c.Breed).ToDictionary(g => g.Key, g => g.Count());
        }

        public int GetTotalCats() => _cats.Count;
    }
}