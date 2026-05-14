using AuthForge.Core.Interfaces;
using AuthForge.Core.Models;
using AuthForge.Core.Services;
using BCrypt.Net;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace AuthForge.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ExcelService _excelService = new();
        private string? _selectedFile;
        private readonly Argon2Service _argon2 = new();
        private readonly BCryptService _bcrypt = new();
        private readonly BuiltInHashService _pbkdf2 = new();
        private readonly LegacyHashService _legacy = new();

        public MainWindow()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        // Позволяет перетаскивать окно мышкой
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Закрыть
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Свернуть
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Развернуть / Свернуть в окно
        private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        // 1. Выбор файла
        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new OpenFileDialog { Filter = "Excel Files|*.xlsx" };
            if (openPicker.ShowDialog() == true)
            {
                // Проверка шаблона
                if (_excelService.IsTemplateValid(openPicker.FileName))
                {
                    _selectedFile = openPicker.FileName;
                    FilePathTxt.Text = Path.GetFileName(_selectedFile);
                    FilePathTxt.BorderBrush = System.Windows.Media.Brushes.Cyan; // Подсветка Cyber Forge
                    StartBtn.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Invalid Template! Please use the 'Generate Template' button to get the correct format.",
                                    "Format Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    FilePathTxt.Text = "Invalid file selected";
                    FilePathTxt.BorderBrush = System.Windows.Media.Brushes.Red;
                    StartBtn.IsEnabled = false;
                }
            }
        }

        // 2. Создание шаблона
        private void CreateTemplate_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new SaveFileDialog { Filter = "Excel Files|*.xlsx", FileName = "AuthForge_Template.xlsx" };
            if (savePicker.ShowDialog() == true)
            {
                _excelService.CreateTemplate(savePicker.FileName);
            }
        }

        // 3. Управление видимостью настроек
        private void AlgoSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Argon2Settings == null || LegacySettings == null) return;

            // Скрываем всё
            Argon2Settings.Visibility = Visibility.Collapsed;
            LegacySettings.Visibility = Visibility.Collapsed;

            // Показываем нужное
            switch (AlgoSelector.SelectedIndex)
            {
                case 0: // Argon2
                    Argon2Settings.Visibility = Visibility.Visible;
                    break;
                case 3: // SHA256 (Legacy)
                    LegacySettings.Visibility = Visibility.Visible;
                    break;
            }

            UpdateActiveSettingsInfo();
        }

        // 4. Основной процесс
        private async void StartHashing_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile)) return;

            // 1. ЗАБИРАЕМ ДАННЫЕ ИЗ UI (В основном потоке)
            int selectedIndex = AlgoSelector.SelectedIndex;
            bool useSaltForLegacy = UseSaltLegacy.IsChecked ?? false;

            // Забираем настройки Argon2 заранее
            int argonMemory = (int)MemorySlider.Value;
            int argonIter = (int)IterSlider.Value;
            int argonParallel = (int)ThreadSlider.Value;

            // 2. ПОДГОТОВКА
            IPasswordService selectedHasher = selectedIndex switch
            {
                0 => _argon2,
                1 => _bcrypt,
                2 => _pbkdf2,
                _ => _legacy
            };

            if (selectedHasher is Argon2Service a2)
            {
                a2.MemoryKb = argonMemory * 1024;
                a2.Iterations = argonIter;
                a2.Parallelism = argonParallel;
            }

            try
            {
                ProgressArea.Visibility = Visibility.Visible;
                StartBtn.IsEnabled = false;
                StatusTxt.Text = "Reading Excel...";
                HashProgress.Value = 0;

                var users = await Task.Run(() => _excelService.ReadEmployees(_selectedFile));
                var results = new List<UserOutput>();

                // 3. ФОНОВЫЙ ПРОЦЕСС (Теперь тут нет обращений к UI!)
                await Task.Run(() =>
                {
                    for (int i = 0; i < users.Count; i++)
                    {
                        HashResult hashResult;

                        // Используем локальную переменную useSaltForLegacy вместо UseSaltLegacy.IsChecked
                        if (selectedHasher is LegacyHashService legacy && !useSaltForLegacy)
                        {
                            hashResult = legacy.HashPlain(users[i].Password);
                        }
                        else
                        {
                            hashResult = selectedHasher.Hash(users[i].Password);
                        }

                        results.Add(new UserOutput(
                            users[i].Login,
                            hashResult.Hash,
                            hashResult.Salt,
                            selectedHasher.AlgorithmName + (!useSaltForLegacy && selectedHasher is LegacyHashService ? " (No Salt)" : "")));

                        // Прогресс обновляем через Dispatcher (это законно)
                        Dispatcher.Invoke(() => HashProgress.Value = (i + 1) * 100 / users.Count);
                    }
                });

                StatusTxt.Text = "Saving...";
                var savePicker = new SaveFileDialog { Filter = "Excel Files|*.xlsx", FileName = "Results.xlsx" };
                if (savePicker.ShowDialog() == true)
                {
                    await Task.Run(() => _excelService.SaveResults(savePicker.FileName, results));
                    MessageBox.Show("Success!", "Forge Complete");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                ProgressArea.Visibility = Visibility.Collapsed;
                StartBtn.IsEnabled = true;
            }
        }

        private void UpdateActiveSettingsInfo()
        {
            if (ActiveMethodTxt == null) return;

            var selectedItem = (AlgoSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            ActiveMethodTxt.Text = selectedItem ?? "Not Selected";

            if (AlgoSelector.SelectedIndex == 0) // Argon2
            {
                ActiveDetailsTxt.Text = $"{MemorySlider.Value} MB, {IterSlider.Value} Iter, {ThreadSlider.Value} Threads";
                ActiveDetailsTxt.Visibility = Visibility.Visible;
            }
            else if (AlgoSelector.SelectedIndex == 3) // Legacy
            {
                ActiveDetailsTxt.Text = UseSaltLegacy.IsChecked == true ? "With Salt" : "No Salt (Unsafe)";
                ActiveDetailsTxt.Visibility = Visibility.Visible;
            }
            else
            {
                ActiveDetailsTxt.Visibility = Visibility.Collapsed;
            }
        }

        // Генерация одиночного хэша
        private void GenerateSingleHash_Click(object sender, RoutedEventArgs e)
        {
            string password = SinglePasswordBox.Password;
            if (string.IsNullOrEmpty(password)) return;

            // Используем тот же метод выбора хэшера, что и для Excel (Strategy Pattern в действии!)
            IPasswordService selectedHasher = AlgoSelector.SelectedIndex switch
            {
                0 => _argon2,
                1 => _bcrypt,
                2 => _pbkdf2,
                _ => _legacy
            };

            // Применяем текущие настройки из UI (Memory, Iterations и т.д.)
            if (selectedHasher is Argon2Service a2)
            {
                a2.MemoryKb = (int)MemorySlider.Value * 1024;
                a2.Iterations = (int)IterSlider.Value;
                a2.Parallelism = (int)ThreadSlider.Value;
            }

            // Хешируем
            HashResult result;
            if (selectedHasher is LegacyHashService legacy && UseSaltLegacy.IsChecked == false)
            {
                result = legacy.HashPlain(password);
            }
            else
            {
                result = selectedHasher.Hash(password);
            }

            // Выводим результат
            ResultHashTxt.Text = result.Hash;
            ResultSaltTxt.Text = result.Salt ?? "No salt used";
            AlgoInfoTxt.Text = $"Algorithm: {selectedHasher.AlgorithmName}";
        }

        // Кнопки копирования
        private void CopyHash_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(ResultHashTxt.Text);
        private void CopySalt_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(ResultSaltTxt.Text);

        private readonly JwtService _jwtService = new();

        // Генерация ключа
        private void GenerateKey_Click(object sender, RoutedEventArgs e)
        {
            int bits = 256; // По умолчанию
            if (Rb128.IsChecked == true) bits = 128;
            if (Rb512.IsChecked == true) bits = 512;

            // Вызываем сервис из Core
            string newKey = _jwtService.GenerateSecretKey(bits);

            GeneratedKeyTxt.Text = newKey;
        }

        // Копирование
        private void CopyGeneratedKey_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GeneratedKeyTxt.Text))
            {
                Clipboard.SetText(GeneratedKeyTxt.Text);
                // Можно добавить маленькое уведомление или просто визуальный эффект
            }
        }

        private void UseSaltLegacy_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateActiveSettingsInfo();
        }

        private void UseSaltLegacy_Checked(object sender, RoutedEventArgs e)
        {
            UpdateActiveSettingsInfo();
        }
    }
}