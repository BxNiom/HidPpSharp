namespace HidPpSharp.HidPp10;

public class Firmware : AbstractRegister {
    public enum InformationItem : byte {
        FwVersionRelease  = 0x01,
        FwBuildNumber     = 0x02,
        HwVersion         = 0x03,
        BootLoaderVersion = 0x04
    }

    public Firmware(IHidPpDevice device) : base(device, RegisterId.Firmware) { }

    /// <summary>
    /// Get Firmware information
    /// </summary>
    /// <param name="item">Which info to receive</param>
    /// <param name="mcu">Unknown</param>
    /// <returns>2 byte value</returns>
    /// <exception cref="RegisterException"></exception>
    public byte[] Get(InformationItem item, int mcu = 0) {
        var response = GetRegisterShort((byte)((mcu << 4) | (byte)item));
        return response.IsSuccess ? response.Data[1..] : throw new RegisterException(response);
    }

    public FirmwareInfo GetFirmwareInfo() {
        var fwVer   = Get(InformationItem.FwVersionRelease);
        var fwBuild = Get(InformationItem.FwBuildNumber);
        var hwVer   = Get(InformationItem.HwVersion);
        var blVer   = Get(InformationItem.BootLoaderVersion);
        return new FirmwareInfo {
            FwVersion         = new Version(fwVer[0], fwVer[1], (fwBuild[0] << 8) | fwBuild[1]),
            HardwareVersion   = new Version(hwVer[0], hwVer[1]),
            BootLoaderVersion = new Version(blVer[0], blVer[1])
        };
    }

    public struct FirmwareInfo {
        public Version FwVersion;
        public Version BootLoaderVersion;
        public Version HardwareVersion;

        public override string ToString() {
            return
                $"{nameof(FwVersion)}: {FwVersion.ToHexString()}, {nameof(BootLoaderVersion)}: {BootLoaderVersion.ToHexString()}, {nameof(HardwareVersion)}: {HardwareVersion.ToHexString()}";
        }
    }
}