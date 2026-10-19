namespace AromaDom.Models.Entities
{
    /// Единица измерения: кг, л, шт, м.
    public class MeasurementUnit
    {
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
    }
}
