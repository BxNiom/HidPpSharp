using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.Root)]
public class Root : AbstractFeature {
    public const int FuncGetFeature         = 0x00;
    public const int FuncGetProtocolVersion = 0x01;

    public Root(HidPp20Features features) : base(features, FeatureId.Root, 0x00) { }

    public FeatureInfo GetFeature(FeatureId feature) {
        Log.DebugFormat("GetFeature({0})", feature);
        var response = CallFunction(FuncGetFeature, ByteUtils.Pack((ushort)feature));

        if (response.IsSuccess) {
            var info = new FeatureInfo(response[0] + 1, (ushort)feature, (FeatureType)response[1], response[2]);
            Log.DebugFormat("feature: {0}", info);
            return info;
        }

        throw new FeatureException(FeatureId, response);
    }

    public ProtocolVersion GetProtocolVersion() {
        Log.Debug("GetProtocolVersion()");
        var pingData = Random.NextBytes(1)[0];
        var response = CallFunction(FuncGetProtocolVersion, 0x00, 0x00, pingData);

        if (response.IsSuccess) {
            return new ProtocolVersion(response[0], response[1]);
        }

        if (response[2] != pingData) {
            throw new FeatureException(FeatureId, ReportError.Unknown, "Wrong ping data received");
        }

        throw new FeatureException(FeatureId, response);
    }
}