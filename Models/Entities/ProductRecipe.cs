namespace AromaDom.Models.Entities
{
    /// Рецептура: связь «многие-ко-многим» между продуктом и материалом.
    /// quantity_per_unit — сколько материала нужно на 1 единицу продукции.
    public class ProductRecipe
    {
        public long ProductId { get; set; }
        public long MaterialId { get; set; }
        public decimal QuantityPerUnit { get; set; }
        public int MeasurementUnitId { get; set; }

        // Joined fields (для формы «Список продукции по материалу»)
        public string ProductName { get; set; }
        public string ProductArticle { get; set; }
        public string ProductTypeName { get; set; }
        public string MeasurementUnitShortName { get; set; }
    }
}
