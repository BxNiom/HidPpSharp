namespace HidPpSharp.HidPp10;

public class RegisterReport : HidPpReport {
    public RegisterReport(byte[] rawData) : base(rawData, HidPpProtocol.HidPp1) {
        HeaderSize = 4;
        IsSuccess  = rawData[2] != 0x8F;
        if (IsSuccess) {
            SubId     = rawData[2];
            Address   = rawData[3];
            ErrorCode = 0x00;
            Error     = ReportError.NoError;
        } else {
            SubId     = rawData[3];
            Address   = rawData[4];
            ErrorCode = rawData[5];
            Error = ErrorCode switch {
                0x00 => ReportError.NoError,
                0x01 => ReportError.InvalidSubId,
                0x02 => ReportError.InvalidAddress,
                0x03 => ReportError.InvalidValue,
                0x04 => ReportError.ConnectFailed,
                0x05 => ReportError.TooManyDevices,
                0x06 => ReportError.AlreadyExists,
                0x07 => ReportError.Busy,
                0x08 => ReportError.UnknownDevice,
                0x09 => ReportError.ResourceError,
                0x0A => ReportError.RequestUnavailable,
                0x0B => ReportError.InvalidArgument,
                0x0C => ReportError.WrongPinCode,
                0xFE => ReportError.Timeout,
                0xFF => ReportError.HidPpInternal,
                _    => ReportError.Unknown
            };
        }
    }

    public byte SubId   { get; }
    public byte Address { get; }
}