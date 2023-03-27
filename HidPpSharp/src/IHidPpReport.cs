namespace HidPpSharp;

public interface IHidPpReport {
    public HidPpProtocol Protocol    { get; }
    public byte[]        RawData     { get; }
    public int           Size        { get; }
    public int           HeaderSize  { get; }
    public byte          Id          { get; }
    public byte          DeviceIndex { get; }

    public bool        IsSuccess { get; }
    public byte        ErrorCode { get; }
    public ReportError Error     { get; }
    public byte this[int index] { get; }
    public byte[] Data { get; }
    public short  ReadInt16(int offset);
    public ushort ReadUInt16(int offset);
    public int    ReadInt32(int offset);
    public uint   ReadUInt32(int offset);
    public ulong  ReadNumber(int offset, int length);
    public bool   ReadBit(int offset, int bit);
}