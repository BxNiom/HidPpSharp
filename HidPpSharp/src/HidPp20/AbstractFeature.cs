using BxEx.Math;
using log4net;

namespace HidPpSharp.HidPp20;

public abstract class AbstractFeature {
    public const              byte       ShortRequestId = 0x10;
    public const              byte       LargeRequestId = 0x11;
    protected static readonly FastRandom Random         = new();

    protected AbstractFeature(HidPp20Features features, FeatureId featureId, int? featureIndex = null) {
        Log = LogManager.GetLogger(
            $"{features}[Feature.{featureId}]");
        Features     = features;
        Device       = features.Device;
        FeatureId    = featureId;
        FeatureIndex = (byte)(featureIndex ?? Features.GetFeatureIndex(featureId));

        if (featureIndex == -1) {
            throw new FeatureException(featureId, ReportError.Unsupported,
                "feature is not supported or feature set is not loaded");
        }

        Version = Features.GetFeatureInfo(featureId)?.Version ?? 0;
    }

    protected ILog Log { get; }

    public IHidPpDevice    Device       { get; }
    public HidPp20Features Features     { get; }
    public FeatureId       FeatureId    { get; }
    public byte            FeatureIndex { get; }
    public byte            Version      { get; }

    public FeatureReport CallFunction(int func, params byte[] parameters) {
        return CallFunction(func, true, parameters)!;
    }

    public FeatureReport? CallFunction(int func, bool waitForResponse, params byte[] parameters) {
        Log.DebugFormat("CallFunction({0}, [{1}])", func, BitConverter.ToString(parameters));
        var requestId = parameters.Length + 4 > 7 ? LargeRequestId : ShortRequestId;
        var sw        = (byte)(Random.NextBytes(1)[0] & 0x0F);
        var funcSw    = (byte)((byte)((func << 4) & 0xF0) | sw);
        var header = new[] {
            requestId,
            Device.DeviceIndex,
            FeatureIndex,
            funcSw
        };

        var wData = ByteUtils.Combine(header, parameters);
        if (wData.Length < 7 && requestId == 0x10) {
            Array.Resize(ref wData, 7);
        } else if (wData.Length < 20 && requestId == 0x11) {
            Array.Resize(ref wData, 20);
        }


        if (!waitForResponse) {
            Features.Device.Write(wData);
            return null;
        }

        return WaitForResponse(wData, TimeSpan.FromMilliseconds(1000));
        // TODO Error handling for timeout!
    }

    private FeatureReport? WaitForResponse(byte[] data, TimeSpan timeout) {
        var deviceIndex = Device.DeviceIndex;
        var featureId   = data[2];
        var swId        = data[3] & 0x0F;
        var funcId      = (data[3] & 0xF0) >> 4;

        Log.DebugFormat("wait for response [{0}], timeout {1:0}ms", funcId,
            timeout.TotalMilliseconds);
        FeatureReport? report = null;

        var ev = new ManualResetEventSlim();
        var callBack = (EventHandler)((_, _) => {
            var buf = new byte[256];
            if (!Device.InputReceiver.TryRead(buf, 0, out var r)) {
                return;
            }

            if (r is not FeatureReport featureReport) {
                return;
            }

            if (featureReport.FeatureIndex == featureId &&
                featureReport.SoftwareId == swId &&
                featureReport.FunctionId == funcId &&
                featureReport.DeviceIndex == deviceIndex) {
                report = featureReport;
                ev.Set();
            }
        });
        Device.InputReceiver.Received += callBack;
        Device.Write(data);

        ev.Wait(timeout);

        if (!ev.IsSet) {
            Log.WarnFormat("Timeout");
            report = new FeatureReport(new byte[] { 0x11, data[1], 0xFF, data[3], 0x00, 0xFE });
        }

        Device.InputReceiver.Received -= callBack;
        Log.DebugFormat("response: {0}", report);
        return report;
    }

    public virtual string GetFeatureDescription() {
        return $"{FeatureId}: \n\tSupported:{Features.IsSupported(FeatureId)}";
    }
}