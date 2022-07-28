using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Messages
{
    public partial class EmailAccountService : BaseService, IEmailAccountService
    {
        public EmailAccountService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }

        /// <summary>
        ///  添加账户
        /// </summary>
        /// <param name="emailAccount"></param>
        public virtual void InsertEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
            {
                throw new ArgumentNullException("emailAccount");
            }

            var uow = EmailAccountRepository.UnitOfWork;
            emailAccount.Email = CommonHelper.EnsureNotNull(emailAccount.Email);
            emailAccount.DisplayName = CommonHelper.EnsureNotNull(emailAccount.DisplayName);
            emailAccount.Host = CommonHelper.EnsureNotNull(emailAccount.Host);
            emailAccount.Username = CommonHelper.EnsureNotNull(emailAccount.Username);
            emailAccount.Password = CommonHelper.EnsureNotNull(emailAccount.Password);

            emailAccount.Email = emailAccount.Email.Trim();
            emailAccount.DisplayName = emailAccount.DisplayName.Trim();
            emailAccount.Host = emailAccount.Host.Trim();
            emailAccount.Username = emailAccount.Username.Trim();
            emailAccount.Password = emailAccount.Password.Trim();

            emailAccount.Email = CommonHelper.EnsureMaximumLength(emailAccount.Email, 255);
            emailAccount.DisplayName = CommonHelper.EnsureMaximumLength(emailAccount.DisplayName, 255);
            emailAccount.Host = CommonHelper.EnsureMaximumLength(emailAccount.Host, 255);
            emailAccount.Username = CommonHelper.EnsureMaximumLength(emailAccount.Username, 255);
            emailAccount.Password = CommonHelper.EnsureMaximumLength(emailAccount.Password, 255);

            EmailAccountRepository.Insert(emailAccount);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(emailAccount);
        }

        /// <summary>
        /// 更新账户
        /// </summary>
        /// <param name="emailAccount"></param>
        public virtual void UpdateEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
            {
                throw new ArgumentNullException("emailAccount");
            }

            emailAccount.Email = CommonHelper.EnsureNotNull(emailAccount.Email);
            emailAccount.DisplayName = CommonHelper.EnsureNotNull(emailAccount.DisplayName);
            emailAccount.Host = CommonHelper.EnsureNotNull(emailAccount.Host);
            emailAccount.Username = CommonHelper.EnsureNotNull(emailAccount.Username);
            emailAccount.Password = CommonHelper.EnsureNotNull(emailAccount.Password);

            emailAccount.Email = emailAccount.Email.Trim();
            emailAccount.DisplayName = emailAccount.DisplayName.Trim();
            emailAccount.Host = emailAccount.Host.Trim();
            emailAccount.Username = emailAccount.Username.Trim();
            emailAccount.Password = emailAccount.Password.Trim();

            emailAccount.Email = CommonHelper.EnsureMaximumLength(emailAccount.Email, 255);
            emailAccount.DisplayName = CommonHelper.EnsureMaximumLength(emailAccount.DisplayName, 255);
            emailAccount.Host = CommonHelper.EnsureMaximumLength(emailAccount.Host, 255);
            emailAccount.Username = CommonHelper.EnsureMaximumLength(emailAccount.Username, 255);
            emailAccount.Password = CommonHelper.EnsureMaximumLength(emailAccount.Password, 255);

            try
            {
                var uow = EmailAccountRepository.UnitOfWork;
                EmailAccountRepository.Update(emailAccount);
                uow.SaveChanges();
            }
            catch (Exception)
            {

            }

            //event notification
            _eventPublisher.EntityUpdated(emailAccount);
        }

        /// <summary>
        /// 删除账户
        /// </summary>
        /// <param name="emailAccount"></param>
        public virtual void DeleteEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
            {
                throw new ArgumentNullException("emailAccount");
            }

            if (GetAllEmailAccounts().Count == 0)
            {
                throw new DCMSException("You cannot delete this email account. At least one account is required.");
            }

            var uow = EmailAccountRepository.UnitOfWork;
            EmailAccountRepository.Delete(emailAccount);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(emailAccount);
        }

        /// <summary>
        /// 单笔获取
        /// </summary>
        /// <param name="emailAccountId"></param>
        /// <returns></returns>
        public virtual EmailAccount GetEmailAccountById(int emailAccountId)
        {
            if (emailAccountId == 0)
            {
                return null;
            }

            return EmailAccountRepository.ToCachedGetById(emailAccountId);
        }

        /// <summary>
        /// 获取所有邮箱账号
        /// </summary>
        /// <returns></returns>
        public virtual IList<EmailAccount> GetAllEmailAccounts()
        {
            var query = from ea in EmailAccountRepository.Table
                        orderby ea.Id
                        select ea;
            var emailAccounts = query.ToList();

            return emailAccounts;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="storeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<EmailAccount> SearchEmailAccounts(string username, int? storeId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = EmailAccountRepository.Table;

            //经销商ID
            if (storeId.HasValue)
            {
                query = query.Where(q => q.StoreId == storeId);
            }

            //用户名
            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(q => q.Username.Contains(username.Trim()));
            }

            query = query.OrderBy(q => q.Email);

            var queuedEmails = new PagedList<EmailAccount>(query, pageIndex, pageSize);
            return queuedEmails;
        }

        /// <summary>
        /// 根据经销商Id 获取所有邮箱账号
        /// </summary>
        /// <returns></returns>
        public virtual IList<EmailAccount> GetAllEmailAccounts(int? storeId)
        {
            if (storeId == null || storeId == 0)
            {
                return new List<EmailAccount>();
            }

            var query = from ea in EmailAccountRepository.Table
                        where ea.StoreId == storeId
                        orderby ea.Id
                        select ea;
            var emailAccounts = query.ToList();
            return emailAccounts;
        }


    }
}
