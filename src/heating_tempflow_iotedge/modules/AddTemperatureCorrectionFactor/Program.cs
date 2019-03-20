namespace AddTemperatureCorrectionFactor
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;

    class Program
    {

        private static readonly ConcurrentDictionary<string, SensorCorrectionFactor> _correctionFactors = new ConcurrentDictionary<string, SensorCorrectionFactor>();
        private static TwinCollection _twinProperties = new TwinCollection();

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
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

            _twinProperties["test"] = new SensorCorrectionFactor
            {
                SensorId = "test",
                SensorDescription = "test description",
                CorrectionFactor = 7.4f
            };

            // Register callback to be called when the Desired Properties for this module are updated.
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdated, ioTHubModuleClient);
            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", ApplyCorrectionFactor, ioTHubModuleClient);

            var twin = await ioTHubModuleClient.GetTwinAsync();

            Console.WriteLine(JsonConvert.SerializeObject(twin.Properties));

            await UpdateReportedProperties(ioTHubModuleClient);
        }

        private static Task<MessageResponse> ApplyCorrectionFactor(Message message, object userContext)
        {
            return Task.FromResult(MessageResponse.Completed);
        }

        private static async Task OnDesiredPropertiesUpdated(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("Updating properties ...");

            _twinProperties = desiredProperties;

            foreach (var property in desiredProperties)
            {
                if (property is SensorCorrectionFactor f)
                {
                    _correctionFactors.AddOrUpdate(f.SensorId, f, (key, _) => f);
                    Console.WriteLine($"SensorCorrectionFactor for {f.SensorId} updated!");
                }
            }

            _twinProperties["LastUpdated"] = DateTime.Now;

            await UpdateReportedProperties(userContext as ModuleClient);

            Console.WriteLine("Properties updated!");
        }

        private static async Task UpdateReportedProperties(ModuleClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            // A new collection is created where the readonly properties (starting with $) are
            // filtered out of it.  If these properties are in the collection, the properties
            // will not be updated and no error will be reported.
            var reportedProperties = new TwinCollection();

            foreach (KeyValuePair<string, dynamic> kvp in _twinProperties)
            {
                if (kvp.Key.StartsWith("$"))
                {
                    continue;
                }

                reportedProperties[kvp.Key] = kvp.Value;
            }

            Console.WriteLine("Reporting properties are these:");
            Console.WriteLine(JsonConvert.SerializeObject(reportedProperties));

            await client.UpdateReportedPropertiesAsync(reportedProperties);
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        // static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        // {
        //     int counterValue = Interlocked.Increment(ref counter);

        //     var moduleClient = userContext as ModuleClient;
        //     if (moduleClient == null)
        //     {
        //         throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
        //     }

        //     byte[] messageBytes = message.GetBytes();
        //     string messageString = Encoding.UTF8.GetString(messageBytes);
        //     Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

        //     if (!string.IsNullOrEmpty(messageString))
        //     {
        //         var pipeMessage = new Message(messageBytes);
        //         foreach (var prop in message.Properties)
        //         {
        //             pipeMessage.Properties.Add(prop.Key, prop.Value);
        //         }
        //         await moduleClient.SendEventAsync("output1", pipeMessage);
        //         Console.WriteLine("Received message sent");
        //     }
        //     return MessageResponse.Completed;
        // }
    }

    public class SensorCorrectionFactor
    {
        public string SensorId { get; set; }
        public string SensorDescription { get; set; }
        public float CorrectionFactor { get; set; }
    }
}