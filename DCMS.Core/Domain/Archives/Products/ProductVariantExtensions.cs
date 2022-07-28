using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Core.Domain.Products
{

    /// <summary>
    /// 变体商品扩展
    /// </summary>
    public static class ProductVariantExtensions
    {
        public static int[] ParseRequiredProductIds(this Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            if (string.IsNullOrEmpty(product.RequiredProductIds))
            {
                return new int[0];
            }

            var ids = new List<int>();

            foreach (var idStr in product.RequiredProductIds
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()))
            {
                if (int.TryParse(idStr, out int id))
                {
                    ids.Add(id);
                }
            }

            return ids.ToArray();
        }
    }
}
