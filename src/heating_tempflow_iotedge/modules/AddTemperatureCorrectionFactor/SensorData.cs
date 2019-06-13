namespace AddTemperatureCorrectionFactor
{
    using System;
    using Newtonsoft.Json;

    public class SensorData
    {
        [JsonProperty("sensorid")]
        public string SensorId { get; set; }

        [JsonProperty("timestamp")]
        public DateTime MeasurementDateTime { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }
    }
}