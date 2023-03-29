using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.PerKeyLighting, FeatureId.PerKeyLightingV2)]
public class PerKeyLightning : AbstractFeature {
    public const int FuncSetSingleKeys = 0x01;
    public const int FuncSetKeyBulk    = 0x06;
    public const int FuncCommit        = 0x07;

    public PerKeyLightning(HidPp20Features features) : base(features, FeatureId.PerKeyLightingV2) { }
}