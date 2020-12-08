using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using RGB.NET.Core;
using SimpleLed;

namespace Driver.RGBnet
{
    public class RGBNetDriver : ISimpleLedWithConfig
    {
        public TimerUpdateTrigger UpdateTrigger { get; private set; }

        private const string DEVICEPROVIDER_DIRECTORY = "DeviceProvider";

        public RGBSurface surface;


        public event Events.DeviceChangeEventHandler DeviceAdded;
        public event Events.DeviceChangeEventHandler DeviceRemoved;

        public static Assembly assembly = Assembly.GetExecutingAssembly();
        public static Stream imageStream = assembly.GetManifestResourceStream("Driver.RGBnet.argebee.png");


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
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
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
                slsDevice.ProductImage = (Bitmap) System.Drawing.Image.FromStream(imageStream);


                List<ControlDevice.LedUnit> deviceLeds = new List<ControlDevice.LedUnit>();

                int i = 0;

                foreach (Led led in device)
                {
                    RGBNetLed newLed = new RGBNetLed();
                    newLed.Data = new ControlDevice.LEDData();
                    newLed.Data.LEDNumber = i;
                    i++;
                    newLed.LedId = (int) led.Id;
                    newLed.DeviceName = led.Device.DeviceInfo.DeviceName;
                    deviceLeds.Add(newLed);
                }

                slsDevice.LEDs = deviceLeds.ToArray();

                DeviceAdded?.Invoke(slsDevice, new Events.DeviceChangeEventArgs(slsDevice));
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
            return null;
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
                Blurb = "Compatibility layer that allows RGB.NET providers to be used in SLS-based apps. RGB.NET library by Darth Affe.",
                CurrentVersion = new ReleaseNumber(1, 0, 0, 3),
                GitHubLink = "https://github.com/SimpleLed/Driver.RGBNet",
                IsPublicRelease = true,
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
            foreach (ControlDevice.LedUnit ledUnit in controlDevice.LEDs)
            {
                RGBNetLed slsLED = (RGBNetLed) ledUnit;
                Led rgbNetLed = RGBSurface.Instance.Leds.First(led => led.Id == (LedId)slsLED.LedId && led.Device.DeviceInfo.DeviceName == slsLED.DeviceName);
                slsLED.Color = new LEDColor((int) rgbNetLed.Color.R * 255, (int)rgbNetLed.Color.G * 255, (int)rgbNetLed.Color.B * 255);
            }
        }

        public void Push(ControlDevice controlDevice)
        {
            foreach (RGBNetLed slsLED in controlDevice.LEDs)
            {
                Led rgbNetLed = RGBSurface.Instance.Leds.First(led => led.Id == (LedId)slsLED.LedId && led.Device.DeviceInfo.DeviceName == slsLED.DeviceName);
                double r = slsLED.Color.Red / 255.0;
                double g = slsLED.Color.Green / 255.0;
                double b = slsLED.Color.Blue / 255.0;
                rgbNetLed.Color = new RGB.NET.Core.Color(r,g,b);
            }
        }

        public void PutConfig<T>(T config) where T : SLSConfigData
        {
            
        }

        public UserControl GetCustomConfig(ControlDevice controlDevice)
        {
            var config = new RGBNetConfig();

            return config;
        }

        public bool GetIsDirty()
        {
            return false;
        }

        public void SetIsDirty(bool val)
        {
            
        }

        public class RGBNetLed : ControlDevice.LedUnit
        {
            public int LedId { get; set; }

            public string DeviceName { get; set; }
        }
    }
}
