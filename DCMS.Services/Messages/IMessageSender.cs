namespace DCMS.Services.Tasks
{
    using DCMS.Core.Domain.Tasks;

    public interface IMessageSender
    {
        bool PushAPP<T>(string perfix, string routeKey, string exChangeName, T data, out string msg) where T : class;
        bool PushWeb<T>(string routeKey, string exChangeName, T data, out string msg) where T : class;
        bool SendMessageOrNotity(MessageStructure messageStructure);
    }
}