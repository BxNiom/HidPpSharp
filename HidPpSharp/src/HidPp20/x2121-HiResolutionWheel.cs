using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

// TODO Events

/// <summary>
/// Reporting of hi-res wheel events.
/// A hi-res wheel must have a freewheel or microratchet mode (more than one microratchet per
/// normal scroll unit) to make the high resolution useful. It may have a user controlled mechanism to
/// switch to normal ratchet mode. If this ratchet control is motorized or has a mechanical switch for
/// the firmware to detect the state, then the wheel has the "ratchet switch" capability described in this
/// feature.
/// The high-resolution setting is applied both when the wheel is reported natively via HID and when
/// diverted to software.
/// The invert setting applies only to values reported natively via HID. Values report via HID++ are
/// unaffected. When active it causes reporting of the additive inverse of normal wheel motion.
/// Normal motion reporting gives positive values when the wheel is moved away from the user.
/// Inverted motion reporting gives negative values when the wheel is moved away from the user.
/// The period count reported is the number of reporting periods that motion took to be reported. It
/// will normally be 1, but may be greater when the HID report is delayed due to HID++ reports being
/// sent. The initial periods of no motion preceding the first motion report do not count towards the
/// number of periods.
/// </summary>
[Feature(FeatureId.HiResWheel)]
public class HiResWheel : Feature {
    [Flags]
    public enum WheelMode : byte {
        /// <summary>
        /// If set HID++ notifications are used, otherwise HID
        /// </summary>
        HidPpNotification = 0x01,

        /// <summary>
        /// If set high resolution is used, otherwise low resolution
        /// </summary>
        HighResolution = 0x02,

        /// <summary>
        /// If set HID reported motion is inverted
        /// </summary>
        Inverted = 0x04,

        /// <summary>
        /// If set some project-specific analytics data will be reported 
        /// </summary>
        CollectAnalyticsData = 0x08
    }

    public const int FuncGetWheelCapability    = 0x00;
    public const int FuncGetWheelMode          = 0x01;
    public const int FuncSetWheelMode          = 0x02;
    public const int FuncGetRatchetSwitchState = 0x03;
    public const int FuncGetAnalyticsData      = 0x04;

    public HiResWheel(HidPp20Features features) : base(features, FeatureId.HiResWheel) { }

    public WheelCapabilities GetWheelCapability() {
        var response = CallFunction(FuncGetWheelCapability);
        if (response.IsSuccess) {
            return new WheelCapabilities {
                Multiplier             = response[0],
                RatchetsPerRotation    = response[2],
                WheelDiameter          = response[3],
                HasRatchetSwitch       = response[1].IsBitSet(2),
                CanInvertHidMotion     = response[1].IsBitSet(3),
                CanReportAnalyticsData = response[1].IsBitSet(4)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public WheelMode GetWheelMode() {
        var response = CallFunction(FuncGetWheelMode);
        return response.IsSuccess ? (WheelMode)response[0] : throw new FeatureException(FeatureId, response);
    }

    public WheelMode SetWheelMode(WheelMode mode) {
        var response = CallFunction(FuncSetWheelMode, (byte)mode);
        return response.IsSuccess ? (WheelMode)response[0] : throw new FeatureException(FeatureId, response);
    }

    public bool GetRatchetSwitchState() {
        var response = CallFunction(FuncGetRatchetSwitchState);
        return response.IsSuccess ? response[0].IsBitSet(0) : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns project specific analytics data related to high-resolution wheels. The analytics data can be retrieved
    /// if the capability "CanReportAnalyticsData" is supported. After retrieving the data, it will be cleaned.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public byte GetAnalyticsData() {
        var response = CallFunction(FuncGetAnalyticsData);
        return response.IsSuccess ? response[0] : throw new FeatureException(FeatureId, response);
    }

    public struct WheelCapabilities {
        public int  Multiplier;
        public int  RatchetsPerRotation;
        public int  WheelDiameter;
        public bool HasRatchetSwitch;
        public bool CanInvertHidMotion;
        public bool CanReportAnalyticsData;
    }
}