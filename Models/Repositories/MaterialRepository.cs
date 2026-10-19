using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AromaDom.Models.Entities;

namespace AromaDom.Models.Repositories
{
    /// Репозиторий материалов: список, получить по ID, добавить, обновить, удалить.
    public class MaterialRepository
    {
        /// Возвращает список всех активных материалов с требуемым количеством.
        public List<Material> GetAll(string search = null, int? typeId = null)
        {
            var result = new List<Material>();

            // SQL: список материалов + JOIN с типами и единицами + расчёт требуемого количества
            // через скалярную UDF dbo.CalculateRequiredQuantity (см. SQL-скрипт).
            const string sql = @"
                SELECT
                    m.id,
                    m.material_type_id,
                    m.name,
                    m.description,
                    m.article_code,
                    m.quantity_per_package,
                    m.measurement_unit_id,
                    m.cost,
                    m.stock_quantity,
                    m.min_stock_quantity,
                    m.is_active,
                    mt.name               AS material_type_name,
                    mt.loss_percentage    AS loss_percentage,
                    mu.short_name         AS unit_short_name,
                    dbo.CalculateRequiredQuantity(m.id) AS required_quantity
                FROM material m
                JOIN material_type   mt ON mt.id = m.material_type_id
                JOIN measurement_unit mu ON mu.id = m.measurement_unit_id
                WHERE m.is_active = 1";

            var where = new List<string> { "m.is_active = 1" };
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                where.Add("m.name LIKE @search");
                parameters.Add(new SqlParameter("@search", "%" + search + "%"));
            }
            if (typeId.HasValue)
            {
                where.Add("m.material_type_id = @typeId");
                parameters.Add(new SqlParameter("@typeId", typeId.Value));
            }

            string fullSql = sql.Replace("WHERE m.is_active = 1", "WHERE " + string.Join(" AND ", where))
                              + " ORDER BY m.name";

            using (var conn = DbConnection.CreateConnection())
            using (var reader = DbConnection.ExecuteReader(conn, fullSql, parameters.ToArray()))
            {
                while (reader.Read())
                {
                    result.Add(MapMaterial(reader));
                }
            }
            return result;
        }

        /// Возвращает материал по ID (для формы редактирования).
        public Material GetById(long id)
        {
            const string sql = @"
                SELECT TOP 1
                    m.id,
                    m.material_type_id,
                    m.name,
                    m.description,
                    m.article_code,
                    m.quantity_per_package,
                    m.measurement_unit_id,
                    m.cost,
                    m.stock_quantity,
                    m.min_stock_quantity,
                    m.is_active,
                    mt.name               AS material_type_name,
                    mt.loss_percentage    AS loss_percentage,
                    mu.short_name         AS unit_short_name,
                    dbo.CalculateRequiredQuantity(m.id) AS required_quantity
                FROM material m
                JOIN material_type   mt ON mt.id = m.material_type_id
                JOIN measurement_unit mu ON mu.id = m.measurement_unit_id
                WHERE m.id = @id";

            using (var conn = DbConnection.CreateConnection())
            using (var reader = DbConnection.ExecuteReader(conn, sql,
                new SqlParameter("@id", id)))
            {
                if (reader.Read())
                {
                    return MapMaterial(reader);
                }
            }
            return null;
        }

        /// Создаёт новый материал. Возвращает ID созданной записи или -1 при ошибке.
        public long Create(Material m)
        {
            const string sql = @"
                INSERT INTO material
                    (material_type_id, name, description, article_code,
                     quantity_per_package, measurement_unit_id, cost,
                     stock_quantity, min_stock_quantity, is_active)
                OUTPUT INSERTED.id
                VALUES
                    (@materialTypeId, @name, @description, @articleCode,
                     @quantityPerPackage, @measurementUnitId, @cost,
                     @stockQuantity, @minStockQuantity, 1)";

            var p = BuildParameters(m);
            var result = DbConnection.ExecuteScalar(sql, p);
            return result != null ? Convert.ToInt64(result) : -1;
        }

        /// Обновляет существующий материал. Возвращает количество затронутых строк.
        public int Update(Material m)
        {
            const string sql = @"
                UPDATE material SET
                    material_type_id      = @materialTypeId,
                    name                  = @name,
                    description           = @description,
                    article_code          = @articleCode,
                    quantity_per_package  = @quantityPerPackage,
                    measurement_unit_id   = @measurementUnitId,
                    cost                  = @cost,
                    stock_quantity        = @stockQuantity,
                    min_stock_quantity    = @minStockQuantity,
                    updated_at            = SYSDATETIME()
                WHERE id = @id";

            var parameters = new List<SqlParameter>(BuildParameters(m))
            {
                new SqlParameter("@id", m.Id)
            };
            return DbConnection.ExecuteNonQuery(sql, parameters.ToArray());
        }

        /// Удаляет материал. Возвращает количество затронутых строк.
        /// Возвращает 0 если есть связанные рецепты (нарушение ссылочной целостности).
        public int Delete(long id)
        {
            // Проверка наличия связанных рецептов
            const string checkSql = "SELECT COUNT(*) FROM product_recipe WHERE material_id = @id";
            var cnt = DbConnection.ExecuteScalar(checkSql, new SqlParameter("@id", id));
            if (cnt != null && Convert.ToInt32(cnt) > 0)
            {
                return 0;  // нельзя удалять — есть связи
            }

            const string sql = "UPDATE material SET is_active = 0, updated_at = SYSDATETIME() WHERE id = @id";
            return DbConnection.ExecuteNonQuery(sql, new SqlParameter("@id", id));
        }

        // ===== Вспомогательные методы =====

        private static Material MapMaterial(SqlDataReader reader)
        {
            return new Material
            {
                Id = (long)reader["id"],
                MaterialTypeId = (int)reader["material_type_id"],
                Name = (string)reader["name"],
                Description = reader["description"] as string,
                ArticleCode = reader["article_code"] as string,
                QuantityPerPackage = reader["quantity_per_package"] as decimal?,
                MeasurementUnitId = (int)reader["measurement_unit_id"],
                Cost = (decimal)reader["cost"],
                StockQuantity = (decimal)reader["stock_quantity"],
                MinStockQuantity = (decimal)reader["min_stock_quantity"],
                IsActive = (bool)reader["is_active"],
                MaterialTypeName = (string)reader["material_type_name"],
                LossPercentage = (decimal)reader["loss_percentage"],
                MeasurementUnitShortName = (string)reader["unit_short_name"],
                RequiredQuantity = reader["required_quantity"] == null ? 0 : (decimal)reader["required_quantity"]
            };
        }

        private static SqlParameter[] BuildParameters(Material m)
        {
            return new[]
            {
                new SqlParameter("@materialTypeId", m.MaterialTypeId),
                new SqlParameter("@name", m.Name ?? string.Empty),
                new SqlParameter("@description", (object)m.Description ?? DBNull.Value),
                new SqlParameter("@articleCode", (object)m.ArticleCode ?? DBNull.Value),
                new SqlParameter("@quantityPerPackage", (object)m.QuantityPerPackage ?? DBNull.Value),
                new SqlParameter("@measurementUnitId", m.MeasurementUnitId),
                new SqlParameter("@cost", m.Cost),
                new SqlParameter("@stockQuantity", m.StockQuantity),
                new SqlParameter("@minStockQuantity", m.MinStockQuantity)
            };
        }
    }
}
