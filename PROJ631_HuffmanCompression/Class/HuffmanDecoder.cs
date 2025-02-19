using PROJ631_HuffmanCompression.Class.Entity;

namespace PROJ631_HuffmanCompression.Class;

public class HuffmanDecoder : Huffman
{
    public HuffmanDecoder(Dictionary<char, int> frequency) {
        this.frequencies = frequency;
        this.huffmanGraph = this.GenerateHuffmanGraph();
        this.charBytes = this.generateByteMap();
    }

    public string Decode(byte[] binary)
    {
        string txt = "";
        Node<int>? currentNode = this.huffmanGraph.Root;
        foreach (byte bit in binary) {
            if (bit == 0)
                currentNode = currentNode?.Left;
            else
                currentNode = currentNode?.Right;

            if (currentNode.IsLeaf()) {
                txt += currentNode.Tag;
                currentNode = this.huffmanGraph.Root;
            }
        }
        return txt;
    }
}