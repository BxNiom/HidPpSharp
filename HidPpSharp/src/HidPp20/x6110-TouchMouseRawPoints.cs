using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.TouchMouseRawPoints)]
public class TouchMouseRawPoints : Feature {
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

    public enum RawMode : byte {
        NativeGestures = 0x00,
        Filtered       = 0x01,
        RawNative      = 0x02,
        Raw            = 0x03,
        RawNativeZ     = 0x04
    }

    public const int FuncGetTouchpadInfo = 0x00;
    public const int FuncGetRawMode      = 0x01;
    public const int FuncSetRawMode      = 0x02;

    public TouchMouseRawPoints(HidPp20Features features) : base(features, FeatureId.TouchMouseRawPoints) { }

    public TouchpadInfo GetTouchpadInfo() {
        var response = CallFunction(FuncGetTouchpadInfo);
        if (response.IsSuccess) {
            return new TouchpadInfo {
                XMax                = response.ReadUInt16(0),
                YMax                = response.ReadUInt16(2),
                Dpi                 = response.ReadUInt16(4),
                Origin              = (Origin)response[6],
                MaxFingerCount      = response[7],
                TouchPointDataRange = response[8]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public RawMode GetRawMode() {
        var response = CallFunction(FuncGetRawMode);
        return response.IsSuccess ? (RawMode)response[0] : throw new FeatureException(FeatureId, response);
    }

    public void SetRawMode(RawMode mode) {
        var response = CallFunction(FuncSetRawMode, (byte)mode);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    public struct TouchpadInfo {
        public int    XMax;
        public int    YMax;
        public int    Dpi;
        public Origin Origin;
        public int    MaxFingerCount;
        public int    TouchPointDataRange;
    }
}