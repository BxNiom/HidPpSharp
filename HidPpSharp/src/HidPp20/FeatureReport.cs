namespace HidPpSharp.HidPp20;

public class FeatureReport : HidPpReport {
    public FeatureReport(byte[] rawData) : base(rawData, HidPpProtocol.HidPp2) {
        HeaderSize = 4;
        IsSuccess  = rawData[2] != 0xFF;
        if (IsSuccess) {
            ErrorCode = 0x00;
            Error     = ReportError.NoError;
        } else {
            ErrorCode = rawData[5];
            Error = ErrorCode switch {
                0x00 => ReportError.NoError,
                0x01 => ReportError.Unknown,
                0x02 => ReportError.InvalidArgument,
                0x03 => ReportError.OutOfRange,
                0x04 => ReportError.HardwareError,
                0x05 => ReportError.LogitechInternal,
                0x06 => ReportError.InvalidFeature,
                0x07 => ReportError.InvalidFunctionId,
                0x08 => ReportError.Busy,
                0x09 => ReportError.Unsupported,
                0xFE => ReportError.Timeout,
                0xFF => ReportError.HidPpInternal,
                _    => ReportError.Unknown
            };
        }
    }

    public byte FeatureIndex {
        get => RawData[2];
    }

    public byte FunctionId {
        get => (byte)((RawData[3] & 0xF0) >> 4);
    }

    public byte SoftwareId {
        get => (byte)(RawData[3] & 0x0F);
    }
}