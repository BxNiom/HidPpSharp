using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// This feature exposes and configures the smart shift enhanced functionality on a 3G/EPM wheel (threshold and tunable
/// torque)
/// </summary>
[Feature(FeatureId.SmartShiftEnhanced)]
public class SmartShiftEnhanced : Feature {
    public enum WheelMode : byte {
        DoNotChange = 0x00,
        Freespin    = 0x01,
        Ratchet     = 0x02
    }

    public const int FuncGetCapabilities       = 0x00;
    public const int FuncGetRatchetControlMode = 0x01;
    public const int FuncSetRatchetControlMode = 0x02;

    public SmartShiftEnhanced(HidPp20Features features) : base(features, FeatureId.SmartShiftEnhanced) { }

    /// <summary>
    /// Returns the EPM configuration capabilities.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public Capabilities GetCapabilities() {
        var response = CallFunction(FuncGetCapabilities);
        if (response.IsSuccess) {
            return new Capabilities {
                TunableTorqueSupported = response[0].IsBitSet(0),
                AutoDisengageDefault   = response[1],
                DefaultTunableTorque   = response[2],
                MaxForce               = response[3]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns the current smart shift configuration.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public RatchetControlMode GetRatchetControlMode() {
        var response = CallFunction(FuncGetRatchetControlMode);
        if (response.IsSuccess) {
            return new RatchetControlMode {
                WheelMode            = (WheelMode)response[0],
                AutoDisengage        = response[1],
                CurrentTunableTorque = response[2]
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
        var response = CallFunction(FuncSetRatchetControlMode,
            (byte)mode.WheelMode, mode.AutoDisengage, mode.CurrentTunableTorque);
        if (response.IsSuccess) {
            return new RatchetControlMode {
                WheelMode            = (WheelMode)response[0],
                AutoDisengage        = response[1],
                CurrentTunableTorque = response[2]
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
        /// 0x01 - 0xFE = Disengage when wheel rotation exceeds N/4 turns per second (i.e., increment in 0.25 turn/s)
        ///        0xFF = Ratchet always engaged
        /// </summary>
        public byte AutoDisengage;

        /// <summary>
        /// 1%-100% = % of the max tunable force
        /// </summary>
        public byte CurrentTunableTorque;
    }

    public struct Capabilities {
        public bool TunableTorqueSupported;
        public byte AutoDisengageDefault;
        public byte DefaultTunableTorque;
        public byte MaxForce;
    }
}