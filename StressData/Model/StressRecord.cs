using NetTopologySuite.Geometries;
using static StressData.Types.StressTypes;

namespace StressData.Model
{
    public class StressRecord
    {
        public long Id { get; set; }
        public string WsmId { get; set; }
        public string ISO { get; set; }
        public Point Location { get; set; }
        public int Azimuth { get; set; }
        public QualityType Quality { get; set; }
        public RegimeType Regime { get; set; }
        public RecordType Type { get; set; }

        public override string ToString()
        {
            return base.ToString() + ": " + WsmId + ", " + Type + ", " + Regime + ", " + Location;
        }
    }
}
