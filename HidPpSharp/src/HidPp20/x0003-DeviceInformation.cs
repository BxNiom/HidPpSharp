using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.DeviceInformation)]
public class DeviceInformation : Feature {
    public enum FwType {
        MainApplication    = 0x00,
        Bootloader         = 0x01,
        Hardware           = 0x02,
        Touchpad           = 0x03,
        OpticalSensor      = 0x04,
        SoftDevice         = 0x05,
        RFCompanionMCU     = 0x06,
        FactoryApplication = 0x07,
        RGBCustomeEffect   = 0x08,
        MotorDrive         = 0x09,
        Unknown            = 0xFF
    }

    [Flags]
    public enum TransportProtocol : ushort {
        Unsupported        = 0x0000,
        Bluetooth          = 0x0001,
        BluetoothLowEnergy = 0x0002,
        eQuad              = 0x0004,
        USB                = 0x0008
    }

    public const int FuncGetDeviceInfo         = 0x00;
    public const int FuncGetFwInfo             = 0x01;
    public const int FuncGetDeviceSerialNumber = 0x02;

    public DeviceInformation(HidPp20Features features) : base(features, FeatureId.DeviceInformation) { }

    public DeviceInfo GetDeviceInfo() {
        Log.Debug("GetDeviceInfo()");
        var response = CallFunction(FuncGetDeviceInfo);

        if (response.IsSuccess) {
            return new DeviceInfo {
                EntityCount = response[0],
                UnitId = Version >= 1 ? response.ReadUInt16(1) : (ushort)0x00,
                Protocol = Version >= 1 ? (TransportProtocol)response.ReadUInt16(5) : TransportProtocol.Unsupported,
                ModelId = Version >= 1 ? response.ReadNumber(7, 6) : 0x00,
                ModelIdEx = Version >= 2 ? response[13] : 0x00,
                SupportSerial = Version >= 4 && (response[14] & 0x01) == 0x01
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public FwInfo GetFwInfo(int entityIdx) {
        Log.DebugFormat("GetFwInfo({0})", entityIdx);
        var response = CallFunction(FuncGetFwInfo, (byte)entityIdx);

        if (response.IsSuccess) {
            return new FwInfo {
                Type         = (FwType)response[0],
                Name         = $"{(char)response[1]}{(char)response[2]}{(char)response[3]}{response[4]:X2}",
                Revision     = response[5],
                Build        = response.ReadInt16(6),
                TransportPid = (response[8] & 0x01) == 0x01 ? response.ReadInt16(9) : 0,
                ExtraVersion = response.ReadNumber(11, 5)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public byte[] GetDeviceSerialNumber() {
        Log.DebugFormat("GetDeviceSerialNumber()");
        if (Version < 4) {
            throw new FeatureException(FeatureId, ReportError.Unsupported);
        }

        var response = CallFunction(FuncGetDeviceSerialNumber);
        if (response is { IsSuccess: true, Data.Length: >= 12 }) {
            return response.Data[..11];
        }

        throw new FeatureException(FeatureId, response);
    }

    public struct DeviceInfo {
        /// <summary>
        /// The number of entities present in the device from which this feature can obtain version information.
        /// </summary>
        public int EntityCount;

        /// <summary>
        /// Random number that serves as per unit identifier, guaranteed to be unique for all units with the same
        /// modelId.
        /// </summary>
        public uint UnitId;

        /// <summary>
        /// The model id is a dense array of the application PIDs of the different transports supported by the device.
        /// </summary>
        public ulong ModelId;

        /// <summary>
        /// A 8 bit value that represents a configurable attribute of the device (on the line) for a given modelId
        /// (e.g. colour of the device). Default value is 0.
        /// </summary>
        public int ModelIdEx;

        /// <summary>
        /// Is serial number supported
        /// </summary>
        public bool SupportSerial;

        /// <summary>
        /// Indicating which communication protocols the device supports.
        /// </summary>
        public TransportProtocol Protocol;
    }

    public struct FwInfo {
        /// <summary>
        /// Indicates the entity type for the requested entity index
        /// </summary>
        public FwType Type;

        /// <summary>
        /// The firmware name is composed of the prefix characters (also known as the firmware class) and a firmware number
        /// between 0 and 99. The firmware name uniquely links an application binary executable with an existing product.
        /// It also uniquely links the application binary to existing documents in manufacturing documentation.
        ///
        /// The binary executable associated to a particular firmware name constitutes the smallest unit of in the field
        /// deployed code that can be updated via DFU.
        /// </summary>
        public string Name;

        /// <summary>
        /// Firmware revision. It is a BCD packed number that gets incremented if a new QA approved firmware is released
        /// in the field, this can happen either via manufacturing or in the field DFU.
        /// </summary>
        public int Revision;

        /// <summary>
        /// Firmware build. It a is packed BCD number that gets incremented for any firmware release (internal, engineering,
        /// external, etc) except for the entity type 5 (Softdevice). For the Softdevice, it is the raw value.
        /// </summary>
        public int Build;

        /// <summary>
        /// Transport PID ("product ID" - For example: USB PID, BT PID, equadId etc.)
        /// </summary>
        public int TransportPid;

        /// <summary>
        /// Optional extra versioning information. Five bytes that link the executing binary to the source control system.
        /// When not implemented this must be 0x00.
        /// </summary>
        public ulong ExtraVersion;
    }
}