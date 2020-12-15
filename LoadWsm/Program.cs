using CsvHelper;
using NetTopologySuite.Geometries;
using StressData.Database.Constants;
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
        private const int maxConnections = 5;
        private const int maxPostRequests = 1000;

        static async Task Main(string[] args)
        {
            bool loadWsm = false;
            bool loadPlates = true;
            
            const string fileNameWsmCsv = "C:\\Users\\asbjo\\Downloads\\wsm2016.csv";
            const string fileNamePlates = "C:\\Users\\asbjo\\Downloads\\PB2002_plates.dig.txt";
            //const string ApiUri = "http://stressapi-dev.azurewebsites.net/";
            const string ApiUri = "https://localhost:5001/";
            
            if (loadWsm) 
            {
                await LoadWsmCsv(fileNameWsmCsv, ApiUri);
            }

            if (loadPlates)
            {
                await LoadPlatesAsync(fileNamePlates, ApiUri);                
            }
        }

        private static async Task LoadPlatesAsync(string fileNamePlates, string apiUri)
        {
            List<StressPlate> plates = new List<StressPlate>();
            var geometryFactory = new GeometryFactory(new PrecisionModel(), GeometryConstants.SRID);

            using (var reader = new StreamReader(fileNamePlates))
            {
                var name = reader.ReadLine();
                List<Coordinate> coordinates = new List<Coordinate>();

                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    if (line != "*** end of line segment ***")
                    {
                        var split = line.Split(',');
                        var format = CultureInfo.CreateSpecificCulture("en-US");
                        var lon = double.Parse(split[0], NumberStyles.Float, format);
                        var lat = double.Parse(split[1], NumberStyles.Float, format);
                        var coordinate = new Coordinate(lon , lat);
                        coordinates.Add(coordinate);
                    }
                    else
                    {
                        var plate = new StressPlate
                        {
                            Name = name,
                            Outline = new Polygon(new LinearRing(coordinates.ToArray()), geometryFactory)
                        };
                        
                        plate.Outline.SRID = GeometryConstants.SRID;

                        plates.Add(plate);
                        name = reader.ReadLine();
                        coordinates = new List<Coordinate>();
                    }
                }
            }

            var handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = maxConnections
            };

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(apiUri);

                var jsonOptions = new JsonSerializerOptions();
                jsonOptions.Converters.Add(new JsonStringEnumConverter());
                jsonOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
                var tasks = new List<Task>();
                var i = 0;
                
                foreach (var plate in plates)
                {
                    var dataString = JsonSerializer.Serialize(plate, jsonOptions);
                    var httpContent = new StringContent(dataString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    //var response = await client.PostAsync("StressPlate", httpContent);
                    //Console.WriteLine(response.StatusCode);
                    //Console.WriteLine(await response.Content.ReadAsStringAsync());

                    tasks.Add(client.PostAsync("StressPlate", httpContent));
                    i++;

                    if (i % maxPostRequests == 0)
                    {
                        Console.WriteLine(i);
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }
                }
                if (tasks.Count > 0)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }
        }

        private static async Task LoadWsmCsv(string fileName, string apiUri)
        {
            var handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = maxConnections
            };
            List<StressRecord> records = ReadWsmCsv(fileName);
            Console.WriteLine(records.Count);

            await SendWsmData(apiUri, maxPostRequests, records);

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(apiUri);

                var responseTask = client.GetAsync("StressRecord");

                Console.WriteLine(responseTask.Result);
                var fromapi = JsonSerializer.Deserialize<List<StressRecord>>(await responseTask.Result.Content.ReadAsStringAsync());

                Console.WriteLine(fromapi.Count);
            }
        }

        private static async Task SendWsmData(string ApiUri, int maxPostRequests, List<StressRecord> records)
        {
            var handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = maxConnections
            };
            
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
                if (tasks.Count > 0)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
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
