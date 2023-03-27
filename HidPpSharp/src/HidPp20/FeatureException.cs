namespace HidPpSharp.HidPp20;

public class FeatureException : Exception {
    public FeatureException(FeatureId featureId, ReportError error, string? message = null) :
        base($"({featureId}) {error} {message}") {
        Error    = error;
        Response = null;
    }

    public FeatureException(FeatureId featureId, FeatureReport report, string? message = null) :
        base($"({featureId}) {report.Error} {message}") {
        Response = report;
        Error    = report.Error;
    }

    public FeatureReport? Response { get; }

    public ReportError Error { get; }
}