using System.Numerics;

namespace PROJ631_HuffmanCompression.Class.Entity;

public class BNGraph<T> where T : IComparable<T>, IAdditionOperators<T, T, T>
{
    private Node<T>? _root;

    public Node<T>? Root
    {
        get => _root;
        set => _root = value;
    }

    public BNGraph(Node<T>? root) {
        _root = root;
    }
    
    public BNGraph() : this(null) { }

    public T Sum() {
        if (this.Root is null)
            return default!;
        
        BNGraph<T> leftGraph = new BNGraph<T>(this.Root.Left);
        BNGraph<T> rightGraph = new BNGraph<T>(this.Root.Right);
        return this.Root.Value + leftGraph.Sum() + rightGraph.Sum();
    }
    
    public T SumLeaf() {
        if (this.Root is null)
            return default!;
        
        BNGraph<T> leftGraph = new BNGraph<T>(this.Root.Left);
        BNGraph<T> rightGraph = new BNGraph<T>(this.Root.Right);
        leftGraph.SumLeaf();
        rightGraph.SumLeaf();
        return this.Root.Value;
    }
    
    public override string ToString()
    {
        string txt = "";
        if (this.Root is null)
            return string.Empty;
        txt += $"{this.Root.Tag}";
        if (this.Root.Left != null) {
            BNGraph<T> leftGraph = new BNGraph<T>(this.Root.Left);
            txt += $"; {leftGraph}";
        }

        if (this.Root != null) {
            BNGraph<T> rightGraph = new BNGraph<T>(this.Root.Right);
            txt += $"; {rightGraph}";
        }
        return txt;
    }

    public byte[] BinaryPath(string value) {
        if (this.Root is null || value == this.Root.Tag)
            return [];
        
        BNGraph<T> leftGraph = new BNGraph<T>(this.Root.Left);
        BNGraph<T> rightGraph = new BNGraph<T>(this.Root.Right);

        if (leftGraph.InPath(value))
            return new byte[] { 0 }.Concat(leftGraph.BinaryPath(value)).ToArray();
        if (rightGraph.InPath(value))
            return new byte[] { 1 }.Concat(rightGraph.BinaryPath(value)).ToArray();
        
        return [];
    }

    public bool InPath(string value)
    {
        if (this.Root is null)
            return false;
        if (value == this.Root.Tag)
            return true;
        
        BNGraph<T> leftGraph = new BNGraph<T>(this.Root.Left);
        BNGraph<T> rightGraph = new BNGraph<T>(this.Root.Right);
        return leftGraph.InPath(value) || rightGraph.InPath(value);
    }

    public List<Node<T>> GetLeafs() {
        if (this.Root is null)
            return new List<Node<T>>();
        if (this.Root.IsLeaf())
            return new List<Node<T>> { this.Root };
        BNGraph<T> leftGraph = new BNGraph<T>(this.Root.Left);
        BNGraph<T> rightGraph = new BNGraph<T>(this.Root.Right);
        return leftGraph.GetLeafs().Concat(rightGraph.GetLeafs()).ToList();
    }
}