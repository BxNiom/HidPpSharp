using System.Text;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.DeviceFriendlyName)]
public class DeviceFriendlyName : Feature {
    public const byte FuncGetFriendlyNameLen     = 0x00;
    public const byte FuncGetFriendlyName        = 0x01;
    public const byte FuncGetDefaultFriendlyName = 0x02;
    public const byte FuncSetFriendlyName        = 0x03;
    public const byte FuncResetFriendlyName      = 0x04;

    public DeviceFriendlyName(HidPp20Features features) : base(features, FeatureId.DeviceFriendlyName) { }

    /// <summary>
    /// Get the length of current name, default name, and maximum allowed length for device name. Length is in terms of
    /// useful bytes.
    /// </summary>
    /// <exception cref="FeatureException"></exception>
    public FriendlyNameLen GetFriendlyNameLen() {
        var response = CallFunction(FuncGetFriendlyNameLen);
        if (response.IsSuccess) {
            return new FriendlyNameLen {
                NameLength        = response[0],
                NameMaxLength     = response[1],
                DefaultNameLength = response[2]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Get a current Friendly Name chunk, starting from a byte index lower than nameLen returned by GetFriendlyNameLen().
    /// </summary>
    /// <param name="index">Index of the first byte to copy [0..nameLen-1]</param>
    /// <exception cref="FeatureException"></exception>
    public string GetFriendlyName(int index) {
        var response = CallFunction(FuncGetFriendlyName, (byte)index);

        if (response.IsSuccess) {
            return Encoding.ASCII.GetString(response.Data[1..]);
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Get a Friendly Name chunk, starting from a byte index lower than defaultNameLen returned by GetFriendlyNameLen().
    /// </summary>
    /// <param name="index">Index of the first byte to copy [0..defaultNameLen-1]</param>
    /// <exception cref="FeatureException"></exception>
    public string GetDefaultFriendlyName(int index) {
        var response = CallFunction(FuncGetDefaultFriendlyName, (byte)index);

        if (response.IsSuccess) {
            return Encoding.ASCII.GetString(response.Data[1..]);
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Set a Device Friendly Name chunk, starting at byteIndex. Existing string is overwritten, extended or shorten by
    /// the chunk, considering that resulting Friendly Name new length is byteIndex + chunk.Length, truncated to maximum
    /// allowed length.
    /// Change is immediate (on device) but usually seen by hosts at reconnection.
    /// </summary>
    /// <param name="index">Index of the first device name byte to write.</param>
    /// <param name="nameChunk">The device name chunk to write</param>
    /// <returns>Resulting Device Friendly Name len, 0 in case of failure (HW write failure).</returns>
    /// <exception cref="FeatureException"></exception>
    public int SetFriendlyName(int index, string nameChunk) {
        if (nameChunk.Length > 14) {
            nameChunk = nameChunk.Substring(0, 14);
        }

        var response = CallFunction(FuncSetFriendlyName,
            ByteUtils.Combine(new[] { (byte)index }, Encoding.ASCII.GetBytes(nameChunk)));

        if (response.IsSuccess) {
            return response[0];
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Reset current Device Friendly Name to Default Friendly Name.
    /// Change is immediate (on device) but usually seen by hosts at reconnection.
    /// </summary>
    /// <returns>Resulting Device Friendly Name len, 0 in case of failure (HW write failure).</returns>
    /// <exception cref="FeatureException"></exception>
    public int ResetFriendlyName() {
        var response = CallFunction(FuncResetFriendlyName);

        if (response.IsSuccess) {
            return response[0];
        }

        throw new FeatureException(FeatureId, response);
    }

    public string GetFriendlyName() {
        var nameLen = GetFriendlyNameLen();
        var name    = "";

        while (name.Length < nameLen.NameLength) {
            name += GetFriendlyName(name.Length);
        }

        return name;
    }

    public string GetDefaultFriendlyName() {
        var nameLen = GetFriendlyNameLen();
        var name    = "";

        while (name.Length < nameLen.DefaultNameLength) {
            name += GetDefaultFriendlyName(name.Length);
        }

        return name;
    }

    public int GetMaxFriendlyNameLength() {
        var nameLen = GetFriendlyNameLen();
        return nameLen.NameMaxLength;
    }

    public int SetFriendlyName(string name) {
        var nameLen = GetFriendlyNameLen();

        if (name.Length > nameLen.NameMaxLength) {
            name = name.Substring(0, nameLen.NameMaxLength);
        }

        var len = 0;
        while (len < name.Length) {
            len = SetFriendlyName(len, name.Substring(len));
        }

        return len;
    }

    public struct FriendlyNameLen {
        /// <summary>
        /// Current Friendly Name byte length.
        /// </summary>
        public byte NameLength;

        /// <summary>
        /// Maximum allowed Friendly Name byte length.
        /// </summary>
        public byte NameMaxLength;

        /// <summary>
        /// Default Friendly Name byte length.
        /// </summary>
        public byte DefaultNameLength;
    }
}