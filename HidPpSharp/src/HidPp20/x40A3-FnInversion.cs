using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

// TODO Events

[Feature(FeatureId.FnInversion)]
public class FnInversion : Feature {
    public const int FuncGetGlobalFnInversion = 0x00;
    public const int FuncSetGlobalFnInversion = 0x01;

    public FnInversion(HidPp20Features features) : base(features, FeatureId.FnInversion) { }

    public FnInversionInfo GetGlobalFnInversion(int hostIndex) {
        var response = CallFunction(FuncGetGlobalFnInversion, (byte)hostIndex);
        if (response.IsSuccess) {
            return new FnInversionInfo(response);
        }

        throw new FeatureException(FeatureId, response);
    }

    public FnInversionInfo SetGlobalFnInversion(int hostIndex, bool state) {
        var response = CallFunction(FuncSetGlobalFnInversion, (byte)(state ? 0x01 : 0x00), (byte)hostIndex);
        if (response.IsSuccess) {
            return new FnInversionInfo(response);
        }

        throw new FeatureException(FeatureId, response);
    }

    public readonly struct FnInversionInfo {
        public readonly int  HostIndex;
        public readonly bool InversionState;
        public readonly bool InversionDefaultState;
        public readonly bool IsFnLocked;

        internal FnInversionInfo(IHidPpReport report) {
            HostIndex             = report[0];
            InversionState        = report[1] == 0x01;
            InversionDefaultState = report[2] == 0x01;
            IsFnLocked            = report[3].IsBitSet(0);
        }
    }
}