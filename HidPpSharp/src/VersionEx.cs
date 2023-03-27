namespace HidPpSharp;

public static class VersionEx {
    public static string ToHexString(this Version version) {
        var s = $"{version.Major:X2}";

        if (version.Minor >= 0) {
            s += $".{version.Minor:X2}";
        }

        if (version.Build >= 0) {
            s += $".{version.Build:X2}";
        }

        if (version.Revision >= 0) {
            s += $".{version.Revision:X2}";
        }

        return s;
    }
}