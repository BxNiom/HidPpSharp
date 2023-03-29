using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// The feature handles high resolution scrolling mode on mice. Ensure that feature 0x2100 is present (vertical roller
/// 0x2100) when using this feature.
/// </summary>
[Feature(FeatureId.HiResScrolling)]
public class HiResScrolling : AbstractFeature {
    public const int FuncGetHighResScrollingMode = 0x00;
    public const int FuncSetHighResScrollingMode = 0x01;

    public HiResScrolling(HidPp20Features features) : base(features, FeatureId.HiResScrolling) { }

    /// <summary>
    /// Returns the current state of roller high-res (highRes-enabled, highRes-disabled) and the resolution
    /// factor(multiplier) when in highRes
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public (bool enabled, int multiplier) GetHighResScrollingMode() {
        var response = CallFunction(FuncGetHighResScrollingMode);
        return response.IsSuccess
            ? new ValueTuple<bool, int>(response[0] == 0x00, response[1])
            : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the state of roller resolution (highRes-enabled, highRes-disabled). Returns confirmation of set mode and
    /// what's the resolution multiplier.
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public (bool enabled, int multiplier) SetHighResScrollingMode(bool enabled) {
        var response = CallFunction(FuncSetHighResScrollingMode, enabled ? (byte)0x01 : (byte)0x00);
        return response.IsSuccess
            ? new ValueTuple<bool, int>(response[0] == 0x00, response[1])
            : throw new FeatureException(FeatureId, response);
    }
}