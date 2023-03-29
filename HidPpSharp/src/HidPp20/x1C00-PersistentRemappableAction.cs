using BxEx;
using HidPpSharp.HidPp20.Attributes;

namespace HidPpSharp.HidPp20;

/// <summary>
/// This feature describes user actions available on a device and allows them to be mapped to a behaviour that will be
/// persistent in the device. The defined behaviour can be the sending of a single HID report or an internal function
/// (e.g. show battery status).
/// </summary>
[Feature(FeatureId.PersistentRemappableAction)]
public class PersistentRemappableAction : AbstractFeature {
    public enum ActionId {
        /// <summary>
        /// send keyboard/keypad report
        /// </summary>
        KeyboardReport = 0x01,

        /// <summary>
        /// send mouse button report
        /// </summary>
        MouseButtonReport = 0x02,

        /// <summary>
        /// send X displacement
        /// </summary>
        XDisplacement = 0x03,

        /// <summary>
        /// send Y displacement
        /// </summary>
        YDisplacement = 0x04,

        /// <summary>
        /// send vertical roller
        /// </summary>
        VerticalRoller = 0x05,

        /// <summary>
        /// send vertical wheel displacement
        /// </summary>
        WheelDisplacement = 0x05,

        /// <summary>
        /// send horizontal roller displacement
        /// </summary>
        HorizontalRoller = 0x06,

        /// <summary>
        /// send horizontal AC pan displacement
        /// </summary>
        ACPanDisplacement = 0x06,

        /// <summary>
        /// send consumer control report
        /// </summary>
        ConsumerControlReport = 0x07,

        /// <summary>
        /// execute internal function (use value parameter as function index)
        /// </summary>
        ExecuteInternalFunction = 0x08,

        /// <summary>
        /// send power key report
        /// </summary>
        PowerKeyReport = 0x09
    }

    public enum FeatureFlags : ushort {
        /// <summary>
        /// capable of sending keyboard keys
        /// </summary>
        KeyboardReport = 0x0001,

        /// <summary>
        /// capable of sending mouse buttons
        /// </summary>
        MouseButtons = 0x0002,

        /// <summary>
        /// capable of sending mouse X displacement
        /// </summary>
        MouseXDisplacement = 0x0004,

        /// <summary>
        /// capable of sending mouse Y displacement
        /// </summary>
        MouseYDisplacement = 0x0008,

        /// <summary>
        /// capable of sending vertical roller increments
        /// </summary>
        VerticalRoller = 0x0010,

        /// <summary>
        /// capable of sending horizontal roller increments (AC PAN)
        /// </summary>
        HorizontalRoller = 0x0020,

        /// <summary>
        /// capable of sending consumer control codes
        /// </summary>
        ConsumerControl = 0x0040,

        /// <summary>
        /// capable of executing internal functions
        /// </summary>
        ExecuteInternalFunctions = 0x0080,

        /// <summary>
        /// capable of sending power keys
        /// </summary>
        PowerKey = 0x0100
    }

    [Flags]
    public enum ModifierFlags : byte {
        LeftCtrl   = 0x01,
        LeftShift  = 0x02,
        LeftAlt    = 0x04,
        LeftSuper  = 0x08,
        RightCtrl  = 0x10,
        RightShift = 0x20,
        RightAlt   = 0x40,
        RightSuper = 0x80
    }

    public const int FuncGetFeatureInfo         = 0x00;
    public const int FuncGetCount               = 0x01;
    public const int FuncGetCidInfo             = 0x02;
    public const int FuncGetPersistentAction    = 0x03;
    public const int FuncSetPersistentAction    = 0x04;
    public const int FuncResetPersistentAction  = 0x05;
    public const int FuncResetToFactorySettings = 0x06;

    public PersistentRemappableAction(HidPp20Features features) :
        base(features, FeatureId.PersistentRemappableAction) { }

    public FeatureFlags GetFeatureInfo() {
        var response = CallFunction(FuncGetFeatureInfo);
        return response.IsSuccess
            ? (FeatureFlags)response.ReadUInt16(0)
            : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns the number of items in the control ID table. These are sources (user stimuli) like controls (hot keys)
    /// and extra native functions (e.g. a given gesture) that a specific action can be mapped to.
    /// Note: The number of control IDs per Host in the scope of multi host devices.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public (int count, int numberOfHosts) GetCount() {
        var response = CallFunction(FuncGetCount);
        return response.IsSuccess
            ? new ValueTuple<int, int>(response[0], response[1])
            : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// Returns a row from the control ID table. This table information describes capabilities and desired software
    /// handling for physical controls in the device.All control IDs will be the exact same per host.
    /// </summary>
    /// <param name="index">The zero based row index to retrieve.</param>
    /// <param name="hostIndex">The host number where the control ID is remmapped (0 based index). When setting hostIndex
    /// to 255 (0xFF), the device shall select the current host.</param>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public int GetCidInfo(int index, int hostIndex) {
        var response = CallFunction(FuncGetCidInfo, (byte)index, (byte)hostIndex);
        return response.IsSuccess ? response.ReadUInt16(0) : throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// For a given control ID and hostIndex supported by the device, returns the corresponding actionId, hidUsage and
    /// modifierMask from the non-volatile memory in the device.
    /// </summary>
    /// <param name="cid">Representing the control ID of the control being requested (physical button or user action).</param>
    /// <param name="hostIndex">Representing the host number where the control ID is remmapped (0 based index). When
    /// setting hostIndex to 255 (0xFF), the device shall select the current host.</param>
    /// <returns></returns>
    /// <exception cref="FeatureException"></exception>
    public PersistentAction GetPersistentAction(int cid, int hostIndex) {
        var response = CallFunction(FuncGetPersistentAction, ByteUtils.Pack((ushort)cid, (byte)hostIndex));
        if (response.IsSuccess) {
            return new PersistentAction {
                CId        = response.ReadUInt16(0),
                HostIndex  = response[2],
                ActionId   = (ActionId)response[3],
                Value      = response.ReadUInt16(4),
                Modifiers  = (ModifierFlags)response[6],
                IsRemapped = response[7].IsBitSet(0)
            };
        }

        throw new FeatureException(FeatureId, response);
    }

    /// <summary>
    /// For a given control ID and hostIndex supported by the device, sets the corresponding actionId,value and
    /// modifierMask to the non-volatile memory in the device.
    /// </summary>
    /// <param name="action"></param>
    /// <exception cref="FeatureException"></exception>
    public void SetPersistentAction(PersistentAction action) {
        var data = ByteUtils.Pack((ushort)action.CId, (byte)action.HostIndex, (byte)action.ActionId,
            (ushort)action.Value, (byte)action.Modifiers);

        var response = CallFunction(FuncSetPersistentAction, data);
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// For a given control ID and hostIndex supported by the device, resets the persistent action to the default factory settings on the device.
    /// </summary>
    /// <param name="cid">Representing the control ID of the control being addressed (physical button or user action).</param>
    /// <param name="hostIndex">Representing the host number where the control ID is remmapped (0 based index). When
    /// setting hostIndex to 255 (0xFF), the device shall select the current host.</param>
    /// <exception cref="FeatureException"></exception>
    public void ResetPersistentAction(int cid, int hostIndex) {
        var response = CallFunction(FuncResetPersistentAction, ByteUtils.Pack((ushort)cid, (byte)hostIndex));
        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    /// <summary>
    /// For a given hostIdMask supported by the device, resets the persistent action to the default factory settings on the device for all control IDs.
    /// </summary>
    /// <param name="host1"></param>
    /// <param name="host2"></param>
    /// <param name="host3"></param>
    /// <exception cref="FeatureException"></exception>
    public void ResetToFactorySettings(bool host1, bool host2, bool host3) {
        var response = CallFunction(FuncResetToFactorySettings,
            (byte)((host1 ? 0x01 : 0x00) | (host2 ? 0x02 : 0x00) | (host3 ? 0x04 : 0x00)));

        if (!response.IsSuccess) {
            throw new FeatureException(FeatureId, response);
        }
    }

    public struct PersistentAction {
        /// <summary>
        /// Representing the control ID of the control being addressed (physical button or user action).
        /// </summary>
        public int CId;

        /// <summary>
        /// Representing the host number where the control ID is remmapped (0 based index). When setting hostIndex to
        /// 255 (0xFF), the device shall select the current host.
        /// </summary>
        public int HostIndex;

        /// <summary>
        /// Representing the action that has to be performed by the device.
        /// </summary>
        public ActionId ActionId;

        /// <summary>
        /// Standard HID code sent when control ID is triggered by the user. It is an unsigned HID usage code for
        /// keyboard and consumer control keys. A HID usage code or a bit-map (TBD) for mouse buttons. A signed value
        /// for all displacements. An index to the internal function within the device(e.g. show battery status).
        /// </summary>
        public int Value;

        /// <summary>
        /// This is the HID usage value for standard modifier keys like Win, Left Shift,Right Shift on Windows,
        /// corresponding to the hid usage page above.
        /// </summary>
        public ModifierFlags Modifiers;

        /// <summary>
        /// If true control is mapped to default behaviour, otherwise false 
        /// </summary>
        public bool IsRemapped;
    }
}