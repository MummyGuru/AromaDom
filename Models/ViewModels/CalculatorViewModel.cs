using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AromaDom.Models.Entities;

namespace AromaDom.Models.ViewModels
{
    /// ViewModel калькулятора расчёта количества продукции из сырья.
    /// Содержит входные параметры + результат + справочники для списков.
    public class CalculatorViewModel
    {
        [Required(ErrorMessage = "Выберите тип продукции.")]
        [Display(Name = "Тип продукции")]
        public int? ProductTypeId { get; set; }

        [Required(ErrorMessage = "Выберите тип материала.")]
        [Display(Name = "Тип материала")]
        public int? MaterialTypeId { get; set; }

        [Required(ErrorMessage = "Укажите количество сырья.")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество сырья должно быть целым положительным числом.")]
        [Display(Name = "Количество сырья")]
        public int? RawQuantity { get; set; }

        [Required(ErrorMessage = "Укажите параметр 1.")]
        [Range(double.Epsilon, double.MaxValue, ErrorMessage = "Параметр 1 должен быть положительным числом.")]
        [Display(Name = "Параметр 1")]
        public double? Parameter1 { get; set; }

        [Required(ErrorMessage = "Укажите параметр 2.")]
        [Range(double.Epsilon, double.MaxValue, ErrorMessage = "Параметр 2 должен быть положительным числом.")]
        [Display(Name = "Параметр 2")]
        public double? Parameter2 { get; set; }

        // Результат расчёта (null = расчёт ещё не выполнялся, -1 = ошибка)
        public int? Result { get; set; }
        public string ResultMessage { get; set; }

        // Детали расчёта (для отображения)
        public decimal? TypeCoefficient { get; set; }
        public decimal? LossPercentage { get; set; }
        public decimal? RequiredPerUnit { get; set; }
        public decimal? UsableMaterial { get; set; }

        // Справочники
        public List<ProductType> ProductTypes { get; set; } = new List<ProductType>();
        public List<MaterialType> MaterialTypes { get; set; } = new List<MaterialType>();
    }
}
