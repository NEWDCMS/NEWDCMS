using DCMS.Core.Domain.Users;
using System.Collections.Generic;

namespace DCMS.Services.Users
{
    public interface IUserAttributeParser
    {
        string AddUserAttribute(string attributesXml, UserAttribute ca, string value);
        IList<string> GetAttributeWarnings(string attributesXml);
        IList<UserAttribute> ParseUserAttributes(string attributesXml);
        IList<UserAttributeValue> ParseUserAttributeValues(string attributesXml);
        IList<string> ParseValues(string attributesXml, int userAttributeId);
    }
}