using HidSharp;
using log4net;

namespace HidPpSharp.Devices;

public abstract class AbstractHidPpDevice : IHidPpDevice {
    protected AbstractHidPpDevice(HidDevice device, byte deviceIndex) {
        HidDevice   = device;
        DeviceIndex = deviceIndex;
        Log         = LogManager.GetLogger($"{device.VendorID:X4}:{device.ProductID:X4}:{deviceIndex:X2}");
    }

    protected ILog      Log       { get; }
    public    HidDevice HidDevice { get; }

    public int ProductId {
        get => HidDevice.ProductID;
    }

    public int VendorId {
        get => HidDevice.VendorID;
    }

    public virtual HidPpDeviceInputReceiver InputReceiver { get; protected init; }

    public virtual HidStream? Stream      { get; protected set; }
    public         byte       DeviceIndex { get; }

    public virtual void Write(params byte[] data) {
        Log.DebugFormat("write: {0}", BitConverter.ToString(data));
        Stream?.Write(data);
        Stream?.Flush();
    }

    public virtual bool TryOpen(OpenConfiguration? openConfiguration = null) {
        var success = false;
        var stream  = (HidStream)null;

        if (openConfiguration == null) {
            success = HidDevice.TryOpen(out stream);
        } else {
            success = HidDevice.TryOpen(openConfiguration, out stream);
        }

        if (!success) {
            return false;
        }

        Stream = stream;
        return true;
    }

    public virtual void Dispose() {
        Stream?.Dispose();
        InputReceiver.Dispose();
    }
}