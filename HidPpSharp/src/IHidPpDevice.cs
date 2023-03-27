using HidSharp;

namespace HidPpSharp;

public interface IHidPpDevice : IDisposable {
    public int                      ProductId     { get; }
    public int                      VendorId      { get; }
    public HidPpDeviceInputReceiver InputReceiver { get; }
    public HidStream?               Stream        { get; }
    public byte                     DeviceIndex   { get; }

    public void Write(params byte[] data);
    public bool TryOpen(OpenConfiguration? openConfiguration = null);
}