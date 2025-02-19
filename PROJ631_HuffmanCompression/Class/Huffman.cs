using System.Text;
using System.Text.RegularExpressions;
using PROJ631_HuffmanCompression.Class.Entity;

namespace PROJ631_HuffmanCompression.Class;

public abstract class Huffman
{
    protected BNGraph<int> huffmanGraph;
    protected Dictionary<char, int> frequencies = new Dictionary<char, int>();
    protected Dictionary<char, byte[]> charBytes = new Dictionary<char, byte[]>();

    public BNGraph<int> HuffmanGraph
    {
        get => huffmanGraph;
        set => huffmanGraph = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Dictionary<char, int> Frequencies
    {
        get => frequencies;
        set => frequencies = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Dictionary<char, byte[]> CharBytes
    {
        get => charBytes;
        set => charBytes = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    protected BNGraph<int> GenerateHuffmanGraph()
    {
        List<BNGraph<int>> graphs = new List<BNGraph<int>>();
        foreach (char key in this.frequencies.Keys) {
            Node<int> leaf = new Node<int>(key.ToString(), this.frequencies[key]);
            graphs.Add(new BNGraph<int>(leaf));
        }

        while (graphs.Count > 1)
        {
            graphs = graphs.OrderBy(g => g.Root?.Value)
                .ThenBy(g => g.Root?.Tag)
                .Reverse()
                .ToList();
            
            BNGraph<int> firstLessGraph = graphs[^1];
            BNGraph<int> secondLessGraph = graphs[^2];
            int sum = firstLessGraph.SumLeaf() + secondLessGraph.SumLeaf();
            Node<int> root = new Node<int>(
                $"_{sum}_",
                sum,
                firstLessGraph.Root,
                secondLessGraph.Root
            );
            BNGraph<int> graph = new BNGraph<int>(root);
            
            graphs.Add(graph);
            graphs.Remove(firstLessGraph);
            graphs.Remove(secondLessGraph);
        }
        return graphs[0];
    }
    
    protected Dictionary<char, byte[]> generateByteMap()
    {
        Dictionary<char, byte[]> charBytes = new Dictionary<char, byte[]>();
        foreach (char character in this.frequencies.Keys)
        {
            byte[] charBit = this.huffmanGraph.BinaryPath(character.ToString());
            charBytes.Add(character, charBit);
        }
        return charBytes;
    }
    
    public double AverageByteCompression()
    {
        int sum = 0;
        foreach (byte[] bytes in this.charBytes.Values) {
            sum += bytes.Count();
        }
        return sum / this.charBytes.Keys.Count();
    }

    public string parseFrequenciesToTxt()
    {
        string txt = $"{this.frequencies.Keys.Count}\n";
        foreach (char key in this.frequencies.Keys) {
            string s = key.ToString();
            if (Regex.IsMatch(s, @"\s")) // If escaping caracter => \ascii\ASCII
                s = "\\ascii\\" + System.Convert.ToInt32(key);
            txt += @$"{s} {this.frequencies[key]}";
            if (this.frequencies.Keys.ToList().IndexOf(key) != this.frequencies.Keys.Count - 1)
                txt += $"\n";
        }
        return txt;
    }
    
    public static Dictionary<char, int> parseTxtToFrequence(string frequencyToString) {
        Dictionary<char, int> frequency = new Dictionary<char, int>();
        foreach (string line in frequencyToString.Split('\n')) {
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

        return frequency;
    }
    
    public static double CompressRatio(string txt, byte[] binary) {
        double byteBinary = binary.Count();
        double byteTxt = txt.Count() * 8;
        return 1 - (byteBinary / byteTxt);
    }
}