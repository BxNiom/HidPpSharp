using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.BatteryStatus)]
public class BatteryStatus : Feature {
    public enum BatteryLevel {
        Invalid          = 0x00,
        Low              = 0x18,
        Good             = 0x38,
        Full             = 0x58,
        Error            = 0x68,
        LevelUnknown     = 0x69,
        WrongBatteryType = 0x6B,
        NotCharging      = 0x6C,
        ChargingUnknown  = 0x6D,
        Charging         = 0x6E,
        ChargingSlow     = 0x6F,
        ChargingFast     = 0x70,
        ChargingComplete = 0x71,
        ChargingError    = 0x72
    }

    public enum ChargingStatus {
        Discharging                 = 0x00,
        Recharging                  = 0x01,
        ChargeInFinalStage          = 0x02,
        ChargeComplete              = 0x03,
        RechargingBelowOptimalSpeed = 0x04,
        InvalidBatteryType          = 0x05,
        ThermalError                = 0x06,
        OtherChargingError          = 0x07,
        Invalid                     = 0xFF
    }

    // TODO Events
    public const int FuncGetBatteryLevelStatus = 0x00;
    public const int FuncGetBatteryCapability  = 0x01;

    public BatteryStatus(HidPp20Features features) : base(features, FeatureId.BatteryStatus) { }

    public LevelStatus GetBatteryLevelStatus() {
        var response = CallFunction(FuncGetBatteryLevelStatus);
        if (response.IsSuccess) {
            return new LevelStatus {
                DischargeLevel     = response[0],
                DischargeNextLevel = response[1],
                Status             = response[2] > 0x07 ? ChargingStatus.Invalid : (ChargingStatus)response[2]
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public Capability GetBatteryCapability() {
        var response = CallFunction(FuncGetBatteryCapability);
        if (response.IsSuccess) {
            return new Capability {
                NumberOfLevels       = response[0],
                NominalBatteryLife   = response.ReadUInt16(2),
                BatteryCriticalLevel = response[4],
                IsOsdDisabled        = response[1].IsBitSet(0),
                IsMileageCalcEnabled = response[1].IsBitSet(1),
                IsRechargeable       = response[1].IsBitSet(2)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    public struct LevelStatus {
        public int            DischargeLevel;
        public int            DischargeNextLevel;
        public ChargingStatus Status;
    }

    public struct Capability {
        public int  NumberOfLevels;
        public int  NominalBatteryLife;
        public int  BatteryCriticalLevel;
        public bool IsOsdDisabled;
        public bool IsMileageCalcEnabled;
        public bool IsRechargeable;
    }
}