using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.KeyboardDisableKeys)]
public class DisableKeys : Feature {
    [Flags]
    public enum Keys : byte {
        CapsLock   = 0x01,
        NumLock    = 0x02,
        ScrollLock = 0x04,
        Insert     = 0x08,
        Windows    = 0x10
    }

    public const int FuncGetCapabilities = 0x00;
    public const int FuncGetDisabledKeys = 0x01;
    public const int FuncSetDisabledKeys = 0x02;

    public DisableKeys(HidPp20Features features) : base(features, FeatureId.KeyboardDisableKeys) { }

    public Keys GetCapabilities() {
        var response = CallFunction(FuncGetCapabilities);
        return response.IsSuccess ? (Keys)response[0] : throw new FeatureException(FeatureId, response);
    }

    public Keys GetDisabledKeys() {
        var response = CallFunction(FuncGetDisabledKeys);
        return response.IsSuccess ? (Keys)response[0] : throw new FeatureException(FeatureId, response);
    }

    public Keys SetDisabledKeys(Keys keys) {
        var response = CallFunction(FuncSetDisabledKeys, (byte)keys);
        return response.IsSuccess ? (Keys)response[0] : throw new FeatureException(FeatureId, response);
    }
}