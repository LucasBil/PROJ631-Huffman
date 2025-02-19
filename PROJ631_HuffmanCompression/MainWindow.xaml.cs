using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PROJ631_HuffmanCompression.Class;
using PROJ631_HuffmanCompression.Class.Entity;

namespace PROJ631_HuffmanCompression;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    // Shortcut
    public static readonly RoutedUICommand Compress = new RoutedUICommand(
        "Compress",  // Nom de la commande
        "Compress",  // Identifiant de la commande
        typeof(MainWindow), // Type propriétaire
        new InputGestureCollection()
        {
            new KeyGesture(Key.C, ModifierKeys.Alt) // Raccourci clavier par défaut
        }
    );
    
    public static readonly RoutedUICommand Decompress = new RoutedUICommand(
        "Decompress",  // Nom de la commande
        "Decompress",  // Identifiant de la commande
        typeof(MainWindow), // Type propriétaire
        new InputGestureCollection()
        {
            new KeyGesture(Key.D, ModifierKeys.Alt) // Raccourci clavier par défaut
        }
    );

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    void NewFile_Command(object sender, RoutedEventArgs e)
    {
        this.FileText.Text = string.Empty;
    }

    void OpenFile_Command(object sender, RoutedEventArgs e)
    {
        var focus = FocusManager.GetFocusedElement(this) as FrameworkElement;
        TextBox textBox = focus is not TextBox ? FileText : focus as TextBox;
        
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
        bool? result = dialog.ShowDialog();
        if (result == true) {
            using (StreamReader reader = new StreamReader(dialog.FileName))
            {
                textBox.Text = reader.ReadToEnd();
            }
        }
    }
    
    void SaveFiles_Command(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFolderDialog dialog = new Microsoft.Win32.OpenFolderDialog();
        bool? result = dialog.ShowDialog();
        if (result == true) {
            try
            {
                string now = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                DirectoryInfo path = System.IO.Directory.CreateDirectory($"{dialog.FolderName}/compress_{now}");
                File.WriteAllText($"{path.FullName}/data.txt", this.FileText.Text);
                File.WriteAllText($"{path.FullName}/data_bin.bin", this.BinaryFile.Text);
                File.WriteAllText($"{path.FullName}/data_freq.txt", this.FrequencyFile.Text);
            } catch (Exception ex) {
                MessageBox.Show($"Error saving file : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    void Exit_Command(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }
    
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    void Compress_OnClick(object sender, RoutedEventArgs e)
    {
        this.BinaryFile.Text = String.Empty;
        this.FrequencyFile.Text = String.Empty;
        Dictionary<char, int> frequency = Huffman.ParseFrequence(this.FileText.Text);
        BNGraph<int> graph = Huffman.GenerateHuffmanGraph(frequency);
        
        this.BinaryFile.Text += Huffman.ParseStringToBinary(graph, this.FileText.Text);
        this.FrequencyFile.Text += $"{frequency.Keys.Count}\n";
        foreach (char key in frequency.Keys)
        {
            string s = key.ToString();
            // If escaping caracter => \ascii\ASCII
            if (Regex.IsMatch(s, @"\s"))
                s = "\\ascii\\" + System.Convert.ToInt32(key);
            this.FrequencyFile.Text += @$"{s} {frequency[key]}";
            if (frequency.Keys.ToList().IndexOf(key) != frequency.Keys.Count - 1)
                this.FrequencyFile.Text += $"\n";
        }

        double avgByteChar = Huffman.AverageByteCompression(graph);
        double compressRatio = Huffman.CompressRatio(this.FileText.Text,this.BinaryFile.Text)*100;
        this.CompressBar.Value = compressRatio;
        this.InfoCompressBar.Text = $"{compressRatio}%";
        this.AverageByteText.Text = $"{avgByteChar}";
    }
    
    void Decompress_OnClick(object sender, RoutedEventArgs e)
    {
        Dictionary<char, int> frequency = new Dictionary<char, int>();
        foreach (string line in this.FrequencyFile.Text.Split('\n')) {
            if (!line.Contains(' '))
                continue;

            int lastSeparator = line.LastIndexOf(' ');
            string c = line.Substring(0, lastSeparator);
            char character = '\0';
            int value = int.Parse(line.Substring(lastSeparator + 1));

            if (c.Contains("\\ascii\\")) {
                c = c.Replace("\\ascii\\", "");
                character = System.Convert.ToChar(int.Parse(c));
            } else {
                character = c[0];
            }
            
            frequency.Add(character, value);
        }

        BNGraph<int> graph = Huffman.GenerateHuffmanGraph(frequency);
        this.FileText.Text = Huffman.ParseBinaryToString(graph, this.BinaryFile.Text);
    }

    void StatePlaceholder_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!(sender is TextBox))
            return;
        
        TextBox textBox = sender as TextBox;
        string name = textBox.Name;
        Label label = this.FindName($"Placeholder_{name}") as Label;
        label.Visibility = textBox.Text != "" ? Visibility.Hidden : Visibility.Visible;
    }

    void SaveCompressFiles_OnClick(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
        dialog.Filter = "Names File |*";
        dialog.FilterIndex = 1;
        bool? result = dialog.ShowDialog();
        if (result == true) {
            try {
                File.WriteAllText($"{dialog.FileName}_bin.bin", this.BinaryFile.Text);
                File.WriteAllText($"{dialog.FileName}_freq.txt", this.FrequencyFile.Text);
            } catch (Exception ex) {
                MessageBox.Show($"Error saving file : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    void SaveOrigineFile_OnClick(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
        dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
        dialog.FilterIndex = 1;
        bool? result = dialog.ShowDialog();
        if (result == true) {
            try {
                File.WriteAllText(dialog.FileName, this.FileText.Text);
            } catch (Exception ex) {
                MessageBox.Show($"Error saving file : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}