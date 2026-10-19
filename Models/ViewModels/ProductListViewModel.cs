using System.Collections.Generic;
using AromaDom.Models.Entities;

namespace AromaDom.Models.ViewModels
{
    /// ViewModel формы «Список продукции по материалу».
    public class ProductListViewModel
    {
        public long MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string MaterialTypeName { get; set; }
        public string MeasurementUnitShortName { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal MinStockQuantity { get; set; }
        public decimal Cost { get; set; }

        public List<ProductRecipe> Products { get; set; } = new List<ProductRecipe>();

        public decimal TotalRequired
        {
            get
            {
                var sum = 0m;
                foreach (var p in Products) sum += p.QuantityPerUnit;
                return sum;
            }
        }
    }
}
