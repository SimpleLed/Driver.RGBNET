using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGB.NET.Core;
using SimpleLed;

namespace Driver.RGBnet
{
    class DeviceTypeConverter
    {
        public static string GetType(RGBDeviceType type)
        {
            switch (type)
            {
                case RGBDeviceType.Keyboard:
                    return DeviceTypes.Keyboard;
                case RGBDeviceType.Keypad:
                    return DeviceTypes.Keypad;
                case RGBDeviceType.Mouse:
                    return DeviceTypes.Mouse;
                case RGBDeviceType.Headset:
                    return DeviceTypes.Mouse;
                case RGBDeviceType.HeadsetStand:
                    return DeviceTypes.Mouse;
                case RGBDeviceType.Mousepad:
                    return DeviceTypes.Mouse;
                case RGBDeviceType.Cooler:
                    return DeviceTypes.Cooler;
                case RGBDeviceType.GraphicsCard:
                    return DeviceTypes.GPU;
                case RGBDeviceType.Mainboard:
                    return DeviceTypes.MotherBoard;
                case RGBDeviceType.DRAM:
                    return DeviceTypes.Memory;
                case RGBDeviceType.LedStripe:
                    return DeviceTypes.LedStrip;
                case RGBDeviceType.Fan:
                    return DeviceTypes.Fan;
                default:
                    return DeviceTypes.Other;
            }
        }
    }
}
