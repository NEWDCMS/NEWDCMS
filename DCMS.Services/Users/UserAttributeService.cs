using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Users
{
    public partial class UserAttributeService : BaseService, IUserAttributeService
    {

        //private readonly IRepository<UserAttribute> UserAttributeRepository;
        //private readonly IRepository<UserAttributeValue> UserAttributeValueRepository;
        public UserAttributeService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
                IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }



        #region Methods

        public virtual void DeleteUserAttribute(UserAttribute userAttribute)
        {
            if (userAttribute == null)
            {
                throw new ArgumentNullException(nameof(userAttribute));
            }

            var uow = UserAttributeRepository.UnitOfWork;
            UserAttributeRepository.Delete(userAttribute);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(userAttribute);
        }

        public virtual IList<UserAttribute> GetAllUserAttributes()
        {
            return _cacheManager.Get(DCMSUserServiceDefaults.UserAttributesAllCacheKey, () =>
            {
                var query = from ca in UserAttributeRepository.Table
                            orderby ca.DisplayOrder, ca.Id
                            select ca;
                return query.ToList();
            });
        }

        public virtual UserAttribute GetUserAttributeById(int userAttributeId)
        {
            if (userAttributeId == 0)
            {
                return null;
            }
            return UserAttributeRepository.ToCachedGetById(userAttributeId);
        }


        public virtual void InsertUserAttribute(UserAttribute userAttribute)
        {
            if (userAttribute == null)
            {
                throw new ArgumentNullException(nameof(userAttribute));
            }

            var uow = UserAttributeRepository.UnitOfWork;
            UserAttributeRepository.Insert(userAttribute);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(userAttribute);
        }


        public virtual void UpdateUserAttribute(UserAttribute userAttribute)
        {
            if (userAttribute == null)
            {
                throw new ArgumentNullException(nameof(userAttribute));
            }

            var uow = UserAttributeRepository.UnitOfWork;
            UserAttributeRepository.Update(userAttribute);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(userAttribute);
        }


        public virtual void DeleteUserAttributeValue(UserAttributeValue userAttributeValue)
        {
            if (userAttributeValue == null)
            {
                throw new ArgumentNullException(nameof(userAttributeValue));
            }

            var uow = UserAttributeValueRepository.UnitOfWork;
            UserAttributeValueRepository.Delete(userAttributeValue);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(userAttributeValue);
        }

        public virtual IList<UserAttributeValue> GetUserAttributeValues(int userAttributeId)
        {
            var key = DCMSUserServiceDefaults.UserAttributeValuesAllCacheKey.FillCacheKey(userAttributeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from cav in UserAttributeValueRepository.Table
                            orderby cav.DisplayOrder, cav.Id
                            where cav.UserAttributeId == userAttributeId
                            select cav;
                var userAttributeValues = query.ToList();
                return userAttributeValues;
            });
        }

        public virtual UserAttributeValue GetUserAttributeValueById(int userAttributeValueId)
        {
            if (userAttributeValueId == 0)
            {
                return null;
            }


            return UserAttributeValueRepository.ToCachedGetById(userAttributeValueId);
        }


        public virtual void InsertUserAttributeValue(UserAttributeValue userAttributeValue)
        {
            if (userAttributeValue == null)
            {
                throw new ArgumentNullException(nameof(userAttributeValue));
            }

            var uow = UserAttributeValueRepository.UnitOfWork;
            UserAttributeValueRepository.Insert(userAttributeValue);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(userAttributeValue);
        }


        public virtual void UpdateUserAttributeValue(UserAttributeValue userAttributeValue)
        {
            if (userAttributeValue == null)
            {
                throw new ArgumentNullException(nameof(userAttributeValue));
            }

            var uow = UserAttributeValueRepository.UnitOfWork;
            UserAttributeValueRepository.Update(userAttributeValue);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(userAttributeValue);
        }

        #endregion
    }


    public static class UserAttributeExtensions
    {
        public static bool ShouldHaveValues(this UserAttribute customerAttribute)
        {
            if (customerAttribute == null)
            {
                return false;
            }

            if (customerAttribute.AttributeControlType == AttributeControlType.TextBox ||
                customerAttribute.AttributeControlType == AttributeControlType.MultilineTextbox ||
                customerAttribute.AttributeControlType == AttributeControlType.Datepicker ||
                customerAttribute.AttributeControlType == AttributeControlType.FileUpload)
            {
                return false;
            }

            //other attribute control types support values
            return true;
        }
    }
}