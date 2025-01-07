using System.Web.Mvc;


namespace ModelBinder.MiceApplyUpdatesRequest
{
	public class TMiceApplyUpdatesRequestModelBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			//var valueProvider = bindingContext.ValueProvider; // Получаем поставщик значений
            //ValueProviderResult vprId = valueProvider.GetValue("Id"); // получаем данные по одному полю

            return null;
		}
	}
}