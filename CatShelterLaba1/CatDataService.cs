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
        public event EventHandler <List<Cat>> DataChanged;

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
            SetupFileWatcher();
        }

        private void SetupFileWatcher()
        {
            try
            {
                string directory = Path.GetDirectoryName(_dataFilePath);
                string fileName = Path.GetFileName(_dataFilePath);

                var fileWatcher = new FileSystemWatcher
                {
                    Path = directory,
                    Filter = fileName,
                    NotifyFilter = NotifyFilters.LastWrite
                };

                fileWatcher.Changed += (s, e) =>
                {
                    System.Threading.Thread.Sleep(100);
                    LoadDataFromFile();
                    OnDataChanged();
                };

                fileWatcher.EnableRaisingEvents = true;
                Console.WriteLine("FileWatcher запущен для отслеживания изменений файла");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка настройки FileWatcher: {ex.Message}");
            }
        }
        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, new List<Cat>(_cats));
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
                string json = JsonConvert.SerializeObject(_cats, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения данных: {ex.Message}");
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
            //OnDataChanged();
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
                //OnDataChanged();
            }
        }

        public void DeleteCat(int id)
        {
            var cat = GetCatById(id);
            if (cat != null)
            {
                _cats.Remove(cat);
                SaveDataToFile();
                //OnDataChanged();
            }
        }

        public Dictionary<string, int> GetCatsByBreedGrouped()
        {
            return _cats.GroupBy(c => c.Breed).ToDictionary(g => g.Key, g => g.Count());
        }

        public int GetTotalCats() => _cats.Count;
    }
}