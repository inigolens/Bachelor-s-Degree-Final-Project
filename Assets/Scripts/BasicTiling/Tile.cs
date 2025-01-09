
using System;

public class Tile : IComparable<Tile>
{
    public int X;
    public int Y;
    public int[] options;
    public bool collapse = false;

    public int CompareTo(Tile other)
    {
        return options.Length.CompareTo(other.options.Length);
    }
    public override string ToString()
    {
        return $"X: {X}, Y: {Y}, Lenght: {options.Length}, Collapse: {collapse}";
    }
}
