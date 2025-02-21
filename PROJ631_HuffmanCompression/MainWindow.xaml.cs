using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PROJ631_HuffmanCompression.Class;
using Microsoft.Msagl;

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
                byte[] bytes = this.BinaryFile.Text.Select(b => (byte)(b - '0')).ToArray();
                File.WriteAllText($"{path.FullName}/data.txt", this.FileText.Text);
                File.WriteAllBytes($"{path.FullName}/data_bin.bin", bytes);
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
        HuffmanEncoder encoder = new HuffmanEncoder(this.FileText.Text);
        
        byte[] bytes = encoder.Encode();
        this.BinaryFile.Text = string.Join("", bytes
            .Select(b => b.ToString()));
        this.FrequencyFile.Text = encoder.parseFrequenciesToTxt();

        double avgByteChar = encoder.AverageByteCompression();
        double compressRatio = double.Round(Huffman.CompressRatio(this.FileText.Text, bytes)*100,2);
        this.CompressBar.Value = compressRatio;
        this.InfoCompressBar.Text = $"{compressRatio}%";
        this.AverageByteText.Text = $"{avgByteChar}";
    }
    
    void Decompress_OnClick(object sender, RoutedEventArgs e)
    {
        HuffmanDecoder decoder = new HuffmanDecoder(
            Huffman.parseTxtToFrequence(this.FrequencyFile.Text)
            );

        byte[] bytes = [];
        foreach (char bit in this.BinaryFile.Text) {
            byte _bit = (byte)(bit - '0');
            bytes = bytes.Concat( new byte[] { _bit }).ToArray();
        }
        this.FileText.Text = decoder.Decode(bytes);
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
            try
            {
                byte[] bytes = this.BinaryFile.Text.Select(b => (byte)(b - '0')).ToArray();
                File.WriteAllBytes($"{dialog.FileName}_bin.bin", bytes);
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

    void DebugGraph_OnClick(object sender, RoutedEventArgs e)
    {
        // viewer object
        Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
        // graph object
        Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph();
        //bind the graph to the viewer
        viewer.Graph = graph;
        //create the graph content 
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
        graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
        graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
        Microsoft.Msagl.Drawing.Node c = graph.FindNode("C");
        c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
        c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;
    }
}