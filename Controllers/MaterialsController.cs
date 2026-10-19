using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AromaDom.Models.Entities;
using AromaDom.Models.Repositories;
using AromaDom.Models.ViewModels;

namespace AromaDom.Controllers
{
    /// Контроллер материалов:
    public class MaterialsController : Controller
    {
        private readonly MaterialRepository _materialRepository;
        private readonly ProductRepository _productRepository;
        private readonly ReferenceRepository _referenceRepository;

        public MaterialsController()
        {
            _materialRepository = new MaterialRepository();
            _productRepository = new ProductRepository();
            _referenceRepository = new ReferenceRepository();
        }

        // Главная форма: список материалов

        // GET: /Materials?search=...&typeId=...
        public ActionResult Index(string search, int? typeId)
        {
            try
            {
                var materials = _materialRepository.GetAll(search, typeId);
                var cards = new List<MaterialCardViewModel>();
                foreach (var m in materials)
                {
                    cards.Add(new MaterialCardViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        MaterialTypeName = m.MaterialTypeName,
                        LossPercentage = m.LossPercentage,
                        Cost = m.Cost,
                        StockQuantity = m.StockQuantity,
                        MinStockQuantity = m.MinStockQuantity,
                        QuantityPerPackage = m.QuantityPerPackage ?? 0,
                        MeasurementUnitShortName = m.MeasurementUnitShortName,
                        RequiredQuantity = m.RequiredQuantity,
                        LowStock = m.LowStock
                    });
                }

                ViewBag.Search = search ?? string.Empty;
                ViewBag.TypeId = typeId;

                // Загружаем справочник типов для выпадающего фильтра
                ViewBag.MaterialTypes = _referenceRepository.GetAllMaterialTypes();

                return View(cards);
            }
            catch (Exception ex)
            {
                // Логируем и показываем пользователю сообщение об ошибке.
                System.Diagnostics.Trace.TraceError("MaterialsController.Index: {0}", ex);
                TempData["ErrorMessage"] = "Не удалось загрузить список материалов. Проверьте подключение к базе данных. Подробности: " + ex.Message;
                return View(new List<MaterialCardViewModel>());
            }
        }

        // Добавление материала

        // GET: /Materials/Create
        public ActionResult Create()
        {
            var model = new MaterialEditViewModel
            {
                MaterialTypes = _referenceRepository.GetAllMaterialTypes(),
                MeasurementUnits = _referenceRepository.GetAllMeasurementUnits()
            };
            return View(model);
        }

        // POST: /Materials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MaterialEditViewModel model)
        {
            try
            {
                model.MaterialTypes = _referenceRepository.GetAllMaterialTypes();
                model.MeasurementUnits = _referenceRepository.GetAllMeasurementUnits();

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Проверка уникальности наименования (дублирующая, на случай если БД откажет)
                var existing = _materialRepository.GetAll(model.Name);
                foreach (var m in existing)
                {
                    if (string.Equals(m.Name, model.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("Name", "Материал с таким наименованием уже существует.");
                        return View(model);
                    }
                }

                var material = MapViewModelToEntity(model);
                material.ArticleCode = null;
                material.Id = _materialRepository.Create(material);

                if (material.Id <= 0)
                {
                    ModelState.AddModelError("", "Не удалось создать материал. Возможно, нарушение уникальности.");
                    return View(model);
                }

                TempData["SuccessMessage"] = string.Format("Материал «{0}» успешно добавлен в базу данных.", material.Name);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("MaterialsController.Create: {0}", ex);
                ModelState.AddModelError("", "Ошибка при создании материала: " + ex.Message);
                return View(model);
            }
        }

        // Редактирование материала

        // GET: /Materials/Edit/5
        public ActionResult Edit(long? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    TempData["ErrorMessage"] = "Не указан идентификатор материала для редактирования.";
                    return RedirectToAction("Index");
                }

                var material = _materialRepository.GetById(id.Value);
                if (material == null)
                {
                    TempData["ErrorMessage"] = "Материал не найден.";
                    return RedirectToAction("Index");
                }

                var model = MapEntityToViewModel(material);
                model.MaterialTypes = _referenceRepository.GetAllMaterialTypes();
                model.MeasurementUnits = _referenceRepository.GetAllMeasurementUnits();
                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("MaterialsController.Edit: {0}", ex);
                TempData["ErrorMessage"] = "Ошибка при загрузке материала: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: /Materials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MaterialEditViewModel model)
        {
            try
            {
                model.MaterialTypes = _referenceRepository.GetAllMaterialTypes();
                model.MeasurementUnits = _referenceRepository.GetAllMeasurementUnits();

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var material = MapViewModelToEntity(model);
                _materialRepository.Update(material);

                TempData["SuccessMessage"] = string.Format("Изменения для материала «{0}» сохранены.", material.Name);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("MaterialsController.Edit POST: {0}", ex);
                ModelState.AddModelError("", "Ошибка при сохранении: " + ex.Message);
                return View(model);
            }
        }

        // Удаление материала (с подтверждением)

        // GET: /Materials/Delete/5 — форма подтверждения
        public ActionResult Delete(long? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    TempData["ErrorMessage"] = "Не указан идентификатор материала для удаления.";
                    return RedirectToAction("Index");
                }

                var material = _materialRepository.GetById(id.Value);
                if (material == null)
                {
                    TempData["ErrorMessage"] = "Материал не найден.";
                    return RedirectToAction("Index");
                }

                var products = _productRepository.GetProductsForMaterial(id.Value);
                ViewBag.LinkedProductsCount = products.Count;

                return View(material);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("MaterialsController.Delete: {0}", ex);
                TempData["ErrorMessage"] = "Ошибка: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: /Materials/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    TempData["ErrorMessage"] = "Не указан идентификатор материала для удаления.";
                    return RedirectToAction("Index");
                }

                int affected = _materialRepository.Delete(id.Value);
                if (affected == 0)
                {
                    TempData["ErrorMessage"] = "Невозможно удалить материал: он используется в рецептурах продукции. Сначала удалите связи из рецептур.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Материал удалён из базы данных.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("MaterialsController.DeleteConfirmed: {0}", ex);
                TempData["ErrorMessage"] = "Ошибка при удалении: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // Список продукции по конкретному материалу

        // GET: /Materials/Products/5
        public ActionResult Products(long? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    TempData["ErrorMessage"] = "Не указан идентификатор материала.";
                    return RedirectToAction("Index");
                }

                var material = _materialRepository.GetById(id.Value);
                if (material == null)
                {
                    TempData["ErrorMessage"] = "Материал не найден.";
                    return RedirectToAction("Index");
                }

                var products = _productRepository.GetProductsForMaterial(id.Value);

                var model = new ProductListViewModel
                {
                    MaterialId = material.Id,
                    MaterialName = material.Name,
                    MaterialTypeName = material.MaterialTypeName,
                    MeasurementUnitShortName = material.MeasurementUnitShortName,
                    StockQuantity = material.StockQuantity,
                    MinStockQuantity = material.MinStockQuantity,
                    Cost = material.Cost,
                    Products = products
                };

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("MaterialsController.Products: {0}", ex);
                TempData["ErrorMessage"] = "Ошибка при получении продукции: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ===== Вспомогательные методы =====

        private static Material MapViewModelToEntity(MaterialEditViewModel model)
        {
            return new Material
            {
                Id = model.Id,
                Name = model.Name,
                MaterialTypeId = model.MaterialTypeId,
                Cost = model.Cost,
                MeasurementUnitId = model.MeasurementUnitId,
                QuantityPerPackage = model.QuantityPerPackage,
                StockQuantity = model.StockQuantity,
                MinStockQuantity = model.MinStockQuantity,
                Description = model.Description
            };
        }

        private static MaterialEditViewModel MapEntityToViewModel(Material material)
        {
            return new MaterialEditViewModel
            {
                Id = material.Id,
                Name = material.Name,
                MaterialTypeId = material.MaterialTypeId,
                Cost = material.Cost,
                MeasurementUnitId = material.MeasurementUnitId,
                QuantityPerPackage = material.QuantityPerPackage ?? 0,
                StockQuantity = material.StockQuantity,
                MinStockQuantity = material.MinStockQuantity,
                Description = material.Description
            };
        }
    }
}
