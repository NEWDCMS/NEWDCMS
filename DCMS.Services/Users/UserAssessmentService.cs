using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

using DCMS.Services.Caching;

namespace DCMS.Services.Users
{
    public class UserAssessmentService : BaseService, IUserAssessmentService
    {
        private readonly IUserService _userService;

        public UserAssessmentService(IServiceGetter serviceGetter, IStaticCacheManager cacheManager, IEventPublisher eventPublisher, IUserService userService) : base(serviceGetter, cacheManager, eventPublisher)
        {
            _userService = userService;
        }

        public BaseResult UserAssessmentCreateOrUpdate(UserAssessment userAssessment, List<UserAssessmentsItems> userAssessmentItems)
        {
            var uow = SaleBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {
                transaction = uow.BeginOrUseTransaction();

                if (userAssessment.Id == 0)
                {
                    InsertUserAssessment(userAssessment);
                    userAssessmentItems.ForEach(item =>
                    {
                        item.AssessmentId = userAssessment.Id;
                        item.CreatedOnUtc = DateTime.Now;
                        item.StoreId = userAssessment.StoreId;
                        InsertUserAssessmentItem(item);
                    });
                }
                else
                {
                    UpdateUserAssessment(userAssessment);
                    userAssessmentItems.ForEach(item =>
                    {
                        item.StoreId = userAssessment.StoreId;
                        if (item.Id == 0)
                        {
                            item.AssessmentId = userAssessment.Id;
                            item.CreatedOnUtc = DateTime.Now;
                            InsertUserAssessmentItem(item);
                        }
                        else
                        {
                            UpdateUserAssessmentItem(item);
                        }
                    });

                }
                //保存事务
                transaction.Commit();
                return new BaseResult { Success = true, Return = userAssessment.Id, Message = "业绩设置成功", Code = userAssessment.Id };
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "业绩设置失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        public void DeleteUserAssessment(int userAssessmentId)
        {
            throw new NotImplementedException();
        }

        public void DeleteUserAssessmentItem(UserAssessmentsItems userAssessmentItem)
        {
            throw new NotImplementedException();
        }

        public IPagedList<UserAssessment> GetAllUserAssessments(int? store = null, int? year = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = UserAssessmentsRepository.Table;
            
            if (store.HasValue && store.Value != 0)
            {
                query = query.Where(c => c.StoreId == store);
            }
            if (year.HasValue && year.Value != 0)
            {
                query = query.Where(c => c.Year == year);
            }

            var plists =  query.ToList();
            return new PagedList<UserAssessment>(plists, pageIndex, pageSize);
        }

        public UserAssessment GetUserAssessmentById(int userAssessmentId)
        {
            throw new NotImplementedException();
        }

        public UserAssessment GetUserAssessmentByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public UserAssessment GetUserAssessmentByStoreId(int storeId,int year)
        {
            var query = from ua in UserAssessmentsRepository.Table
                        select ua;
            var userAssessment = query.Where(ua => ua.StoreId == storeId && ua.Year == year).FirstOrDefault();
            return userAssessment;
        }

        public UserAssessmentsItems GetUserAssessmentItemById(int userAssessmentItemId)
        {
            throw new NotImplementedException();
        }

        public List<UserAssessmentsItems> GetUserAssessmentItems(int storeid, int userAssessmentId,List<int> userids)
        {
            var query = UserAssessmentsItemsRepository.Table;
            if (storeid > 0)
            {
                query = query.Where(c => c.StoreId == storeid);
            }
            if (userAssessmentId > 0)
            {
                query = query.Where(c => c.AssessmentId == userAssessmentId);
            }
            if (userids.Any())
            {
                query = query.Where(c => userids.Contains(c.UserId));
            }
            return query.ToList();
        }

        public void InsertUserAssessment(UserAssessment userAssessment)
        {
            if (userAssessment == null)
            {
                throw new ArgumentNullException("userAssessment");
            }

            var uow = UserAssessmentsRepository.UnitOfWork;
            UserAssessmentsRepository.Insert(userAssessment);
            uow.SaveChanges();

            //event notification
            //_eventPublisher.EntityInserted(brands);
        }

        public void InsertUserAssessmentItem(UserAssessmentsItems userAssessmentItem)
        {
            if (userAssessmentItem == null)
            {
                throw new ArgumentNullException("userAssessmentItem");
            }

            var uow = UserAssessmentsItemsRepository.UnitOfWork;
            UserAssessmentsItemsRepository.Insert(userAssessmentItem);
            uow.SaveChanges();
        }

        public void InsertUserAssessmentItems(List<UserAssessmentsItems> userAssessmentItems)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserAssessment(UserAssessment userAssessment)
        {
            if (userAssessment == null)
            {
                throw new ArgumentNullException("userAssessment");
            }

            var uow = UserAssessmentsRepository.UnitOfWork;
            UserAssessmentsRepository.Update(userAssessment);
            uow.SaveChanges();

            //通知
            //_eventPublisher.EntityUpdated(saleItem);
        }

        public void UpdateUserAssessmentItem(UserAssessmentsItems userAssessmentItem)
        {
            if (userAssessmentItem == null)
            {
                throw new ArgumentNullException("userAssessmentItem");
            }

            var uow = UserAssessmentsItemsRepository.UnitOfWork;
            UserAssessmentsItemsRepository.Update(userAssessmentItem);
            uow.SaveChanges();

            //通知
            //_eventPublisher.EntityUpdated(saleItem);
        }

        public void UpdateUserAssessmentItems(List<UserAssessmentsItems> userAssessmentItems)
        {
            throw new NotImplementedException();
        }
    }
}
