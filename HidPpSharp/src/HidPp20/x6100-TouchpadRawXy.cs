using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

// TODO Events

[Feature(FeatureId.TouchpadRawXy)]
public class TouchpadRawXy : Feature {
    /// <summary>
    /// The corners are defined by looking at the device from above, with the lower edge towards the user and the upper
    /// facing the PC screen
    /// </summary>
    public enum Origin : byte {
        LowerLeft  = 0x01,
        LowerRight = 0x02,
        UpperLeft  = 0x03,
        UpperRight = 0x04
    }

    [Flags]
    public enum ReportSetting : byte {
        /// <summary>
        /// raw reporting
        /// </summary>
        Raw = 0x01,

        /// <summary>
        /// force data to 16-bit reporting (deprecated)
        /// </summary> 
        ForceData = 0x02,

        /// <summary>
        /// enhanced reporting
        /// </summary>
        Enhanced = 0x04,

        /// <summary>
        /// Width/Height instead of Area (16Bit)
        /// </summary>
        WidthHeight16Bit = 0x08,

        /// <summary>
        /// reporting of native gestures
        /// </summary>
        NativeGesture = 0x10,

        /// <summary>
        /// reporting of Major/Minor/Orientation
        /// </summary>
        MajorMinorOrientation = 0x20,

        /// <summary>
        /// report Width and Height bytes instead of Area
        /// </summary>
        WidthHeight8Bit = 0x40
    }

    public const int FuncGetTouchpadInfo   = 0x00;
    public const int FuncGetRawReportState = 0x01;
    public const int FuncSetRawReportState = 0x02;

    public TouchpadRawXy(HidPp20Features features) : base(features, FeatureId.TouchpadRawXy) { }

    public TouchpadInfo GetTouchpadInfo() {
        var response = CallFunction(FuncGetTouchpadInfo);
        if (response.IsSuccess) {
            return new TouchpadInfo {
                XSize                   = response.ReadUInt16(0),
                YSize                   = response.ReadUInt16(2),
                ZDataRange              = response[4],
                AreaDataRange           = response[5],
                TimeStampUnits          = response[6],
                MaxFingerCount          = response[7],
                Origin                  = (Origin)response[8],
                HasPenSupport           = response[9] == 0x01,
                RawReportMappingVersion = response[12],
                Dpi                     = response.ReadUInt16(13)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns a bitmap, indicating the currently selected state for reporting.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public ReportSetting GetRawReportState() {
        var response = CallFunction(FuncGetRawReportState);
        return response.IsSuccess ? (ReportSetting)response[0] : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns a bitmap, indicating the currently selected state for reporting.
    /// </summary>
    /// <param name="settings">bitmap of report setting</param>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public ReportSetting SetRawReportState(ReportSetting settings) {
        var response = CallFunction(FuncSetRawReportState, (byte)settings);
        return response.IsSuccess ? (ReportSetting)response[0] : throw new FeatureException(FeatureId, response);
    }

    public struct TouchpadInfo {
        /// <summary>
        /// The extent of the touch pad coordinates, in native resolution.
        /// </summary>
        public int XSize;

        /// <summary>
        /// The extent of the touch pad coordinates, in native resolution.
        /// </summary>
        public int YSize;

        /// <summary>
        /// 0x00 means no range, 0x0f means 16-bit.
        /// </summary>
        public int ZDataRange;

        /// <summary>
        /// 0x0f means 16-bit.
        /// </summary>
        public int AreaDataRange;

        /// <summary>
        /// Number of 0.1 milliseconds per timestamp increment.
        /// </summary>
        public int TimeStampUnits;

        /// <summary>
        /// Maximum number of fingers that can be tracked.
        /// </summary>
        public int MaxFingerCount;

        /// <summary>
        /// Position of the origin. 
        /// </summary>
        public Origin Origin;

        /// <summary>
        /// 
        /// </summary>
        public bool HasPenSupport;

        /// <summary>
        /// 
        /// </summary>
        public int RawReportMappingVersion;

        /// <summary>
        /// 
        /// </summary>
        public int Dpi;
    }
}