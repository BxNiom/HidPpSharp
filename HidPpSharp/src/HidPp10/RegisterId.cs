namespace HidPpSharp.HidPp10;

public enum RegisterId {
    Notification       = 0x00,
    IndividualFeatures = 0x01,
    ConnectionState    = 0x02,
    DeviceParing       = 0xB2,
    DeviceActivity     = 0xB3,
    PairingInformation = 0xB5,
    Firmware           = 0xF1
}