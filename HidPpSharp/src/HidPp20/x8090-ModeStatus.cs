using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// This interface is used to notify and be queried by software.
/// </summary>
[Feature(FeatureId.ModeStatus)]
public class ModeStatus : AbstractFeature {
    [Flags]
    public enum ConfigFlags : ushort {
        HardwareSwitch = 0x0001,
        SoftwareSwitch = 0x0002
    }

    public const int FuncGetModeStatus = 0x00;
    public const int FuncSetModeStatus = 0x01;
    public const int FuncGetDevConfig  = 0x02;

    public ModeStatus(HidPp20Features features) : base(features, FeatureId.ModeStatus) { }

    /// <summary>
    /// Retrieves information about the feature.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public bool GetModeStatus() {
        var response = CallFunction(FuncGetModeStatus);
        return response.IsSuccess ? response[0].IsBitSet(0) : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Apply the desired mode to the device.
    /// </summary>
    /// <param name="performanceMode"></param>
    /// <exception cref="FeatureException"></exception>
    public void SetModeStatus(bool performanceMode) {
        var response = CallFunction(FuncSetModeStatus, (byte)(performanceMode ? 0x01 : 0x00), 0x00, 0x01, 0x00);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Returns the configuration flags supported by the device.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public ConfigFlags GetDevConfig() {
        var response = CallFunction(FuncGetDevConfig);
        return response.IsSuccess ? (ConfigFlags)response[0] : throw new FeatureException(FeatureId, response);
    }
}