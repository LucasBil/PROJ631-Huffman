namespace PROJ631_HuffmanCompression.Class.Entity;

public class Node<T> where T : IComparable<T>
{
    private string _tag;
    private T _value;
    private Node<T>? _left;
    private Node<T>? _right;

    public string Tag
    {
        get => _tag;
        set => _tag = value ?? throw new ArgumentNullException(nameof(value));
    }

    public T Value
    {
        get => _value;
        set => _value = value;
    }

    public Node<T>? Left
    {
        get => _left;
        set => _left = value;
    }

    public Node<T>? Right
    {
        get => _right;
        set => _right = value;
    }

    public Node(string tag, T value, Node<T>? left, Node<T>? right) {
        _tag = tag;
        _value = value;
        _left = left;
        _right = right;
    }
    
    public Node(string tag, T value) : this(tag, value, null, null) {}

    public bool IsLeaf() {
        return _left == null && _right == null;
    }

    public bool Equals(string? value) => _tag.Equals(value);

    public int GetHashCode(StringComparison comparisonType) => _tag.GetHashCode(comparisonType);

    public override string ToString()
    {
        return $"{Tag} : {Value}";
    }
    
    public static bool operator <(Node<T> node1, Node<T> node2)
    {
        return node1.Value.CompareTo(node2.Value) < 0;
    }

    public static bool operator >(Node<T> node1, Node<T> node2)
    {
        return node1.Value.CompareTo(node2.Value) > 0;
    }
}