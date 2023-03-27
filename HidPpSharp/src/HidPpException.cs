using System.Runtime.Serialization;

namespace HidPpSharp;

public class HidPpException : Exception {
    public HidPpException(IHidPpDevice? device) : this(device, string.Empty, null) { }
    public HidPpException(IHidPpDevice? device, string? message) : this(device, message, null) { }
    public HidPpException(string message) : this(null, message, null) { }
    public HidPpException(string message, Exception? innerException) : this(null, message, innerException) { }

    public HidPpException(IHidPpDevice? device, string? message, Exception? innerException) : base(
        $"{device?.ToString() ?? string.Empty}: {message}", innerException) { }

    protected HidPpException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}