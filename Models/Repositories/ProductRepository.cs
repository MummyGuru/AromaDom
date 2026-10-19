using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AromaDom.Models.Entities;

namespace AromaDom.Models.Repositories
{
    /// Репозиторий продукции: список продукции по конкретному материалу,
    /// а также метод расчёта количества продукции из сырья.
    public class ProductRepository
    {
        /// Возвращает список продукции, в рецептуре которой используется указанный материал.
        /// Форма «Список продукции по материалу».
        public List<ProductRecipe> GetProductsForMaterial(long materialId)
        {
            var result = new List<ProductRecipe>();

            const string sql = @"
                SELECT
                    pr.product_id,
                    pr.material_id,
                    pr.quantity_per_unit,
                    pr.measurement_unit_id,
                    p.article_code,
                    p.name               AS product_name,
                    pt.name               AS product_type_name,
                    mu.short_name         AS unit_short_name
                FROM product_recipe pr
                JOIN product p        ON p.id = pr.product_id
                JOIN product_type pt  ON pt.id = p.product_type_id
                JOIN measurement_unit mu ON mu.id = pr.measurement_unit_id
                WHERE pr.material_id = @materialId
                ORDER BY p.name";

            using (var conn = DbConnection.CreateConnection())
            using (var reader = DbConnection.ExecuteReader(conn, sql,
                new SqlParameter("@materialId", materialId)))
            {
                while (reader.Read())
                {
                    result.Add(new ProductRecipe
                    {
                        ProductId = (long)reader["product_id"],
                        MaterialId = (long)reader["material_id"],
                        QuantityPerUnit = (decimal)reader["quantity_per_unit"],
                        MeasurementUnitId = (int)reader["measurement_unit_id"],
                        ProductArticle = (string)reader["article_code"],
                        ProductName = (string)reader["product_name"],
                        ProductTypeName = (string)reader["product_type_name"],
                        MeasurementUnitShortName = (string)reader["unit_short_name"]
                    });
                }
            }
            return result;
        }

        /// Метод расчёта количества продукции из сырья.
        public int CalculateProductQuantity(int productTypeId, int materialTypeId,
            int rawQuantity, double parameter1, double parameter2)
        {
            // Жёсткая валидация 
            if (productTypeId <= 0 || materialTypeId <= 0 || rawQuantity <= 0
                || parameter1 <= 0 || parameter2 <= 0)
            {
                return -1;
            }

            // Получаем коэффициент типа продукции и процент потерь материала.
            const string sql = @"
                SELECT
                    (SELECT type_coefficient FROM product_type  WHERE id = @ptId) AS coef,
                    (SELECT loss_percentage   FROM material_type WHERE id = @mtId) AS loss";

            using (var conn = DbConnection.CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ptId", productTypeId);
                    cmd.Parameters.AddWithValue("@mtId", materialTypeId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return -1;
                        var coefObj = reader["coef"];
                        var lossObj = reader["loss"];
                        if (coefObj == null || lossObj == null || coefObj == DBNull.Value || lossObj == DBNull.Value)
                            return -1;

                        decimal coef = (decimal)coefObj;
                        decimal loss = (decimal)lossObj;

                        // Сырьё на 1 ед. продукции
                        decimal perUnit = (decimal)(parameter1 * parameter2) * coef;
                        if (perUnit <= 0) return -1;

                        // Полезное сырьё с учётом потерь
                        decimal usable = (decimal)rawQuantity * (1m - loss);

                        // Целое количество продукции
                        return (int)Math.Floor(usable / perUnit);
                    }
                }
            }
        }
    }
}
