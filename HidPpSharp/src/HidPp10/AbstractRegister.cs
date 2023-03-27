using log4net;

namespace HidPpSharp.HidPp10;

public abstract class AbstractRegister {
    protected const byte ShortRequestId = 0x10;
    protected const byte LongRequestId  = 0x11;

    protected const byte SubIdSetShortRegister = 0x80;
    protected const byte SubIdGetShortRegister = 0x81;
    protected const byte SubIdSetLongRegister  = 0x82;
    protected const byte SubIdGetLongRegister  = 0x83;

    protected AbstractRegister(IHidPpDevice device, RegisterId registerId) {
        Device     = device;
        RegisterId = registerId;
        Log        = LogManager.GetLogger($"{device.VendorId:X4}:{device.ProductId:X4}:{Device.DeviceIndex:X2}:REG");
    }

    protected ILog Log { get; }

    public IHidPpDevice Device     { get; }
    public RegisterId   RegisterId { get; }

    protected RegisterReport SetRegisterShort(params byte[] parameters) {
        return RequestRegister((byte)RegisterId, SubIdSetShortRegister, parameters);
    }

    protected RegisterReport GetRegisterShort(params byte[] parameters) {
        return RequestRegister((byte)RegisterId, SubIdGetShortRegister, parameters);
    }

    protected RegisterReport SetRegisterLong(params byte[] parameters) {
        return RequestRegister((byte)RegisterId, SubIdSetLongRegister, parameters);
    }

    protected RegisterReport GetRegisterLong(params byte[] parameters) {
        return RequestRegister((byte)RegisterId, SubIdGetLongRegister, parameters);
    }

    protected RegisterReport RequestRegister(byte register, byte subId, params byte[] parameters) {
        var isShort = parameters.Length <= 7;

        var data = ByteUtils.Combine(new[] {
            isShort ? ShortRequestId : LongRequestId,
            Device.DeviceIndex,
            subId,
            register
        }, parameters);

        if (isShort && data.Length < 7) {
            Array.Resize(ref data, 7);
        } else if (!isShort && data.Length < 20) {
            Array.Resize(ref data, 20);
        }

        return WaitForResponse(data, TimeSpan.FromMilliseconds(500));
    }

    private RegisterReport WaitForResponse(byte[] data, TimeSpan timeout) {
        var report = (RegisterReport)null;
        var ev     = new ManualResetEventSlim();

        var callBack = (EventHandler)((_, _) => {
            var buf = new byte[256];
            if (!Device.InputReceiver.TryRead(buf, 0, out var r)) {
                return;
            }

            if (r is not RegisterReport regReport) {
                return;
            }

            if (regReport.DeviceIndex == Device.DeviceIndex &&
                regReport.SubId == data[2] &&
                regReport.Address == data[3]) {
                report = regReport;
                ev.Set();
            }
        });
        Device.InputReceiver.Received += callBack;
        Device.Write(data);

        if (!ev.Wait(timeout)) {
            Log.Warn("Timeout");
            report = new RegisterReport(new byte[] { 0x10, Device.DeviceIndex, 0x8F, data[2], data[3], 0xFE, 0x00 });
        }

        Device.InputReceiver.Received -= callBack;
        return report!;
    }
}