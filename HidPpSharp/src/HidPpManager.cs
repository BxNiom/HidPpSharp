using System.Collections;
using System.Reflection;
using HidPpSharp.Devices;
using HidSharp;
using log4net;

namespace HidPpSharp;

public class HidPpManager : IEnumerable<KeyValuePair<int, Type>> {
    private readonly Dictionary<int, Type> _knownDevices;

    public HidPpManager() {
        _knownDevices = new Dictionary<int, Type>();
        Log           = LogManager.GetLogger("HidPpManager");
    }

    private ILog Log { get; }

    public IEnumerator<KeyValuePair<int, Type>> GetEnumerator() {
        return _knownDevices.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void AddKnownDevice<T>(int productId) where T : IHidPpDevice {
        AddKnownDevice(productId, typeof(T));
    }

    public void AddKnownDevice(int productId, Type hidPpDeviceType) {
        _knownDevices[productId] = hidPpDeviceType;
        Log.DebugFormat("Added known device {0}(0x{1:X4})", hidPpDeviceType.Name, productId);
    }

    public void RemoveKnownDevice(int productId) {
        _knownDevices.Remove(productId);
        Log.DebugFormat("Removed known device 0x{0:X4}", productId);
    }

    public bool TryOpenDevice(HidDevice device, out IHidPpDevice? hidPpDevice) {
        hidPpDevice = null;
        if (device.VendorID != 0x046D) {
            // Only Logitech devices
            Log.Warn("No logitech device");
            throw new NotSupportedException("no logitech device");
        }

        try {
            if (_knownDevices.ContainsKey(device.ProductID)) {
                var cType = _knownDevices[device.ProductID];
                var ctor = cType.GetConstructor(BindingFlags.Instance | BindingFlags.Public,
                    null, CallingConventions.Standard | CallingConventions.HasThis, new[] { typeof(HidDevice) }, null);

                if (ctor == null) {
                    Log.ErrorFormat("No valid contructor found for {0} - product id: 0x{1:X4}", cType,
                        device.ProductID);
                }

                hidPpDevice = (IHidPpDevice)ctor?.Invoke(new[] { device });
            } else {
                hidPpDevice = new CommonHidPpDevice(device);
            }

            return hidPpDevice?.TryOpen() ?? false;
        }
        catch (Exception ex) {
            Log.ErrorFormat("Failed to open or create device class 0x{0:X4}: {1}", device.ProductID, ex);
            throw;
        }
    }
}