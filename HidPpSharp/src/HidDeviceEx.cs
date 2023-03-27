using System.Text.RegularExpressions;
using HidSharp;

namespace HidPpSharp;

public struct HidDeviceLinuxInfo {
    public int RootHub;
    public int HubPort;
    public int Config;
    public int Interface;

    public override string ToString() {
        return
            $"{nameof(RootHub)}: {RootHub}, {nameof(HubPort)}: {HubPort}, {nameof(Config)}: {Config}, {nameof(Interface)}: {Interface}";
    }
}

public static class HidDeviceEx {
    public static HidDeviceLinuxInfo? FsInfo(this HidDevice device) {
        if (!OperatingSystem.IsLinux()) {
            throw new NotImplementedException("only available under linux");
        }

        var pattern = @"(?<roothub>\d{1,2})-(?<hubport>\d{1,2})\:(?<config>\d{1,2})\.(?<interface>\d{1,2})";
        var match   = Regex.Match(device.DevicePath, pattern);
        if (match.Groups.TryGetValue("roothub", out var rootHubStr) &&
            match.Groups.TryGetValue("hubport", out var hubPortStr) &&
            match.Groups.TryGetValue("config", out var configStr) &&
            match.Groups.TryGetValue("interface", out var iFaceStr)) {
            return new HidDeviceLinuxInfo {
                RootHub   = int.Parse(rootHubStr.Value),
                HubPort   = int.Parse(hubPortStr.Value),
                Config    = int.Parse(configStr.Value),
                Interface = int.Parse(iFaceStr.Value)
            };
        }

        return null;
    }
}