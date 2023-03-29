using System.Collections.Generic;
using System.Linq;
using DeviceSniffer.Models;
using HidSharp;
using ReactiveUI;

namespace DeviceSniffer.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    private WorkingDevice              _workingDevice;
    public  IEnumerable<HidDeviceInfo> HidDeviceInfos { get; }
    
    public WorkingDevice WorkingDevice {
        get => _workingDevice;
        set {
            _workingDevice = value;
            this.RaisePropertyChanged();
        }
    }
    
    public MainWindowViewModel() {
        HidDeviceInfos = from d in DeviceList.Local.GetHidDevices(vendorID: 0x046d)
                         select new HidDeviceInfo(d);
    }
}