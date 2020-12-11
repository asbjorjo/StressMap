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
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
                
            const string fileName = "C:\\Users\\asbjo\\Downloads\\wsm2016.csv";
            const string ApiUri = "http://stressapi-dev.azurewebsites.net/";
            const int maxPostRequests = 1000;

            await LoadWsmCsv(fileName, ApiUri, maxPostRequests);
        }

        private static async Task LoadWsmCsv(string fileName, string apiUri, int maxPostRequests)
        {
            var handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = 5
            };
            List<StressRecord> records = ReadWsmCsv(fileName);
            Console.WriteLine(records.Count);

            await SendWsmData(apiUri, maxPostRequests, records, handler);

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(apiUri);

                var responseTask = client.GetAsync("StressRecord");

                Console.WriteLine(responseTask.Result);
                var fromapi = JsonSerializer.Deserialize<List<StressRecord>>(await responseTask.Result.Content.ReadAsStringAsync());

                Console.WriteLine(fromapi.Count);
            }
        }

        private static async Task SendWsmData(string ApiUri, int maxPostRequests, List<StressRecord> records, HttpClientHandler handler)
        {
            using (var client = new HttpClient(handler))
            {
                //client.BaseAddress = new Uri(ApiUri);
                client.BaseAddress = new Uri(ApiUri);

                var jsonOptions = new JsonSerializerOptions();
                jsonOptions.Converters.Add(new JsonStringEnumConverter());
                jsonOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());

                var tasks = new List<Task>();
                var i = 0;
                foreach (var stressrecord in records)
                {
                    var dataString = JsonSerializer.Serialize(stressrecord, jsonOptions);
                    var httpContent = new StringContent(dataString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    tasks.Add(client.PostAsync("StressRecord", httpContent));
                    i++;

                    if (i % maxPostRequests == 0)
                    {
                        Console.WriteLine(i);
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }
                }
            }
        }

        private static List<StressRecord> ReadWsmCsv(string fileName)
        {
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

                //var recordEnum = records.GetEnumerator();

                //recordEnum.MoveNext();
                //Console.WriteLine(recordEnum.Current);
            }

            return records;
        }
    }
}
