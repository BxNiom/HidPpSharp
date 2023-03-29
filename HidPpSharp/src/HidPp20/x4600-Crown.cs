using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

// TODO Events

[Feature(FeatureId.Crown)]
public class Crown : AbstractFeature {
    [Flags]
    public enum Capabilities : ushort {
        Proximity         = 0x0001,
        Touch             = 0x0002,
        TapGesture        = 0x0004,
        DoubleTapGesture  = 0x0008,
        Button            = 0x0100,
        ButtonLongPress   = 0x0200,
        MechanizedRatchet = 0x0400,
        RotationTimeout   = 0x0800,
        ShortLongTimeout  = 0x1000,
        DoubleTapSpeed    = 0x2000
    }

    public enum Diverting : byte {
        NoChange = 0x00,
        HID      = 0x01,
        Diverted = 0x02
    }

    public enum RatchetMode : byte {
        NoChange = 0x00,
        Free     = 0x01,
        Ratchet  = 0x02
    }

    public const int FuncGetInfo = 0x00;
    public const int FuncGetMode = 0x01;
    public const int FuncSetMode = 0x02;

    public Crown(HidPp20Features features) : base(features, FeatureId.Crown) { }

    /// <summary>
    /// Returns crown info constants
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public CrownInfo GetInfo() {
        var response = CallFunction(FuncGetInfo);
        if (response.IsSuccess) {
            return new CrownInfo {
                Capabilities                  = (Capabilities)response.ReadUInt16(0),
                Slots                         = response.ReadUInt16(2),
                NumberOfRatchetsPerRevolution = response.ReadUInt16(4)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public Mode GetMode() {
        var response = CallFunction(FuncGetMode);
        if (response.IsSuccess) {
            return new Mode(response);
        }

        throw new FeatureException(FeatureId, response);
    }

    public Mode SetMode(Mode mode) {
        var response = CallFunction(FuncSetMode,
            (byte)mode.Reporting,
            (byte)mode.RatchetMode,
            (byte)mode.RotationTimeout,
            (byte)mode.ShortLongTimeout,
            (byte)mode.DoubleTapSpeed);

        if (response.IsSuccess) {
            return new Mode(response);
        }

        throw new FeatureException(FeatureId, response);
    }

    public struct CrownInfo {
        public Capabilities Capabilities;
        public int          Slots;
        public int          NumberOfRatchetsPerRevolution;
    }

    public struct Mode {
        public Diverting   Reporting;
        public RatchetMode RatchetMode;
        public int         RotationTimeout;
        public int         ShortLongTimeout;
        public int         DoubleTapSpeed;

        internal Mode(IHidPpReport report) {
            Reporting        = (Diverting)report[0];
            RatchetMode      = (RatchetMode)report[1];
            RotationTimeout  = report[2];
            ShortLongTimeout = report[3];
            DoubleTapSpeed   = report[4];
        }
    }
}