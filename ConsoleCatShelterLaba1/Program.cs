using System;
using Model;
using System.IO;

namespace ConsoleCatShelter
{
    class Program
    {
        static CatDataService catService = CatDataService.Instance;
        private readonly string _dataFilePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "SharedCatsData.json"));


        static void Main(string[] args)
        {


            while (true)
            {
                Console.Clear();
                Console.WriteLine("Приют для кошек");
                Console.WriteLine("1. Посмотреть всех котов");
                Console.WriteLine("2. Добавить кота");
                Console.WriteLine("3. Изменить кота");
                Console.WriteLine("4. Удалить кота");
                Console.WriteLine("5. Статистика по породам");
                Console.WriteLine("6. Выход");
                Console.Write("Выберите: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ShowAllCats(); break;
                    case "2": AddCat(); break;
                    case "3": EditCat(); break;
                    case "4": DeleteCat(); break;
                    case "5": ShowStats(); break;
                    case "6":
                        Console.WriteLine("Данные сохранены. До свидания!");
                        return;
                }
            }
        }

        static void ShowAllCats()
        {
            Console.Clear();
            Console.WriteLine("Все коты:");
            Console.WriteLine("ID | Имя | Порода | Возраст");
            Console.WriteLine("---------------------------");

            var cats = catService.GetAllCats();
            if (cats.Count == 0)
            {
                Console.WriteLine("Котов нет в приюте");
            }
            else
            {
                foreach (var cat in cats)
                {
                    Console.WriteLine($"{cat.Id} | {cat.Name} | {cat.Breed} | {cat.Age}");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        static void AddCat()
        {
            Console.Clear();
            Console.WriteLine("Добавление кота:");

            var cat = new Cat();

            Console.Write("Имя: ");
            cat.Name = Console.ReadLine();

            Console.Write("Порода: ");
            cat.Breed = Console.ReadLine();

            Console.Write("Возраст: ");
            if (int.TryParse(Console.ReadLine(), out int age))
            {
                cat.Age = age;
            }
            else
            {
                Console.WriteLine("Некорректный возраст!");
                Console.ReadKey();
                return;
            }

            try
            {
                catService.AddCat(cat);
                Console.WriteLine("Кот добавлен и сохранен в файл!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.ReadKey();
        }

        // Остальные методы (EditCat, DeleteCat, ShowStats) остаются без изменений
        static void EditCat()
        {
            Console.Clear();
            ShowAllCats();

            Console.Write("Введите ID кота для изменения: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var cat = catService.GetCatById(id);
                if (cat == null)
                {
                    Console.WriteLine("Кот не найден!");
                    Console.ReadKey();
                    return;
                }

                Console.Write($"Имя ({cat.Name}): ");
                string name = Console.ReadLine();
                if (!string.IsNullOrEmpty(name)) cat.Name = name;

                Console.Write($"Порода ({cat.Breed}): ");
                string breed = Console.ReadLine();
                if (!string.IsNullOrEmpty(breed)) cat.Breed = breed;

                Console.Write($"Возраст ({cat.Age}): ");
                string ageInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(ageInput) && int.TryParse(ageInput, out int age))
                    cat.Age = age;

                try
                {
                    catService.UpdateCat(cat);
                    Console.WriteLine("Кот изменен и данные сохранены!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Некорректный ID!");
            }

            Console.ReadKey();
        }

        static void DeleteCat()
        {
            Console.Clear();
            ShowAllCats();

            Console.Write("Введите ID кота для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var cat = catService.GetCatById(id);
                if (cat == null)
                {
                    Console.WriteLine("Кот не найден!");
                    Console.ReadKey();
                    return;
                }

                Console.Write($"Удалить кота {cat.Name}? (y/n): ");
                if (Console.ReadLine().ToLower() == "y")
                {
                    try
                    {
                        catService.DeleteCat(id);
                        Console.WriteLine("Кот удален и данные сохранены!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Некорректный ID!");
            }

            Console.ReadKey();
        }

        static void ShowStats()
        {
            Console.Clear();
            Console.WriteLine("Статистика по породам:");

            var stats = catService.GetCatsByBreedGrouped();
            if (stats.Count == 0)
            {
                Console.WriteLine("Нет данных для статистики");
            }
            else
            {
                foreach (var item in stats)
                {
                    Console.WriteLine($"{item.Key}: {item.Value} котов");
                }
            }

            Console.WriteLine($"\nВсего котов: {catService.GetTotalCats()}");
            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}