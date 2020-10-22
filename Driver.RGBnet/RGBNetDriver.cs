using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RGB.NET.Core;
using SimpleLed;

namespace Driver.RGBnet
{
    public class RGBNetDriver : ISimpleLed
    {
        public TimerUpdateTrigger UpdateTrigger { get; private set; }

        private const string DEVICEPROVIDER_DIRECTORY = "DeviceProvider";

        public RGBSurface surface;


        public event Events.DeviceChangeEventHandler DeviceAdded;
        public event Events.DeviceChangeEventHandler DeviceRemoved;

        public void Configure(DriverDetails driverDetails)
        {
            surface = RGBSurface.Instance;

            string deviceProvierDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty, DEVICEPROVIDER_DIRECTORY);
            if (!Directory.Exists(deviceProvierDir)) return;

            foreach (string file in Directory.GetFiles(deviceProvierDir, "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(file);
                    foreach (Type loaderType in assembly.GetTypes().Where(t =>
                        !t.IsAbstract && !t.IsInterface && t.IsClass
                        && typeof(IRGBDeviceProviderLoader).IsAssignableFrom(t)))
                    {
                        if (Activator.CreateInstance(loaderType) is IRGBDeviceProviderLoader deviceProviderLoader)
                        {

                            if (deviceProviderLoader.RequiresInitialization) continue;
                            RGBSurface.Instance.LoadDevices(deviceProviderLoader, RGBDeviceType.All);


                        }
                    }
                }
                catch
                {

                }
            }

            surface.AlignDevices();

            foreach (IRGBDevice device in surface.Devices)
            {
                device.UpdateMode = DeviceUpdateMode.Sync | DeviceUpdateMode.SyncBack;
                ControlDevice slsDevice = new ControlDevice();
                slsDevice.Name = device.DeviceInfo.Manufacturer + "-" + device.DeviceInfo.DeviceName;
                slsDevice.DeviceType = DeviceTypeConverter.GetType(device.DeviceInfo.DeviceType);
                slsDevice.Driver = this;
                slsDevice.Has2DSupport = false;

                List<ControlDevice.LedUnit> deviceLeds = new List<ControlDevice.LedUnit>();

                foreach (Led led in device)
                {
                    ControlDevice.LedUnit newLed = new ControlDevice.LedUnit();
                    newLed.Data = new ControlDevice.LEDData();
                    newLed.Data.LEDNumber = (int) led.Id;
                }
            }

            UpdateTrigger = new TimerUpdateTrigger { UpdateFrequency = 1.0 / 30 };
            surface.RegisterUpdateTrigger(UpdateTrigger);
            UpdateTrigger.Start();
        }

        public void Dispose()
        {
            UpdateTrigger.Stop();
            surface.Dispose();
        }

        public T GetConfig<T>() where T : SLSConfigData
        {
            throw new NotImplementedException();
        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPull = false,
                SupportsPush = true,
                IsSource = false,
                SupportsCustomConfig = true,
                Id = Guid.Parse("09fc46d2-6880-487f-9a8e-16b907b20eb1"),
                Author = "Fanman03",
                Blurb = "Compatibility layer that allows RGB.NET plugins to be used in SLS-based apps.",
                CurrentVersion = new ReleaseNumber(1, 0, 0, 1),
                GitHubLink = "https://github.com/SimpleLed/Driver.RGBNet",
                IsPublicRelease = false,
            };
        }

        public void InterestedUSBChange(int VID, int PID, bool connected)
        {
            throw new NotImplementedException();
        }

        public string Name()
        {
            return "RGB.NET";
        }

        public void Pull(ControlDevice controlDevice)
        {
            foreach (ControlDevice.LedUnit slsLED in controlDevice.LEDs)
            {
                Led rgbNetLed = RGBSurface.Instance.Leds.First(led => led.Id == (LedId)slsLED.Data.LEDNumber);
                slsLED.Color = new LEDColor((int) rgbNetLed.Color.R * 255, (int)rgbNetLed.Color.G * 255, (int)rgbNetLed.Color.B * 255);
            }
        }

        public void Push(ControlDevice controlDevice)
        {
            foreach (ControlDevice.LedUnit slsLED in controlDevice.LEDs)
            {
                Led rgbNetLed = RGBSurface.Instance.Leds.First(led => led.Id == (LedId)slsLED.Data.LEDNumber);
                double r = slsLED.Color.Red / 255;
                double g = slsLED.Color.Green / 255;
                double b = slsLED.Color.Blue / 255;
                rgbNetLed.Color = new Color(r,g,b);
            }
        }

        public void PutConfig<T>(T config) where T : SLSConfigData
        {
            throw new NotImplementedException();
        }
    }
}
