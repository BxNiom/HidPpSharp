namespace HidPpSharp.HidPp10;

public class ConnectionState : AbstractRegister {
    public enum ActionFlags : byte {
        FakeDeviceArrival = 0x02
    }

    public ConnectionState(IHidPpDevice device) : base(device, RegisterId.ConnectionState) { }

    public State Get() {
        var response = GetRegisterShort();
        return response.IsSuccess
            ? new State { ConnectedDevices = response[1], RemainingSlots = response[2] }
            : throw new RegisterException(response);
    }

    public void Set(ActionFlags flags = ActionFlags.FakeDeviceArrival) {
        var response = SetRegisterShort((byte)flags, 0x00, 0x00);
        if (!response.IsSuccess) {
            throw new RegisterException(response);
        }
    }

    public struct State {
        public int ConnectedDevices;
        public int RemainingSlots;

        public bool HasNoLimit {
            get => RemainingSlots == 0;
        }

        public bool HasFreeSlots {
            get => RemainingSlots < 255;
        }

        public override string ToString() {
            return
                $"{nameof(ConnectedDevices)}: {ConnectedDevices:00}, {nameof(RemainingSlots)}: {RemainingSlots:00}, {nameof(HasNoLimit)}: {HasNoLimit}, {nameof(HasFreeSlots)}: {HasFreeSlots}";
        }
    }
}