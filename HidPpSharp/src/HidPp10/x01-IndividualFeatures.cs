namespace HidPpSharp.HidPp10;

public class IndividualFeatures : AbstractRegister {
    [Flags]
    public enum FeatureFlags : ushort {
        /// <summary>
        ///Buttons have a special function (multimedia, local setting, etc.)
        /// </summary>
        SpecialButtonFunctions = 0x0002,

        /// <summary>
        ///Fn+AlphaNumKey is reported as button + HID++ notification 0x0C
        /// </summary>
        EnhancedKeyUsage = 0x0004,

        /// <summary>
        /// A long press on the next/previous track keys acts as Fast Forward/Rewind
        /// </summary>
        FastForwardRewind = 0x0008,

        /// <summary>
        /// Enabled, device handles scrolling acceleration
        /// </summary>
        ScrollingAcceleration = 0x0040,

        /// <summary>
        /// Buttons control the resolution locally (i.e. without SW intervention)
        /// </summary>
        ButtonsControlResolution = 0x0080,

        /// <summary>
        /// Key sound is inhibited (no sound)
        /// </summary>
        InhibitLockKeySound = 0x0100,

        /// <summary>
        /// 3D engine enabled
        /// </summary>
        Engine3D = 0x0200,

        /// <summary>
        /// LEDs are controlled by SW using reg. 0x51
        /// </summary>
        SoftwareControlLed = 0x0400
    }

    public IndividualFeatures(IHidPpDevice device) : base(device, RegisterId.IndividualFeatures) { }

    public FeatureFlags Get() {
        var response = GetRegisterShort();
        return response.IsSuccess
            ? (FeatureFlags)((response[2] << 8) | response[0])
            : throw new RegisterException(response);
    }

    public void Set(FeatureFlags flags) {
        var response = SetRegisterShort((byte)((ushort)flags & 0x00FF), 0x00, (byte)(((ushort)flags & 0xFF00) >> 8));
        if (!response.IsSuccess) {
            throw new RegisterException(response);
        }
    }
}