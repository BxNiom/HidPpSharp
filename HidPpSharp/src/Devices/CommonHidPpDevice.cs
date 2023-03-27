using HidSharp;

namespace HidPpSharp.Devices;

public class CommonHidPpDevice : AbstractHidPpDevice {
    public CommonHidPpDevice(HidDevice device, byte deviceIndex = 0xFF) : base(device, deviceIndex) {
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