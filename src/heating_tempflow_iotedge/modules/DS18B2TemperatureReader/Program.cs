namespace DS18B2TemperatureReader
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Starting ds18b2temperaturereader at {DateTime.Now}");

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();

            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();

            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();

            while (!cts.IsCancellationRequested)
            {
                var allSensors = Directory.GetDirectories("/w1devices", "28*");

                var sensorReadTasks = new List<Task<SensorData>>();

                foreach (var sensor in allSensors)
                {
                    sensorReadTasks.Add(ReadSensorDataAsync(sensor, cts.Token));
                }

                await Task.WhenAll(sensorReadTasks);

                await SendDataToIotHubAsync(sensorReadTasks.Select(t => t.Result), ioTHubModuleClient);
                
                var delay = TimeSpan.FromSeconds(282);
                await Task.Delay( (int)delay.TotalMilliseconds, cts.Token);
            }

            await ioTHubModuleClient.CloseAsync();
            await WhenCancelled(cts.Token);

            Console.WriteLine("ds18b2temperaturereader module stopped.");
        }

        private static async Task<SensorData> ReadSensorDataAsync(string sensor, CancellationToken cts)
        {
            var sensorId = Path.GetFileName(sensor);

            var sensorDataFile = $"{sensor}/w1_slave";

            var content = await File.ReadAllTextAsync(sensorDataFile, cts);

            var temperature = GetTemperatureFromContent(content);

            return new SensorData
            {
                SensorId = sensorId,
                MeasurementDateTime = DateTime.Now,
                Temperature = temperature
            };

            double GetTemperatureFromContent(string sensorData)
            {
                int index = content.LastIndexOf(" t=");
                return Convert.ToDouble(content.Substring(index + 3)) / 1000;
            }
        }
        
        private static async Task SendDataToIotHubAsync(IEnumerable<SensorData> sensorData, ModuleClient moduleClient)
        {
            List<Message> messages = new List<Message>();

            foreach (var measurement in sensorData)
            {
                messages.Add(new Message(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(measurement))));
            }

            Console.WriteLine($"Sending {messages.Count} messages to temperature_output endpoint");
            await moduleClient.SendEventBatchAsync("temperature_output", messages);
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>) s).SetResult(true), tcs);
            return tcs.Task;
        }
    }
}