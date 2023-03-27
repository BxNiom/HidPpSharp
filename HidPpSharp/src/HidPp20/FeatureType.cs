namespace HidPpSharp.HidPp20;

[Flags]
public enum FeatureType : byte {
    /// <summary>
    /// A compliance feature that can be permanently deactivated. It is usually also hidden and
    /// engineering.
    /// </summary>
    ComplianceDeactivatable = 0x08,

    /// <summary>
    /// A manufacturing feature that can be permanently deactivated. It is usually also hidden and
    /// engineering.
    /// </summary>
    ManufacturingDeactivatable = 0x10,

    /// <summary>
    /// A hidden feature that has been disabled for user software. Used for internal testing and
    /// manufacturing.
    /// </summary>
    Engineering = 0x20,

    /// <summary>
    /// A SW hidden feature is a feature that should not be known/managed/used by end user
    /// configuration SW. The host should ignore this type of features.
    /// </summary>
    Hidden = 0x40,

    /// <summary>
    /// An obsolete feature is a feature that has been replaced by a newer one, but is advertised in
    /// order for older SWs to still be able to support the feature (in case the old SW does not know
    /// yet the newer one).
    /// </summary>
    Obsolete = 0x80
}