using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

// TODO Events

[Feature(FeatureId.SpecialKeysMseButtons, FeatureId.SpecialKeysMseButtonsV6)]
public class SpecialKeysMseButtons : AbstractFeature {
    [Flags]
    public enum Capabilities : byte {
        ResetAllCidReportSettings = 0x01
    }

    [Flags]
    public enum CidInfoAdditionalFlags : byte {
        RawXy              = 0x01,
        ForceRawXy         = 0x02,
        AnalyticsKeyEvents = 0x04,
        RawWheel           = 0x08
    }

    [Flags]
    public enum CidInfoFlags : byte {
        Mouse                = 0x01,
        FKey                 = 0x02,
        HotKey               = 0x04,
        FnToggle             = 0x08,
        Reprogrammable       = 0x10,
        TempDivertable       = 0x20,
        PersistentDivertable = 0x40,
        Virtual              = 0x80
    }

    [Flags]
    public enum DivertFlags : uint {
        TemporarilyDivert  = 0x01000000,
        PersistentlyDivert = 0x04000000,
        RawXy              = 0x10000000,
        AnalyticsKeyEvents = 0x00000001,
        RawWheelEvents     = 0x00000004
    }


    [Flags]
    public enum UpdateFlags : uint {
        TemporarilyDivert = 0x02000000,
        PersistentDivert  = 0x08000000,
        RawXy             = 0x20000000,
        ForceRawXy        = 0x80000000,
        AnalyticsKeyEvent = 0x00000002,
        RawWheelEvent     = 0x00000008
    }

    public const int FuncGetCount                  = 0x00;
    public const int FuncGetCidInfo                = 0x01;
    public const int FuncGetCidReporting           = 0x02;
    public const int FuncSetCidReporting           = 0x03;
    public const int FuncGetCapabilities           = 0x04;
    public const int FuncResetAllCidReportSettings = 0x05;

    public SpecialKeysMseButtons(HidPp20Features features) : base(features, FeatureId.SpecialKeysMseButtons) { }

    public int GetCount() {
        var response = CallFunction(FuncGetCount);
        return response.IsSuccess ? response[0] : throw new FeatureException(FeatureId, response);
    }

    public CidInfo GetCidInfo(int index) {
        var response = CallFunction(FuncGetCidInfo, BitConverter.GetBytes((ushort)index));

        if (response.IsSuccess) {
            return new CidInfo {
                Index           = index,
                ControlId       = response.ReadUInt16(0),
                TaskId          = response.ReadUInt16(2),
                Flags           = (CidInfoFlags)response[4],
                Position        = response[5],
                Group           = response[6],
                GMask           = response[7],
                AdditionalFlags = (CidInfoAdditionalFlags)response[8]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public CidReport GetCidReporting(int cid) {
        var response = CallFunction(FuncGetCidReporting, BitConverter.GetBytes((ushort)cid));
        if (response.IsSuccess) {
            return new CidReport {
                Cid     = response.ReadUInt16(0),
                Divert  = (DivertFlags)(response.ReadUInt32(2) & 0xFF0000FF),
                Update  = 0,
                RemapId = response.ReadUInt16(3)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public CidReport SetCidReporting(int cid, CidReport report) {
        var data = ByteUtils.Pack(cid,
            (ushort)report.Divert | (uint)report.Update | (uint)((ushort)report.RemapId << 8));

        var response = CallFunction(FuncSetCidReporting, data);
        if (response.IsSuccess) {
            return new CidReport {
                Cid     = response.ReadUInt16(0),
                Divert  = (DivertFlags)(response.ReadUInt32(2) & 0xFF0000FF),
                Update  = (UpdateFlags)(response.ReadUInt32(2) & 0xFF0000FF),
                RemapId = response.ReadUInt16(3)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public Capabilities GetCapabilities() {
        if (Version < 6) {
            throw new NotSupportedException();
        }

        var response = CallFunction(FuncGetCapabilities);
        return response.IsSuccess ? (Capabilities)response[0] : throw new FeatureException(FeatureId, response);
    }

    public void ResetAllCidReportSettings() {
        if (Version < 6) {
            throw new NotSupportedException();
        }

        var response = CallFunction(FuncResetAllCidReportSettings);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    public struct CidInfo {
        public int                    Index;
        public int                    ControlId;
        public int                    TaskId;
        public CidInfoFlags           Flags;
        public int                    Position;
        public int                    Group;
        public byte                   GMask;
        public CidInfoAdditionalFlags AdditionalFlags;

        public override string ToString() {
            return
                $"{nameof(Index)}: {Index}, {nameof(ControlId)}: {ControlId}, {nameof(TaskId)}: {TaskId}, {nameof(Flags)}: {Flags}, {nameof(Position)}: {Position}, {nameof(Group)}: {Group}, {nameof(GMask)}: {GMask}, {nameof(AdditionalFlags)}: {AdditionalFlags}";
        }
    }

    public struct CidReport {
        public int         Cid;
        public int         RemapId;
        public DivertFlags Divert;
        public UpdateFlags Update;
    }
}