namespace HidPpSharp.HidPp10;

public class DeviceActivity : AbstractRegister {
    public DeviceActivity(IHidPpDevice device) : base(device, RegisterId.DeviceActivity) { }

    public byte[] Get() {
        var response = GetRegisterLong();
        return response.IsSuccess ? response.Data[..6] : throw new RegisterException(response);
    }
}