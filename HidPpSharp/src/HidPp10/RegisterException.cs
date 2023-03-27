namespace HidPpSharp.HidPp10;

public class RegisterException : Exception {
    public RegisterException(int address, ReportError error, string? message = null) :
        base($"({(RegisterId)address}) {error} {message}") {
        Error    = error;
        Response = null;
    }

    public RegisterException(RegisterReport report, string? message = null) :
        base($"({(RegisterId)report.Address}) {report.Error} {message}") {
        Response = report;
        Error    = report.Error;
    }

    public RegisterReport? Response { get; }

    public ReportError Error { get; }
}