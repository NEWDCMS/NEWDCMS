using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Services.News;
using DCMS.ViewModel.Models.News;
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
    /// 用于资讯查询服务
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/news")]
    public class NewsController : BaseAPIController
    {
        private readonly INewsService _newsService;
        private readonly INewsCategoryService _newsCategoryService;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newsService"></param>
        /// <param name="newsCategoryService"></param>
        public NewsController(
            INewsService newsService,
           
            INewsCategoryService newsCategoryService
         , ILogger<BaseAPIController> logger) : base(logger)
        {
            _newsService = newsService;
            _newsCategoryService = newsCategoryService;
            
        }

        /// <summary>
        /// 获取最新资讯
        /// </summary>
        /// <param name="store"></param>
        /// <param name="pagenumber"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("latestnews/{pagenumber}/{limit}")]
        [SwaggerOperation("latestnews")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<NewsInfoModel>>> GetLatestNews(int? store, int pagenumber = 0, int limit = 30)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (pagenumber > 1)
                    {
                        pagenumber -= 1;
                    }

                    var news = _newsService.GetAllNews("", null, 0, 0, pagenumber, limit);

                    var listdatas = new List<NewsInfoModel>();

                    var qycode = "HRXHJS";

                    foreach (var item in news)
                    {
                        var pictures = new List<string>();
                        var path = item.PictureId == null ? "" : "http://resources.jsdcms.com:9100/" + qycode + "/document/image/" + item.PictureId;

                        var cateName = _newsCategoryService.GetCategoryById(store, item.NewsCategoryId)?.Name;

                        listdatas.Add(new NewsInfoModel()
                        {
                            Id = item.Id,
                            NewsCategoryId = item.NewsCategoryId,
                            NewsCategoryName = cateName,
                            Title = item.Title,
                            Content = CreateHtmlString(item.Title, path, item.Content),
                            Short = item.Short,
                            Full = item.Full,
                            MetaTitle = null,
                            MetaKeywords = null,
                            MetaDescription = null,
                            CreateDate = item.CreatedOnUtc,
                            Navigation = null,
                            PicturePath = path
                        });
                    };

                    return this.Successful2("", listdatas);
                }
                catch (Exception ex)
                {
                    return this.Error2<NewsInfoModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取具体新闻资讯
        /// </summary>
        /// <param name="store"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [HttpGet("news/{itemId}")]
        [SwaggerOperation("news")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<NewsInfoModel>> GetNewsById(int itemId, int? store = 0)
        {
            if (!store.HasValue || store.Value > 0)
                return this.Error3<NewsInfoModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = new NewsInfoModel();
                    var news = _newsService.GetNewsById(itemId, store);
                    if (news != null)
                    {
                        var qycode = "HRXHJS";
                        var cateName = _newsCategoryService.GetCategoryById(store, news.NewsCategoryId)?.Name;
                        var path = news.PictureId == null ? "" : "http://resources.jsdcms.com:9100/" + qycode + "/document/image/" + news.PictureId;

                        result = new NewsInfoModel()
                        {
                            Id = news.Id,
                            NewsCategoryId = news.NewsCategoryId,
                            NewsCategoryName = cateName,
                            Title = news.Title,
                            Content = CreateHtmlString(news.Title, path, news.Content),
                            Short = news.Short,
                            Full = news.Full,
                            MetaTitle = null,
                            MetaKeywords = null,
                            MetaDescription = null,
                            CreateDate = news.CreatedOnUtc,
                            Navigation = null,
                            PicturePath = path
                        };
                    }

                    return this.Successful3("", result);
                }
                catch (Exception ex)
                {
                    return this.Error3<NewsInfoModel>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 获取指定条数新闻
        /// </summary>
        /// <param name="count"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("topnews/{store}/{count}")]
        [SwaggerOperation("topnews")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<NewsInfoModel>>> GetNewsLastByCount(int? count, int? store = 0)
        {
            if (!store.HasValue || store.Value > 0)
                return this.Error2<NewsInfoModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var models = new List<NewsInfoModel>();
                    var news = _newsService.GetNewsByCount(count ?? 0);
                    if (news != null)
                    {
                        var qycode = "HRXHJS";
                        models = news.Select(st =>
                        {
                            var path = st.PictureId == null ? "" : "http://resources.jsdcms.com:9100/" + qycode + "/document/image/" + st.PictureId;
                            return new NewsInfoModel()
                            {
                                Id = st.Id,
                                Title = st.Title,
                                PicturePath = st.PictureId == null ? "" : "http://resources.jsdcms.com:9100/" + qycode + "/document/image/" + st.PictureId,
                                Content = CreateHtmlString(st.Title, path, st.Content),
                                CreateDate = st.CreatedOnUtc
                            };
                        }).ToList();
                    }

                    return this.Successful2("", models);
                }
                catch (Exception ex)
                {
                    return this.Error2<NewsInfoModel>(ex.Message);
                }

            });
        }


        private string CreateHtmlString(string title, string path, string content)
        {
            var html = "<div>" +
                         "<div class=\"news-title\">"
                          + title +
                         "</div>" +
                       "<div class=\"news-img\">" +
                         "<p><img src =\" " + path + "\" alt=\"Sample\" style=\"width:100%;\" /></p>" +
                       "</div>" +
                       "<div class=\"news-content\">"
                        + content +
                       "</div>" +
                    "</div>";
            return html;
        }
    }
}
