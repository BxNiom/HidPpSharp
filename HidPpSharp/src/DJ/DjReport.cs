using HidSharp.Reports;

namespace HidPpSharp.DJ;

public class DjReport : HidPpReport {
    public byte[] HidPayload { get; }

    public DjReportType DjReportType {
        get {
            return RawData[2] switch {
                <= 0x3F => DjReportType.RFReport,
                <= 0x7F => DjReportType.Notification,
                _       => DjReportType.Command
            };
        }
    }

    public RfReportType RfReportType { get; }

    public byte RfReportTypeCode {
        get => RawData[2];
    }

    internal DjReport(byte[] rawData) : base(rawData, HidPpProtocol.Dj) {
        HeaderSize = 2;
        IsSuccess  = rawData[2] != 0x7F;
        if (IsSuccess) {
            Error     = ReportError.NoError;
            ErrorCode = 0x00;
        } else {
            ErrorCode = rawData[3];
            Error = ErrorCode switch {
                0x01 => ReportError.KeepAliveTimeout,
                _    => ReportError.Unknown
            };
        }

        if (rawData[2] <= 0x3F) {
            RfReportType = (RfReportType)rawData[2];
            HidPayload = rawData[1] switch {
                0x01 or 0x02         => rawData[3..10],
                0x03                 => rawData[3..6],
                0x04 or 0x08 or 0x0E => rawData[3..3],
                _                    => rawData[3..]
            };
        } else {
            RfReportType = RfReportType.None;
            HidPayload   = Array.Empty<byte>();
        }
    }
}

public enum DjReportType {
    RFReport,
    Notification,
    Command
}

public enum RfReportType {
    None                 = 0x00,
    Keyboard             = 0x01,
    Mouse                = 0x02,
    ConsumerControl      = 0x03,
    SystemControl        = 0x04,
    MicrosoftMediaCenter = 0x08,
    KeyboardLed          = 0x0E
}