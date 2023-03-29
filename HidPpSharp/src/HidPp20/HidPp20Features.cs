using System.Collections;
using System.Reflection;
using HidPpSharp.HidPp20.Attributes;
using log4net;

namespace HidPpSharp.HidPp20;

public class HidPp20Features : IEnumerable<FeatureId> {
    private static readonly Dictionary<FeatureId, ConstructorInfo> FeatureClasses = new();

    private readonly List<FeatureInfo> _features;

    static HidPp20Features() {
        // Load all feature classes
        if (FeatureClasses.Any()) {
            return;
        }

        var log = LogManager.GetLogger("FeatureLoader");
        foreach (var t in typeof(HidPp20Features).Assembly.GetTypes()) {
            if (typeof(AbstractFeature).IsAssignableFrom(t)) {
                log.DebugFormat("found feature class: {0}", t);
                var attrib = t.GetCustomAttribute<FeatureAttribute>();
                if (attrib == null) {
                    log.DebugFormat("class has no feature attribute");
                    continue;
                }

                var ctor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
                    new[] { typeof(HidPp20Features) }, null);

                if (ctor == null) {
                    log.DebugFormat("no constructor found for {0}", t);
                    continue;
                }

                foreach (var f in attrib.Feature) {
                    log.InfoFormat("-> using '{0}' for feature '{1}'", t, f);
                    FeatureClasses.Add(f, ctor);
                }
            }
        }
    }

    public HidPp20Features(IHidPpDevice device) {
        Device    = device;
        _features = new List<FeatureInfo>();
        _features.Clear();
        ReadFeatures();
    }

    public IHidPpDevice Device { get; }

    public IEnumerator<FeatureId> GetEnumerator() {
        return (from fi in _features
                select (FeatureId)fi.Code).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public int GetFeatureIndex(FeatureId featureId) {
        var fid = (ushort)featureId;
        return (from f in _features where f.Code == fid select f.Index).FirstOrDefault(-1);
    }

    public FeatureInfo? GetFeatureInfo(FeatureId featureId) {
        var fid = (ushort)featureId;
        return (from f in _features where f.Code == fid select f).FirstOrDefault();
    }

    public bool IsSupported(FeatureId featureId) {
        var fid = (ushort)featureId;
        return (from f in _features where f.Code == fid select f).Any();
    }

    public AbstractFeature GetFeature(FeatureId featureId) {
        if (!FeatureClasses.ContainsKey(featureId)) {
            throw new NotSupportedException();
        }

        var ctor = FeatureClasses[featureId];
        return (AbstractFeature)ctor.Invoke(new object?[] { this });
    }

    public T GetFeature<T>() where T : AbstractFeature {
        var ctor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
            new[] { typeof(HidPp20Features) }, null);

        if (ctor == null) {
            throw new InvalidOperationException("no constructor found");
        }

        return (T)ctor.Invoke(new object?[] { this });
    }

    private void ReadFeatures() {
        var featureSetInfo = new Root(this).GetFeature(FeatureId.FeatureSet);
        _features.Add(featureSetInfo);

        var featureSet = new FeatureSet(this);
        var count      = featureSet.GetCount();
        for (var ii = 1; ii <= count; ii++) {
            var info = featureSet.GetFeatureId(ii);
            _features.Add(info);
        }
    }
}