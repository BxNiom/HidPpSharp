using System.Text;

namespace HidPpSharp.HidPp10;

public class PairingInformation : AbstractRegister {
    public enum Interval : byte {
        I8ms  = 0x08,
        I20ms = 0x14
    }

    public enum Kind : byte {
        Unknown   = 0x00,
        Keyboard  = 0x01,
        Mouse     = 0x02,
        Numpad    = 0x03,
        Presenter = 0x04,
        Trackball = 0x08,
        Touchpad  = 0x09,
        Reserved  = 0xFF
    }

    public enum Location : byte {
        Reserved     = 0x00,
        Base         = 0x01,
        TopCase      = 0x02,
        TopRightEdge = 0x03,
        Other        = 0x04,
        TopLeft      = 0x05,
        BottomLeft   = 0x06,
        TopRight     = 0x07,
        BottomRight  = 0x08,
        TopEdge      = 0x09,
        RightEdge    = 0x0A,
        LeftEdge     = 0x0B,
        BottomEdge   = 0x0C,
        BottomFront  = 0x0F
    }

    public PairingInformation(IHidPpDevice device) : base(device, RegisterId.PairingInformation) { }

    public DevicePairingInfo GetDeviceInfo(int deviceIndex) {
        var response = GetRegisterLong((byte)(deviceIndex | 0x20));
        if (!response.IsSuccess) {
            throw new RegisterException(response);
        }

        var info = new DevicePairingInfo {
            DeviceIndex   = response[0] - 0x20,
            DestinationId = response[1],
            Interval      = (Interval)response[2],
            WirelessPid   = response.ReadUInt16(3),
            DeviceKind    = (Kind)response[7]
        };

        response = GetRegisterLong((byte)(deviceIndex | 0x30));
        if (!response.IsSuccess) {
            throw new RegisterException(response);
        }

        info.SerialNumber        = response.ReadUInt32(1);
        info.ReportTypes         = response.ReadUInt32(5);
        info.PowerSwitchLocation = (Location)(response[9] & 0x0F);

        response = GetRegisterLong((byte)(deviceIndex | 0x40));
        if (!response.IsSuccess) {
            throw new RegisterException(response);
        }

        var len = response[1];
        info.CodeName = Encoding.UTF8.GetString(response.Data[2..len]);

        return info;
    }

    public uint GetReceiverSerial() {
        var response = GetRegisterLong(0x03);
        return response.IsSuccess ? response.ReadUInt32(1) : throw new RegisterException(response);
    }

    public struct DevicePairingInfo {
        public string   CodeName;
        public int      DeviceIndex;
        public int      DestinationId;
        public Interval Interval;
        public int      WirelessPid;
        public Kind     DeviceKind;
        public uint     SerialNumber;
        public uint     ReportTypes;
        public Location PowerSwitchLocation;

        public override string ToString() {
            return $"{nameof(CodeName)}: {CodeName}, {nameof(DeviceIndex)}: {DeviceIndex}, " +
                   $"{nameof(DestinationId)}: 0x{DestinationId:X2}, {nameof(Interval)}: {Interval}, " +
                   $"{nameof(WirelessPid)}: 0x{WirelessPid:X4}, {nameof(DeviceKind)}: {DeviceKind}, " +
                   $"{nameof(SerialNumber)}: 0x{SerialNumber:X8}, {nameof(ReportTypes)}: 0x{ReportTypes:X8}, " +
                   $"{nameof(PowerSwitchLocation)}: {PowerSwitchLocation}";
        }
    }
}