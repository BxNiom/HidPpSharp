namespace HidPpSharp.HidPp20.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class FeatureAttribute : Attribute {
    public FeatureAttribute(params FeatureId[] feature) {
        Feature = feature;
    }

    public FeatureId[] Feature { get; }
}