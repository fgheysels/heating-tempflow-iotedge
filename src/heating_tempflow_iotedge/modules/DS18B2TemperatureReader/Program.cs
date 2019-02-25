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
        static int counter;

        static async Task Main(string[] args)
        {
          //  await Init();

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
                    sensorReadTasks.Add(ReadSensorDataAsync($"{sensor}/w1_slave", cts.Token));
                }

                await Task.WhenAll(sensorReadTasks);

                await SendDataToIotHubAsync(sensorReadTasks.Select(t => t.Result), ioTHubModuleClient);

                await Task.Delay(30000);
            }

            await ioTHubModuleClient.CloseAsync();
            await WhenCancelled(cts.Token);
        }

        private static async Task<SensorData> ReadSensorDataAsync(string sensor, CancellationToken cts)
        {
            var content = await File.ReadAllTextAsync(sensor, cts);

            var temperature = GetTemperatureFromContent(content);

            return new SensorData
            {
                SensorId = sensor,
                    MeasurementDateTime = DateTime.Now,
                    Temperature = temperature
            };

            double GetTemperatureFromContent(string sensorData)
            {
                int index = content.LastIndexOf(" t=");
                return Convert.ToDouble(content.Substring(index + 3));
            }
        }

        private static async Task SendDataToIotHubAsync(IEnumerable<SensorData> sensorData, ModuleClient moduleClient)
        {
            List<Message> messages = new List<Message>();

            foreach (var measurement in sensorData)
            {
                messages.Add(new Message(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(measurement))));
            }

            await moduleClient.SendEventBatchAsync(messages);
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

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
                var pipeMessage = new Message(messageBytes);
                foreach (var prop in message.Properties)
                {
                    pipeMessage.Properties.Add(prop.Key, prop.Value);
                }
                await moduleClient.SendEventAsync("output1", pipeMessage);
                Console.WriteLine("Received message sent");
            }
            return MessageResponse.Completed;
        }
    }
}