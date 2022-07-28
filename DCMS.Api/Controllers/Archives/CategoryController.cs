using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Products;
using DCMS.Services.Products;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{

    /// <summary>
    /// 用于商品类别信息管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/archives")]
    public class CategoryController : BaseAPIController
    {
        private readonly ICategoryService _productCategoryService;
        private readonly IUserService _userService;
        private readonly IBrandService _brandService;
        

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productCategoryService"></param>
        public CategoryController(ICategoryService productCategoryService,
            IUserService userService,
            IBrandService brandService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _productCategoryService = productCategoryService;
            _userService = userService;
            _brandService = brandService;
            
        }

        /// <summary>
        /// 获取经销商类别树
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("category/getAllProductCategories/{store}")]
        [SwaggerOperation("getAllProductCategories")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<FancyTree>>> GetAllProductCategories(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<FancyTree>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var categories = _productCategoryService.GetAllCategories(store);
                    var fancyTrees = GetCategoryList(store ?? 0, categories, 0);

                    return this.Successful2("", fancyTrees);
                }
                catch (Exception ex)
                {
                    return this.Error2<FancyTree>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取经销商所有商品类别(改为获取所有叶子节点)
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("category/getAllCategories/{store}")]
        [SwaggerOperation("getAllCategories")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CategoryModel>>> GetAllCategories(int? store, bool? leaf = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<CategoryModel>(Resources.ParameterError);


            return await Task.Run(() =>
            {
                try
                {
                    var categories = new List<CategoryModel>();
                    var allCategories = _productCategoryService.GetAllCategories(store);

                    if (leaf.HasValue && leaf == true)//取叶子节点
                    {
                        var fancyTrees = new List<int>();
                        var minId = allCategories.Where(s => s.StoreId == store).Min(s => s.Id);
                        var leafs = GetCategoryLeafNode(fancyTrees, allCategories, store, minId);
                        categories = _productCategoryService.GetAllCategoriesByIds(store, leafs.ToArray()).Select(u => u.ToModel<CategoryModel>()).ToList();
                    }
                    else
                    {
                        categories = allCategories.Select(u => u.ToModel<CategoryModel>()).GroupBy(p => p.Id).Select(q => q.First()).ToList();
                      
                    }

                    return this.Successful("", categories);
                }
                catch (Exception ex)
                {
                    return this.Error<CategoryModel>(ex.Message);
                }


            });
        }



        /// <summary>
        /// 获取经销商商品叶子类别
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("category/getLeafCategories/{store}")]
        [SwaggerOperation("getLeafCategories")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CategoryModel>>> GetLeafCategories(int? store)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<CategoryModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var fancyTrees = new List<int>();

                    var allCategories = _productCategoryService.GetAllCategories(store);
                    var minId = allCategories.Where(s => s.StoreId == store).Min(s => s.Id);
                    var leafs = GetCategoryLeafNode(fancyTrees, allCategories, store, minId);

                    var categories = _productCategoryService.GetAllCategoriesByIds(store, leafs.ToArray()).Select(u => u.ToModel<CategoryModel>()).ToList();

                    return this.Successful("", categories);
                }
                catch (Exception ex)
                {
                    return this.Error<CategoryModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 递归获取类别数
        /// </summary>
        /// <param name="store"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [NonAction]
        private List<FancyTree> GetCategoryList(int? store, List<Category> allCategoies, int Id)
        {
            List<FancyTree> fancyTrees = new List<FancyTree>();
            var perentList = _productCategoryService.GetAllCategoriesByParentCategoryId(store.Value, allCategoies, Id);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    List<FancyTree> tempList = GetCategoryList(store.Value, allCategoies, b.Id);
                    var node = new FancyTree
                    {
                        id = b.Id,
                        title = b.Name,
                        expanded = true,
                        children = new List<FancyTree>()
                    };

                    if (tempList.Count > 0)
                    {
                        node.folder = true;
                        node.children = tempList;
                    }
                    fancyTrees.Add(node);
                }
            }
            return fancyTrees;
        }

        /// <summary>
        /// 获取商品子类别
        /// </summary>
        /// <param name="fancyTrees"></param>
        /// <param name="store"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [NonAction]
        private List<int> GetCategoryLeafNode(List<int> fancyTrees, List<Category> allCategoies, int? store, int Id)
        {
            var perentList = _productCategoryService.GetAllCategoriesByParentCategoryId(store.Value, allCategoies, Id);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    GetCategoryLeafNode(fancyTrees, allCategoies, store.Value, b.Id);
                }
            }
            else
            {
                fancyTrees.Add(Id);
            }

            return fancyTrees;
        }

        /// <summary>
        /// 创建/修改
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("category/createOrUpdate/{store}")]
        [SwaggerOperation("createOrUpdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(CategoryModel data, int? store, int? billId, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);


            return await Task.Run(() =>
           {
               var result = new APIResult<dynamic>();
               var user = _userService.GetUserById(store ?? 0, userId ?? 0);

               try
               {
                   var category = new Category();
                   if (data != null)
                   {

                       if (billId.HasValue && billId.Value != 0)
                       {
                           category = _productCategoryService.GetCategoryById(store, billId ?? 0);
                           if (category == null)
                           {
                               return this.Error("单据不存在");

                           }

                           //单开经销商不等于当前经销商
                           if (data.StoreId != store)
                           {
                               return this.Error("单据经销商不等于当前经销商");

                           }
                           //父类不能为空
                           if (data.ParentId != 0)
                           {
                               return this.Error("父类不能为空");

                           }
                           //验证品牌不能为空
                           var brand = _brandService.GetBrandById(store, data.BrandId);
                           if (brand == null)
                           {
                               return this.Error("品牌不存在");

                           }
                           if (category != null)
                           {
                               var model = data.ToEntity(category);

                               model.StoreId = store ?? 0;

                               _productCategoryService.UpdateCategory(model);
                           }
                       }

                       else
                       {
                           #region 添加

                           var model = data.ToEntity<Category>();

                           model.StoreId = store ?? 0;

                           _productCategoryService.InsertCategory(model);

                           #endregion
                       }
                   }

                   return this.Successful("成功");
               }
               catch (Exception ex)
               {
                   return this.Error(ex.Message);
               }


           });
        }
    }
}
