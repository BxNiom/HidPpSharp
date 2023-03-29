using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// This feature describes the adjustment of sidetone gain for audio devices.
/// Sidetone is a loopback mechanism by which sound from an audio device’s microphone is
/// transmitted directly to the earpiece.
/// </summary>
[Feature(FeatureId.AudioSideToneAdjustment)]
public class AudioSideToneAdjustment : AbstractFeature {
    public enum MuteJack {
        Jack1 = 0x00,
        Jack2 = 0x01,
        Jack3 = 0x02,
        Jack4 = 0x04,
        Jack5 = 0x08,
        Jack6 = 0x10,
        Jack7 = 0x20,
        Jack8 = 0x40,
        Jack9 = 0x80
    }

    public const int FuncGetSideToneLevel = 0x00;
    public const int FuncSetSideToneLevel = 0x01;
    public const int FuncGetSideToneMute  = 0x02;
    public const int FuncSetSideToneMute  = 0x03;

    public AudioSideToneAdjustment(HidPp20Features features) : base(features, FeatureId.AudioSideToneAdjustment) { }

    /// <summary>
    /// Returns the current sidetone level from 0-100.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public int GetSideToneLevel() {
        var response = CallFunction(FuncGetSideToneLevel);
        return response.IsSuccess ? response[0] : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the current sidetone level. The level must be a value from 0-100.
    /// </summary>
    /// <param name="level">Value from 0-100.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="FeatureException"></exception>
    public void SetSideToneLevel(int level) {
        if (level is < 0 or > 100) {
            throw new ArgumentOutOfRangeException(nameof(level), ">= 0 and  <= 100");
        }

        var response = CallFunction(FuncSetSideToneLevel, (byte)level);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// Returns the current sidetone mute status for a specific channel. 0 = Mute OFF (sidetone on). 1 = Mute ON (no sidetone).
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public MuteJack GetSideToneMute() {
        var response = CallFunction(FuncGetSideToneMute);
        return response.IsSuccess ? (MuteJack)response[0] : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Sets the current mute status for every channel (up to 8). 0 = Mute OFF (sidetone on). 1 = Mute ON (no sidetone).
    /// You can individually control up to 8 sidetone channels.BIT 0 is for the first channel.
    /// </summary>
    /// <param name="jackStates">Set to MUTE sidetone.</param>
    /// <param name="mask">Set 1 to change sidetone value of this channel. Set 0 to ignore that channel.</param>
    /// <exception cref="FeatureException"></exception>
    public void SetSideToneMute(MuteJack jackStates, byte mask = 0xFF) {
        var response = CallFunction(FuncSetSideToneMute, mask, (byte)jackStates);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }
}