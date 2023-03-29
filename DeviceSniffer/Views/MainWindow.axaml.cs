using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DeviceSniffer.Models;
using DeviceSniffer.ViewModels;
using HidPpSharp;
using HidPpSharp.Devices;

namespace DeviceSniffer.Views;

public partial class MainWindow : Window {
    private HidPpManager _manager = new HidPpManager();
    public MainWindow() {
        InitializeComponent();
    }

    private void DeviceComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (deviceComboBox.SelectedItem == null || this.DataContext == null)
            return;
        
        var model = (MainWindowViewModel)DataContext;

        try {
            var devInfo = (HidDeviceInfo)deviceComboBox.SelectedItem;
            if (!_manager.TryOpenDevice(devInfo.HidDevice, out var hidPpDevice)) {
                throw new IOException("Could not open device");
            }

            model.WorkingDevice = new WorkingDevice(hidPpDevice!);
        }
        catch (Exception ex) {
            var msgBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error", ex.Message, icon: MessageBox.Avalonia.Enums.Icon.Error);
            msgBox.ShowDialog(this);
            Console.WriteLine(ex);
        }
    }

    private void SendButton_OnClick(object? sender, RoutedEventArgs e) {
        if (funcIdTextBox.Text.Trim() == "" ||  this.DataContext == null || featureComboBox.SelectedItem == null)
            return;
        
        var model   = (MainWindowViewModel)DataContext;
        var dev     = model.WorkingDevice;
        var feature = (WorkingDevice.FeatureModelInfo)featureComboBox.SelectedItem;

        if (!int.TryParse(funcIdTextBox.Text.Trim(), out var funcId))
            return;

        var args    = new byte[0];
        if (argsTextBox.Text != null) {
            var argsStr = argsTextBox.Text.Replace(" ", "");
            args = new byte[argsStr.Length / 2];
            for (var i = 0; i < argsStr.Length; i += 2) {
                args[i / 2] = Convert.ToByte(argsStr.Substring(i, 2), 16);
            }
        }

        var devFeature = dev.CreateDevFeature(feature.FeatureId);
        WriteToLog($"calling function {funcId} on feature {feature.FullName} with arguments: {BitConverter.ToString(args).Replace("-", " ")}");
        var report = devFeature.CallFunction(funcId, args);
        if (report.IsSuccess) {
            WriteToLog($"response: {BitConverter.ToString(report.RawData).Replace("-", " ")}");
        } else {
            WriteToLog($"response: error - {report.Error}");
        }
        
        WriteToLog("----");
    }

    private void WriteToLog(string message) {
        textBoxData.Text += message + Environment.NewLine;
    }
}