using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// This features gives the possibility to play various effects or waveforms.
/// The configuration consists to enable / disable the backlight or the effects and to choose the backlight
/// effect.
/// An event is generated whenever user changes the backlight.
/// This feature also can be used by keyboard that has similar backlight sytem.
/// </summary>
[Feature(FeatureId.Backlight, FeatureId.BacklightV1, FeatureId.BacklightV3)]
public class Backlight : AbstractFeature {
    public enum BacklightEffect : byte {
        Static         = 0x00,
        None           = 0x01,
        BreathingLight = 0x02,
        Contrast       = 0x03,
        Reaction       = 0x04,
        Random         = 0x05,
        Waves          = 0x06,
        NoChange       = 0xFF
    }

    public enum BacklightStatus {
        DisabledBySoftware        = 0x00,
        DisabledByCriticalBatters = 0x01,
        AutomaticMode             = 0x02,
        AutomaticModeSaturated    = 0x03,
        ManualMode                = 0x04
    }

    public const int FuncGetBacklightConfig = 0x00;
    public const int FuncSetBacklightConfig = 0x01;
    public const int FuncGetBacklightInfo   = 0x02;
    public const int FuncSetBacklightEffect = 0x03;

    public Backlight(HidPp20Features features) : base(features, FeatureId.Backlight) { }

    /// <summary>
    /// The function returns the current configuration as well as the options that can be set through SW
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public BacklightConfig GetBacklightConfig() {
        var response = CallFunction(FuncGetBacklightConfig);
        if (response.IsSuccess) {
            var effects = new List<BacklightEffect>();
            if (response[3].IsBitSet(0)) {
                effects.Add(BacklightEffect.Static);
            }

            if (response[3].IsBitSet(1)) {
                effects.Add(BacklightEffect.None);
            }

            if (response[3].IsBitSet(2)) {
                effects.Add(BacklightEffect.BreathingLight);
            }

            if (response[3].IsBitSet(3)) {
                effects.Add(BacklightEffect.Contrast);
            }

            if (response[3].IsBitSet(4)) {
                effects.Add(BacklightEffect.Reaction);
            }

            if (response[3].IsBitSet(5)) {
                effects.Add(BacklightEffect.Random);
            }

            if (response[3].IsBitSet(6)) {
                effects.Add(BacklightEffect.Waves);
            }

            return new BacklightConfig {
                Enabled               = response[0].IsBitSet(0),
                WowEffectEnabled      = response[1].IsBitSet(0),
                WowEffectSupported    = response[2].IsBitSet(0),
                CrownEffectEnabled    = response[1].IsBitSet(1),
                CrownEffectSupported  = response[2].IsBitSet(1),
                PowerSaveEnabled      = response[1].IsBitSet(2),
                PowerSaveSupported    = response[2].IsBitSet(2),
                SupportedEffects      = effects.ToArray(),
                OtherEffectsSupported = response.ReadUInt16(3) > 0x00EF
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Set the configuration in persistent manner (written in NVM)
    /// If an option is not supported, the setting for that option is just discarded (and read back as false)
    /// Note:
    /// This configuration is only active when there is no active keymap customization. In case of active keymap
    /// customization, the backlight effect defined in the active keymap  customization will be used.
    /// </summary>
    /// <param name="backlight">Enable backlight; when false, the whole backlight system is totally disabled, all other settings are
    /// not applicable, and no event is sent.</param>
    /// <param name="powerSave">Enable "pwrSave" disable the whole backlight system at critical level</param>
    /// <param name="crownEffect">Enable the "crown" effect whenever the crown is touched</param>
    /// <param name="wowEffect">Enable the "wow" effect at power-on.</param>
    /// <param name="effect">Set the effect to apply for FADE-IN/FADE-OUT phases.</param>
    /// <exception cref="FeatureException"></exception>
    public void SetBacklightConfig(bool backlight, bool powerSave, bool crownEffect, bool wowEffect,
                                   BacklightEffect effect) {
        var config = backlight ? 0x01 : 0x00;
        powerSave = powerSave && Version >= 1;
        var options  = (powerSave ? 0x04 : 0x00) | (crownEffect ? 0x02 : 0x00) | (wowEffect ? 0x01 : 0x00);
        var response = CallFunction(FuncSetBacklightConfig, (byte)config, (byte)options, (byte)effect);

        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// The function returns various backlight information
    /// </summary>
    /// <exception cref="FeatureException"></exception>
    public BacklightInfo GetBacklightInfo() {
        var response = CallFunction(FuncGetBacklightInfo);
        if (response.IsSuccess) {
            return new BacklightInfo {
                NumberOfLevels = response[0],
                CurrentLevel   = response[1],
                Status         = (BacklightStatus)response[2],
                Effect         = (BacklightEffect)response[3]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Set the backlight effect in temporary manner (written in RAM).
    /// </summary>
    /// <param name="effect">Set the effect to apply for FADE-IN/FADE-OUT phases.</param>
    /// <exception cref="FeatureException"></exception>
    public void SetBacklightEffect(BacklightEffect effect) {
        if (Version < 2) {
            throw new NotSupportedException("wrong feature version, must be equal or greater 2");
        }

        var response = CallFunction(FuncSetBacklightEffect, (byte)effect);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    public struct BacklightConfig {
        /// <summary>
        /// Enable backlight; when false, the whole backlight system is totally disabled, all other settings are
        /// not applicable, and no event is sent.
        /// This option must always be supported by the device.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// Enable the "wow" effect at power-on.
        /// </summary>
        public bool WowEffectEnabled;

        /// <summary>
        /// The "wow" option is supported by the device
        /// </summary>
        public bool WowEffectSupported;

        /// <summary>
        /// Enable the "crown" effect whenever the crown is touched
        /// </summary>
        public bool CrownEffectEnabled;

        /// <summary>
        /// The "crown" option is supported by the device
        /// </summary>
        public bool CrownEffectSupported;

        /// <summary>
        /// Enable "pwrSave" when true, the whole backlight system is totally disabled at critical battery level.
        /// </summary>
        public bool PowerSaveEnabled;

        /// <summary>
        /// The "pwrSave" option is supported by the device
        /// </summary>
        public bool PowerSaveSupported;

        /// <summary>
        /// List of predefined effects supported by the device
        /// </summary>
        public BacklightEffect[] SupportedEffects;

        /// <summary>
        /// Other effects supported by RFU
        /// </summary>
        public bool OtherEffectsSupported;
    }

    public struct BacklightInfo {
        /// <summary>
        /// The number of light intensity levels that user can set.
        /// Levels are starting from lowest level 0 (backlight off or at 0%) to highest level NumberOfLevels-1 (backlight
        /// at 100%). For example, if nbLevels = 16, valid levels are 0, 1, 2 … 14, 15.
        /// These levels are either set automatically by the ALS system, or forced manually (Backlight +/-buttons are
        /// used to switch from one level to another one). Note that effects as "wow" or "crown" may actually use much
        /// more levels.
        /// </summary>
        public int NumberOfLevels;

        /// <summary>
        /// The current backlight intensity
        /// </summary>
        public int CurrentLevel;

        /// <summary>
        /// Backlight status
        /// </summary>
        public BacklightStatus Status;

        /// <summary>
        /// current effect applied to FADE-IN/FADE-OUT phases. See BacklightConfig
        /// </summary>
        public BacklightEffect Effect;
    }
}