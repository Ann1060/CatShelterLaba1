using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Model;
using System.IO;
using WinFormCatShelterLaba1;

namespace WindowsFormsCatShelter
{
    public partial class MainForm : Form
    {
        private CatDataService catService = CatDataService.Instance;
        private BindingList<Cat> catsBindingList;

        public MainForm()
        {
            InitializeComponent();
            InitializeDataGridView();

            // Регистрируем форму как наблюдателя
            catService.DataChanged += OnDataChanged;

            LoadCats();
        }

        private void InitializeDataGridView()
        {
            // Настраиваем DataGridView
            dataGridViewCats.AutoGenerateColumns = true;
            dataGridViewCats.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewCats.ReadOnly = true;
            dataGridViewCats.AllowUserToAddRows = false;

            // Настраиваем автоматическое изменение размера колонок
            dataGridViewCats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void OnDataChanged(object sender, System.Collections.Generic.List<Cat> cats)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => LoadCats()));
            }
            else
            {
                LoadCats();
            }
        }

        public void LoadCats()
        {
            try
            {
                var cats = catService.GetAllCats();
                catsBindingList = new BindingList<Cat>(cats);
                dataGridViewCats.DataSource = catsBindingList;
                labelTotal.Text = $"Всего котов: {cats.Count}";
                dataGridViewCats.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var addForm = new AddCatForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    catService.AddCat(addForm.NewCat);
                    MessageBox.Show("Кот успешно добавлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewCats.SelectedRows.Count > 0)
            {
                var selectedCat = (Cat)dataGridViewCats.SelectedRows[0].DataBoundItem;
                var editForm = new EditCatForm(selectedCat);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        catService.UpdateCat(editForm.UpdatedCat);
                        MessageBox.Show("Данные кота обновлены!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите кота для редактирования", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewCats.SelectedRows.Count > 0)
            {
                var selectedCat = (Cat)dataGridViewCats.SelectedRows[0].DataBoundItem;

                if (MessageBox.Show($"Удалить кота {selectedCat.Name}?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        catService.DeleteCat(selectedCat.Id);
                        MessageBox.Show("Кот удален!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите кота для удаления", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonStats_Click(object sender, EventArgs e)
        {
            var stats = catService.GetCatsByBreedGrouped();
            string message = "🐱 Коты по породам:\n\n";

            foreach (var item in stats)
            {
                string catWord = GetCorrectCatWord(item.Value);
                message += $"{item.Key}: {item.Value} {catWord}\n";
            }

            message += $"\nВсего котов: {catService.GetTotalCats()}";

            MessageBox.Show(message, "Статистика приюта");
        }

        private string GetCorrectCatWord(int count)
        {
            int lastDigit = count % 10;
            int lastTwoDigits = count % 100;

            // Исключения для чисел 11-14
            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            {
                return "котов";
            }

            switch (lastDigit)
            {
                case 1:
                    return "кот";
                case 2:
                case 3:
                case 4:
                    return "кота";
                default:
                    return "котов";
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            LoadCats();
        }

        private void dataGridViewCats_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                buttonEdit_Click(sender, e);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            catService.DataChanged -= OnDataChanged;
        }
    }
}