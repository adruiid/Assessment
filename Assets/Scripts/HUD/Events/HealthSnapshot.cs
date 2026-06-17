using System;

public readonly struct HealthSnapshot : IEquatable<HealthSnapshot>
{
    public HealthSnapshot(int current, int max)
    {
        Current = current;
        Max = max;
    }

    public int Current { get; }
    public int Max { get; }

    public bool Equals(HealthSnapshot other)
    {
        return Current == other.Current && Max == other.Max;
    }

    public override bool Equals(object obj)
    {
        return obj is HealthSnapshot other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Current * 397) ^ Max;
        }
    }
}
