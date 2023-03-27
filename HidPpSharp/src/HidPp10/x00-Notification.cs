namespace HidPpSharp.HidPp10;

public class Notification : AbstractRegister {
    [Flags]
    public enum DeviceReportFlags : ushort {
        /// <summary>
        /// Multimedia and MS vendor specific keys are reported by HID++ notification 0x03
        /// </summary>
        ConsumerVendorSpecificControl = 0x0001,

        /// <summary>
        /// Power keys are reported by HID++ notification 0x04
        /// </summary>
        PowerKeys = 0x0002,

        /// <summary>
        ///  Vertical scroll is reported by HID++ notification 0x05
        /// </summary>
        RollerVertical = 0x0004,

        /// <summary>
        /// Mouse buttons not available in the standard HID mouse report are reported by HID++ notification 0x06
        /// </summary>
        MouseExtraButtons = 0x0008,

        /// <summary>
        /// Battery state or mileage are reported by HID++ notification 0x07 or 0x0D (depending on the device)
        /// </summary>
        BatteryStatus = 0x0010,

        /// <summary>
        /// Horizontal scroll is reported by HID++ notification 0x05 
        /// </summary>
        RollerHorizontal = 0x0020,

        /// <summary>
        /// F-Lock status is reported by HID++ notification 0x09 
        /// </summary>
        FLockStatus = 0x0040,

        /// <summary>
        /// Numpad keys are reported as buttons by HID++ notification 0x03
        /// </summary>
        NumpadNumericKeys = 0x0080,

        /// <summary>
        /// 3D gestures are reported by HID++ notification 0x65
        /// </summary>
        Gesture3D = 0x0100
    }

    [Flags]
    public enum ReceiverReportFlags : byte {
        None                 = 0x00,
        WirelessNotification = 0x01,
        SoftwarePresent      = 0x08
    }

    public Notification(IHidPpDevice device) : base(device, RegisterId.Notification) { }

    public void SetReceiver(ReceiverReportFlags flags) {
        var response = SetRegisterShort(0x00, (byte)flags, 0x00);
        if (!response.IsSuccess) {
            throw new RegisterException(response);
        }
    }

    public ReceiverReportFlags GetReceiver() {
        var response = GetRegisterShort();
        return response.IsSuccess ? (ReceiverReportFlags)response[1] : throw new RegisterException(response);
    }

    public void SetDevice(DeviceReportFlags flags) {
        var response = SetRegisterShort((byte)((ushort)flags & 0x00FF), 0x00, (byte)(((ushort)flags & 0xFF00) >> 8));
        if (!response.IsSuccess) {
            throw new RegisterException(response);
        }
    }

    public DeviceReportFlags GetDevice() {
        var response = GetRegisterShort();
        return response.IsSuccess
            ? (DeviceReportFlags)((response[2] << 8) | response[1])
            : throw new RegisterException(response);
    }
}