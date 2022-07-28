using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;


namespace DCMS.Core
{

    /// <summary>
    /// 用于安全签名算法的的辅助操作方法
    /// </summary>
    public static class SecuritySignHelper
    {
        /// <summary>
        /// 表示合作账号前缀
        /// </summary>
        public const string Partner = "partner";
        /// <summary>
        /// 表示签名前缀
        /// </summary>
        public const string Sign = "sign";


        public static string GetSecuritySign(this NameValueCollection getCollection, string partner, string partnerKey, NameValueCollection postCollection = null)
        {
            if (string.IsNullOrWhiteSpace(partner) || string.IsNullOrWhiteSpace(partnerKey))
            {
                throw new ArgumentNullException();
            }
            var dic = SecuritySignHelper.GetSortedDictionary(getCollection,
                (k) =>
                {
                    //过滤partner及sign
                    return string.Equals(k, SecuritySignHelper.Partner, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(k, SecuritySignHelper.Sign, StringComparison.OrdinalIgnoreCase);
                });
            dic.Add(SecuritySignHelper.Partner, partner);
            StringBuilder tmp = new StringBuilder();
            SecuritySignHelper.FillStringBuilder(tmp, dic);//将QueryString填入StringBuilder
            dic = SecuritySignHelper.GetSortedDictionary(postCollection);
            SecuritySignHelper.FillStringBuilder(tmp, dic);//将Form填入StringBuilder
            tmp.Append(partnerKey);//在尾部添加key
            tmp.Remove(0, 1);//移除第一个&
            return tmp.ToString().GetMD5_32();//获取32位长度的Md5摘要
        }

        /// <summary>
        /// 获取防篡改签名
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="partnerKey"></param>
        /// <param name="dataCollection"></param>
        /// <returns></returns>
        public static string GetSecuritySign(string partner, string partnerKey, DCMSValueCollection dataCollection = null)
        {
            if (dataCollection == null)
            {
                dataCollection = new DCMSValueCollection();
            }

            if (string.IsNullOrWhiteSpace(partner) || string.IsNullOrWhiteSpace(partnerKey))
            {
                throw new ArgumentNullException();
            }
            var dic = SecuritySignHelper.GetSortedDictionary(dataCollection.Get,
                (k) =>
                {
                    //过滤partner及sign
                    return string.Equals(k, SecuritySignHelper.Partner, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(k, SecuritySignHelper.Sign, StringComparison.OrdinalIgnoreCase);
                });
            dic.Add(SecuritySignHelper.Partner, partner);
            StringBuilder tmp = new StringBuilder();
            SecuritySignHelper.FillStringBuilder(tmp, dic);//将QueryString填入StringBuilder
            dic = SecuritySignHelper.GetSortedDictionary(dataCollection.Post);
            SecuritySignHelper.FillStringBuilder(tmp, dic);//将Form填入StringBuilder
            tmp.Append(partnerKey);//在尾部添加key
            tmp.Remove(0, 1);//移除第一个&
            return tmp.ToString().GetMD5_32();//获取32位长度的Md5摘要
        }


        /// <summary>
        /// 获取防篡改签名
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="partnerKey"></param>
        /// <param name="dataCollection"></param>
        /// <returns></returns>
        public static string GetSecuritySign(string partner, string partnerKey, NameValueCollection dataCollection = null)
        {
            if (dataCollection == null)
            {
                dataCollection = new DCMSValueCollection();
            }

            if (string.IsNullOrWhiteSpace(partner) || string.IsNullOrWhiteSpace(partnerKey))
            {
                throw new ArgumentNullException();
            }
            var dic = SecuritySignHelper.GetSortedDictionary(dataCollection,
                (k) =>
                {
                    //过滤partner及sign
                    return string.Equals(k, SecuritySignHelper.Partner, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(k, SecuritySignHelper.Sign, StringComparison.OrdinalIgnoreCase);
                });
            dic.Add(SecuritySignHelper.Partner, partner);
            StringBuilder tmp = new StringBuilder();
            SecuritySignHelper.FillStringBuilder(tmp, dic);//将QueryString填入StringBuilder
            dic = SecuritySignHelper.GetSortedDictionary(dataCollection);
            SecuritySignHelper.FillStringBuilder(tmp, dic);//将Form填入StringBuilder
            tmp.Append(partnerKey);//在尾部添加key
            tmp.Remove(0, 1);//移除第一个&
            return tmp.ToString().GetMD5_32();//获取32位长度的Md5摘要
        }

        /// <summary>
        /// 获取排序的键值对
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static SortedDictionary<string, string> GetSortedDictionary(NameValueCollection collection, Func<string, bool> filter = null)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            if (collection != null && collection.Count > 0)
            {
                foreach (var k in collection.AllKeys)
                {
                    if (filter == null || !filter(k.ToLower()))
                    {
                        //如果没设置过滤条件或者无需过滤
                        dic.Add(k.ToLower(), collection[k.ToLower()].ToLower());
                    }
                }
            }
            return dic;
        }

        private static void FillStringBuilder(StringBuilder builder, SortedDictionary<string, string> dic)
        {
            foreach (var kv in dic)
            {
                builder.Append('&');
                builder.Append(kv.Key.ToLower());
                builder.Append('=');
                builder.Append(kv.Value.ToLower());
            }//按key顺序组织字符串
        }

        /// <summary>
        /// 获取32位长度的Md5摘要
        /// </summary>
        /// <param name="inputStr"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetMD5_32(this string inputStr, Encoding encoding = null)
        {
            RefEncoding(ref encoding);
            byte[] data = GetMD5(inputStr, encoding);
            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                tmp.Append(data[i].ToString("x2"));
            }
            return tmp.ToString();
        }
        private static byte[] GetMD5(string inputStr, Encoding encoding)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                return md5Hash.ComputeHash(encoding.GetBytes(inputStr));
            }
        }
        private static void RefEncoding(ref Encoding encoding)
        {
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }
        }

        /// <summary>
        /// 构造 URL参数
        /// </summary>
        /// <param name="getCollection"></param>
        /// <param name="partner"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static string SetUrlParams(this DCMSValueCollection dataCollection, string partner, string sign)
        {
            if (dataCollection == null)
            {
                dataCollection = new DCMSValueCollection();
            }

            var getCollection = dataCollection.Get;

            try
            {
                getCollection.Add(SecuritySignHelper.Partner, partner.Trim());
            }
            catch
            {
                getCollection[SecuritySignHelper.Partner] = partner.Trim();
            }
            try
            {
                getCollection.Add(SecuritySignHelper.Sign, sign.Trim());
            }
            catch
            {
                getCollection[SecuritySignHelper.Sign] = sign.Trim();
            }
            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < getCollection.Count; i++)
            {

                if (getCollection[i].ToString().Split(',').Length > 1)
                {
                    string[] cates = getCollection[i].ToString().Split(',');
                    foreach (string cate in cates)
                    {
                        tmp.Append('&');
                        tmp.Append(getCollection.GetKey(i).ToLower());
                        tmp.Append('=');
                        tmp.Append(cate.ToLower());
                    }
                }
                else
                {
                    tmp.Append('&');
                    tmp.Append(getCollection.GetKey(i).ToLower());
                    tmp.Append('=');
                    tmp.Append(getCollection[i].ToLower());
                }
            }
            tmp.Remove(0, 1);
            return tmp.ToString();
        }

        /// <summary>
        /// 构造 URL参数
        /// </summary>
        /// <param name="getCollection"></param>
        /// <param name="partner"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static string SetUrlParams(this NameValueCollection getCollection, string partner, string sign)
        {
            try
            {
                getCollection.Add(SecuritySignHelper.Partner, partner.Trim());
            }
            catch
            {
                getCollection[SecuritySignHelper.Partner] = partner.Trim();
            }
            try
            {
                getCollection.Add(SecuritySignHelper.Sign, sign.Trim());
            }
            catch
            {
                getCollection[SecuritySignHelper.Sign] = sign.Trim();
            }
            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < getCollection.Count; i++)
            {
                tmp.Append('&');
                tmp.Append(getCollection.GetKey(i).ToLower());
                tmp.Append('=');
                tmp.Append(getCollection[i].ToLower());
            }
            tmp.Remove(0, 1);
            return tmp.ToString();
        }

        public static NameValueCollection Add(this NameValueCollection dataCollection, string name, string value = "")
        {
            dataCollection.Add(name, "1");
            return dataCollection;
        }


        public static NameValueCollection ToNameValueCollection1<T>(this T t)
        {
            var nameValueCollection = new NameValueCollection();
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(t))
            {
                string value = propertyDescriptor.GetValue(t).ToString();
                nameValueCollection.Add(propertyDescriptor.Name, value);
            }
            return nameValueCollection;
        }

        public static NameValueCollection ToNameValueCollection<T>(T t)
        {
            var nameValueCollection = new NameValueCollection();
            string tStr = string.Empty;
            if (t == null)
            {
                return new NameValueCollection();
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return new NameValueCollection();
            }

            //Type t = o.GetType();
            PropertyInfo[] pi = t.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();

                if (mi != null && mi.IsPublic)
                {
                    var v = p.GetValue(t);
                    string value = v != null ? v.ToString() : "";
                    nameValueCollection.Add(p.Name.ToLower(), value.ToLower());
                }
            }
            return nameValueCollection;
        }

        public static NameValueCollection ToNameValueCollection(object t)
        {
            var nameValueCollection = new NameValueCollection();
            string tStr = string.Empty;
            if (t == null)
            {
                return new NameValueCollection();
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return new NameValueCollection();
            }

            //Type t = o.GetType();
            PropertyInfo[] pi = t.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();

                if (mi != null && mi.IsPublic)
                {
                    var v = p.GetValue(t);
                    string value = v != null ? v.ToString() : "";
                    nameValueCollection.Add(p.Name.ToLower(), value.ToLower());
                }
            }
            return nameValueCollection;
        }

        public static List<string> GetProperties<T>(T t)
        {
            List<string> ignorParams = new List<string>();
            string tStr = string.Empty;
            if (t == null)
            {
                return new List<string>();
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return new List<string>();
            }
            //foreach (System.Reflection.PropertyInfo item in properties)
            //{
            //    string name = item.Name;
            //    //object value = item.GetValue(t, null);
            //    if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
            //    {
            //        ignorParams.Add(name);
            //    }
            //}

            //Type t = o.GetType();
            PropertyInfo[] pi = t.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();

                if (mi != null && mi.IsPublic)
                {
                    ignorParams.Add(p.Name);
                }
            }

            return ignorParams;
        }

        /// <summary>  
        /// 对未知类型的对象的属性进行递归访问  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="obj"></param>  
        public static List<string> VisitProperties<T>(object obj)
        {
            List<string> ignorParams = new List<string>();
            var type = obj.GetType();
            var paraExpression = Expression.Parameter(typeof(T), "object");
            foreach (var prop in type.GetProperties())
            {
                var propType = prop.PropertyType;
                //判断是否为基本类型或String  
                //访问方式的表达式树为：obj =>obj.Property  
                if (propType.IsPrimitive || propType == typeof(string))
                {
                    VisitProperty<T>(obj, prop, paraExpression, paraExpression, ref ignorParams);
                }
                else
                {
                    //对于访问方式的表达式树为： obj=>obj.otherObj.Property。  
                    //Console.WriteLine("not primitive property: " + prop.Name);
                    var value = prop.GetValue(obj, null);
                    if (value != null && value.ToString() == "1")
                    {
                        ignorParams.Add(prop.Name);
                    }
                    var otherType = prop.PropertyType;
                    MemberExpression memberExpression = Expression.Property(paraExpression, prop);
                    //访问obj.otherObj里的所有公有属性  
                    //foreach (var otherProp in otherType.GetProperties())
                    //{
                    //    VisitProperty<T>(obj, otherProp, memberExpression, paraExpression, ref ignorParams);
                    //}
                }
            }
            return ignorParams;
        }

        /// <summary>  
        /// 执行表达式树为： obj=>obj.Property 或 obj=>obj.otherObj.Property的计算  
        /// </summary>  
        /// <param name="instanceExpression">最终访问属性的obj对象的表达式树的表示</param>  
        /// <param name="parameterExpression">类型T的参数表达式树的表示</param>  
        public static void VisitProperty<T>(object obj, PropertyInfo prop, Expression instanceExpression, ParameterExpression parameterExpression, ref List<string> ignorParams)
        {
            //List<string> ignorParams = new List<string>();
            //Console.WriteLine("property name: " + prop.Name);
            //ignorParams.Add(prop.Name);
            if (prop.PropertyType.IsValueType || prop.PropertyType.Name.StartsWith("String"))
            {
                var value = prop.GetValue(obj, null);
                if (value != null && value.ToString() == "1")
                {
                    ignorParams.Add(prop.Name);
                }
            }
            //MemberExpression memExpression = Expression.Property(instanceExpression, prop);
            //实现类型转换，如将Id的int类型转为object类型，便于下面的通用性  
            //Expression objectExpression = Expression.Convert(memExpression, typeof(object));
            //Expression<Func<T, object>> lambdaExpression = Expression.Lambda<Func<T, object>>(objectExpression, parameterExpression);
            //打印表达式树  
            //Console.WriteLine("expression tree: " + lambdaExpression);
            //Func<T, object> func = lambdaExpression.Compile();
            //Console.WriteLine("value: " + func((T)obj)); //打印出得到的属性值  
            //return ignorParams;
        }
    }
}
