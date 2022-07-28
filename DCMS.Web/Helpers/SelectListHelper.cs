namespace DCMS.Web.Helpers
{
    /// <summary>
    /// 选择列表
    /// </summary>
    public static class SelectListHelper
    {

        //public static List<SelectListItem> GetCategoryList(ICategoryService categoryService, IStaticCacheManager cacheManager, bool showHidden = false)
        //{
        //    if (categoryService == null)
        //        throw new ArgumentNullException(nameof(categoryService));

        //    if (cacheManager == null)
        //        throw new ArgumentNullException(nameof(cacheManager));

        //    var cacheKey = string.Format(DCMSModelCacheDefaults.CategoriesListKey, showHidden);
        //    var listItems = cacheManager.Get(cacheKey, () =>
        //    {
        //        var categories = categoryService.GetAllCategories(showHidden: showHidden);
        //        return categories.Select(c => new SelectListItem
        //        {
        //            Text = categoryService.GetFormattedBreadCrumb(c, categories),
        //            Value = c.Id.ToString()
        //        });
        //    });

        //    var result = new List<SelectListItem>();
        //    //clone the list to ensure that "selected" property is not set
        //    foreach (var item in listItems)
        //    {
        //        result.Add(new SelectListItem
        //        {
        //            Text = item.Text,
        //            Value = item.Value
        //        });
        //    }

        //    return result;
        //}


        //public static List<SelectListItem> GetManufacturerList(IManufacturerService manufacturerService, IStaticCacheManager cacheManager, bool showHidden = false)
        //{
        //    if (manufacturerService == null)
        //        throw new ArgumentNullException(nameof(manufacturerService));

        //    if (cacheManager == null)
        //        throw new ArgumentNullException(nameof(cacheManager));

        //    var cacheKey = string.Format(DCMSModelCacheDefaults.ManufacturersListKey, showHidden);
        //    var listItems = cacheManager.Get(cacheKey, () =>
        //    {
        //        var manufacturers = manufacturerService.GetAllManufacturers(showHidden: showHidden);
        //        return manufacturers.Select(m => new SelectListItem
        //        {
        //            Text = m.Name,
        //            Value = m.Id.ToString()
        //        });
        //    });

        //    var result = new List<SelectListItem>();
        //    //clone the list to ensure that "selected" property is not set
        //    foreach (var item in listItems)
        //    {
        //        result.Add(new SelectListItem
        //        {
        //            Text = item.Text,
        //            Value = item.Value
        //        });
        //    }

        //    return result;
        //}

    }
}