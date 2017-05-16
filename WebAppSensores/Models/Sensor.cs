using System;
using System.Linq;

namespace WebAppSensores.Models
{
    public class Sensor
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string Type { get; set; }
        public float Value { get; set; }
        public DateTime TimeStamp { get; set; }
    }
    
}