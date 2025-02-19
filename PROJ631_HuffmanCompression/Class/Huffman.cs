using System.Text;
using PROJ631_HuffmanCompression.Class.Entity;

namespace PROJ631_HuffmanCompression.Class;

public static class Huffman
{
    public static Dictionary<char, int> ParseFrequence(string data) {
        Dictionary<char, int> frequency = new Dictionary<char, int>();
        foreach (char character in data) {
            frequency.TryAdd(character, 0);
            frequency[character]++;
        }
        return frequency.OrderBy(x => x.Value)
            .ThenBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public static BNGraph<int> GenerateHuffmanGraph(Dictionary<char, int> frequency)
    {
        List<BNGraph<int>> graphs = new List<BNGraph<int>>();
        foreach (char key in frequency.Keys) {
            Node<int> leaf = new Node<int>(key.ToString(), frequency[key]);
            graphs.Add(new BNGraph<int>(leaf));
        }

        while (graphs.Count > 1)
        {
            graphs = graphs.OrderBy(g => g.Root.Value)
                .ThenBy(g => g.Root.Tag)
                .Reverse()
                .ToList();
            
            BNGraph<int> firstLessGraph = graphs[graphs.Count - 1];
            BNGraph<int> secondLessGraph = graphs[graphs.Count - 2];
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

    public static string ParseStringToBinary(BNGraph<int> graph, string txt)
    {
        string binary = "";
        foreach (char character in txt) {
            binary += graph.BinaryPath(character.ToString());
        }
        return binary;
    }

    public static string ParseBinaryToString(BNGraph<int> graph, string binary)
    {
        string txt = "";
        Node<int> currentNode = graph.Root;
        foreach (char bit in binary) {
            if (bit == '0')
                currentNode = currentNode.Left;
            else
                currentNode = currentNode.Right;

            if (currentNode.IsLeaf()) {
                txt += currentNode.Tag;
                currentNode = graph.Root;
            }
        }
        return txt;
    }

    public static double AverageByteCompression(BNGraph<int> graph)
    {
        List<Node<int>> nodes = graph.GetLeafs();
        int sum = 0;
        foreach (Node<int> node in nodes) {
            sum += Huffman.ParseStringToBinary(graph, node.Tag).Count();
        }
        return sum / nodes.Count;
    }

    public static double CompressRatio(string txt, string binary) {
        byte[] origine_bytes = Encoding.UTF8.GetBytes(txt);
        byte[] compress_bytes = Encoding.UTF8.GetBytes(binary);
        return (1 - compress_bytes.Count()) / origine_bytes.Count();
    }
}