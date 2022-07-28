namespace DCMS.ViewModel.Models.Common
{
    public partial class PagerModel
    {
        #region Constructors

        public PagerModel()
        {

        }

        #endregion Constructors

        #region Fields

        private int individualPagesDisplayedCount;
        private int pageIndex = -2;
        private int pageSize;

        private bool? showFirst;
        private bool? showIndividualPages;
        private bool? showLast;
        private bool? showNext;
        private bool? showPagerItems;
        private bool? showPrevious;
        private bool? showTotalSummary;

        private int totalRecords;
        private string firstButtonText;
        private string lastButtonText;
        private string nextButtonText;
        private string previousButtonText;
        private string currentPageText;

        #endregion Fields

        #region Properties

        public int CurrentPage
        {
            get
            {
                return (PageIndex + 1);
            }
        }

        public int IndividualPagesDisplayedCount
        {
            get
            {
                if (individualPagesDisplayedCount <= 0)
                {
                    return 5;
                }
                else
                {
                    return individualPagesDisplayedCount;
                }
            }
            set
            {
                individualPagesDisplayedCount = value;
            }
        }

        public int PageIndex
        {
            get
            {
                if (pageIndex < 0)
                {
                    return 0;
                }
                return pageIndex;
            }
            set
            {
                pageIndex = value;
            }
        }

        public int PageSize
        {
            get
            {
                return (pageSize <= 0) ? 10 : pageSize;
            }
            set
            {
                pageSize = value;
            }
        }


        public bool ShowFirst
        {
            get
            {
                return showFirst ?? true;
            }
            set
            {
                showFirst = value;
            }
        }


        public bool ShowIndividualPages
        {
            get
            {
                return showIndividualPages ?? true;
            }
            set
            {
                showIndividualPages = value;
            }
        }


        public bool ShowLast
        {
            get
            {
                return showLast ?? true;
            }
            set
            {
                showLast = value;
            }
        }

        public bool ShowNext
        {
            get
            {
                return showNext ?? true;
            }
            set
            {
                showNext = value;
            }
        }

        public bool ShowPagerItems
        {
            get
            {
                return showPagerItems ?? true;
            }
            set
            {
                showPagerItems = value;
            }
        }

        public bool ShowPrevious
        {
            get
            {
                return showPrevious ?? true;
            }
            set
            {
                showPrevious = value;
            }
        }

        public bool ShowTotalSummary
        {
            get
            {
                return showTotalSummary ?? false;
            }
            set
            {
                showTotalSummary = value;
            }
        }


        public int TotalPages
        {
            get
            {
                if ((TotalRecords == 0) || (PageSize == 0))
                {
                    return 0;
                }
                int num = TotalRecords / PageSize;
                if ((TotalRecords % PageSize) > 0)
                {
                    num++;
                }
                return num;
            }
        }


        public int TotalRecords
        {
            get
            {
                return totalRecords;
            }
            set
            {
                totalRecords = value;
            }
        }


        public string FirstButtonText
        {
            get
            {
                return (!string.IsNullOrEmpty(firstButtonText)) ?
                    firstButtonText : "首页";
            }
            set
            {
                firstButtonText = value;
            }
        }

        public string LastButtonText
        {
            get
            {
                return (!string.IsNullOrEmpty(lastButtonText)) ?
                    lastButtonText : "尾页";
            }
            set
            {
                lastButtonText = value;
            }
        }

        /// <summary>
        /// Gets or sets the next button text
        /// </summary>
        public string NextButtonText
        {
            get
            {
                return (!string.IsNullOrEmpty(nextButtonText)) ?
                    nextButtonText : "下一页";
            }
            set
            {
                nextButtonText = value;
            }
        }

        public string PreviousButtonText
        {
            get
            {
                return (!string.IsNullOrEmpty(previousButtonText)) ?
                    previousButtonText : "上一页";
            }
            set
            {
                previousButtonText = value;
            }
        }

        public string CurrentPageText
        {
            get
            {
                return (!string.IsNullOrEmpty(currentPageText)) ?
                    currentPageText : "当前页";
            }
            set
            {
                currentPageText = value;
            }
        }


        public string RouteActionName { get; set; }


        public bool UseRouteLinks { get; set; }


        public IRouteValues RouteValues { get; set; }

        #endregion Properties

        #region Methods


        public int GetFirstIndividualPageIndex()
        {
            if ((TotalPages < IndividualPagesDisplayedCount) ||
                ((PageIndex - (IndividualPagesDisplayedCount / 2)) < 0))
            {
                return 0;
            }
            if ((PageIndex + (IndividualPagesDisplayedCount / 2)) >= TotalPages)
            {
                return (TotalPages - IndividualPagesDisplayedCount);
            }
            return (PageIndex - (IndividualPagesDisplayedCount / 2));
        }


        public int GetLastIndividualPageIndex()
        {
            int num = IndividualPagesDisplayedCount / 2;
            if ((IndividualPagesDisplayedCount % 2) == 0)
            {
                num--;
            }
            if ((TotalPages < IndividualPagesDisplayedCount) ||
                ((PageIndex + num) >= TotalPages))
            {
                return (TotalPages - 1);
            }
            if ((PageIndex - (IndividualPagesDisplayedCount / 2)) < 0)
            {
                return (IndividualPagesDisplayedCount - 1);
            }
            return (PageIndex + num);
        }

        #endregion Methods
    }

    #region Classes


    public interface IRouteValues
    {
        int page { get; set; }
    }


    public partial class RouteValues : IRouteValues
    {
        public int id { get; set; }
        public string slug { get; set; }
        public int page { get; set; }
    }

    public partial class ForumSearchRouteValues : IRouteValues
    {
        public string searchterms { get; set; }
        public string adv { get; set; }
        public string forumId { get; set; }
        public string within { get; set; }
        public string limitDays { get; set; }
        public int page { get; set; }
    }


    public partial class PrivateMessageRouteValues : IRouteValues
    {
        public string tab { get; set; }
        public int page { get; set; }
    }


    public partial class ForumSubscriptionsRouteValues : IRouteValues
    {
        public int page { get; set; }
    }


    public partial class BackInStockSubscriptionsRouteValues : IRouteValues
    {
        public int page { get; set; }
    }

    #endregion Classes
}