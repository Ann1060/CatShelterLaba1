using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatShelterLaba1
{
    public class CatService
    {
        private List<Cat> cats = new List<Cat>();
        private int nextId = 1;

        // Добавить кота
        public void AddCat(Cat cat)
        {
            if (cat == null)
                throw new ArgumentException("Кот не может быть пустым");

            if (string.IsNullOrEmpty(cat.Name))
                throw new ArgumentException("Имя кота обязательно");

            if (string.IsNullOrEmpty(cat.Breed))
                throw new ArgumentException("Порода кота обязательна");

            cat.Id = nextId++;
            cats.Add(cat);
        }

        // Получить всех котов
        public List<Cat> GetAllCats()
        {
            return cats;
        }

        // Найти кота по ID
        public Cat GetCatById(int id)
        {
            return cats.FirstOrDefault(c => c.Id == id);
        }

        // Обновить кота
        public void UpdateCat(Cat updatedCat)
        {
            var cat = GetCatById(updatedCat.Id);
            if (cat != null)
            {
                cat.Name = updatedCat.Name;
                cat.Breed = updatedCat.Breed;
                cat.Age = updatedCat.Age;
            }
        }

        // Удалить кота
        public void DeleteCat(int id)
        {
            var cat = GetCatById(id);
            if (cat != null)
            {
                cats.Remove(cat);
            }
        }

        // Получить котов по породе
        public List<Cat> GetCatsByBreed(string breed)
        {
            return cats.Where(c => c.Breed == breed).ToList();
        }

        // Группировка котов по породе
        public Dictionary<string, int> GetCatsByBreedGrouped()
        {
            return cats
                .GroupBy(c => c.Breed)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // Получить общее количество котов
        public int GetTotalCats()
        {
            return cats.Count;
        }
    }
}
