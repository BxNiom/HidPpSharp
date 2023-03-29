using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// This feature exposes and configures the smart shift enhanced functionality on a 3G/EPM wheel.
/// </summary>
[Feature(FeatureId.SmartShift)]
public class SmartShift : AbstractFeature {
    public enum WheelMode : byte {
        Freespin = 0x01,
        Ratchet  = 0x02
    }

    public const int FuncGetRatchetControlMode = 0x00;
    public const int FuncSetRatchetControlMode = 0x01;

    public SmartShift(HidPp20Features features) : base(features, FeatureId.SmartShift) { }

    /// <summary>
    /// Returns the current smartshift configuration.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public RatchetControlMode GetRatchetControlMode() {
        var response = CallFunction(FuncGetRatchetControlMode);
        if (response.IsSuccess) {
            return new RatchetControlMode {
                WheelMode            = (WheelMode)response[0],
                AutoDisengage        = response[1],
                AutoDisengageDefault = response[2]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the wheel mode and configures the automatic disengage setting.
    /// Setting the wheel mode by writing a nonzero value will cause the ratchet engagement motor/EPM to activate as
    /// needed to immediately put the ratchet in the requested state. Writing zero (0 = do not change) to the wheel mode
    /// value will not activate the motor/EPM.
    /// Changing the autoDisengage or autoDisengageDefault values will not activate the motor/EPM.
    /// </summary>
    public RatchetControlMode SetRatchetControlMode(RatchetControlMode mode) {
        var response = CallFunction(FuncSetRatchetControlMode, (byte)mode.WheelMode, mode.AutoDisengage,
            mode.AutoDisengageDefault);
        if (response.IsSuccess) {
            return new RatchetControlMode {
                WheelMode            = (WheelMode)response[0],
                AutoDisengage        = response[1],
                AutoDisengageDefault = response[2]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public struct RatchetControlMode {
        /// <summary>
        /// The wheel mode.
        /// Note: that the wheel mode can also be changed by the user pressing the ratchet control button.
        /// </summary>
        public WheelMode WheelMode;

        /// <summary>
        /// The speed at which the ratchet automatically disengages
        /// 0x01 - 0xFE = Disengage when wheel rotation exceeds N/4 turns per second
        ///        0xFF = Ratchet always engaged
        /// </summary>
        public byte AutoDisengage;

        /// <summary>
        /// The default value of the autoDisengage setting
        /// 0x01 - 0xFE = Disengage when wheel rotation exceeds N/4 turns per second
        ///        0xFF = Ratchet always engaged
        /// </summary>
        public byte AutoDisengageDefault;
    }
}