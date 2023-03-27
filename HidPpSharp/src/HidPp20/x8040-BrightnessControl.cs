using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.BrightnessControl)]
public class BrightnessControl : Feature {
    public BrightnessControl(HidPp20Features features) : base(features, FeatureId.BrightnessControl) {
        throw new NotImplementedException();
    }
}