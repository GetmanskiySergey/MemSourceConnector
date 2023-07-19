using MemSourceConnector.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace MemSourceConnector
{
    /// <summary>
    /// App main window
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region fields

        private MemsourceApi _memApi;
        private readonly SynchronizationContext _sync = SynchronizationContext.Current;

        /// <summary>
        /// Memsource api url
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// User login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Db output folder
        /// </summary>
        public string OutputDir { get; set; }

        /// <summary>
        /// Create in outpute folder sub folders
        /// </summary>
        public bool OutputGroupByDate { get; set; }

        /// <summary>
        /// Zip downloaded Dbs
        /// </summary>
        public bool OutputZip { get; set; }

        /// <summary>
        /// Open folder after download
        /// </summary>
        public bool OutputopenAfterDownload { get; set; }

        /// <summary>
        /// Info messages list
        /// </summary>
        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Exported types
        /// </summary>
        public IEnumerable<ExportType> ExportFormats { get; }

        /// <summary>
        /// Current export type
        /// </summary>
        public ExportType ExportFormat { get; set; }

        /// <summary>
        /// List of transmem databases
        /// </summary>
        public ObservableCollection<TransmemItem> TransMemories { get; }
            = new ObservableCollection<TransmemItem>();

        /// <summary>
        /// Current selected transmem DB
        /// </summary>
        public TransmemItem CurrentTransMemory { get; set; }

        #endregion //fileds

        #region constructor

        public MainWindow()
        {
            ExportFormats = Enum.GetValues(typeof(ExportType)).Cast<ExportType>();

            InitializeComponent();

            ReadConfiguration();

            DataContext = this;
            Closing += MainWindow_Closing;
        }

        #endregion //constructor

        #region methods

        private void ReadConfiguration()
        {
            try
            {
                Login = ConfigurationManager.AppSettings.Get("Username");
                tbPassword.Password = ConfigurationManager.AppSettings.Get("Password");
                ApiUrl = ConfigurationManager.AppSettings.Get("ApiUrl");
                OutputDir = ConfigurationManager.AppSettings.Get("OutputDir");

                bool.TryParse(ConfigurationManager.AppSettings.Get("OutputGroupByDate"), out bool outputGroupByDate);
                OutputGroupByDate = outputGroupByDate;

                bool.TryParse(ConfigurationManager.AppSettings.Get("OutputZip"), out bool outputZip);
                OutputZip = outputZip;

                bool.TryParse(ConfigurationManager.AppSettings.Get("OutputopenAfterDownload"), out bool outputopenAfterDownload);
                OutputopenAfterDownload = outputopenAfterDownload;

                var expType = ConfigurationManager.AppSettings.Get("OutputType");

                if (!string.IsNullOrEmpty(expType) && Enum.TryParse<ExportType>(expType, out ExportType expTypeEnum))
                    ExportFormat = expTypeEnum;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show($"Ошибка чтения конфигурации: {ex.Message}. Продолжить?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(OutputDir))
                OutputDir = AppDomain.CurrentDomain.BaseDirectory;
        }

        private void SaveConfiguration()
        {
            try
            {
                var login = ConfigurationManager.AppSettings.Get("Username");
                var pass = ConfigurationManager.AppSettings.Get("Password");
                var apiUrl = ConfigurationManager.AppSettings.Get("ApiUrl");
                var outputDir = ConfigurationManager.AppSettings.Get("OutputDir");
                var expType = ConfigurationManager.AppSettings.Get("OutputType");

                bool.TryParse(ConfigurationManager.AppSettings.Get("OutputGroupByDate"), out bool outputGroupByDate);
                bool.TryParse(ConfigurationManager.AppSettings.Get("OutputZip"), out bool outputZip);
                bool.TryParse(ConfigurationManager.AppSettings.Get("OutputopenAfterDownload"), out bool outputopenAfterDownload);

                Enum.TryParse(expType ?? string.Empty, out ExportType expTypeEnum);

                if (!string.Equals(Login, login)
                    || !string.Equals(pass, tbPassword.Password)
                    || !string.Equals(apiUrl, ApiUrl)
                    || !string.Equals(outputDir, OutputDir)
                    || expTypeEnum != ExportFormat
                    || outputGroupByDate != OutputGroupByDate
                    || outputZip != OutputZip
                    || outputopenAfterDownload != OutputopenAfterDownload)
                {
                    if (MessageBox.Show("Сохранить конфигурацию?", "Закрытие", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);
                        config.AppSettings.Settings.Remove("Username");
                        config.AppSettings.Settings.Remove("Password");
                        config.AppSettings.Settings.Remove("ApiUrl");
                        config.AppSettings.Settings.Remove("OutputDir");
                        config.AppSettings.Settings.Remove("OutputType");
                        config.AppSettings.Settings.Remove("OutputGroupByDate");
                        config.AppSettings.Settings.Remove("OutputZip");
                        config.AppSettings.Settings.Remove("OutputopenAfterDownload");

                        config.AppSettings.Settings.Add("Username", Login);
                        config.AppSettings.Settings.Add("Password", tbPassword.Password);
                        config.AppSettings.Settings.Add("ApiUrl", ApiUrl);
                        config.AppSettings.Settings.Add("OutputDir", OutputDir);
                        config.AppSettings.Settings.Add("OutputType", ExportFormat.ToString());
                        config.AppSettings.Settings.Add("OutputGroupByDate", OutputGroupByDate.ToString());
                        config.AppSettings.Settings.Add("OutputZip", OutputZip.ToString());
                        config.AppSettings.Settings.Add("OutputopenAfterDownload", OutputopenAfterDownload.ToString());
                        config.Save(ConfigurationSaveMode.Minimal);
                    }
                }
            }
            catch
            { }
        }

        public static void Compress(string sourceFile, string compressedFile)
        {
            if (!File.Exists(sourceFile))
                return;

            // поток для чтения исходного файла
            using (var sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                // поток для записи сжатого файла
                using (var targetStream = File.Create(compressedFile))
                {
                    // поток архивации
                    using (var compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        #endregion //methods

        #region handlers

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveConfiguration();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TransMemories.Clear();

                if (_memApi == null)
                    _memApi = new MemsourceApi(ApiUrl, Login, tbPassword.Password);

                var res = _memApi.GetTransMemories();

                foreach (var item in res.Select(x => new TransmemItem() { Content = x }))
                    TransMemories.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения БД переводов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TransMemories)
                item.IsChecked = true;
        }

        private void BnUnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TransMemories)
                item.IsChecked = false;
        }

        private async void BnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_memApi == null)
                {
                    MessageBox.Show("Сперва выполните загрузку списка БД переводов", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (_memApi == null)
                {
                    MessageBox.Show("Сперва выполните загрузку списка БД переводов", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var ids = TransMemories.Where(x => x.IsChecked).Select(x => x.Content).ToArray();

                if (!ids.Any())
                {
                    MessageBox.Show("Не выбраны выгружаемые БД переводов", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var folder = OutputDir;

                if (OutputGroupByDate)
                    folder = Path.Combine(OutputDir, DateTime.Now.ToString("yyyy_MM_dd_hh_mm"));

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var sb = new StringBuilder();
                var counter = 0;
                var ld = new Load() { Owner = this };
                ld.PgBar.Maximum = ids.Length;
                ld.Show();
                ld.LdText.Text = "Загрузка БД";

                var files = await _memApi?.ExportTransMemories(ids, ExportFormat, folder, OutputZip, (state) =>
                {
                    sb.AppendLine(state);
                    counter++;

                    _sync.Post(s =>
                    {
                        try
                        {
                            ld.PgBar.Value = counter;
                            Messages.Insert(0, state);
                        }
                        catch (Exception ex)
                        {
                            Messages.Insert(0, ex.Message);
                        }

                    }, null);
                });

                try
                {
                    File.WriteAllText(Path.Combine(OutputDir, "log.txt"), sb.ToString() + Environment.NewLine + string.Join(",", ids.SelectMany(x => x.name)));
                }
                catch
                { }

                try
                {
                    if (OutputZip && files?.Any() == true)
                    {
                        ld.LdText.Text = "Архивирование...";

                        if (OutputGroupByDate)
                        {
                            var targetZip = $"{folder}.zip";

                            if (File.Exists(targetZip))
                                File.Delete(targetZip);

                            ZipFile.CreateFromDirectory(folder, targetZip);
                            Directory.Delete(folder, true);
                        }
                        else
                        {
                            foreach (var file in files)
                            {
                                var targetZip = $"{file}.zip";

                                if (File.Exists(targetZip))
                                    File.Delete(targetZip);

                                Compress(file, $"{file}.zip");
                                File.Delete(file);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка архивации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                ld?.Close();

                if (OutputopenAfterDownload)
                    Process.Start("explorer.exe", OutputDir);
                else
                    MessageBox.Show($"Загрузка завершена", "Загрузка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки БД переводов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BnSelectOutputDir_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();

            var result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
            {
                OutputDir = dlg.SelectedPath;
                DoPropertyChanged(nameof(OutputDir));
            }
        }

        private void BnOpenOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(OutputDir) && Directory.Exists(OutputDir))
                    Process.Start("explorer.exe", OutputDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки БД переводов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion //handlers

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void DoPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion //INotifyPropertyChanged
    }
}
