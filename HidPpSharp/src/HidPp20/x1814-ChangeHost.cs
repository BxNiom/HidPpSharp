using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.ChangeHost)]
public class ChangeHost : AbstractFeature {
    public const byte FuncGetHostInfo    = 0x00;
    public const byte FuncSetCurrentHost = 0x01;
    public const byte FuncGetCookies     = 0x02;
    public const byte FuncSetCookie      = 0x03;

    public ChangeHost(HidPp20Features features) : base(features, FeatureId.ChangeHost) { }

    /// <summary>
    /// Get info on the host implementation
    /// </summary>
    /// <exception cref="FeatureException"></exception>
    public HostInfo GetHostInfo() {
        var response = CallFunction(FuncGetHostInfo);
        if (response.IsSuccess) {
            return new HostInfo {
                NumberOfHosts      = response[0],
                CurrentHost        = response[1],
                EnhancedHostSwitch = response[2].IsBitSet(0)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Set the current host; no return, since, if successful, the device will most probably reset
    /// </summary>
    /// <param name="host">Index of the new host to select</param>
    /// <exception cref="FeatureException"></exception>
    public bool SetCurrentHost(int host) {
        var response = CallFunction(FuncSetCurrentHost, (byte)host);

        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }

        return true;
    }

    /// <summary>
    /// Get the data byte for each host
    /// </summary>
    /// <returns>For every host, the SW has the possibility to read / write a personal data byte ("Cookie"), that will
    /// be stored permanently in the device’s non volatile memory. It can be used for example to determine if a given
    /// host has a specific SW installed</returns>
    /// <exception cref="FeatureException"></exception>
    public byte[] GetCookies() {
        var response = CallFunction(FuncGetCookies);
        if (response.IsSuccess) {
            return response.Data;
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Write the specified cookie
    /// </summary>
    /// <param name="host">Channel / host index</param>
    /// <param name="cookie">The value to write</param>
    /// <exception cref="FeatureException"></exception>
    public bool SetCookie(int host, int cookie) {
        var response = CallFunction(FuncSetCookie, (byte)host, (byte)cookie);

        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }

        return true;
    }

    public struct HostInfo {
        /// <summary>
        /// The number of hosts / Rf channels
        /// </summary>
        public int NumberOfHosts;

        /// <summary>
        /// The current host index, starting from 0. If we have nbHost, the current host can be 0…nbHost - 1
        /// </summary>
        public int CurrentHost;

        /// <summary>
        /// Currently default is false, if true the device tries to connect to host, if no success it reconnects to the
        /// original host
        /// </summary>
        public bool EnhancedHostSwitch;
    }
}