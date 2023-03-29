using System.Text;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

[Feature(FeatureId.DeviceName)]
public class DeviceTypeName : AbstractFeature {
    public enum DeviceType {
        Keyboard               = 0x00,
        RemoteControl          = 0x01,
        Numpad                 = 0x02,
        Mouse                  = 0x03,
        Trackpad               = 0x04,
        Trackball              = 0x05,
        Presenter              = 0x06,
        Receiver               = 0x07,
        Headset                = 0x08,
        Webcam                 = 0x09,
        SteeringWheel          = 0x0A,
        Joystick               = 0x0B,
        Gamepad                = 0x0C,
        Dock                   = 0x0D,
        Speaker                = 0x0E,
        Microphone             = 0x0F,
        IlluminationLight      = 0x10,
        ProgrammableController = 0x11,
        CarSimPedals           = 0x12,
        Adapter                = 0x13,
        Unknown                = 0xFF
    }

    public const int FuncGetDeviceNameCount = 0x00;
    public const int FuncGetDeviceName      = 0x01;
    public const int FuncGetDeviceType      = 0x02;

    public DeviceTypeName(HidPp20Features features) : base(features, FeatureId.DeviceName) { }

    /// <summary>
    /// Returns the length of the device name in single byte characters. Note this does not include any terminating
    /// zeros.
    /// </summary>
    /// <returns></returns>
    public int GetDeviceNameCount() {
        Log.Debug("GetDeviceNameCount()");
        var response = CallFunction(FuncGetDeviceNameCount);
        return response.IsSuccess ? response[0] : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns a chunk of characters of the name, starting from the index specified in the requested.
    /// </summary>
    /// <param name="charIndex">Zero based index. The function will retrieve as many remaining characters as the payload
    /// will allow.</param>
    /// <returns></returns>
    public string GetDeviceName(int charIndex) {
        Log.DebugFormat("GetDeviceName({0})", charIndex);
        var response = CallFunction(FuncGetDeviceName, (byte)charIndex);

        return response.IsSuccess
            ? Encoding.ASCII.GetString(response.Data)
            : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns the device type best associated with the device.
    /// </summary>
    /// <returns></returns>
    public DeviceType GetDeviceType() {
        Log.Debug("GetDeviceType()");
        var response = CallFunction(FuncGetDeviceType);

        return response.IsSuccess ? (DeviceType)response[0] : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns the device name (using DeviceTypeName feature)
    /// </summary>
    /// <returns></returns>
    public string GetDeviceName() {
        var len = GetDeviceNameCount();
        var res = "";
        while (res.Length < len) {
            res += GetDeviceName(res.Length);
        }

        return res;
    }
}