using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

namespace EnergyPlatformRabbitMQ
{
    public class CSVMapper : CsvMapping<Data>
    {
        public CSVMapper()
                : base()
        {
            MapProperty(0, x => x.Measurement);
        }
    }
}
