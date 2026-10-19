using AromaDom.Models.Entities;

namespace AromaDom.Models.ViewModels
{
    /// ViewModel карточки материала (для главной формы).
    /// Содержит все поля, отображаемые на карточке по макету из задания.
    public class MaterialCardViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string MaterialTypeName { get; set; }
        public decimal LossPercentage { get; set; }
        public decimal Cost { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal MinStockQuantity { get; set; }
        public decimal QuantityPerPackage { get; set; }
        public string MeasurementUnitShortName { get; set; }
        public decimal RequiredQuantity { get; set; }
        public bool LowStock { get; set; }
    }
}
