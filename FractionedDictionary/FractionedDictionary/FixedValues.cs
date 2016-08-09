namespace FractionedDictionary
{
    internal static class FixedValues
    {
        internal const int MaxIndirection = 3;
        internal const int ArraySize = 256;
        internal static readonly int[] PositionMask = { 0x00000000, 0x000000FF, 0x0000FFFF, 0x00FFFFFF };
        internal static readonly int[] LocalIndexShift = { 0, 8, 16, 24 };
    }
}