namespace kustoreport.Data
{
    using System;
    public class TelemetryData
    {
        public string site { get; set; }
        public DateTime timestamp { get; set; }
        public int revision { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
        public double methane { get; set; }
    }
}