using System.Web.Mvc;

namespace AromaDom.App_Start
{
    /// Регистрация глобальных фильтров.
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
