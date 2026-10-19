/* =============================================================================
   БАЗА ДАННЫХ: AromaHouse
   ============================================================================= */

USE master;
GO

IF DB_ID('AromaHouse') IS NOT NULL
BEGIN
    ALTER DATABASE AromaHouse SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AromaHouse;
END
GO

CREATE DATABASE AromaHouse;
GO

USE AromaHouse;
GO

-- ЧАСТЬ 1. СПРАВОЧНИКИ

CREATE TABLE material_type (
    id INT IDENTITY(1,1) NOT NULL,
    name NVARCHAR(60) NOT NULL,
    loss_percentage DECIMAL(5,4) NOT NULL DEFAULT 0,
    is_active BIT NOT NULL DEFAULT 1,
    CONSTRAINT pk_material_type PRIMARY KEY CLUSTERED (id),
    CONSTRAINT uq_material_type_name UNIQUE (name),
    CONSTRAINT chk_material_type_loss CHECK (loss_percentage >= 0 AND loss_percentage < 1)
);
GO

CREATE TABLE measurement_unit (
    id INT IDENTITY(1,1) NOT NULL,
    short_name NVARCHAR(10) NOT NULL,
    full_name NVARCHAR(60) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    CONSTRAINT pk_measurement_unit PRIMARY KEY CLUSTERED (id),
    CONSTRAINT uq_measurement_unit_short UNIQUE (short_name)
);
GO

CREATE TABLE supplier_type (
    id INT IDENTITY(1,1) NOT NULL,
    name NVARCHAR(60) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    CONSTRAINT pk_supplier_type PRIMARY KEY CLUSTERED (id),
    CONSTRAINT uq_supplier_type_name UNIQUE (name)
);
GO

CREATE TABLE product_type (
    id INT IDENTITY(1,1) NOT NULL,
    name NVARCHAR(60) NOT NULL,
    type_coefficient DECIMAL(5,3) NOT NULL DEFAULT 1,
    is_active BIT NOT NULL DEFAULT 1,
    CONSTRAINT pk_product_type PRIMARY KEY CLUSTERED (id),
    CONSTRAINT uq_product_type_name UNIQUE (name),
    CONSTRAINT chk_product_type_coef CHECK (type_coefficient > 0)
);
GO

-- ЧАСТЬ 2. ОСНОВНЫЕ ТАБЛИЦЫ

CREATE TABLE supplier (
    id INT IDENTITY(1,1) NOT NULL,
    supplier_type_id INT NOT NULL,
    name NVARCHAR(150) NOT NULL,
    inn CHAR(12) NULL,
    delivery_days INT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    updated_at DATETIME NULL,
    CONSTRAINT pk_supplier PRIMARY KEY CLUSTERED (id),
    CONSTRAINT fk_supplier_type
        FOREIGN KEY (supplier_type_id)
        REFERENCES supplier_type (id)
        ON UPDATE CASCADE
        ON DELETE NO ACTION,
    CONSTRAINT uq_supplier_inn UNIQUE (inn),
    CONSTRAINT chk_supplier_delivery_days CHECK (delivery_days IS NULL OR delivery_days >= 0)
);
CREATE NONCLUSTERED INDEX ix_supplier_type ON supplier (supplier_type_id);
GO

CREATE TABLE material (
    id BIGINT IDENTITY(1,1) NOT NULL,
    material_type_id INT NOT NULL,
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX) NULL,
    image VARBINARY(MAX) NULL,
    article_code NVARCHAR(50) NULL,
    quantity_per_package DECIMAL(10,3) NULL,
    measurement_unit_id INT NOT NULL,
    cost DECIMAL(12,2) NOT NULL,
    stock_quantity DECIMAL(14,3) NOT NULL DEFAULT 0,
    min_stock_quantity DECIMAL(14,3) NOT NULL DEFAULT 0,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    updated_at DATETIME2 NULL,
    CONSTRAINT pk_material PRIMARY KEY CLUSTERED (id),
    CONSTRAINT fk_material_type FOREIGN KEY (material_type_id) REFERENCES material_type (id)
        ON UPDATE CASCADE
        ON DELETE NO ACTION,
    CONSTRAINT fk_material_unit FOREIGN KEY (measurement_unit_id) REFERENCES measurement_unit (id)
        ON UPDATE CASCADE
        ON DELETE NO ACTION,
    CONSTRAINT chk_material_cost CHECK (cost >= 0),
    CONSTRAINT chk_material_stock CHECK (stock_quantity >= 0),
    CONSTRAINT chk_material_min_stock CHECK (min_stock_quantity >= 0),
    CONSTRAINT chk_material_qty_pkg CHECK (quantity_per_package IS NULL OR quantity_per_package > 0),
    CONSTRAINT uq_material_name UNIQUE (name)
);
CREATE NONCLUSTERED INDEX ix_material_type ON material (material_type_id);
CREATE NONCLUSTERED INDEX ix_material_unit ON material (measurement_unit_id);
CREATE NONCLUSTERED INDEX ix_material_name ON material (name);
CREATE NONCLUSTERED INDEX ix_material_article ON material (article_code);
GO

CREATE TABLE material_supplier (
    material_id BIGINT NOT NULL,
    supplier_id INT NOT NULL,
    is_preferred BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT pk_material_supplier PRIMARY KEY CLUSTERED (material_id, supplier_id),
    CONSTRAINT fk_ms_material FOREIGN KEY (material_id) REFERENCES material (id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT fk_ms_supplier FOREIGN KEY (supplier_id) REFERENCES supplier (id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);
GO

CREATE TABLE material_stock_log (
    id BIGINT IDENTITY(1,1) NOT NULL,
    material_id BIGINT NOT NULL,
    old_quantity DECIMAL(14,3) NULL,
    new_quantity DECIMAL(14,3) NOT NULL,
    change_reason NVARCHAR(200) NULL,
    changed_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    changed_by NVARCHAR(100) NULL,
    CONSTRAINT pk_material_stock_log PRIMARY KEY CLUSTERED (id),
    CONSTRAINT fk_msl_material FOREIGN KEY (material_id) REFERENCES material (id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX ix_msl_material ON material_stock_log (material_id);
CREATE NONCLUSTERED INDEX ix_msl_changed  ON material_stock_log (changed_at);
GO

CREATE TABLE product (
    id BIGINT IDENTITY(1,1) NOT NULL,
    product_type_id INT NOT NULL,
    article_code NVARCHAR(50) NOT NULL,
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX) NULL,
    image VARBINARY(MAX) NULL,
    min_partner_price DECIMAL(12,2) NOT NULL,
    standard_code NVARCHAR(50) NULL,
    production_minutes INT NULL,
    cost_price DECIMAL(12,2) NOT NULL,
    parameter1 DECIMAL(10,3) NOT NULL DEFAULT 1,
    parameter2 DECIMAL(10,3) NOT NULL DEFAULT 1,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    updated_at DATETIME2 NULL,
    CONSTRAINT pk_product PRIMARY KEY CLUSTERED (id),
    CONSTRAINT fk_product_type FOREIGN KEY (product_type_id) REFERENCES product_type (id)
        ON UPDATE CASCADE
        ON DELETE NO ACTION,
    CONSTRAINT uq_product_article UNIQUE (article_code),
    CONSTRAINT uq_product_name UNIQUE (name),
    CONSTRAINT chk_product_min_price CHECK (min_partner_price >= 0),
    CONSTRAINT chk_product_cost CHECK (cost_price >= 0),
    CONSTRAINT chk_product_param1 CHECK (parameter1 > 0),
    CONSTRAINT chk_product_param2 CHECK (parameter2 > 0)
);
CREATE NONCLUSTERED INDEX ix_product_type ON product (product_type_id);
GO

CREATE TABLE product_recipe (
    product_id BIGINT NOT NULL,
    material_id BIGINT NOT NULL,
    quantity_per_unit DECIMAL(10,3) NOT NULL,
    measurement_unit_id INT NOT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT pk_product_recipe PRIMARY KEY CLUSTERED (product_id, material_id),
    CONSTRAINT fk_pr_product FOREIGN KEY (product_id) REFERENCES product (id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT fk_pr_material FOREIGN KEY (material_id) REFERENCES material (id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT fk_pr_unit FOREIGN KEY (measurement_unit_id) REFERENCES measurement_unit (id),
    CONSTRAINT chk_pr_qty CHECK (quantity_per_unit > 0)
);
CREATE NONCLUSTERED INDEX ix_pr_material ON product_recipe (material_id);
CREATE NONCLUSTERED INDEX ix_pr_product  ON product_recipe (product_id);
GO

-- ЧАСТЬ 3. ЗАГРУЗКА ДАННЫХ

SET IDENTITY_INSERT material_type ON;
INSERT INTO material_type (id, name, loss_percentage, is_active) VALUES
    (1, N'Соевый воск', 0.0012, 1),
    (2, N'Пчелиный воск', 0.0018, 1),
    (3, N'Ароматическое масло', 0.0025, 1),
    (4, N'Краситель', 0.0008, 1),
    (5, N'Фитиль', 0.0003, 1),
    (6, N'Стеклянная тара', 0.0004, 1),
    (7, N'Керамическая тара', 0.0006, 1),
    (8, N'Декор', 0.0010, 1),
    (9, N'Упаковка', 0.0005, 1),
    (10, N'Этикетка', 0.0002, 1);
SET IDENTITY_INSERT material_type OFF;
GO

SET IDENTITY_INSERT measurement_unit ON;
INSERT INTO measurement_unit (id, short_name, full_name, is_active) VALUES
    (1, N'кг', N'килограмм', 1),
    (2, N'л', N'литр', 1),
    (3, N'шт', N'штука', 1),
    (4, N'м', N'метр', 1);
SET IDENTITY_INSERT measurement_unit OFF;
GO

SET IDENTITY_INSERT supplier_type ON;
INSERT INTO supplier_type (id, name, is_active) VALUES
    (1, N'Юридическое лицо', 1),
    (2, N'Индивидуальный предприниматель', 1);
SET IDENTITY_INSERT supplier_type OFF;
GO

SET IDENTITY_INSERT product_type ON;
INSERT INTO product_type (id, name, type_coefficient, is_active) VALUES
    (1, N'Классическая свеча', 1.000, 1),
    (2, N'Премиальная свеча', 1.150, 1),
    (3, N'Подарочный набор', 1.300, 1),
    (4, N'Интерьерная свеча', 1.200, 1),
    (5, N'Мини-свеча', 0.800, 1),
    (6, N'Сезонная коллекция', 1.250, 1);
SET IDENTITY_INSERT product_type OFF;
GO

SET IDENTITY_INSERT material ON;
INSERT INTO material (id, material_type_id, name, description, article_code, quantity_per_package, measurement_unit_id, cost, stock_quantity, min_stock_quantity, is_active) VALUES
    (1, 1, N'Соевый воск премиум', N'Соевый воск, кг', N'M-001', 20.000, 1, 890.00, 120.000, 40.000, 1),
    (2, 1, N'Соевый воск EcoBlend', N'Соевый воск, кг', N'M-002', 25.000, 1, 760.00, 95.000, 30.000, 1),
    (3, 2, N'Пчелиный воск натуральный', N'Пчелиный воск, кг', N'M-003', 10.000, 1, 1450.00, 48.000, 15.000, 1),
    (4, 3, N'Масло лаванды', N'Ароматическое масло, л', N'M-004', 1.000, 2, 3200.00, 18.500, 6.000, 1),
    (5, 3, N'Масло ванили', N'Ароматическое масло, л', N'M-005', 1.000, 2, 2950.00, 22.000, 8.000, 1),
    (6, 3, N'Масло цитрусовое', N'Ароматическое масло, л', N'M-006', 1.000, 2, 2870.00, 16.700, 5.000, 1),
    (7, 4, N'Краситель янтарный', N'Краситель, кг', N'M-007', 0.500, 1, 640.00, 6.200, 2.000, 1),
    (8, 4, N'Краситель розовый', N'Краситель, кг', N'M-008', 0.500, 1, 615.00, 5.100, 1.500, 1),
    (9, 5, N'Фитиль хлопковый 8 см', N'Фитиль, шт', N'M-009', 500.000, 3, 4.50, 2500.000, 800.000, 1),
    (10, 5, N'Фитиль хлопковый 12 см', N'Фитиль, шт', N'M-010', 400.000, 3, 5.80, 1800.000, 600.000, 1),
    (11, 6, N'Стакан прозрачный 200 мл', N'Стеклянная тара, шт', N'M-011', 100.000, 3, 28.00, 950.000, 300.000, 1),
    (12, 6, N'Стакан матовый 250 мл', N'Стеклянная тара, шт', N'M-012', 60.000, 3, 34.00, 620.000, 200.000, 1),
    (13, 7, N'Чаша керамическая белая', N'Керамическая тара, шт', N'M-013', 40.000, 3, 72.00, 280.000, 80.000, 1),
    (14, 8, N'Сухоцветы лаванды', N'Декор, кг', N'M-014', 0.300, 1, 520.00, 4.800, 1.500, 1),
    (15, 9, N'Лента сатиновая бежевая', N'Упаковка, м', N'M-015', 50.000, 4, 12.00, 340.000, 120.000, 1),
    (16, 9, N'Коробка подарочная малая', N'Упаковка, шт', N'M-016', 50.000, 3, 18.00, 420.000, 150.000, 1),
    (17, 10, N'Этикетка фирменная круглая', N'Этикетка, шт', N'M-017', 250.000, 3, 3.20, 1800.000, 500.000, 1),
    (18, 10, N'Наклейка предупреждающая', N'Этикетка, шт', N'M-018', 200.000, 3, 2.10, 1600.000, 400.000, 1);
SET IDENTITY_INSERT material OFF;
GO

SET IDENTITY_INSERT product ON;
INSERT INTO product (id, product_type_id, article_code, name, min_partner_price, cost_price, parameter1, parameter2, is_active) VALUES
    (1, 1, N'AR-001', N'Свеча «Лавандовый вечер»', 980.00, 686.00, 7.000, 10.000, 1),
    (2, 1, N'AR-002', N'Свеча «Ванильный крем»', 950.00, 665.00, 7.000, 10.000, 1),
    (3, 1, N'AR-003', N'Свеча «Цитрусовый сад»', 970.00, 679.00, 7.000, 10.000, 1),
    (4, 2, N'AR-004', N'Свеча «Белый мускус»', 1290.00, 903.00, 8.000, 12.000, 1),
    (5, 2, N'AR-005', N'Свеча «Дикая орхидея»', 1360.00, 952.00, 8.000, 12.000, 1),
    (6, 4, N'AR-006', N'Свеча «Теплый янтарь»', 1490.00, 1043.00, 10.000, 14.000, 1),
    (7, 4, N'AR-007', N'Свеча «Сандаловое дерево»', 1520.00, 1064.00, 10.000, 14.000, 1),
    (8, 5, N'AR-008', N'Мини-свеча «Лаванда»', 420.00, 294.00, 5.000, 6.000, 1),
    (9, 5, N'AR-009', N'Мини-свеча «Ваниль»', 410.00, 287.00, 5.000, 6.000, 1),
    (10, 5, N'AR-010', N'Мини-свеча «Цитрус»', 430.00, 301.00, 5.000, 6.000, 1),
    (11, 3, N'AR-011', N'Набор «Уютный дом»', 2190.00, 1533.00, 15.000, 20.000, 1),
    (12, 3, N'AR-012', N'Набор «Теплый вечер»', 2340.00, 1638.00, 15.000, 20.000, 1),
    (13, 6, N'AR-013', N'Свеча «Зимние специи»', 1380.00, 966.00, 8.500, 11.000, 1),
    (14, 6, N'AR-014', N'Свеча «Хвойный лес»', 1410.00, 987.00, 8.500, 11.000, 1);
SET IDENTITY_INSERT product OFF;
GO

INSERT INTO product_recipe (product_id, material_id, quantity_per_unit, measurement_unit_id) VALUES
    (1, 1, 0.180, 1),
    (1, 4, 0.009, 2),
    (1, 9, 1.000, 3),
    (1, 11, 1.000, 3),
    (1, 17, 1.000, 3),
    (2, 1, 0.180, 1),
    (2, 5, 0.010, 2),
    (2, 9, 1.000, 3),
    (2, 11, 1.000, 3),
    (2, 17, 1.000, 3),
    (3, 2, 0.175, 1),
    (3, 6, 0.011, 2),
    (3, 9, 1.000, 3),
    (3, 11, 1.000, 3),
    (3, 17, 1.000, 3),
    (4, 1, 0.220, 1),
    (4, 5, 0.006, 2),
    (4, 10, 1.000, 3),
    (4, 12, 1.000, 3),
    (4, 17, 1.000, 3),
    (5, 1, 0.225, 1),
    (5, 4, 0.004, 2),
    (5, 10, 1.000, 3),
    (5, 12, 1.000, 3),
    (5, 17, 1.000, 3),
    (6, 3, 0.280, 1),
    (6, 7, 0.004, 1),
    (6, 10, 1.000, 3),
    (6, 13, 1.000, 3),
    (6, 17, 1.000, 3),
    (7, 3, 0.285, 1),
    (7, 5, 0.003, 2),
    (7, 10, 1.000, 3),
    (7, 13, 1.000, 3),
    (7, 17, 1.000, 3),
    (8, 2, 0.070, 1),
    (8, 4, 0.003, 2),
    (8, 9, 1.000, 3),
    (8, 11, 1.000, 3),
    (9, 2, 0.070, 1),
    (9, 5, 0.003, 2),
    (9, 9, 1.000, 3),
    (9, 11, 1.000, 3),
    (10, 2, 0.070, 1),
    (10, 6, 0.003, 2),
    (10, 9, 1.000, 3),
    (10, 11, 1.000, 3),
    (11, 1, 0.180, 1),
    (11, 4, 0.009, 2),
    (11, 9, 1.000, 3),
    (11, 11, 1.000, 3),
    (11, 15, 1.200, 4),
    (11, 16, 1.000, 3),
    (11, 17, 1.000, 3),
    (12, 1, 0.225, 1),
    (12, 5, 0.010, 2),
    (12, 10, 1.000, 3),
    (12, 12, 1.000, 3),
    (12, 15, 1.500, 4),
    (12, 16, 1.000, 3),
    (12, 17, 1.000, 3),
    (13, 1, 0.210, 1),
    (13, 5, 0.008, 2),
    (13, 7, 0.003, 1),
    (13, 10, 1.000, 3),
    (13, 12, 1.000, 3),
    (13, 18, 1.000, 3),
    (14, 1, 0.215, 1),
    (14, 6, 0.004, 2),
    (14, 7, 0.002, 1),
    (14, 10, 1.000, 3),
    (14, 12, 1.000, 3),
    (14, 18, 1.000, 3);
GO

-- ЧАСТЬ 4. ПОЛЬЗОВАТЕЛЬСКИЕ ФУНКЦИИ (UDF)

CREATE OR ALTER FUNCTION dbo.CalculateRequiredQuantity (@material_id BIGINT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @total DECIMAL(18,2) = 0;
    SELECT @total = COALESCE(SUM(quantity_per_unit), 0)
      FROM product_recipe
     WHERE material_id = @material_id;
    RETURN ROUND(@total, 2);
END;
GO

CREATE OR ALTER FUNCTION dbo.CalculateProductQuantity (
    @product_type_id  INT,
    @material_type_id INT,
    @raw_quantity      INT,
    @param1            FLOAT,
    @param2            FLOAT)
RETURNS INT
AS
BEGIN
    DECLARE @result INT = -1;
    DECLARE @coef DECIMAL(5,3);
    DECLARE @loss DECIMAL(5,4);

    -- Проверки входных данных
    IF @raw_quantity IS NULL OR @param1 IS NULL OR @param2 IS NULL
        RETURN -1;
    IF @raw_quantity <= 0 OR @param1 <= 0 OR @param2 <= 0
        RETURN -1;

    SELECT @coef = type_coefficient
      FROM product_type
     WHERE id = @product_type_id;
    IF @coef IS NULL RETURN -1;

    SELECT @loss = loss_percentage
      FROM material_type
     WHERE id = @material_type_id;
    IF @loss IS NULL RETURN -1;

    DECLARE @per_unit FLOAT = @param1 * @param2 * @coef;
    IF @per_unit <= 0 RETURN -1;

    DECLARE @usable FLOAT = @raw_quantity * (1.0 - @loss);
    SET @result = FLOOR(@usable / @per_unit);
    RETURN @result;
END;
GO

-- ЧАСТЬ 5. ПРОВЕРОЧНЫЕ ЗАПРОСЫ (для самопроверки)

SELECT
    m.id,
    mt.name AS material_type,
    m.name AS material_name,
    m.cost AS unit_price,
    mu.short_name AS unit_of_measure,
    m.stock_quantity,
    m.min_stock_quantity,
    dbo.CalculateRequiredQuantity(m.id) AS required_quantity,
    CASE WHEN m.stock_quantity < m.min_stock_quantity THEN 1 ELSE 0 END AS low_stock
FROM material m
JOIN material_type mt ON mt.id = m.material_type_id
JOIN measurement_unit mu ON mu.id = m.measurement_unit_id
WHERE m.is_active = 1
ORDER BY m.name;
GO

DECLARE @material_id BIGINT = 1;
SELECT
    p.id,
    p.article_code,
    p.name AS product_name,
    pt.name AS product_type,
    pr.quantity_per_unit,
    mu.short_name AS unit_of_measure
FROM product_recipe pr
JOIN product p ON p.id = pr.product_id
JOIN product_type pt ON pt.id = p.product_type_id
JOIN measurement_unit mu ON mu.id = pr.measurement_unit_id
WHERE pr.material_id = @material_id
ORDER BY p.name;
GO

SELECT dbo.CalculateProductQuantity(1, 1, 100, 7.0, 10.0) AS product_count;
GO
