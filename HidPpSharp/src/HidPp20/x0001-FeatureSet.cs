using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.FeatureSet)]
public class FeatureSet : AbstractFeature {
    public const int FuncGetCount     = 0x00;
    public const int FuncGetFeatureId = 0x01;

    public FeatureSet(HidPp20Features features) : base(features, FeatureId.FeatureSet) { }

    public int GetCount() {
        Log.Debug("GetCount()");
        var response = CallFunction(FuncGetCount);
        return response.IsSuccess ? response[0] : throw new FeatureException(FeatureId, response);
    }

    public FeatureInfo GetFeatureId(int featureIndex) {
        Log.DebugFormat("GetFeatureId({0})", featureIndex);
        var response = CallFunction(FuncGetFeatureId, (byte)featureIndex);

        if (response.IsSuccess) {
            var info = new FeatureInfo(featureIndex, response.ReadUInt16(0), (FeatureType)response[2], response[3]);
            Log.DebugFormat("feature: {0}", info);
            return info;
        }

        throw new FeatureException(FeatureId, response);
    }
}