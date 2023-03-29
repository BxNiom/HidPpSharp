using HidPpSharp.HidPp20;

namespace DeviceSniffer; 

public class DevFeature : AbstractFeature {
    public DevFeature(HidPp20Features features, FeatureId featureId) : base(features, featureId, null) { }
}