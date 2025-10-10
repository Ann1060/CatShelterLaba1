using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace Model
{
    public class FileWatcherService : IDisposable
    {
        private readonly string _filePath;
        private FileSystemWatcher _watcher;
        private readonly CatDataService _dataService;

        public FileWatcherService()
        {
            _dataService = CatDataService.Instance;

            // Тот же путь что в CatDataService
            string solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

            _filePath = Path.Combine(solutionDirectory, "SharedCatsData.json");

            Console.WriteLine($"FileWatcher следит за: {_filePath}");
            InitializeWatcher();
        }

        private void InitializeWatcher()
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                string fileName = Path.GetFileName(_filePath);

                _watcher = new FileSystemWatcher
                {
                    Path = directory,
                    Filter = fileName,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _watcher.Changed += OnFileChanged;
                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception) { }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            ReloadData();
        }

        private void ReloadData()
        {
            try
            {
                _dataService.GetType().GetMethod("NotifyObservers",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(_dataService, null);
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}