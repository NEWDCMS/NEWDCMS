namespace DCMS.Services.Users
{
    public interface IUserAttributeFormatter
    {
        string FormatAttributes(string attributesXml, string separator = "<br />", bool htmlEncode = true);
    }
}