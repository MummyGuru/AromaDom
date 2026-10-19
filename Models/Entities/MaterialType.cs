namespace AromaDom.Models.Entities
{
    /// Тип материала (справочник): Соевый воск, Пчелиный воск, и т.д.
    /// loss_percentage — процент потери сырья при производстве.
    public class MaterialType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal LossPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}
