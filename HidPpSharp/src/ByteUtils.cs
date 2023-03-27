using System.Text;

namespace HidPpSharp;

internal static class ByteUtils {

    public static byte[] Pack(params object[] primitives) {
        using var ms = new MemoryStream();
        
        foreach (var o in primitives) {
            var oType = o.GetType();
            var buf   = Array.Empty<byte>();
            
            if (oType == typeof(byte)) {
                ms.WriteByte((byte)o);
                continue;
            } 
            
            if (oType == typeof(byte[])) {
                buf = (byte[])o;
            } else if (oType == typeof(ushort) || oType == typeof(short)) {
                buf = BitConverter.GetBytes((ushort)o);
            } else if (oType == typeof(uint) || oType == typeof(int)) {
                buf = BitConverter.GetBytes((uint)o);
            } else if (oType == typeof(ulong) || oType == typeof(long)) {
                buf = BitConverter.GetBytes((ulong)o);
            } else if (oType == typeof(float)) {
                buf = BitConverter.GetBytes((float)o);
            } else if (oType == typeof(double)) {
                buf = BitConverter.GetBytes((double)o);
            } else if (oType == typeof(string)) {
                buf = Encoding.ASCII.GetBytes((string)o);
            } else {
                throw new ArgumentException($"{oType} is not primitive");
            }
            
            ms.Write(buf, 0, buf.Length);
            ms.Flush();
        }

        return ms.ToArray();
    }
    
    public static bool PartEqual(this byte[] left, byte[] right) {
        for (var ii = 0; ii < Math.Min(left.Length, right.Length); ii++) {
            if (left[ii] != right[ii]) {
                return false;
            }
        }

        return true;
    }

    public static byte[] Combine(params byte[][] arrays) {
        var res = Array.Empty<byte>();
        foreach (var ar in arrays) {
            var offset = res.Length;
            Array.Resize(ref res, res.Length + ar.Length);
            Buffer.BlockCopy(ar, 0, res, offset, ar.Length);
        }

        return res;
    }

    public static string ToString(byte[] array) {
        var len = Math.Min(array.Length, array[0] == 0x10 ? 7 : 20);
        var res = BitConverter.ToString(array[..len]);
        return res;
    }

    public static byte SetBit(this byte b, int bit, bool set = true) {
        if (bit > 7) {
            throw new ArgumentOutOfRangeException(nameof(bit));
        }

        return set ? (byte)(b | (1 << bit)) : (byte)(b & ~(1 << bit));
    }

    public static bool IsBitSet(this byte b, int bit) {
        return (b & 1 << bit) > 0;
    }

    public static ushort ToUInt16(this byte[] b, int offset) {
        return (ushort)(b[offset] << 8 | b[offset + 1]);
    }

    public static short ToInt16(this byte[] b, int offset) {
        return (short)(b[offset] << 8 | b[offset + 1]);
    }
}