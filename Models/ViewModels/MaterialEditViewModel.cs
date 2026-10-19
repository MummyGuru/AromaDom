using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AromaDom.Models.Entities;

namespace AromaDom.Models.ViewModels
{
    /// ViewModel формы добавления/редактирования материала.
    /// Содержит сам материал + справочники для выпадающих списков.
    /// Атрибуты DataAnnotations — для клиентской и серверной валидации.
    public class MaterialEditViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Наименование материала обязательно для заполнения.")]
        [StringLength(200, ErrorMessage = "Наименование не может превышать 200 символов.")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Выберите тип материала из списка.")]
        [Display(Name = "Тип материала")]
        public int MaterialTypeId { get; set; }

        [Required(ErrorMessage = "Укажите цену единицы материала.")]
        [Range(0, double.MaxValue, ErrorMessage = "Цена единицы не может быть отрицательной.")]
        [Display(Name = "Цена единицы, ₽")]
        public decimal Cost { get; set; }

        [Required(ErrorMessage = "Выберите единицу измерения.")]
        [Display(Name = "Единица измерения")]
        public int MeasurementUnitId { get; set; }

        [Required(ErrorMessage = "Укажите количество в упаковке.")]
        [Range(double.Epsilon, double.MaxValue, ErrorMessage = "Количество в упаковке должно быть больше нуля.")]
        [Display(Name = "Количество в упаковке")]
        public decimal QuantityPerPackage { get; set; }

        [Required(ErrorMessage = "Укажите количество на складе.")]
        [Range(0, double.MaxValue, ErrorMessage = "Количество на складе не может быть отрицательным.")]
        [Display(Name = "Количество на складе")]
        public decimal StockQuantity { get; set; }

        [Required(ErrorMessage = "Укажите минимальное количество.")]
        [Range(0, double.MaxValue, ErrorMessage = "Минимальное количество не может быть отрицательным.")]
        [Display(Name = "Минимальное количество")]
        public decimal MinStockQuantity { get; set; }

        [StringLength(200, ErrorMessage = "Описание не может превышать 200 символов.")]
        [Display(Name = "Описание (необязательно)")]
        public string Description { get; set; }

        // Справочники для выпадающих списков
        public List<MaterialType> MaterialTypes { get; set; } = new List<MaterialType>();
        public List<MeasurementUnit> MeasurementUnits { get; set; } = new List<MeasurementUnit>();
    }
}
