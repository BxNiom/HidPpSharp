using HidSharp;

namespace HidPpSharp.Devices;

public class CommonReceiverClientHidPpDevice : AbstractHidPpDevice {
    public CommonReceiverClientHidPpDevice(CommonReceiverHidPpDevice receiver, byte deviceIndex) : base(
        receiver.HidDevice, deviceIndex) {
        Receiver = receiver;
    }

    public CommonReceiverHidPpDevice Receiver { get; }

    public override HidPpDeviceInputReceiver InputReceiver {
        get => Receiver.InputReceiver;
    }

    public override HidStream? Stream {
        get => Receiver.Stream;
    }

    public override bool TryOpen(OpenConfiguration? openConfiguration = null) {
        return true;
    }
}