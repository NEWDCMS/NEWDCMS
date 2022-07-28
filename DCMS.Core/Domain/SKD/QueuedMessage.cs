using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace DCMS.Core.Domain.Tasks
{
    /// <summary>
    /// 表示（DCMS）消息队列项目
    /// </summary>
    public class QueuedMessage : BaseEntity
    {

        [Column(TypeName = "BIT(1)")]
        public bool IsRead { get; set; }
        public int Priority { get; set; } = 0;
        public MTypeEnum MType
        {
            get { return (MTypeEnum)MTypeId; }
            set { MTypeId = (int)value; }
        }
        public int MTypeId { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Icon { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public BillTypeEnum BillType
        {
            get { return (BillTypeEnum)BillTypeId; }
            set { BillTypeId = (int)value; }
        }
        public int? BillTypeId { get; set; } = 0;
        public string BillNumber { get; set; } = "";
        public int? BillId { get; set; } = 0;
        public DateTime CreatedOnUtc { get; set; } = DateTime.Now;
        public DateTime? SentOnUtc { get; set; }
        public int SentTries { get; set; } = 0;
        /// <summary>
        /// 手机号（标识）
        /// </summary>
        public string ToUser { get; set; } = "";
        public string TerminalNames { get; set; } = "";
        public string ProductNames { get; set; } = "";
        public string BillNumbers { get; set; } = "";
        public string BusinessUser { get; set; } = "";
        public string TerminalName { get; set; } = "";
        public double? Distance { get; set; } = 0;
        public int? Month { get; set; } = 0;
        public decimal? Amount { get; set; } = 0;


    }


    [Serializable]
    public class MessageStructure : QueuedMessage
    {
        /// <summary>
        /// 客户序列“|”分割
        /// </summary>
        [DataMember]
        public List<string> Terminals { get; set; } = new List<string>();
        /// <summary>
        /// 商品序列“|”分割
        /// </summary>
        [DataMember]
        public List<string> Products { get; set; } = new List<string>();
        /// <summary>
        /// 单据序列“|”分割
        /// </summary>
        [DataMember]
        public List<string> Bills { get; set; } = new List<string>();


        public QueuedMessage ToEntity()
        {
            TerminalNames = string.Join("|", Terminals);
            ProductNames = string.Join("|", Products);
            BillNumbers = string.Join("|", Bills);
            return this;
        }

    }


    public static class MessagExt
    {
        public static MessageStructure ToStructure(this QueuedMessage queuedMessage)
        {
            var terminalNames = new List<string>();
            var productNames = new List<string>();
            var billNumbers = new List<string>();

            if (!string.IsNullOrEmpty(queuedMessage.TerminalNames))
            {
                terminalNames = queuedMessage.TerminalNames.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (!string.IsNullOrEmpty(queuedMessage.ProductNames))
            {
                productNames = queuedMessage.ProductNames.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (!string.IsNullOrEmpty(queuedMessage.BillNumbers))
            {
                billNumbers = queuedMessage.BillNumbers.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return new MessageStructure()
            {
                Id = queuedMessage.Id,
                StoreId = queuedMessage.StoreId,
                IsRead = queuedMessage.IsRead,
                Priority = queuedMessage.Priority,
                MType = (MTypeEnum)queuedMessage.MTypeId,
                MTypeId = queuedMessage.MTypeId,
                Title = queuedMessage.Title,
                Content = queuedMessage.Content,
                Icon = queuedMessage.Icon,
                Date = queuedMessage.Date,
                BillType = (BillTypeEnum)queuedMessage.BillTypeId,
                BillTypeId = queuedMessage.BillTypeId ?? 0,
                BillNumber = queuedMessage.BillNumber,
                BillId = queuedMessage.BillId,
                CreatedOnUtc = queuedMessage.CreatedOnUtc,
                SentOnUtc = queuedMessage.SentOnUtc,
                SentTries = queuedMessage.SentTries,
                ToUser = queuedMessage.ToUser,
                TerminalNames = queuedMessage.TerminalNames,
                ProductNames = queuedMessage.ProductNames,
                BillNumbers = queuedMessage.BillNumbers,
                BusinessUser = queuedMessage.BusinessUser,
                TerminalName = queuedMessage.TerminalName,
                Distance = queuedMessage.Distance ?? 0,
                Month = queuedMessage.Month ?? 0,
                Amount = queuedMessage.Amount ?? 0,
                Terminals = terminalNames,
                Products = productNames,
                Bills = billNumbers
            };

        }

    }

}
