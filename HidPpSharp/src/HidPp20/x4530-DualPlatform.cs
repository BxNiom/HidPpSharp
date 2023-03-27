using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

// TODO Events
/// <summary>
/// To support dual platforms, device has a persistent "platform" setting which affects which HID codes are sent by certain
/// keys. The platform selection is set during pairing, and can be changed at any time by the user short pressing an OS
/// selection button.
/// There is no default setting since the platform is selected by the user as part of the pairing process.
/// </summary>
[Feature(FeatureId.DualPlatform)]
public class DualPlatform : Feature {
    public enum Platform {
        IOs     = 0x00,
        MacOsX  = 0x00,
        Android = 0x01,
        Windows = 0x01
    }

    public const int FuncGetPlatform = 0x00;
    public const int FuncSetPlatform = 0x00;

    public DualPlatform(HidPp20Features features) : base(features, FeatureId.DualPlatform) { }

    /// <summary>
    /// Returns the current platform setting in device
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public Platform GetPlatform() {
        var response = CallFunction(FuncGetPlatform);
        return response.IsSuccess ? (Platform)response[0] : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets a new value for the platform setting. Does not cause a platform change event to be sent by this feature.
    /// </summary>
    /// <param name="platform">New platform setting</param>
    /// <returns>Echo the platform parameter</returns>
    /// <exception cref="FeatureException"></exception>
    public Platform SetPlatform(Platform platform) {
        var response = CallFunction(FuncSetPlatform, (byte)platform);
        return response.IsSuccess ? (Platform)response[0] : throw new FeatureException(FeatureId, response);
    }
}