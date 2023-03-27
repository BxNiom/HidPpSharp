namespace HidPpSharp;

public enum ReportError {
    NoError = 0x00,
    Unknown,
    InvalidSubId,
    InvalidAddress,
    InvalidValue,
    ConnectFailed,
    TooManyDevices,
    AlreadyExists,
    Busy,
    UnknownDevice,
    ResourceError,
    RequestUnavailable,
    InvalidArgument,
    WrongPinCode,
    HardwareError,
    LogitechInternal,
    InvalidFeature,
    InvalidFunctionId,
    Unsupported,
    Timeout,
    HidPpInternal,
    OutOfRange,
    KeepAliveTimeout
}