using CsvHelper;
using LoadWsm.Map;
using NetTopologySuite.Geometries;
using StressData.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
                //csv.Configuration.RegisterClassMap<StressRecordMap>();
                //var records = csv.GetRecords<StressRecord>();
                
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

            //var stressrecord = new StressRecord()
            //{
            //    WsmId = "tst0000",
            //    ISO = "NO0000",
            //    Azimuth = 15,
            //    Location = new Point(new CoordinateZ(50, 50, -10)),
            //    Quality = StressData.Types.StressTypes.QualityType.E,
            //    Regime = StressData.Types.StressTypes.RegimeType.NF,
            //    Type = StressData.Types.StressTypes.RecordType.SWB
            //};
            //stressrecord.Location.SRID = 4236;

            //var stressrecord = records.First();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:5001/StressApi/");

                var jsonOptions = new JsonSerializerOptions();
                jsonOptions.Converters.Add(new JsonStringEnumConverter());
                jsonOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());

                int i = 0;
                foreach (var stressrecord in records)
                {
                    if (i % 100 == 0)
                    {
                        await Task.Delay(1000);
                    }
                    var dataString = JsonSerializer.Serialize(stressrecord, jsonOptions);
                    var httpContent = new StringContent(dataString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    var postTask = client.PostAsync("", httpContent);
                    i++;
                }

                //Console.WriteLine(responseTask.Result);
                //Console.WriteLine(await responseTask.Result.Content.ReadAsStringAsync());

                var responseTask = client.GetAsync("");

                Console.WriteLine(responseTask.Result);
                var fromapi = JsonSerializer.Deserialize<List<StressRecord>>(await responseTask.Result.Content.ReadAsStringAsync());

                Console.WriteLine(fromapi.Count);
            }
        }
    }
}
