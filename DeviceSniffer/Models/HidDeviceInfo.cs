using HidSharp;

namespace DeviceSniffer.Models; 

public class HidDeviceInfo {
    public string    Name      => HidDevice.GetProductName();
    public int       ProductId => HidDevice.ProductID;
    public HidDevice HidDevice { get; }

    public HidDeviceInfo(HidDevice hidDevice) {
        HidDevice = hidDevice;
    }
}