namespace HidPpSharp.HidPp10;

public class DevicePairing : AbstractRegister {
    public enum LockState : byte {
        NoChange   = 0x00,
        OpenLock   = 0x01,
        CloseLock  = 0x02,
        Disconnect = 0x03
    }

    public DevicePairing(IHidPpDevice device) : base(device, RegisterId.DeviceParing) { }

    public void ConnectionSetup(LockState lockState, int devNumber, int timeout = 0) {
        timeout = Math.Clamp(timeout, 0, 255);
        var response = SetRegisterShort((byte)lockState, (byte)devNumber, (byte)timeout);
        if (!response.IsSuccess) {
            throw new RegisterException(response);
        }
    }
}