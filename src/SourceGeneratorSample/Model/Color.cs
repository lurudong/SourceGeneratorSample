using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace SourceGeneratorSample.Model;

partial struct Color(byte a, byte r, byte g, byte b) : IEquatable<Color>,
       IEqualityOperators<Color, Color, bool>
{
    public byte A { get; } = a;

    public byte R { get; } = r;

    public byte G { get; } = g;

    public byte B { get; } = b;

    public override string ToString()
    {
        return base.ToString();
    }

    public override int GetHashCode()
    {
        return A << 24 | R << 16 | G << 8 | B;
    }

    public bool Equals(Color other)
    {
        return A == other.A && R == other.R && G == other.G && B == other.B;
    }


    [Deconstruct]
    public partial void Deconstruct(out byte r, out byte g, out byte b);

    [Deconstruct]
    public partial void Deconstruct(out byte a, out byte r, out byte g, out byte b);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Color comparer && base.Equals(comparer);
    }

    public static bool operator ==(Color left, Color right)
    {

        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !(left == right);
    }
}