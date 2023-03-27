using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.WirelessDeviceStatus)]
public class WirelessDeviceStatus : Feature {
    // TODO Events
    public WirelessDeviceStatus(HidPp20Features features) : base(features, FeatureId.WirelessDeviceStatus) { }
}