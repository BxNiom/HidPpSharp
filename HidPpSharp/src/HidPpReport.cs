using HidPpSharp.DJ;
using HidPpSharp.HidPp10;
using HidPpSharp.HidPp20;

namespace HidPpSharp;

public abstract class HidPpReport : IHidPpReport {
    protected HidPpReport(byte[] rawData, HidPpProtocol protocol) {
        RawData  = rawData;
        Protocol = protocol;
        Size     = GetSize(rawData[0]);
    }

    public HidPpProtocol Protocol { get; }

    public byte[] RawData { get; }

    public int Size { get; }

    public byte Id {
        get => RawData[0];
    }

    public byte DeviceIndex {
        get => RawData[1];
    }

    public bool        IsSuccess  { get; protected init; }
    public byte        ErrorCode  { get; protected init; }
    public ReportError Error      { get; protected init; }
    public int         HeaderSize { get; protected init; }

    public byte[] Data {
        get => RawData[HeaderSize..];
    }

    public byte this[int index] {
        get => RawData[index + HeaderSize];
    }

    public virtual short ReadInt16(int offset) {
        CheckDataSize(offset, 2);
        offset += HeaderSize;
        return (short)((RawData[offset] << 8) | RawData[offset + 1]);
    }

    public virtual ushort ReadUInt16(int offset) {
        CheckDataSize(offset, 2);
        offset += HeaderSize;
        return (ushort)((RawData[offset] << 8) | RawData[offset + 1]);
    }

    public virtual int ReadInt32(int offset) {
        CheckDataSize(offset, 4);
        offset += HeaderSize;
        return (RawData[offset] << 24) |
               (RawData[offset + 1] << 16) |
               (RawData[offset + 2] << 8) |
               RawData[offset + 3];
    }

    public virtual uint ReadUInt32(int offset) {
        CheckDataSize(offset, 4);
        offset += HeaderSize;
        return (uint)((RawData[offset] << 24) |
                      (RawData[offset + 1] << 16) |
                      (RawData[offset + 2] << 8) |
                      RawData[offset + 3]);
    }

    public virtual ulong ReadNumber(int offset, int length) {
        CheckDataSize(offset, length);
        offset += 4;
        ulong res = 0;
        for (var ii = 0; ii < length; ii++) {
            res = (res << 8) | RawData[offset + ii];
        }

        return res;
    }

    public virtual bool ReadBit(int offset, int bit) {
        CheckDataSize(offset, 1);
        offset += 4;
        return RawData[offset].IsBitSet(bit);
    }

    public static int GetSize(byte id) {
        return id switch {
            // Short HID++
            0x10 => 7,
            // Long HID++
            0x11 => 20,
            // DJ Short
            0x20 => 15,
            // DJ Long
            0x21 => 32,
            _    => 0
        };
    }

    protected void CheckDataSize(int offset, int count) {
        if (RawData.Length <= HeaderSize + offset + count) {
            throw new ArgumentOutOfRangeException();
        }
    }

    public static bool TryParse(byte[] data, out IHidPpReport? report) {
        report = data[0] switch {
            0x10 => new RegisterReport(data),
            0x11 =>
                data[2] switch {
                    >= 0x80 => new RegisterReport(data),
                    _       => new FeatureReport(data)
                },
            0x20 or 0x21 => new DjReport(data), // DJ
            _            => null
        };

        return report != null;
    }

    public override string ToString() {
        return
            $"{nameof(Protocol)}: {Protocol}, {nameof(Size)}: {Size}, {nameof(Id)}: {Id}, " +
            $"{nameof(DeviceIndex)}: {DeviceIndex}, {nameof(IsSuccess)}: {IsSuccess}, {nameof(ErrorCode)}: {ErrorCode}, " +
            $"{nameof(Error)}: {Error}, {nameof(HeaderSize)}: {HeaderSize}";
    }
}