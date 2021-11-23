using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TinyCsvParser;
using System.Windows.Forms;

namespace EnergyPlatformRabbitMQ
{
    class Program
    {
        static async Task Main(string[] args)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (DialogResult.OK == dialog.ShowDialog())
            {
                string path = dialog.FileName;
            }
            Console.WriteLine("Hello World!");
            //var deviceID = GetDeviceIdFromFile();
            var deviceID = GetDeviceIDFromArguments(args);
            if (deviceID == null || deviceID == Guid.Empty)
            {
                Console.WriteLine("Error, GUID not in a valid format");
            }
            var measurements = ReadMeasurementsFromFile();
            await SendMessageToQueue(deviceID.Value, 3000, 100, measurements);
        }

        private static Guid? GetDeviceIdFromFile()
        {
            try
            {
                using var sr = new StreamReader("../../../DeviceID.txt");
                var readGuid = Guid.Empty;
                _ = Guid.TryParse(sr.ReadToEnd().Trim(), out readGuid);
                return readGuid;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading from file ");
                return null;
            }
        }

        private static Guid? GetDeviceIDFromArguments(string[] args)
        {
            if(args == null || args.Length <= 0)
            {
                return null;
            }
            try
            {
                var readGuid = Guid.Empty;
                _ = Guid.TryParse(args[0], out readGuid);
                return readGuid;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing the Device ID ");
                return null;
            }
        }

        private static async Task SendMessageToQueue(Guid deviceId, int interval, int numberOfMessagesToSend, List<Data> measurements)
        {
            var count = 0;
            var factory = new ConnectionFactory() { Uri = new Uri("amqps://kirltfqj:4sABkRnTegiS9L6wLcmZgijy4sWlnFHX@goose.rmq2.cloudamqp.com/kirltfqj") };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "energy-platform-queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                while(count <= numberOfMessagesToSend || count > measurements.Count)
                {
                    var measurement = measurements[count];

                    var message = new Message
                    {
                        TimeStamp = DateTimeOffset.UtcNow.AddMinutes(10 * count),
                        DeviceId = deviceId,
                        MeasurementValue = measurement.Measurement
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                    Console.WriteLine("Sending message");

                    channel.BasicPublish(exchange: "",
                                     routingKey: "energy-platform-queue",
                                     basicProperties: null,
                                     body: body);

                    count++;
                    await Task.Delay(interval);
                }
            }
        }

        private static List<Data> ReadMeasurementsFromFile()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CSVMapper csvMapper = new CSVMapper();
            CsvParser<Data> csvParser = new CsvParser<Data>(csvParserOptions, csvMapper);
            var result = csvParser
                         .ReadFromFile(@"../../../sensor.csv", Encoding.ASCII)
                         .ToList();
            List<Data> returnData = new List<Data>();
            foreach (var details in result)
            {
                returnData.Add(details.Result);
            }
            return returnData;
        }
    }
}
