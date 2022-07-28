using DCMS.Core;
using DCMS.Core.Configuration;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure;
using DCMS.Core.RabbitMQ;
using Newtonsoft.Json;
using System;

namespace DCMS.Services.Tasks
{

    /// <summary>
    /// 用于消息发布服务
    /// </summary>
    public class MessageSender : IMessageSender
    {

        /// <summary>
        /// 发布消息到APP客户端
        /// </summary>
        /// <param name="perfix">类型</param>
        /// <param name="routeKey">路由key</param>  Z'x
        /// <param name="exChangeName">指定交换机</param>
        /// <param name="data">发送数据</param>
        /// <param name="msg">异常返回消息</param>
        /// <returns></returns>
        public bool PushAPP<T>(string perfix, string routeKey, string exChangeName, T data, out string msg) where T : class
        {
            try
            {
                var config = EngineContext.Current.Resolve<DCMSConfig>();
                msg = string.Empty;
                if (!string.IsNullOrEmpty(routeKey) && !string.IsNullOrEmpty(exChangeName) && data != null)
                {
                    var succesed = MQFactory.Use(config).DirectPush<string>(JsonConvert.SerializeObject(data), out msg, exChangeName, perfix, $"{perfix}_{routeKey}".ToLower());
                    return true;
                }
                else
                {
                    msg = "指定参数未实现";
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }


        /// <summary>
        /// 发布消息到Web客户端
        /// </summary>
        /// <param name="routeKey">路由key</param>
        /// <param name="exChangeName">指定交换机</param>
        /// <param name="data">发送数据</param>
        /// <param name="msg">异常返回消息</param>
        /// <returns></returns>
        public bool PushWeb<T>(string routeKey, string exChangeName, T data, out string msg) where T : class
        {
            try
            {
                var config = EngineContext.Current.Resolve<DCMSConfig>();
                msg = string.Empty;
                if (!string.IsNullOrEmpty(routeKey) && !string.IsNullOrEmpty(exChangeName) && data != null)
                {
                    var succesed = MQFactory.Use(config).TopicPublish<T>(data, routeKey, out msg, exChangeName);
                    return true;
                }
                else
                {
                    msg = "指定参数未实现";
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }



        /// <summary>
        /// 发送消息/通知
        /// </summary>
        public bool SendMessageOrNotity(MessageStructure ms)
        {
            string msg = string.Empty;

            try
            {
                bool isMessage = false;
                string title = CommonHelper.GetEnumDescription<MTypeEnum>(ms.MType);
                string billName = CommonHelper.GetEnumDescription<BillTypeEnum>(ms.BillType);
                switch (ms.MType)
                {
                    case MTypeEnum.Message://审批
                        ms.Content = $"{billName} ({ms.BillNumber}) 需要审批，请尽快处理。";
                        isMessage = true;
                        break;
                    case MTypeEnum.Receipt://收款
                        ms.Content = $"{string.Join("，", ms.Terminals)} 等 {ms.Terminals.Count} 家门店欠款近 {ms.Month} 月，累计 {ms.Amount.Value.ToString("#.00")} 元，请尽快收取。";
                        isMessage = true;
                        break;
                    case MTypeEnum.Hold://交账
                        ms.Content = $"你有{ms.Amount.Value.ToString("#.00")} 元需要交账，请尽快处理。";
                        isMessage = true;
                        break;
                    case MTypeEnum.Audited://审核完成
                        ms.Content = $"你保存的 {billName} ({ms.BillNumber}) 已经审核通过，请留意。";
                        break;
                    case MTypeEnum.Scheduled://调度完成
                        ms.Content = $"你保存的 {billName} ({ms.BillNumber}) 已经调度完成，请留意。";
                        break;
                    case MTypeEnum.InventoryCompleted://盘点完成
                        ms.Content = $"{billName} ({ms.BillNumber}) 已经盘点完成，请留意。";
                        break;
                    case MTypeEnum.TransferCompleted://转单/签收完成
                        ms.Content = $"{billName} ({ms.BillNumber}) 转单/签收完成，请留意。";
                        break;
                    case MTypeEnum.InventoryWarning://库存预警
                        ms.Content = $"{string.Join("，", ms.Products)}  等 {ms.Products.Count}件商品缺货，请尽快处理。";
                        break;
                    case MTypeEnum.CheckException://签到异常
                        ms.Content = $"{ms.BusinessUser}在拜访{ms.TerminalName}时，距离{ms.Distance}米签到，请留意。";
                        break;
                    case MTypeEnum.LostWarning://客户流失预警
                        ms.Content = $"你有{ms.Terminals.Count}个客户可能流失，请关注。";
                        break;
                    case MTypeEnum.LedgerWarning://开单异常
                        ms.Content = $"{ms.BusinessUser} 在交账后，开具单据({ms.BillNumber})，请留意。";
                        break;
                    case MTypeEnum.Paymented://交账完成/撤销
                        ms.Content = $"于{ms.Date.ToString("yyyy年MM月dd日hh时MM分")}，你已上交钱款{ms.Amount.Value.ToString("#.00")}，包含单据： ({string.Join("，", ms.Bills)})， 请留意。";
                        break;
                    default:
                        break;
                }

                if (isMessage)
                {
                    //发送消息到APP
                    PushAPP("message", ms.ToUser, "direct_mq", ms, out msg);
                    //发送消息到Web
                    PushWeb(ms.ToUser, "amq.topic", ms, out msg);
                }
                else
                {
                    //发送通知到APP
                    PushAPP("notice", ms.ToUser, "direct_mq", ms, out msg);
                    //发送通知到Web
                    PushWeb(ms.ToUser, "amq.topic", ms, out msg);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
