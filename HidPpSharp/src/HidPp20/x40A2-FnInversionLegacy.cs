using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// Legacy Support
/// </summary>
[Feature(FeatureId.FnInversionLegacy)]
public class FnInversionLegacy : AbstractFeature {
    public const int FuncGetGlobalFnInversion = 0x00;
    public const int FuncSetGlobalFnInversion = 0x01;

    public FnInversionLegacy(HidPp20Features features) : base(features, FeatureId.FnInversionLegacy) { }

    /// <summary>
    /// Returns the Fn Inversion state (common for all keys)
    /// When Fn Inversion is ON pressing Fn+FKey outputs FKey and pressing a FKey by itself performs the special function.
    /// When Fn Inversion is OFF pressing Fn+FKey performs the special function and pressing a FKey by itself outputs the FKey.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public (bool state, bool defaultState) GetGlobalFnInversion() {
        var response = CallFunction(FuncGetGlobalFnInversion);
        return response.IsSuccess
            ? new ValueTuple<bool, bool>(response[0].IsBitSet(0), response[0].IsBitSet(1))
            : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the Fn Inversion state (for all keys)
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public (bool state, bool defaultState) SetGlobalFnInversion(bool state) {
        var response = CallFunction(FuncSetGlobalFnInversion, (byte)(state ? 0x01 : 0x00));
        return response.IsSuccess
            ? new ValueTuple<bool, bool>(response[0].IsBitSet(0), response[0].IsBitSet(1))
            : throw new FeatureException(FeatureId, response);
    }
}