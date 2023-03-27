using HidSharp;

namespace HidPpSharp.Devices;

public class CommonReceiverHidPpDevice : AbstractHidPpDevice {
    public CommonReceiverHidPpDevice(HidDevice device, byte deviceIndex = 0xFF) : base(device, deviceIndex) {
        InputReceiver = new HidPpDeviceInputReceiver(this);
    }

    public override bool TryOpen(OpenConfiguration? openConfiguration = null) {
        if (base.TryOpen(openConfiguration)) {
            InputReceiver.Start();
            return true;
        }

        return false;
    }
}