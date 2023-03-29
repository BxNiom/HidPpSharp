using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.VerticalScrolling)]
public class VerticalScrolling : AbstractFeature {
    public enum RollerType : byte {
        /// <summary>
        /// Standard roller, 1D & 2D type
        /// </summary>
        StandardRoller = 0x01,

        /// <summary>
        /// 3G roller, all types (small or big)
        /// </summary>
        Roller3G = 0x03,

        /// <summary>
        /// Micro-ratchet (Gyro and future) - hybrid between 3G and std roller. No mode toggle anymore, heavy wheel.
        /// </summary>
        MicroRatchet = 0x04,

        /// <summary>
        /// Touch pad scrolling (normal scrolling direction: finger follows in the direction of scroll bar)
        /// </summary>
        TouchPadScrolling = 0x05,

        /// <summary>
        /// Touch pad scrolling with natural scrolling enabled by default in FW (inverted direction, finger direction follows content direction)
        /// </summary>
        TouchPadNaturalScrolling = 0x06,

        /// <summary>
        /// Reserved / Unknown
        /// </summary>
        Unknown = 0xFF
    }

    public const int FuncGetRollerInfo = 0x00;

    public VerticalScrolling(HidPp20Features features) : base(features, FeatureId.VerticalScrolling) { }

    /// <summary>
    /// Returns the type & description of supported roller (vertical scrolling)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public RollerInfo GetRollerInfo() {
        var response = CallFunction(FuncGetRollerInfo);
        if (response.IsSuccess) {
            return new RollerInfo {
                NumberOfRatchetByTurn = response[1],
                ScrollLines           = response[2],
                Type = response[0] switch {
                    0x00 or 0x02 or > 0x06 => RollerType.Unknown,
                    _                      => (RollerType)response[0]
                }
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public struct RollerInfo {
        /// <summary>
        /// Typical values are:
        ///   * 18 for mini rollers
        ///   * 24 for all 20mm wheels "big wheel"
        ///   * 36 for uRatchet wheel(18mm)
        /// Other values are also possible, depending on your control user for scrolling. May
        /// create a warning in HID++2.0 SW checker tool. If it's a linear scroller (touchpad, slider), specify the
        /// total counts on the full distance that finger can do on the surface.
        /// </summary>
        public int NumberOfRatchetByTurn;

        /// <summary>
        /// 0 -> do not change system setting
        /// 1 -> change scrolling speed to 1 line/scroll
        /// 2 -> preferred scrolling speed to 2 lines/scroll event
        /// 3 -> preferred scrolling speed to 3 lines/scroll event - this is usually the default on Windows.
        /// .
        /// .
        /// .
        /// 254 -> lines/scroll
        /// 255 -> Page/screen scrolling
        /// </summary>
        public int ScrollLines;

        public RollerType Type;
    }
}