using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// This feature provides control over brightness, color temperature, and pre-defined levels for illumination light
/// devices.
/// The unit for brightness values is Lumen.
/// The unit for color temperature values is Kelvin.
/// </summary>
[Feature(FeatureId.Illumination)]
public class Illumination : AbstractFeature {
    [Flags]
    public enum Capability : byte {
        /// <summary>
        /// The device supports change event notification for this particular control.
        /// </summary>
        Events = 0x01,

        /// <summary>
        /// The device supports linear levels for this particular control.
        /// </summary>
        LinearLevels = 0x02,

        /// <summary>
        /// The device supports non-linear levels for this particular control. If this flag is unset (0) maxLevels
        /// must be set to 0.
        /// </summary>
        NonLinearLevels = 0x04
    }

    public const int FuncGetIllumination           = 0x00;
    public const int FuncSetIllumination           = 0x01;
    public const int FuncGetBrightnessInfo         = 0x02;
    public const int FuncGetBrightness             = 0x03;
    public const int FuncSetBrightness             = 0x04;
    public const int FuncGetBrightnessLevels       = 0x05;
    public const int FuncSetBrightnessLevels       = 0x06;
    public const int FuncGetColorTemperatureInfo   = 0x07;
    public const int FuncGetColorTemperature       = 0x08;
    public const int FuncSetColorTemperature       = 0x09;
    public const int FuncGetColorTemperatureLevels = 0x0A;
    public const int FuncSetColorTemperatureLevels = 0x0B;

    public Illumination(HidPp20Features features) : base(features, FeatureId.Illumination) { }

    private Info ReadInfoFromResponse(IHidPpReport report) {
        return new Info {
            Capabilities = (Capability)report[0],
            Min          = report.ReadUInt16(1),
            Max          = report.ReadUInt16(3),
            Resolution   = report.ReadUInt16(5),
            MaxLevels    = report[7] & 0x0F
        };
    }

    private Levels ReadLevelsFromResponse(IHidPpReport report) {
        if (report[0].IsBitSet(0)) {
            return new LinearLevels(report);
        }

        return new LinearLevels(report);
    }

    /// <summary>
    /// Retrieves the current illumination state.
    /// </summary>
    /// <returns>True if enabled, otherwise false</returns>
    /// <exception cref="FeatureException"></exception>
    public bool GetIllumination() {
        var response = CallFunction(FuncGetIllumination);
        return response.IsSuccess ? response[0].IsBitSet(0) : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Turns the illumination on or off.
    /// </summary>
    /// <exception cref="FeatureException">
    /// * HW_ERROR if the illumination cannot currently be turned on (e.g. because of insufficient power).
    /// </exception>
    public void SetIllumination(bool state) {
        var response = CallFunction(FuncSetIllumination, state ? (byte)0x01 : (byte)0x00);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Returns information about the device’s brightness capabilities.
    /// </summary>
    public Info GetBrightnessInfo() {
        var response = CallFunction(FuncGetBrightnessInfo);
        if (response.IsSuccess) {
            return ReadInfoFromResponse(response);
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns the current hardware brightness value.
    /// </summary>
    public int GetBrightness() {
        var response = CallFunction(FuncGetBrightness);
        return response.IsSuccess ? response.ReadUInt16(0) : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the hardware brightness value.
    /// </summary>
    /// <exception cref="FeatureException">
    /// * NOT_ALLOWED if setting the control is not supported.
    /// * INVALID_ARGUMENT if the value is out of the [min, max] range or value − min is not a multiple of the resolution.
    /// </exception>
    public void SetBrightness(int value) {
        var response = CallFunction(FuncSetBrightness, ByteUtils.Pack((ushort)value));
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Returns the device’s current brightness level configuration.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <exception cref="FeatureException">
    /// * NOT_ALLOWED if levels are not supported for this control.
    /// * INVALID_ARGUMENT if startIndex is out of range (i.e. ≥ maxLevels).
    /// </exception>
    public Levels GetBrightnessLevels(int startIndex) {
        var response = CallFunction(FuncGetBrightnessLevels, (byte)(startIndex << 4));
        if (response.IsSuccess) {
            return ReadLevelsFromResponse(response);
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the device’s brightness level configuration.
    /// </summary>
    /// <exception cref="FeatureException">
    /// * NOT_ALLOWED if levels are not supported for this control.
    /// * INVALID_ARGUMENT with "T" if the level type (linear or non-linear) implied by the linear flag is not supported.
    /// * INVALID_ARGUMENT with "D" if the   payload data is too short.
    /// * INVALID_ARGUMENT with "MMS" if the conditions on levelMinValue, levelMaxValue, or levelStepValue are violated.
    /// * INVALID_ARGUMENT with "MMR" if the conditions on the control’s min, max, or res restrictions are violated.
    /// * INVALID_ARGUMENT with "LV" if the entire set of level values is otherwise inconsistent (e.g. not monotonically
    /// increasing). (Optional, see above.)
    /// * INVALID_ARGUMENT     with "VC" if validCount is out of range (i.e. < 1 or > 7).
    /// * INVALID_ARGUMENT     with "SI" if startIndex is out of range (i.e. ≥ maxLevels).
    /// * INVALID_ARGUMENT with "PC" if levelCount is out of range (i.e. > maxLevels).
    /// </exception>
    public void SetBrightnessLevels(Levels levels) {
        var response = CallFunction(FuncSetBrightnessLevels, levels.Pack());
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Returns information about the device’s color temperature capabilities.
    /// </summary>
    public Info GetColorTemperatureInfo() {
        var response = CallFunction(FuncGetColorTemperatureInfo);
        if (response.IsSuccess) {
            return ReadInfoFromResponse(response);
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns the current hardware color temperature value.
    /// </summary>
    public int GetColorTemperature() {
        var response = CallFunction(FuncGetColorTemperature);
        return response.IsSuccess ? response.ReadUInt16(0) : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the hardware color temperature value.
    /// </summary>
    /// <exception cref="FeatureException">
    /// * NOT_ALLOWED if setting the control is not supported.
    /// * INVALID_ARGUMENT if the value is out of the [min, max] range or value − min is not a multiple of the resolution.
    /// </exception>
    public void SetColorTemperature(int value) {
        var response = CallFunction(FuncSetColorTemperature, ByteUtils.Pack((ushort)value));
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Returns the device’s current color temperature level configuration.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <exception cref="FeatureException">
    /// * NOT_ALLOWED if levels are not supported for this control.
    /// * INVALID_ARGUMENT if startIndex is out of range (i.e. ≥ maxLevels).
    /// </exception>
    public Levels GetColorTemperatureLevels(int startIndex) {
        var response = CallFunction(FuncGetColorTemperatureLevels, (byte)(startIndex << 4));
        if (response.IsSuccess) {
            return ReadLevelsFromResponse(response);
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the device’s color temperature level configuration.
    /// </summary>
    /// <param name="levels"></param>
    /// <exception cref="FeatureException">
    /// * NOT_ALLOWED if levels are not supported for this control.
    /// * INVALID_ARGUMENT with "T" if the level type (linear or non-linear) implied by the linear flag is not supported.
    /// * INVALID_ARGUMENT with "D" if the   payload data is too short.
    /// * INVALID_ARGUMENT with "MMS" if the conditions on levelMinValue, levelMaxValue, or levelStepValue are violated.
    /// * INVALID_ARGUMENT with "MMR" if the conditions on the control’s min, max, or res restrictions are violated.
    /// * INVALID_ARGUMENT with "LV" if the entire set of level values is otherwise inconsistent (e.g. not monotonically
    ///   increasing). (Optional, see above.)
    /// * INVALID_ARGUMENT     with "VC" if validCount is out of range (i.e. < 1 or > 7).
    /// * INVALID_ARGUMENT     with "SI" if startIndex is out of range (i.e. ≥ maxLevels).
    /// * INVALID_ARGUMENT with "PC" if levelCount is out of range (i.e. > maxLevels).
    /// </exception>
    public void SetColorTemperatureLevels(Levels levels) {
        var response = CallFunction(FuncSetColorTemperatureLevels, levels.Pack());
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    public struct Info {
        public Capability Capabilities;

        /// <summary>
        ///  The minimum control value supported by the device.
        /// </summary>
        public int Min;

        /// <summary>
        /// The maximum control value supported by the device. If min == max only a single control setting
        /// is supported, in which case res must be zero.In such a case the setControl() function is not
        /// supported by the device.
        /// </summary>
        public int Max;

        /// <summary>
        /// The resolution (step size) of control values supported by the device.
        /// The following conditions must hold: min ≤ max && res > 0 && (max − min) % res == 0
        /// </summary>
        public int Resolution;

        /// <summary>
        /// The maximum number of control levels the device supports for non-linear levels. A value of 0
        /// indicates that non-linear   levels are not supported for this particular control and
        /// hasNonLinearLevels must not be set.If levels are not supported the hasLinearLevels and
        /// hasNonLinearLevels must both be 0.
        /// </summary>
        public int MaxLevels;
    }

    public abstract class Levels {
        protected Levels(bool isLinear) {
            IsLinear = isLinear;
        }

        protected Levels(IHidPpReport response) {
            IsLinear   = response[0].IsBitSet(0);
            StartIndex = (response[1] & 0xF0) >> 4;
            IsReset    = false;
        }

        /// <summary>
        /// If set true the level definition contains linear levels. Parameters exclusive to non-linear mode are
        /// ignored.
        /// If false the level definition contains non-linear levels.Parameters exclusive to linear mode are ignored.
        /// Note that not all devices may support both linear and non-linear levels. 
        /// </summary>
        public bool IsLinear { get; init; }

        /// <summary>
        /// If false the level value fields describe the complete set (linear) or a subset (non-linear) of new levels.
        /// If true all other fields are ignored and the complete set of levels for this control is reset to their factory
        /// defaults. This includes the level type (non-linear vs. linear), the min/max/step values (linear), and the
        /// level count and individual level values (non-linear).
        /// </summary>
        public bool IsReset { get; set; }

        /// <summary>
        /// The zero-based index at which to update non-linear levels.
        /// </summary>
        public int StartIndex { get; set; }

        public abstract byte[] Pack();
    }

    public sealed class LinearLevels : Levels {
        public LinearLevels() : base(true) { }

        internal LinearLevels(IHidPpReport response) : base(response) {
            MinLevel = response.ReadUInt16(2);
            MaxLevel = response.ReadUInt16(4);
            Step     = response.ReadUInt16(6);
        }

        /// <summary>
        /// The value of the first level.
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// The value of the last level, subject to the following condition: MinLevel ≤ MaxLevel
        /// </summary>
        public int MaxLevel { get; set; }

        /// <summary>
        /// The value steps between levels, subject to the following conditions:
        /// Step > 0 && (MaxLevel − MinLevel) % Step == 0
        /// </summary>
        public int Step { get; set; }

        public override byte[] Pack() {
            return ByteUtils.Pack(
                (byte)(IsReset ? 0x03 : 0x01),
                (byte)(StartIndex << 4),
                (ushort)MinLevel,
                (ushort)MaxLevel,
                (ushort)Step
            );
        }
    }

    public sealed class NonLinearLevels : Levels {
        public NonLinearLevels(params int[] values) : base(false) {
            if (values.Length > 7) {
                throw new ArgumentOutOfRangeException(nameof(values), "max 7 levels are allowed");
            }

            if (values.Length < 1) {
                throw new ArgumentOutOfRangeException(nameof(values), "minimum 1 level must be specified");
            }

            Values = values;
            Count  = values.Length;
        }

        internal NonLinearLevels(IHidPpReport response) : base(response) {
            Count      = response[0] & (0xE0 >> 5);
            LevelCount = response[1] & 0x0F;
            Values     = new int[Count];
            for (var ii = 0; ii < Count; ii++) {
                Values[ii] = response.ReadUInt16(2 + ii * 2);
            }
        }

        /// <summary>
        /// The number of valid level values contained in the data. Valid values for this field are in the range 1 to 7.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Sets the level count, that is the number of levels available for selection by the user.
        /// Valid values for this field are in the range 0 to 15 but must not exceed maxLevels. If this field is
        /// identical to the device’s current level count the number of levels does not change. A value of 0 resets the
        /// number of available levels to the factory default, though this does not reset the level values themselves.
        /// Once a new levelCount value has taken effect all level values with indices ≥ levelCount are invalidated.
        /// If LevelCount is set to include indices whose level values are currently invalid the values of those levels
        /// is undefined.
        /// </summary>
        public int LevelCount { get; }

        /// <summary>
        /// The level values
        /// </summary>
        public int[] Values { get; }

        public override byte[] Pack() {
            var valuesData = (from v in Values select BitConverter.GetBytes((ushort)v)).SelectMany(bytes => bytes).ToArray();

            return ByteUtils.Pack(
                (byte)((Count & 0x06) << 5) | (byte)(IsReset ? 0x02 : 0x00),
                (byte)((StartIndex & 0x0F) << 4) | (byte)(LevelCount & 0x0F),
                valuesData
            );
        }
    }
}