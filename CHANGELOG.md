# HidPpSharp Changelog

## HPS-008

__Release HidPpSharp 0.1__
* Added DJ report support
* Removed dependencies for BxEx libraries
* Created DeviceSniffer to test undocumented HID++ features and functions
  * Using Avalonia UI Framework

## HPS-007

* Moved to GitHub (https://github.com/BxNiom/HidPpSharp)
* HidPpManager accepts known devices and their implementations (use this to create a new HidPpDevice)

### HidPp10 (HID++ 1.0)

* RegisterId added
* Registers added:
    * x00 - Notification (Device and Receiver)
    * x01 - IndividualFeatures
    * x02 - ConnectionState
    * xB2 - DevicePairing
    * xB3 - DeviceActivity
    * xB5 - PairingInformation
    * xF1 - Firmware

## HPS-006

* HidPpDevice -> IHidPpDevice
* DeviceFeatures -> HidPp20Features
* HidPpDeviceInputReceiver
    * Inspired from HidInputReceiver
    * Parsing reports
    * TryRead returns IHidPpReport (RegisterReport for HID++ 1.0, FeatureReport for HID++ 2.0)
* Added HidPp10Registers
    * Used for HID++ 1.0 registers
    * RegisterId known registers

## HPS-005

### HidPpSharp

* Finished x1B04 feature
* Added features:
    * x1000 - BatteryStatus
    * x1C00 - PersistentRemappableAction
    * x1D4B - WirelessDeviceStatus
    * x2100 - VerticalScrolling
    * x2110 - SmartShift
    * x2111 - SmartShiftEnhanced
    * x2120 - HiResolutionScrolling
    * x2121 - HiResolutionWheel
    * x2150 - ThumbWheel
    * x2200 - MousePointer
    * x40A2 - FnInversionLegacy
    * x40A3 - FnInversion
    * x4521 - DisableKeys
    * x4522 - DisableKeysByUsage
    * x4530 - DualPlatform
    * x4531 - MultiPlatform
    * x4600 - Crown
    * x6100 - TouchpadRawXy
    * x6110 - TouchMouseRawPoints
    * x8060 - ReportRate
    * x8090 - ModeStatus
    * x8300 - AudioSideToneAdjustment
    * x8310 - AudioEqualizer

## HPS-004

* Added ControlIdUtils (required by 0x1B04 and 0x1C00 feature)
    * ControlId Enum
    * Lookup table to obtained control id codes and task ids

## HPS-003

* Added FsInfo (only works on linux)
    * Get device root, hub, config and interface via sysfs
* ByteUtils
    * Added SetBit
    * Changed Pack to accept strings
* Reorganize DeviceFeatures class
    * Features no longer working as extensions for DeviceFeatures
    * Get feature classes via ```GetFeature(FeatureId)``` or ```GetFeature<Feature>()``` in DeviceFeatures class
    * FeatureAttribute added
* Features added:
    * 0x0007 - DeviceFriendlyName
    * 0x1814 - ChangeHost
    * 0x1815 - HostInfo
    * 0x1982 - Backlight
    * 0x1990 - Illumination
    * 0x1B04 - SpecialKeysMseButtons

## HPS-002

* Complete rewritten HID++ library (now HidPpSharp)
* Features throw FeatureException if request fails
* Receiver reads data from device and raises callbacks

## HPS-001

* Added CHANGELOG.md
* Added HIDpp project

HIDpp:

* Based on HIDSharp
* Added Request class
* Added Response class
* HIDppDevice added
    * DeviceFeatures are extensions of HIDppDevice
    * Features added:
        * 0x0000 - IRoot
        * 0x0001 - IFeatureSet
        * 0x0003 - DeviceInformation
        * 0x0005 - DeviceTypeName

## HPS-000

* Init Commit
* Added logitech hid++ documents