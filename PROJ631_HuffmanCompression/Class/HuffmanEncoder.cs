using PROJ631_HuffmanCompression.Class.Entity;

namespace PROJ631_HuffmanCompression.Class;

public class HuffmanEncoder : Huffman
{
    private string _baseTxt;

    public string BaseTxt
    {
        get => _baseTxt;
        set => _baseTxt = value ?? throw new ArgumentNullException(nameof(value));
    }

    public HuffmanEncoder(string baseTxt)
    {
        this._baseTxt = baseTxt;
        this.frequencies = this.generateFrequenceMap();
        this.huffmanGraph = this.GenerateHuffmanGraph();
        this.charBytes = this.generateByteMap();
    }
    
    private Dictionary<char, int> generateFrequenceMap() {
        Dictionary<char, int> frequency = new Dictionary<char, int>();
        foreach (char character in this._baseTxt) {
            frequency.TryAdd(character, 0);
            frequency[character]++;
        }
        return frequency.OrderBy(x => x.Value)
            .ThenBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
    }
    
    public byte[] Encode() {
        byte[] binary = [];
        foreach (char character in this._baseTxt) {
            binary = binary.Concat(this.charBytes[character]).ToArray();
        }
        return binary;
    }
}