namespace HidPpSharp.HidPp20;

public readonly struct FeatureInfo {
    public readonly int         Index;
    public readonly ushort      Code;
    public readonly byte        Version;
    public readonly FeatureType Type;

    internal FeatureInfo(int index, ushort code, FeatureType type, byte version) {
        Index   = index;
        Code    = code;
        Version = version;
        Type    = type;
    }

    public override string ToString() {
        return $"{Index:00}: {(FeatureId)Code} ({Code:X4}) V{Version}   {Type}";
    }
}