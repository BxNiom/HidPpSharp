namespace HidPpSharp;

public static class RandomEx {
    public static byte[] NextBytes(this Random random, int count) {
        var buf = new byte[count];
        random.NextBytes(buf);
        return buf;
    }
}