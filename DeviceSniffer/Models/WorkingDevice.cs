using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Reactive;
using HidPpSharp;
using HidPpSharp.HidPp10;
using HidPpSharp.HidPp20;
using HidSharp;
using ReactiveUI;

namespace DeviceSniffer.Models;

public class WorkingDevice : ReactiveObject {
    public readonly struct FeatureModelInfo {
        public FeatureId FeatureId { get; }
        public string    Name      => FeatureId.ToString();
        public string    FullName  => $"({(ushort)FeatureId:X4}) {FeatureId}";

        public FeatureModelInfo(FeatureId featureId) {
            FeatureId = featureId;
        }
    }

    private HidPp10Registers _registers;
    private HidPp20Features  _features;

    public IHidPpDevice Device             { get; }
    public HidDevice    HidDevice          { get => Device.HidDevice; }
    public bool         IsHidPp10Supported { get; }
    public bool         IsHidPp20Supported { get; }
    public string       ProtocolVersion    { get; private set; }

    public IEnumerable<FeatureModelInfo> DeviceFeatures { get; private set; }


    public WorkingDevice(IHidPpDevice device) {
        Device             = device;
        IsHidPp10Supported = InitializeHid10();
        IsHidPp20Supported = InitializeHid20();
    }

    private bool InitializeHid10() {
        try {
            _registers = new HidPp10Registers(Device);
            return true;
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            return false;
        }
    }

    private bool InitializeHid20() {
        try {
            _features      = new HidPp20Features(Device);
            DeviceFeatures = from f in _features select new FeatureModelInfo(f);

            var rootFeature = _features.GetFeature<Root>();
            var version     = rootFeature.GetProtocolVersion();
            ProtocolVersion = $"HID++ {version.ProtocolNumber} Target 0x{version.TargetSoftware:X2}";
            
            return true;
        }
        catch (Exception ex) {
            ProtocolVersion = "Unknown";
            throw;
        }
    }

    public DevFeature CreateDevFeature(FeatureId featureId) => new DevFeature(_features, featureId);
}