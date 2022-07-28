using DCMS.Core;
using DCMS.Core.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace DCMS.Services.Users
{
    public interface IUserAssessmentService
    {
        IPagedList<UserAssessment> GetAllUserAssessments(int? store = null, int? year = null, int pageIndex = 0, int pageSize = int.MaxValue);
        UserAssessment GetUserAssessmentById(int userAssessmentId);
        UserAssessment GetUserAssessmentByUserId(int userId);
        UserAssessment GetUserAssessmentByStoreId(int storeId, int year);
        UserAssessmentsItems GetUserAssessmentItemById(int userAssessmentItemId);
        List<UserAssessmentsItems> GetUserAssessmentItems(int storeid,int userAssessmentId, List<int> userids);

        BaseResult UserAssessmentCreateOrUpdate(UserAssessment userAssessment, List<UserAssessmentsItems> userAssessmentItems);
        void InsertUserAssessment(UserAssessment userAssessment);
        void InsertUserAssessmentItem(UserAssessmentsItems userAssessmentItem);
        void InsertUserAssessmentItems(List<UserAssessmentsItems> userAssessmentItems);
        void UpdateUserAssessment(UserAssessment userAssessment);
        void UpdateUserAssessmentItem(UserAssessmentsItems userAssessmentItem);
        void UpdateUserAssessmentItems(List<UserAssessmentsItems> userAssessmentItems);

        void DeleteUserAssessment(int userAssessmentId);
        void DeleteUserAssessmentItem(UserAssessmentsItems userAssessmentItem);
    }
}
