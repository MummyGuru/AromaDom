namespace AromaDom.Models.Entities
{
    /// Материал — основная сущность подсистемы учёта материалов.
    /// Хранит складские остатки, цену и привязку к типу + единице измерения.
    public class Material
    {
        public long Id { get; set; }
        public int MaterialTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ArticleCode { get; set; }
        public decimal? QuantityPerPackage { get; set; }
        public int MeasurementUnitId { get; set; }
        public decimal Cost { get; set; }              // цена единицы материала
        public decimal StockQuantity { get; set; }     // на складе
        public decimal MinStockQuantity { get; set; }  // минимально допустимое
        public bool IsActive { get; set; }

        // Joined fields (для отображения карточки)
        public string MaterialTypeName { get; set; }
        public string MeasurementUnitShortName { get; set; }
        public decimal LossPercentage { get; set; }
        public decimal RequiredQuantity { get; set; }  // SUM по рецептам
        public bool LowStock { get { return StockQuantity < MinStockQuantity; } }
    }
}
