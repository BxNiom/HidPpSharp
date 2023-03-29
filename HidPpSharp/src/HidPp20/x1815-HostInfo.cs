using System.Text;
using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

// TODO Host descriptors

/// <summary>
/// This feature allows managing host information on devices with multihost capabilities. Every device channel is linked
/// to a different host, one active   at a time, on eQuad/Unifying, USB, BT or BLE buses.
/// This feature supports reading host bus  information, saving and reading host os version, moving host
/// connection across channels, and deleting host connections.
/// </summary>
[Feature(FeatureId.HostInfo)]
public class HostInfo : AbstractFeature {
    public enum BusType {
        Undefined             = 0x00,
        EQuad                 = 0x01,
        Usb                   = 0x02,
        Bluetooth             = 0x03,
        BluetoothLowEnergy    = 0x04,
        BluetoothLowEnergyPro = 0x05
    }

    [Flags]
    public enum Capabilities : ushort {
        GetName            = 0x0100,
        SetName            = 0x0200,
        MoveHost           = 0x0400,
        DeleteHost         = 0x0800,
        SetOsVersion       = 0x1000,
        EQuad              = 0x0001,
        UsbHd              = 0x0002,
        Bluetooth          = 0x0004,
        BluetoothLowEnergy = 0x0008
    }

    public enum OsType : byte {
        Unknown = 0x00,
        Windows = 0x01,
        WinEmb  = 0x02,
        Linux   = 0x03,
        Chrome  = 0x04,
        Android = 0x05,
        MacOs   = 0x06,
        Ios     = 0x07
    }

    public const byte FuncGetFeatureInfo      = 0x00;
    public const byte FuncGetHostInfo         = 0x01;
    public const byte FuncGetHostDescriptor   = 0x02;
    public const byte FuncGetHostFriendlyName = 0x03;
    public const byte FuncSetHostFriendlyName = 0x04;
    public const byte FuncMoveHost            = 0x05;
    public const byte FuncDeleteHost          = 0x06;
    public const byte FuncGetHostOsVersion    = 0x07;
    public const byte FuncSetHostOsVersion    = 0x08;

    public HostInfo(HidPp20Features features) : base(features, FeatureId.HostInfo) { }

    /// <summary>
    /// Get the number of hosts in the registry and the currently active host index (logically the caller). Also returns
    /// a capability_mask for the device.
    /// </summary>
    /// <exception cref="FeatureException"></exception>
    public FeatureInfo GetFeatureInfo() {
        var response = CallFunction(FuncGetFeatureInfo);
        if (response.IsSuccess) {
            return new FeatureInfo {
                CapabilityMask = (Capabilities)response.ReadUInt16(0),
                HostCount      = response[2],
                CurrentHost    = response[3]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Get a particular host basic information.
    /// </summary>
    /// <param name="hostIndex">Channel / host index [0..numHosts-1]. 0xFF = Current Host.</param>
    /// <exception cref="FeatureException">InvalidArgument: Invalid host index</exception>
    public Info GetHostInfo(int hostIndex) {
        var response = CallFunction(FuncGetHostInfo, (byte)hostIndex);
        if (response.IsSuccess) {
            return new Info {
                HostIndex     = response[0],
                Paired        = response[1] == 1,
                BusType       = (BusType)response[2],
                PageCount     = response[3],
                NameLength    = response[4],
                MaxNameLength = response[5]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Get a host descriptor page. The host descriptor is the collection of data describing the host profile (VendorID,
    /// Version), collected from its transport channel and (BT, USB..) and depending on it.
    /// </summary>
    /// <param name="hostIndex">Channel / host index. 0xFF = Current Host.</param>
    /// <param name="pageIndex">Index of the host descriptor page to query (0..numPages-1 returned by GetHostInfo()).</param>
    /// <returns></returns>
    /// <exception cref="FeatureException">
    /// InvalidArgument: Invalid host index or page index.
    /// HardwareError: Data could not be retrieved.
    /// </exception>
    public byte[] GetHostDescriptor(int hostIndex, int pageIndex) {
        throw new NotImplementedException();
        // create and read host descriptors correctly
        var response = CallFunction(FuncGetHostDescriptor, (byte)hostIndex, (byte)pageIndex);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }

        return response.Data;
    }

    /// <summary>
    /// Get a Host Friendly Name chunk. Can be null if the host channel is not paired. The Host Friendly Name is the
    /// name provided by the host when establishing the link with the device. This name is usually meant to identify the
    /// host for the user.
    /// The full ASCII name byte length is given by getHostInfo().
    /// </summary>
    /// <param name="hostIndex">Channel / host index. 0xFF = Current Host.</param>
    /// <param name="byteIndex">Index of the first host name byte to copy (0..nameLen-1 returned by GetHostInfo()).</param>
    /// <returns></returns>
    /// <exception cref="FeatureException">
    /// InvalidArgument: Invalid host index.
    /// NotAllowed: Operation not permitted/not implemented (see "capabilityMask").
    /// </exception>
    public HostFriendlyName GetHostFriendlyName(int hostIndex, int byteIndex) {
        var response = CallFunction(FuncGetHostFriendlyName, (byte)hostIndex, (byte)byteIndex);
        if (response.IsSuccess) {
            return new HostFriendlyName(response[0], response[1], Encoding.ASCII.GetString(response.Data[2..]));
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Write a host name chunk, starting at byteIndex. Existing string is overwritten, extended or shorten by the chunk,
    /// considering that resulting Host Friendly Name new byte length is byteIndex + strlen(chunk), truncated  at maxLen
    /// maximum allowed length.
    /// Host Friendly name is ASCII  encoded, which doesn’t contain null byte.
    /// </summary>
    /// <param name="hostIndex">Channel / host index. 0xFF = Current Host.</param>
    /// <param name="byteIndex">Index of the first host name byte to write (0..strlen-1).</param>
    /// <param name="nameChunk">The host name chunk to write, padded with null bytes '\0' if it is shorter than the
    /// payload size (HPPLong: 16 bytes).</param>
    /// <returns></returns>
    /// <exception cref="FeatureException">
    /// InvalidArgument: Invalid host index.
    /// HardwareError: Data could not be written.
    /// NotAllowed: Operation not permitted/not implemented (see "capabilityMask").
    /// </exception>
    public int SetHostFriendlyName(int hostIndex, int byteIndex, string nameChunk) {
        if (nameChunk.Length > 13) {
            nameChunk = nameChunk[..13];
        }

        var data = ByteUtils.Pack((byte)hostIndex, (byte)byteIndex, nameChunk);

        var response = CallFunction(FuncSetHostFriendlyName, data);

        if (response.IsSuccess) {
            return response[1];
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Change a given host index by associating it to a new index. Other hosts indexes might be shifted up or down.
    /// </summary>
    /// <param name="hostIndex">Channel / host index to move.</param>
    /// <param name="newIndex">New index of the channel.</param>
    /// <exception cref="FeatureException">
    /// InvalidArgument: Invalid old or new host index.
    /// HardwareError: Data could not be retrieved or written.
    /// NotAllowed: Operation not permitted/not implemented (see "capabilityMask").
    /// </exception>
    public void MoveHost(int hostIndex, int newIndex) {
        var response = CallFunction(FuncMoveHost, (byte)hostIndex, (byte)newIndex);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Clear the hostIndex Host entry (undo pairing, clear host info), ready for a new pairing. If the host is the
    /// current host, connection is lost after disconnection.
    /// </summary>
    /// <param name="hostIndex">Channel / host index to delete.</param>
    /// <exception cref="FeatureException">
    /// InvalidArgument: Invalid host index.
    /// HardwareError: Data could not be written.
    /// NotAllowed: Operation not permitted/not implemented (see "capabilityMask").
    /// </exception>
    public void DeleteHost(int hostIndex) {
        var response = CallFunction(FuncDeleteHost, (byte)hostIndex);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Read Host OS Type and Version, saved previously by SW. Can be null.
    /// Implemented if GetOsVersion bit set in capability bits.
    /// </summary>
    /// <param name="hostIndex">Channel / host index to query.</param>
    /// <returns></returns>
    /// <exception cref="FeatureException">
    /// InvalidArgument: Invalid host index.
    /// HardwareError: Data could not be written.
    /// NotAllowed: Operation not permitted/not implemented (see "capabilityMask").
    /// </exception>
    public HostOsVersion GetHostOsVersion(int hostIndex) {
        var response = CallFunction(FuncGetHostOsVersion, (byte)hostIndex);
        if (response.IsSuccess) {
            return new HostOsVersion(response[0],
                (OsType)response[1],
                response[2],
                response.ReadUInt16(3),
                response.ReadUInt16(5));
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Write Host OS Type and Version.Can be null.
    /// 
    /// Implemented if SetOsVersion bit set in capability bits.
    /// 
    /// Note: FW does only store this information, but does neither use nor analyze it. Usage of this data is fully
    /// upon SW responsibility.
    /// </summary>
    /// <param name="hostIndex">Channel / host index to write.</param>
    /// <param name="osType">Refer to GetHostOsVersion() [OsType].</param>
    /// <param name="version"></param>
    /// <param name="reversion"></param>
    /// <param name="build"></param>
    /// <exception cref="FeatureException"></exception>
    public void SetHostOsVersion(int hostIndex, OsType osType, int version, int reversion, int build) {
        var data     = ByteUtils.Pack((byte)hostIndex, (byte)osType, (byte)version, (ushort)reversion, (ushort)build);
        var response = CallFunction(FuncSetHostOsVersion, data);

        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    public struct FeatureInfo {
        /// <summary>
        /// Sub-features implemented by the device. A bit is 1 if associated capability is available, else it has to be 0.
        /// </summary>
        public Capabilities CapabilityMask;

        /// <summary>
        /// The number of hosts / channels, including paired and unpaired.
        /// </summary>
        public int HostCount;

        /// <summary>
        /// The current host channel index [0..numHosts - 1].
        /// </summary>
        public int CurrentHost;
    }

    public struct Info {
        /// <summary>
        /// Host index
        /// </summary>
        public int HostIndex;

        /// <summary>
        /// Paring status
        /// </summary>
        public bool Paired;

        /// <summary>
        /// The host bus type
        /// </summary>
        public BusType BusType;

        /// <summary>
        /// Number of Host Descriptor pages available for the host, depending on busType (may be less, including 0, if
        /// the corresponding information could not be collected):
        /// • Undefined: always 0.
        /// • eQuad: always 0.
        /// • USB: Reserved for future use.
        /// • BT: 2 pages with data  collected from Device ID Service Record.
        /// • BLE: 1 or  more pages with data collected from GAP and DIS services.
        /// </summary>
        public int PageCount;

        /// <summary>
        /// The byte length of the host friendly name (without null terminator).
        /// </summary>
        public int NameLength;

        /// <summary>
        /// The maximum byte length of the host friendly name (without null terminator).
        /// </summary>
        public int MaxNameLength;
    }

    public record HostOsVersion(int HostIndex, OsType Os, int Version, int Reversion, int Build);

    /// <param name="HostIndex">Channel / host index. 0xFF = Current Host.</param>
    /// <param name="ByteIndex">Same as parameter.</param>
    /// <param name="NameChunk">The host name chunk, copied from full host name byteIndex’th byte, padded with null bytes
    /// '\0' if the copied string is shorter than the payload size (HPPLong: 16 bytes).</param>
    public record HostFriendlyName(int HostIndex, int ByteIndex, string NameChunk);

    public abstract class HostDescriptor {
        protected HostDescriptor(byte[] rawData) {
            RawData = rawData;
        }

        public byte[] RawData { get; }

        public int HostIndex {
            get => RawData[0];
        }

        public BusType BusType {
            get => (BusType)(RawData[1] & 0xE0);
        }

        public int PageIndex {
            get => RawData[1] & 0x1F;
        }

        public bool ValidAddress {
            get => (RawData[2] & 0x01) == 0x01;
        }

        public byte[] BluetoothAddress {
            get => RawData[3..8];
        }
    }

    public class BluetoothDescriptor : HostDescriptor {
        public BluetoothDescriptor(byte[] rawData) : base(rawData) { }

        public bool HasValidVendorIdSource {
            get => RawData[2].IsBitSet(5);
        }

        public bool HasValidPrimaryRecord {
            get => RawData[2].IsBitSet(4);
        }

        public bool HasValidVersion {
            get => RawData[2].IsBitSet(3);
        }

        public bool HasValidProductId {
            get => RawData[2].IsBitSet(2);
        }

        public bool HasValidVendorId {
            get => RawData[2].IsBitSet(1);
        }

        public bool HasValidSpecificationId {
            get => RawData[2].IsBitSet(0);
        }

        public bool IsPrimaryRecord {
            get => RawData[3].IsBitSet(0);
        }

        public ushort SpecificationId {
            get => RawData.ToUInt16(4);
        }

        public ushort VendorId {
            get => RawData.ToUInt16(6);
        }

        public ushort ProductId {
            get => RawData.ToUInt16(8);
        }

        public ushort Version {
            get => RawData.ToUInt16(10);
        }

        public ushort VendorIdSource {
            get => RawData.ToUInt16(12);
        }
    }

    public class BluetoothLowEnergyDescriptor : HostDescriptor {
        public BluetoothLowEnergyDescriptor(byte[] rawData) : base(rawData) { }

        public int NumberOfUuids {
            get => RawData[2];
        }

        public ushort UuidsTotalByteSize {
            get => RawData.ToUInt16(3);
        }
    }
}