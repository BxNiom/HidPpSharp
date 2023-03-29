using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// 
/// </summary>
[Feature(FeatureId.MousePointer)]
public class MousePointer : AbstractFeature {
    public enum Acceleration : byte {
        None   = 0x00,
        Low    = 0x01,
        Medium = 0x02,
        High   = 0x03
    }

    public const int FuncGetMousePointerInfo = 0x00;

    public MousePointer(HidPp20Features features) : base(features, FeatureId.MousePointer) { }

    /// <summary>
    /// Returns information about the mouse pointer, like resolution, need for sensor orientation tuning, need for
    /// acceleration, etc.. basically all information needed by SetPoint UI for pointer/cursor settings
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public PointerInfo GetMousePointerInfo() {
        var response = CallFunction(FuncGetMousePointerInfo);
        if (response.IsSuccess) {
            return new PointerInfo {
                SensorResolution    = response.ReadUInt16(0),
                Acceleration        = (Acceleration)(response[2] & 0x03),
                UseNativeBallistics = response[2].IsBitSet(2),
                UseVerticalTuning   = response[2].IsBitSet(3)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public struct PointerInfo {
        /// <summary>
        /// This value gives the typical sensor resolution on a standard surface, in real life; the optical sensor
        /// resolution can change up to +/- 20% the indicated resolution depending on the surface.
        /// </summary>
        public int SensorResolution;

        /// <summary>
        /// The device informs which ballistics curve is more appropriate given its physical characteristics. If the
        /// host SW has several ballistics curves, (up to 4) the host can choose a default curve based on this
        /// information. If the host does not provide multiple ballistics, then this information is to be ignored.
        /// </summary>
        public Acceleration Acceleration;

        /// <summary>
        /// If true the device suggests that the OS native ballistics is  used, otherwise the host SW can override the
        /// OS native ballistics, then this setting suggests that it is OK to do so
        /// </summary>
        public bool UseNativeBallistics;

        /// <summary>
        /// If true the device provide vertical tuning (orientation), otherwise no vertical tuning is available
        /// </summary>
        public bool UseVerticalTuning;
    }
}