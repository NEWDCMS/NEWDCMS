using DCMS.Core;
using DCMS.Core.Domain.Users;
using DCMS.Services.Users;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Security
{
    public interface IPartnerService
    {
        PasswordChangeResult ChangePassword(ChangePasswordRequest request);
        void DeletePartner(int partnerId);
        void DeletePartner(Partner partner);
        IPagedList<Partner> GetAllPartners(string username = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Partner GetPartnerByGUID(Guid guid);
        Partner GetPartnerById(int partnerId);
        Partner GetPartnerByUserEmail(string email);
        Partner GetPartnerByUserName(string userName);
        Partner GetPartnerByUserNameAndPassWord(string userName, string passWord);
        List<Partner> GetPartnersByIds(int[] partnerIds);
        void InsertPartner(Partner partner);
        void UpdatePartner(Partner partner);
    }
}