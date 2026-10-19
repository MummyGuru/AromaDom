using System;
using System.Web.Mvc;
using AromaDom.Models.Repositories;
using AromaDom.Models.ViewModels;

namespace AromaDom.Controllers
{
    /// Контроллер калькулятора — реализует метод расчёта количества продукции
    /// из сырья — спецификация метода CalculateProductQuantity.
    public class CalculatorController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly ReferenceRepository _referenceRepository;

        public CalculatorController()
        {
            _productRepository = new ProductRepository();
            _referenceRepository = new ReferenceRepository();
        }

        // GET: /Calculator
        public ActionResult Index()
        {
            var model = new CalculatorViewModel
            {
                ProductTypes = _referenceRepository.GetAllProductTypes(),
                MaterialTypes = _referenceRepository.GetAllMaterialTypes()
            };
            return View(model);
        }

        // POST: /Calculator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(CalculatorViewModel model)
        {
            try
            {
                model.ProductTypes = _referenceRepository.GetAllProductTypes();
                model.MaterialTypes = _referenceRepository.GetAllMaterialTypes();

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                int result = _productRepository.CalculateProductQuantity(
                    model.ProductTypeId.Value,
                    model.MaterialTypeId.Value,
                    model.RawQuantity.Value,
                    model.Parameter1.Value,
                    model.Parameter2.Value);

                model.Result = result;

                if (result == -1)
                {
                    model.ResultMessage = "Метод вернул -1: переданы некорректные или неположительные параметры, " +
                        "либо указанные типы не существуют. Проверьте входные данные.";
                }
                else
                {
                    model.ResultMessage = string.Format("Из указанного сырья можно произвести {0} ед. продукции " +
                        "(с учётом коэффициента типа и процента потерь).", result);

                    var coef = model.ProductTypes.Find(t => t.Id == model.ProductTypeId.Value)?.TypeCoefficient ?? 0;
                    var loss = model.MaterialTypes.Find(t => t.Id == model.MaterialTypeId.Value)?.LossPercentage ?? 0;
                    model.TypeCoefficient = coef;
                    model.LossPercentage = loss;
                    model.RequiredPerUnit = (decimal)((model.Parameter1 ?? 0) * (model.Parameter2 ?? 0)) * coef;
                    model.UsableMaterial = (decimal)(model.RawQuantity ?? 0) * (1m - loss);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("CalculatorController.Index POST: {0}", ex);
                ModelState.AddModelError("", "Ошибка при расчёте: " + ex.Message);
                return View(model);
            }
        }
    }
}
