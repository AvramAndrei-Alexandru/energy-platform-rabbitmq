using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPlatformRabbitMQ
{
    public class Message
    {
        public DateTimeOffset TimeStamp { get; set; }
        public Guid DeviceId { get; set; }
        public decimal MeasurementValue { get; set; }
    }
}
