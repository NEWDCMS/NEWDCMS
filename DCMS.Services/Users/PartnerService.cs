using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Security
{
    public class PartnerService : BaseService, IPartnerService
    {

        private readonly IEncryptionService _encryptionService;


        public PartnerService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IEncryptionService encryptionService) : base(getter, cacheManager, eventPublisher)
        {

            _encryptionService = encryptionService;

        }


        #region (合作者账户)管理表

        public virtual void InsertPartner(Partner partner)
        {
            var uow = PartnersRepository.UnitOfWork;
            PartnersRepository.Insert(partner);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(partner);
        }

        public virtual void UpdatePartner(Partner partner)
        {
            if (partner == null)
            {
                throw new ArgumentNullException("partner");
            }

            var uow = PartnersRepository.UnitOfWork;
            PartnersRepository.Update(partner);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(partner);

        }

        public virtual void DeletePartner(int partnerId)
        {
            Partner partner = GetPartnerById(partnerId);
            if (partner == null)
            {
                throw new ArgumentNullException("partner");
            }

            var uow = PartnersRepository.UnitOfWork;
            PartnersRepository.Delete(partner);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(partner);

        }

        public virtual void DeletePartner(Partner partner)
        {

            if (partner == null)
            {
                throw new ArgumentNullException("partner");
            }

            var uow = PartnersRepository.UnitOfWork;
            PartnersRepository.Delete(partner);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(partner);

        }

        public virtual Partner GetPartnerById(int partnerId)
        {
            if (partnerId == 0)
            {
                return null;
            }

            return PartnersRepository.ToCachedGetById(partnerId);
        }

        public virtual Partner GetPartnerByGUID(Guid guid)
        {
            var key = DCMSDefaults.PARTNER_BY_GUID_KEY.FillCacheKey(guid);
            return _cacheManager.Get(key, () =>
            {
                var query = PartnersRepository.Table;
                var partner = query.Where(a => a.UserGuid == guid.ToString()).FirstOrDefault();
                return partner;
            });
        }

        public virtual List<Partner> GetPartnersByIds(int[] partnerIds)
        {
            if (partnerIds == null || partnerIds.Length == 0)
            {
                return new List<Partner>();
            }

            var query = from c in PartnersRepository.Table
                        where partnerIds.Contains(c.Id)
                        select c;
            var partners = query.ToList();

            var list = new List<Partner>();
            foreach (int id in partnerIds)
            {
                var p = partners.Find(x => x.Id == id);
                if (p != null)
                {
                    list.Add(p);
                }
            }
            return list;
        }

        public virtual Partner GetPartnerByUserNameAndPassWord(string userName, string passWord)
        {
            var query = PartnersRepository.Table;
            var partner = query.Where(a => a.UserName == userName && a.Password == passWord).FirstOrDefault();
            return partner;
        }

        public virtual Partner GetPartnerByUserName(string userName)
        {
            var query = PartnersRepository.Table;
            var partner = query.Where(a => a.UserName == userName).FirstOrDefault();
            return partner;
        }

        public virtual Partner GetPartnerByUserEmail(string email)
        {
            var query = PartnersRepository.Table;
            var partner = query.Where(a => a.Email == email).FirstOrDefault();
            return partner;
        }

        public virtual IPagedList<Partner> GetAllPartners(string username = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = PartnersRepository.Table;

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(a => a.UserName == username);
            }

            query = query.OrderBy(c => c.Id);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Partner>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual PasswordChangeResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var result = new PasswordChangeResult();
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                result.AddError("UserNameOrEmailIsNotProvided");
                return result;
            }
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError("PasswordIsNotProvided");
                return result;
            }

            Partner partner = null;
            if (CommonHelper.IsValidEmail(request.Email))
            {
                partner = GetPartnerByUserEmail(request.Email);
            }

            if (partner == null)
            {
                result.AddError("UserNotFound");
                return result;
            }

            var requestIsValid = false;
            if (request.ValidateRequest)
            {
                //password
                string oldPwd = "";
                oldPwd = _encryptionService.CreatePasswordHash(request.OldPassword, partner.PasswordSalt, "SHA1");

                bool oldPasswordIsValid = oldPwd == partner.Password;
                if (!oldPasswordIsValid)
                {
                    result.AddError("OldPasswordDoesntMatch");
                }

                if (oldPasswordIsValid)
                {
                    requestIsValid = true;
                }
            }
            else
            {
                requestIsValid = true;
            }


            //at this point request is valid
            if (requestIsValid)
            {
                string saltKey = _encryptionService.CreateSaltKey(5);
                partner.PasswordSalt = saltKey;
                partner.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey, "SHA1");
                partner.PasswordFormatId = (int)request.NewPasswordFormat;
                UpdatePartner(partner);
            }

            return result;

        }

        #endregion








    }
}
