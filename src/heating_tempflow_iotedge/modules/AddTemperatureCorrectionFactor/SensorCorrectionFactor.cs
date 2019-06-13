namespace AddTemperatureCorrectionFactor
{
    public class SensorCorrectionFactor
    {
        public string SensorId { get; set; }
        public string SensorDescription { get; set; }
        public float CorrectionFactor { get; set; }

        public static SensorCorrectionFactor DefaultFor(string sensorId)
        {
            return new SensorCorrectionFactor
            {
                SensorId = sensorId,
                    SensorDescription = "Unknown",
                    CorrectionFactor = 0.0f
            };
        }
    }
}