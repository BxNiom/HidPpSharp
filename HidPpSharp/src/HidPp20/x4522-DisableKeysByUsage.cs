using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// This feature provides the ability to disable/enable any keys by HID usage.
/// </summary>
[Feature(FeatureId.KeyboardDisableByUsage)]
public class DisableKeysByUsage : Feature {
    public const int FuncGetCapabilities = 0x00;
    public const int FuncDisableKeys     = 0x01;
    public const int FuncEnableKeys      = 0x02;
    public const int FuncEnableAllKeys   = 0x03;

    public DisableKeysByUsage(HidPp20Features features) : base(features, FeatureId.KeyboardDisableKeys) { }

    /// <summary>
    /// Returns the capabilities of this feature.
    /// Note: The maximum number of usages can exceed the number of usages that be sent at a time on a HID packet.For
    /// example, a HID long packet can  disable 16 usages at a time, but we may wish for 32 usages to be disabled in total.
    /// So we must send 2 long packets to disable 32 usage.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public int GetMaxDisabledUsages() {
        var response = CallFunction(FuncGetCapabilities);
        return response.IsSuccess ? response[0] : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Disable a list of 8-bit keyboard key usages. Multiple calls to 'disableKeys' will add to the set of disabled
    /// keys, not replace them. It will not return an error when keys already disable or disabling a key that does not
    /// exist on the keyboard.
    /// </summary>
    /// <param name="keysToDisable">it is a list of 8-bit keyboard key HID usage to be disabled. A usage of 0 indicates the end of the list.</param>
    /// <exception cref="FeatureException">
    /// HIDPP_NO_ERROR when setting correct.
    /// HIDPP_ERR_INVALID_ARGUMENT if reach the maximum number of keys.
    /// </exception>
    public void DisableKeys(params byte[] keysToDisable) {
        var response = CallFunction(FuncDisableKeys, keysToDisable);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Enable a list of 8-bit keyboard key usages. It does nothing and returns success when enabling a key that does
    /// not exist on the disable key list.
    /// </summary>
    /// <param name="keysToDisable">it is a list of 8-bit keyboard key HID usage to be enabled. A usage of 0 indicates
    /// the end of the list.</param>
    /// <exception cref="FeatureException"></exception>
    public void EnableKeys(params byte[] keysToDisable) {
        var response = CallFunction(FuncEnableKeys, keysToDisable);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Enable all keyboard key HID usages.
    /// </summary>
    /// <exception cref="FeatureException"></exception>
    public void EnableAllKeys() {
        var response = CallFunction(FuncEnableAllKeys);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }
}