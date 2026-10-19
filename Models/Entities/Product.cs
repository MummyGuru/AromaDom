namespace AromaDom.Models.Entities
{
    /// Готовая продукция компании «Арома Дом».
    /// parameter1, parameter2 — параметры изделия.
    public class Product
    {
        public long Id { get; set; }
        public int ProductTypeId { get; set; }
        public string ArticleCode { get; set; }
        public string Name { get; set; }
        public decimal MinPartnerPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Parameter1 { get; set; }
        public decimal Parameter2 { get; set; }
        public bool IsActive { get; set; }

        // Joined fields
        public string ProductTypeName { get; set; }
        public decimal TypeCoefficient { get; set; }
    }
}
