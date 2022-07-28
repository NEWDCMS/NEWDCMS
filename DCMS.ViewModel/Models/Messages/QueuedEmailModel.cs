using DCMS.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Messages
{

    public partial class QueuedEmailModel : BaseEntityModel
    {

        public override int Id { get; set; }


        public string PriorityName { get; set; }

        public string From { get; set; }

        public string FromName { get; set; }

        public string To { get; set; }


        public string ToName { get; set; }

        public string ReplyTo { get; set; }


        public string ReplyToName { get; set; }


        public string CC { get; set; }


        public string Bcc { get; set; }


        public string Subject { get; set; }


        public string Body { get; set; }


        public string AttachmentFilePath { get; set; }


        [UIHint("Download")]
        public int AttachedDownloadId { get; set; }


        public DateTime CreatedOn { get; set; }


        public bool SendImmediately { get; set; }


        [UIHint("DateTimeNullable")]
        public DateTime? DontSendBeforeDate { get; set; }


        public int SentTries { get; set; }


        public DateTime? SentOn { get; set; }


        public string EmailAccountName { get; set; }


    }
}