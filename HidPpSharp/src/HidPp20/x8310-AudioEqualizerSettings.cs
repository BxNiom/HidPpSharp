using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.AudioEqualizer)]
public class AudioEqualizer : AbstractFeature {
    public enum GainsLocation : byte {
        /// <summary>
        /// Custom EQ
        /// </summary>
        Eeprom = 0x00,

        /// <summary>
        /// Active EQ
        /// </summary>
        Ram = 0x01
    }

    public enum Persistence {
        Ram          = 0x00,
        RamAndEeprom = 0x01,
        Eeprom       = 0x02
    }

    public enum ValueStore : byte {
        Default = 0x00,

        /// <summary>
        /// EQ values are stored as gains
        /// </summary>
        Gains = 0x01,

        /// <summary>
        /// EQ values are stored as coefficients
        /// </summary>
        Coefficients = 0x02
    }

    public const int FuncGetEqInfo            = 0x00;
    public const int FuncGetFrequencies       = 0x01;
    public const int FuncGetFrequencyGains    = 0x02;
    public const int FuncSetFrequencyGains    = 0x03;
    public const int FuncGetMicNoiseReduction = 0x04;
    public const int FuncSetMicNoiseReduction = 0x05;

    public AudioEqualizer(HidPp20Features features) : base(features, FeatureId.AudioEqualizer) { }

    /// <summary>
    /// Returns information related to the eq table.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public EqualizerInfo GetEqInfo() {
        var response = CallFunction(FuncGetEqInfo);
        if (response.IsSuccess) {
            return new EqualizerInfo {
                BandCount  = response[0],
                DbRange    = response[1],
                ValueStore = (ValueStore)response[2],
                DbMin      = response[3],
                DbMax      = response[4]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Retrieves as many frequencies (in Hz) from the specified index as supported by the device. Frequencies are
    /// returned as unsigned 16-bit values in big-endian format. Using a HID++ long format, up to 7 frequencies can be
    /// returned at a time.
    /// * Example: Device has 10 frequencies and using HID++ long reports (16 bytes payload). 2 calls are needed:
    ///   - GetFrequencies(0, 6) → retrieves first 6 bands
    ///   - GetFrequencies(7, 3) → retrieves last 3 bands
    /// </summary>
    /// <param name="bandIndex">Value from 0 to bandCount-1</param>
    /// <param name="count"></param>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public ushort[] GetFrequencies(int bandIndex, int count) {
        var response = CallFunction(FuncGetFrequencies, (byte)bandIndex);
        if (response.IsSuccess) {
            var res = new List<ushort>();
            for (var ii = 1; ii < response.Data.Length && res.Count < count; ii += 2) {
                res.Add(response.ReadUInt16(ii));
            }

            return res.ToArray();
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Gets the active EQ gain values (in dB) for each frequency. Frequency gains are stored as 1 byte signed values.
    /// It is the responsibility of the device to ensure that the HID++ payload is large enough to receive each gain.
    /// For HID++ long reports, up to 15 bands can be retrieved at a time.
    /// </summary>
    /// <param name="location">Determines where the gains are retrieved from.</param>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public byte[] GetFrequencyGains(GainsLocation location) {
        var response = CallFunction(FuncGetFrequencyGains, (byte)location);
        if (response.IsSuccess) {
            return response.Data[1..];
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets EQ gain values.
    /// It is the responsibility of the device to ensure that the HID++ payload is large enough to send each gain.
    /// For HID++ long reports, up to 15 bands can be sent at a time.
    /// </summary>
    /// <param name="persistence">Determines how the frequency gains are persisted through a power cycle</param>
    /// <param name="values">Value in dB (signed 8-bit) of band at index N into the main EQ table</param>
    /// <exception cref="FeatureException"></exception>
    public void SetFrequencyGains(Persistence persistence, byte[] values) {
        var data     = ByteUtils.Combine(new[] { (byte)persistence }, values);
        var response = CallFunction(FuncSetFrequencyGains, data);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Returns whether the hardware noise reduction is currently enabled
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public bool GetMicNoiseReduction() {
        var response = CallFunction(FuncGetMicNoiseReduction);
        return response.IsSuccess ? response[0] == 0x01 : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the current hardware noise reduction
    /// </summary>
    /// <param name="enable"></param>
    /// <exception cref="FeatureException"></exception>
    public void SetMicNoiseReduction(bool enable) {
        var response = CallFunction(FuncSetMicNoiseReduction, (byte)(enable ? 0x01 : 0x00));
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    public struct EqualizerInfo {
        /// <summary>
        /// Number of frequency bands.
        /// </summary>
        public int BandCount;

        /// <summary>
        /// Range of dB. If dbMin and dbMax are both zero, the db min and max are calcuated as -dbRange and dbRange respectively.
        /// </summary>
        public int DbRange;

        /// <summary>
        ///  
        /// </summary>
        public ValueStore ValueStore;

        /// <summary>
        /// Minimum gain as a signed 8-bit value. If zero, a min of -dBRange is implied.
        /// </summary>
        public int DbMin;

        /// <summary>
        /// Maximum gain as a signed 8-bit value. If zero, a max of +dBRange is implied.
        /// </summary>
        public int DbMax;
    }
}