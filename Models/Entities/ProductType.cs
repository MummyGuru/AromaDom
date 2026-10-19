namespace AromaDom.Models.Entities
{
    /// Тип продукции (справочник): Классическая свеча, Премиальная свеча, и т.д.
    /// type_coefficient — коэффициент, влияющий на расход сырья.
    public class ProductType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal TypeCoefficient { get; set; }
        public bool IsActive { get; set; }
    }
}
