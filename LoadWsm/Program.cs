using CsvHelper;
using NetTopologySuite.Geometries;
using StressData.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static StressData.Types.StressTypes;

namespace LoadWsm
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var fileName = "C:\\Users\\asbjo\\Downloads\\wsm2016.csv";

            var records = new List<StressRecord>();

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var record = new StressRecord
                    {
                        WsmId = csv.GetField("ID"),
                        ISO = csv.GetField("ISO"),
                        Azimuth = csv.GetField<int>("AZI"),
                        Quality = csv.GetField<QualityType>("QUALITY"),
                        Regime = csv.GetField<RegimeType>("REGIME"),
                        Type = csv.GetField<RecordType>("TYPE"),
                        Location = new Point(
                            csv.GetField<double>("LON"),
                            csv.GetField<double>("LAT"),
                            csv.GetField<double>("DEPTH") * 1000
                            )
                    };

                    record.Location.SRID = 4326;

                    records.Add(record);
                }

                var recordEnum = records.GetEnumerator();

                recordEnum.MoveNext();
                Console.WriteLine(recordEnum.Current);
            }

            Console.WriteLine(records.Count);

            var handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = 5
            };

            using (var client = new HttpClient(handler))
            {
                //client.BaseAddress = new Uri("https://localhost:5001/StressRecord/");
                client.BaseAddress = new Uri("http://stressmap-dev.azurewebsites.net/");

                var jsonOptions = new JsonSerializerOptions();
                jsonOptions.Converters.Add(new JsonStringEnumConverter());
                jsonOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());

                var tasks = new List<Task>();
                var i = 0;
                foreach (var stressrecord in records)
                {
                    i++;
                    var dataString = JsonSerializer.Serialize(stressrecord, jsonOptions);
                    var httpContent = new StringContent(dataString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    tasks.Add(client.PostAsync("StressRecord", httpContent));

                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }
                }


                var responseTask = client.GetAsync("StressRecord");

                Console.WriteLine(responseTask.Result);
                var fromapi = JsonSerializer.Deserialize<List<StressRecord>>(await responseTask.Result.Content.ReadAsStringAsync());

                Console.WriteLine(fromapi.Count);
            }
        }
    }
}
