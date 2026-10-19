using System.Collections.Generic;
using System.Data.SqlClient;
using AromaDom.Models.Entities;

namespace AromaDom.Models.Repositories
{
    /// Справочники: типы материалов, типы продукции, единицы измерения.
    /// Используются для выпадающих списков в формах.
    public class ReferenceRepository
    {
        public List<MaterialType> GetAllMaterialTypes()
        {
            var result = new List<MaterialType>();
            const string sql = @"
                SELECT id, name, loss_percentage, is_active
                FROM material_type
                WHERE is_active = 1
                ORDER BY name";

            using (var conn = DbConnection.CreateConnection())
            using (var reader = DbConnection.ExecuteReader(conn, sql))
            {
                while (reader.Read())
                {
                    result.Add(new MaterialType
                    {
                        Id = (int)reader["id"],
                        Name = (string)reader["name"],
                        LossPercentage = (decimal)reader["loss_percentage"],
                        IsActive = (bool)reader["is_active"]
                    });
                }
            }
            return result;
        }

        public List<ProductType> GetAllProductTypes()
        {
            var result = new List<ProductType>();
            const string sql = @"
                SELECT id, name, type_coefficient, is_active
                FROM product_type
                WHERE is_active = 1
                ORDER BY name";

            using (var conn = DbConnection.CreateConnection())
            using (var reader = DbConnection.ExecuteReader(conn, sql))
            {
                while (reader.Read())
                {
                    result.Add(new ProductType
                    {
                        Id = (int)reader["id"],
                        Name = (string)reader["name"],
                        TypeCoefficient = (decimal)reader["type_coefficient"],
                        IsActive = (bool)reader["is_active"]
                    });
                }
            }
            return result;
        }

        public List<MeasurementUnit> GetAllMeasurementUnits()
        {
            var result = new List<MeasurementUnit>();
            const string sql = @"
                SELECT id, short_name, full_name, is_active
                FROM measurement_unit
                WHERE is_active = 1
                ORDER BY short_name";

            using (var conn = DbConnection.CreateConnection())
            using (var reader = DbConnection.ExecuteReader(conn, sql))
            {
                while (reader.Read())
                {
                    result.Add(new MeasurementUnit
                    {
                        Id = (int)reader["id"],
                        ShortName = (string)reader["short_name"],
                        FullName = (string)reader["full_name"],
                        IsActive = (bool)reader["is_active"]
                    });
                }
            }
            return result;
        }
    }
}
