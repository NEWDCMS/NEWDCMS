using System.Collections.Generic;

namespace DCMS.Core.Domain.Common
{
    public class ZTree
    {
        public double id { get; set; }

        public int? pId { get; set; }

        public string name { get; set; }

        public bool @checked
        {
            get;
            set;
        }
        public bool isParent { get; set; }

        public bool open { get; set; }

        public string value { get; set; }
        public string code { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public int? AccountCodeTypeId { get; set; }
        /// <summary>
        /// 可用金额
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// 会计科目主键
        /// </summary>
        public int AccountingOptionId { get; set; }

    }


    public class FancyTree
    {
        public int id { get; set; }
        public string title { get; set; }
        public bool expanded { get; set; }
        public bool folder { get; set; }
        public List<FancyTree> children { get; set; }
    }

    public class FancyTreeExt
    {
        public string title1 { get; set; }
        public string title2 { get; set; }
        public string title3 { get; set; }
        public int id1 { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public bool expanded { get; set; }
        public bool folder { get; set; }
        public List<FancyTreeExt> children { get; set; }
    }


    /// <summary>
    /// 递归树节点
    /// </summary>
    public class CommonTreeNode
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

    }



}
