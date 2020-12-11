using NetTopologySuite.Geometries;

namespace StressData.Model
{
    public class StressPlate
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Geometry Outline { get; set; }
    }
}
