using DCMS.Core.Domain.Users;
using System.Collections.Generic;

namespace DCMS.Services.Users
{
    public interface IUserAttributeService
    {
        void DeleteUserAttribute(UserAttribute userAttribute);
        void DeleteUserAttributeValue(UserAttributeValue userAttributeValue);
        IList<UserAttribute> GetAllUserAttributes();
        UserAttribute GetUserAttributeById(int userAttributeId);
        UserAttributeValue GetUserAttributeValueById(int userAttributeValueId);
        IList<UserAttributeValue> GetUserAttributeValues(int userAttributeId);
        void InsertUserAttribute(UserAttribute userAttribute);
        void InsertUserAttributeValue(UserAttributeValue userAttributeValue);
        void UpdateUserAttribute(UserAttribute userAttribute);
        void UpdateUserAttributeValue(UserAttributeValue userAttributeValue);
    }
}