namespace HidPpSharp.HidPp20;

public readonly struct ProtocolVersion {
    public byte ProtocolNumber { get; }
    public byte TargetSoftware { get; }

    public ProtocolVersion() : this(0, 0) { }

    public ProtocolVersion(byte protocolNumber, byte targetSoftware) {
        ProtocolNumber = protocolNumber;
        TargetSoftware = targetSoftware;
    }

    /// <summary>
    /// Lenovo software is the intended OEM software
    /// </summary>
    public bool Lenovo {
        get => ProtocolNumber == 3 && IsTargetBitSet(0x01);
    }

    /// <summary>
    /// Dell software is the intended OEM software
    /// </summary>
    public bool Dell {
        get => ProtocolNumber == 3 && IsTargetBitSet(0x02);
    }

    public bool LogitechDeviceManager {
        get => ProtocolNumber == 4 && IsTargetBitSet(0x01);
    }

    public bool LogitechGameingSoftware {
        get => ProtocolNumber == 4 && IsTargetBitSet(0x02);
    }

    public bool LogitechPreferenceManager {
        get => ProtocolNumber == 4 && IsTargetBitSet(0x04);
    }

    public bool WindowsPresenterSoftware {
        get => ProtocolNumber == 4 && IsTargetBitSet(0x08);
    }

    public bool MacPresenterSoftware {
        get => ProtocolNumber == 4 && IsTargetBitSet(0x10);
    }

    public bool TargetSoftwareFeature {
        get => ProtocolNumber == 4 && IsTargetBitSet(0x80);
    }

    public bool LogitechSetPoint {
        get => ProtocolNumber == 2;
    }

    private bool IsTargetBitSet(byte bit) {
        return (TargetSoftware & bit) == bit;
    }

    public override string ToString() {
        return $"{nameof(ProtocolNumber)}: {ProtocolNumber}, {nameof(TargetSoftware)}: {TargetSoftware}";
    }
}