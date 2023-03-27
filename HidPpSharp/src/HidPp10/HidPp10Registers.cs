namespace HidPpSharp.HidPp10;

public class HidPp10Registers {
    public HidPp10Registers(IHidPpDevice device) {
        Device             = device;
        Notification       = new Notification(device);
        IndividualFeatures = new IndividualFeatures(device);
        ConnectionState    = new ConnectionState(device);
        DevicePairing      = new DevicePairing(device);
        DeviceActivity     = new DeviceActivity(device);
        PairingInformation = new PairingInformation(device);
        Firmware           = new Firmware(device);
    }

    public IHidPpDevice Device { get; }

    public Notification       Notification       { get; }
    public IndividualFeatures IndividualFeatures { get; }
    public ConnectionState    ConnectionState    { get; }
    public DevicePairing      DevicePairing      { get; }
    public DeviceActivity     DeviceActivity     { get; }
    public PairingInformation PairingInformation { get; }
    public Firmware           Firmware           { get; }
}