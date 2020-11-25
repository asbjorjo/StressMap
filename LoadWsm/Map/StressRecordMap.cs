using CsvHelper.Configuration;
using StressData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadWsm.Map
{
    public class StressRecordMap : ClassMap<StressRecord> 
    {
        public StressRecordMap()
        {
            Map(m => m.WsmId).Name("ID");
            Map(m => m.ISO).Name("ISO");
            Map(m => m.Azimuth).Name("AZI");
            Map(m => m.Quality).Name("QUALITY");
            Map(m => m.Type).Name("TYPE");
            Map(m => m.Regime).Name("REGIME");
            Map(m => m.Location.Z).Name("DEPTH");
        }
    }
}
