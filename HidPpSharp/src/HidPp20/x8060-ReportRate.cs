using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// Feature to change the device report rate
/// </summary>
[Feature(FeatureId.ReportRate)]
public class ReportRate : AbstractFeature {
    public const int FuncGetReportRateList = 0x00;
    public const int FuncGetReportRate     = 0x01;
    public const int FuncSetReportRate     = 0x02;

    public ReportRate(HidPp20Features features) : base(features, FeatureId.ReportRate) { }

    /// <summary>
    /// Retrieve the various report rates supported by the device
    /// Standard report rates are 1, 2, 4 and 8ms
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public RateList GetReportRateList() {
        var response = CallFunction(FuncGetReportRateList);
        return response.IsSuccess
            ? new RateList { RateValue = response[0] }
            : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns the current report rate in ms
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public int GetReportRate() {
        var response = CallFunction(FuncGetReportRate);
        return response.IsSuccess ? response[0] : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// This function can be called only in host mode
    /// </summary>
    /// <param name="ms">The new report rate in ms (0-8)</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="FeatureException"></exception>
    public void SetReportRate(int ms) {
        if (ms is < 0 or > 8) {
            throw new ArgumentOutOfRangeException(nameof(ms), "must be between 0 and 8");
        }

        var response = CallFunction(FuncSetReportRate, (byte)ms);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    public struct RateList {
        public byte RateValue;

        public bool Support1ms {
            get => RateValue.IsBitSet(0);
        }

        public bool Support2ms {
            get => RateValue.IsBitSet(1);
        }

        public bool Support3ms {
            get => RateValue.IsBitSet(2);
        }

        public bool Support4ms {
            get => RateValue.IsBitSet(3);
        }

        public bool Support5ms {
            get => RateValue.IsBitSet(4);
        }

        public bool Support6ms {
            get => RateValue.IsBitSet(5);
        }

        public bool Support7ms {
            get => RateValue.IsBitSet(6);
        }

        public bool Support8ms {
            get => RateValue.IsBitSet(7);
        }
    }
}