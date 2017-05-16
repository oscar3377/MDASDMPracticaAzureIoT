using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//using BuildAzure.IoT.Adafruit.BME280;

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using BuildAzure.IoT.Adafruit.BME280;

// La plantilla de elemento Página en blanco está documentada en http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EnvioDatos
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer _timer;
        BME280Sensor _bme280;

        const float seaLevelPressure = 1001.1f;

        static DeviceClient deviceClient;
        static string iotHubUri = "getStarted-IoThub.azure-devices.net";
        static string deviceKey = "s5xyCqnN70PzoGSnxgzJj72SOyRLMoEFK8AEHQPC+0I=";
        static string deviceId = "raspwintfm";

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _bme280 = new BuildAzure.IoT.Adafruit.BME280.BME280Sensor();
            await _bme280.Initialize();

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Mqtt);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(60); //Para enviar al IoTHub 4 mensajes por minuto - 240 a la hora - 5.760 al día (el límite son 8.000)
            _timer.Tick += _timer_Tick;

            _timer.Start();
        }

        private void _timer_Tick(object sender, object e)
        {
            var temp = _bme280.ReadTemperature();
            var humidity = _bme280.ReadHumidity();
            var pressure = _bme280.ReadPressure();
            var altitude = _bme280.ReadAltitude(seaLevelPressure);

            if (temp.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
                Debug.WriteLine("Temp: {0} deg C", temp.Result);
                this.t_temp.Text = temp.Result.ToString();
                SendDeviceToCloudMessagesAsync(temp.Result, "Temp");
            }
            else
            {
                Debug.WriteLine("Temp: {0} deg C", temp.Status);
                this.t_temp.Text = temp.Status.ToString();
            }

            if (humidity.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
                Debug.WriteLine("Humidity: {0} %", humidity.Result);
                this.t_hum.Text = humidity.Result.ToString();
                SendDeviceToCloudMessagesAsync(humidity.Result, "Humidity");
            }
            else
            {
                Debug.WriteLine("Humidity: {0} %", humidity.Status);
                this.t_hum.Text = humidity.Status.ToString();
            }

            if (pressure.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
                Debug.WriteLine("Pressure: {0} Pa", pressure.Result);
                this.t_pres.Text = pressure.Result.ToString();
                SendDeviceToCloudMessagesAsync(pressure.Result, "Pressure");
            }
            else
            {
                Debug.WriteLine("Pressure: {0} Pa", pressure.Status);
                this.t_pres.Text = pressure.Status.ToString();
            }

            if (altitude.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
                Debug.WriteLine("Altitude: {0} m", altitude.Result);
                this.t_alt.Text = altitude.Result.ToString();
                SendDeviceToCloudMessagesAsync(altitude.Result, "Altitude");
            }
            else
            {
                Debug.WriteLine("Altitude: {0} m", altitude.Status);
                this.t_alt.Text = altitude.Status.ToString();
            }

        }

        private static async void SendDeviceToCloudMessagesAsync(float data, string type)
        {
            /*double avgWindSpeed = 10; // m/s
            Random rand = new Random();

            while (true)
            {
                double currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;
                */
            var telemetryDataPoint = new
            {
                deviceId = deviceId,
                type = type,
                value = data,
                timestamp = DateTime.Now
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                /*await Task.Delay(1000);
            }*/
        }
    }
}
