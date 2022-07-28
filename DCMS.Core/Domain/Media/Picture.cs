namespace DCMS.Core.Domain.Media
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 用于表示图片
    /// </summary>
    public partial class Picture : BaseEntity
    {
        /// <summary>
        /// Gets or sets the picture mime type
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the SEO friendly filename of the picture
        /// </summary>
        public string SeoFilename { get; set; }

        /// <summary>
        /// Gets or sets the "alt" attribute for "img" HTML element. If empty, then a default rule will be used (e.g. product name)
        /// </summary>
        public string AltAttribute { get; set; }

        /// <summary>
        /// Gets or sets the "title" attribute for "img" HTML element. If empty, then a default rule will be used (e.g. product name)
        /// </summary>
        public string TitleAttribute { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the picture binary
        /// </summary>
        public virtual PictureBinary PictureBinary { get; set; }

        /// <summary>
        /// Gets or sets the picture virtual path
        /// </summary>
        public string VirtualPath { get; set; }
    }
}
