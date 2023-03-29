using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

// TODO Events

/// <summary>
/// Multi-Platform devices behave specifically to the host operating system and version, described as platforms.
/// x4531 feature reports the Platforms defined and supported by the device, and which Platform is configured per host.
/// It does not describe how a Platform affects the device. A Platform, identified with a unique number, is a collection
/// of OS version ranges, and can cover one OS range("iOS 8") or multiple ranges or multiple OSes ("MacOs all versions +
/// iOs 1 to 7"). Each range is defined by a Platform Descriptor: an OS name bitfield with one version range (all versions
/// if the range is 0.0..0.0).
/// The Platform selected when the device connects to a host is persistent. It can be determined automatically at pairing
/// or connection if the device supports automatic OS detection, and set by the user or software (then should not be
/// overwritten by OS detection).
/// </summary>
[Feature(FeatureId.MultiPlatform)]
public class MultiPlatform : AbstractFeature {
    [Flags]
    public enum Platform : ushort {
        Tizen           = 0x0001,
        Windows         = 0x0100,
        WindowsEmbedded = 0x0200,
        Linux           = 0x0400,
        Chrome          = 0x0800,
        Android         = 0x1000,
        MacOs           = 0x2000,
        IOs             = 0x4000,
        WebOs           = 0x8000
    }

    public enum PlatformSource : byte {
        Default  = 0x00,
        Auto     = 0x01,
        Manual   = 0x02,
        Software = 0x03
    }

    public const int FuncGetFeatureInfos       = 0x00;
    public const int FuncGetPlatformDescriptor = 0x01;
    public const int FuncGetHostPlatform       = 0x02;
    public const int FuncSetHostPlatform       = 0x03;

    public MultiPlatform(HidPp20Features features) : base(features, FeatureId.MultiPlatform) { }

    /// <summary>
    /// Get the number of Platforms and Platform Descriptors defined by the device, the number of host configurations
    /// supported by the device  and the  index of the currently active host (logically the caller).
    /// Also returns the device’s capability mask for.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public Capabilities GetFeatureInfos() {
        var response = CallFunction(FuncGetFeatureInfos);
        if (response.IsSuccess) {
            return new Capabilities {
                AutomatedOsDetection        = response[0].IsBitSet(0),
                CanSetHostPlatform          = response[0].IsBitSet(1),
                NumberOfPlatforms           = response[2],
                NumberOfPlatformDescriptors = response[3],
                HostsCount                  = response[4],
                CurrentHost                 = response[5],
                CurrentHostPlatform         = response[6]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Get the Platform Descriptor specified by index.
    /// </summary>
    /// <param name="index">Index of the Platform Descriptor [0..numPlatformDescr-1] (returned by GetFeatureInfos())</param>
    /// <returns></returns>
    /// <exception cref="FeatureException">InvalidArgument</exception>
    public PlatformDescriptor GetPlatformDescriptor(int index) {
        var response = CallFunction(FuncGetPlatformDescriptor, (byte)index);
        if (response.IsSuccess) {
            return new PlatformDescriptor {
                PlatformIndex           = response[0],
                PlatformDescriptorIndex = response[1],
                Platform                = (Platform)response.ReadUInt16(2),
                FromVersion             = response[4],
                FromRevision            = response[5],
                ToVersion               = response[6],
                ToRevision              = response[7]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Get the Platform index configured for a specific host.
    /// </summary>
    /// <param name="index">Channel / host index. 0xFF = Current Host.</param>
    /// <returns></returns>
    /// <exception cref="FeatureException">InvalidArgument</exception>
    public HostPlatform GetHostPlatform(int index) {
        var response = CallFunction(FuncGetHostPlatform, (byte)index);
        if (response.IsSuccess) {
            return new HostPlatform {
                HostIndex              = response[0],
                IsPaired               = response[1] == 0x01,
                PlatformIndex          = response[2],
                PlatformSource         = (PlatformSource)response[3],
                AutoPlatform           = response[4],
                AutoPlatformDescriptor = response[5]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Change the Platform index configuration for a specific host.
    /// </summary>
    /// <param name="hostIndex">Channel / host index. 0xFF = Current Host.</param>
    /// <param name="platformIndex">Platform index to set.</param>
    /// <exception cref="FeatureException">InvalidArgument, NotAllowed</exception>
    public void SetHostPlatform(int hostIndex, int platformIndex) {
        var response = CallFunction(FuncSetHostPlatform, (byte)hostIndex, (byte)platformIndex);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    public struct Capabilities {
        /// <summary>
        /// Automatic osDetection is implemented.
        /// </summary>
        public bool AutomatedOsDetection;

        /// <summary>
        /// SetHostPlatform is supported.
        /// </summary>
        public bool CanSetHostPlatform;

        /// <summary>
        /// Number of Platforms defined by the device.
        /// </summary>
        public int NumberOfPlatforms;

        /// <summary>
        /// Number of Platform Descriptors defined by the device.
        /// </summary>
        public int NumberOfPlatformDescriptors;

        /// <summary>
        /// Number of hosts / channels.
        /// </summary>
        public int HostsCount;

        /// <summary>
        /// Current host index [0..numHosts - 1].
        /// </summary>
        public int CurrentHost;

        /// <summary>
        /// Current host’s Platform index [0..numPlatforms - 1].
        /// </summary>
        public int CurrentHostPlatform;
    }

    /// <summary>
    /// A Platform is the union of one or more Platform Descriptors. Each Platform Descriptor contains the Platform index
    /// it belongs to, an OS list - <osMask> bit field - and a <fromVersion>.<fromRevision>..<toVersion>.<toRevision>
    /// range. If all version and revision fields are 0, then all versions for the OS list are covered.
    /// </summary>
    public struct PlatformDescriptor {
        /// <summary>
        /// Index of the Platform [0..numPlatforms-1].
        /// </summary>
        public int PlatformIndex;

        /// <summary>
        /// Index of the Platform Descriptor [0..numPlatformDescr-1].
        /// </summary>
        public int PlatformDescriptorIndex;

        /// <summary>
        /// Bitfield of OS covered by the Platform Descriptor.
        /// </summary>
        public Platform Platform;

        /// <summary>
        /// OS Version start number, 0 if no lower limit.
        /// </summary>
        public int FromVersion;

        /// <summary>
        /// OS Revision start number, 0 if not lower limit.
        /// </summary>
        public int FromRevision;

        /// <summary>
        /// OS Version end number, 0 if no upper limit.
        /// </summary>
        public int ToVersion;

        /// <summary>
        /// OS Revision end number, 0 if no upper limit.
        /// </summary>
        public int ToRevision;
    }

    public struct HostPlatform {
        /// <summary>
        /// Channel / host index. 0xFF = Current Host.
        /// </summary>
        public int HostIndex;

        /// <summary>
        /// 
        /// </summary>
        public bool IsPaired;

        /// <summary>
        /// Platform index currently used (auto or manually set).
        /// • 0xFF - Undefined (slot empty).
        /// • else - Platform index.
        /// </summary>
        public int PlatformIndex;

        /// <summary>
        /// Origin of current Platform index configuration
        /// </summary>
        public PlatformSource PlatformSource;

        /// <summary>
        /// Platform index automatically defined at pairing.
        /// • 0xFF - Undefined / failed.
        /// • else - Platform index.
        /// </summary>
        public int AutoPlatform;

        /// <summary>
        /// Platform Descriptor index automatically defined at pairing.
        /// • 0xFF - Undefined / failed.
        /// • else - PlatformDescr index.
        /// </summary>
        public int AutoPlatformDescriptor;
    }
}