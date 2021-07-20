using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Message.Model.Enums;
using YQTrack.Backend.Message.Model.Models;
using YQTrack.Backend.Message.RabbitMQHelper;
using YQTrack.Backend.Models;
using YQTrack.Backend.OrderComplete.DTO.Buyer;
using YQTrack.Backend.OrderComplete.Entity.Enume;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrackV6.Common.Utils;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService
{
    /// <summary>
    /// 用户会员过期提醒服务
    /// </summary>
    public class UserMemberExpiresWarnService
    {
        /// <summary>
        /// 订阅产品的固定产品代码
        /// </summary>
        private static readonly string[] SubscriptionSkuCodeArray = { "buyer_ordinary_subscription", "buyer_senior_subscription" };

        #region <私有变量>

        private IBusinessCtrlBLL _businessCtrlBLL;
        private IUserBLL _userBLL;


        #endregion

        public UserMemberExpiresWarnService(int nodeId, int dbno, int tableNo)
        {
            _businessCtrlBLL = FactoryContainer.Create<IBusinessCtrlBLL>();
            _businessCtrlBLL.SetDataRoute(new DataRouteModel
            {
                NodeId = Convert.ToByte(nodeId),
                DbNo = Convert.ToByte(dbno),
                TableNo = Convert.ToByte(tableNo)
            });

            _userBLL = FactoryContainer.Create<IUserBLL>();
            _userBLL.SetDataRoute(new DataRouteModel
            {
                NodeId = Convert.ToByte(nodeId),
                DbNo = Convert.ToByte(dbno),
                TableNo = Convert.ToByte(tableNo)
            });


        }

        /*
         * 按数据库节点和数据库编号，来循环处理当前下用的用户数据
         * */


        /// <summary>
        ///  执行会员7天过期的提醒
        /// </summary>
        public void DoWorkUserMemberGoingToExpiredWarn()
        {

            long userId = 0;
            int pageSize = 100;
            do
            {
                LogHelper.Log(new LogDefinition(LogLevel.Verbose, "DoWorkUserMemberGoingToExpiredWarn:执行会员7天过期的提醒,userId=" + userId));
                var dto = new SearchExpiresMemberInfoDTO() { ExpiresDay = 7, FUserId = userId, StartIndex = 0, PageSize = pageSize };
                var tasklist = _businessCtrlBLL.GetExpiresMemberBusinessCtrl(dto);

                var list = tasklist.Result;
                if (list != null)
                {
                    userId = 0;
                    int index = 1;
                    foreach (var item in list)
                    {
                        if (pageSize == index)
                        {
                            userId = item.FUserId;
                        }
                        try
                        {

                            SendMessageUserMemberGoingToExpiredWarn(item);
                        }
                        catch (Exception ex)
                        {

                            LogHelper.LogObj(new LogDefinition(LogLevel.Error, "DoWorkUserMemberGoingToExpiredWarn:执行会员7天过期的提醒,userId=" + userId), ex, item);
                        }
                        index++;
                    }


                }
            } while (userId > 0);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        private void SendMessageUserMemberGoingToExpiredWarn(ExpiresMemberInfoDTO dto)
        {
            var list = new List<long>() { dto.FUserId };
            var userInfoList = _userBLL.GetUserConfigsByUserId(list);
            var _logBll = FactoryContainer.Create<ILogBll>();
            _logBll.SetDataRoute(new DataRouteModel() { NodeId = _userBLL.Context.DataRoute.NodeId });
            foreach (var item in userInfoList)
            {
                var sendDate = dto.FStopTime.AddDays(-7).ToString("yyyy-MM-dd");
                var isExist = _logBll.ExistWarnLog(item.FUserId, Log_WarnType.UserMemberGoingToExpiredWarn, sendDate);
                if (!isExist)
                {
                    LogHelper.LogObj(new LogDefinition(LogLevel.Verbose, "SendMessageUserMemberGoingToExpiredWarn:执行会员7天过期的提醒"), dto);

                    // 判断是否用户是订阅产品即将到期
                    var isSubscription = SubscriptionSkuCodeArray.Contains(dto.FProductSkuId);

                    new Task(() =>
                    {
                        ////发送邮件通知
                        bool isSuccss = false;

                        //发送 追踪数不足，提醒邮件
                        var messageModel = new MessageModel()
                        {

                            MessageType = isSubscription ? MessageTemplateType.BuyerSubscribeExpiring : MessageTemplateType.UserMemberGoingToExpiredWarn,
                            TemplateData = new
                            {
                                StopTime = dto.FStopTime.ToString("yyyy-MM-dd"),
                                ExpiresDay = dto.ExpiresDay,
                                CanTrackNum = dto.CanTrackNum,
                                NickName = string.IsNullOrWhiteSpace(item.FNickName) ? "" : item.FNickName

                            }.MySerializeToJson(),
                            UserId = new UserInfoExt()
                            {
                                FUserId = item.FUserId,
                                FUserRole = (item.FUserRole),
                                FNickname = item.FNickName,
                                FEmail = item.FEmail,
                                FLanguage = item.FLanguage
                            }
                        };
                        isSuccss = MessageHelper.SendMessage(messageModel);

                        LogHelper.LogObj(new LogDefinition(LogLevel.Debug, "会员即将过期"), new
                        {
                            isSuccss,
                            isSubscription,
                            MessageType = messageModel.MessageType.ToString()
                        });

                        if (isSuccss)
                        {

                            _logBll.AddUserWarnLog(Log_WarnType.UserMemberGoingToExpiredWarn, item.FUserId, item.FNickName, item.FEmail, YQTrackV6.Common.Utils.SerializeExtend.MySerializeToJson(dto));
                        }

                    }).Start();

                }
            }
        }


        /// <summary>
        /// 执行会员已经过期1天的提醒
        /// </summary>
        public void DoWorkUserMemberExpiredWarn()
        {
            long userId = 0;
            do
            {
                LogHelper.Log(new LogDefinition(LogLevel.Verbose, "DoWorkUserMemberExpiredWarn:执行会员已经过期1天的提醒,userId=" + userId));
                var dto = new SearchExpiresMemberInfoDTO() { ExpiresDay = -1, FUserId = userId, StartIndex = 0, PageSize = 100 };
                var tasklist = _businessCtrlBLL.GetExpiresMemberBusinessCtrl(dto);

                var list = tasklist.Result;
                if (list != null)
                {
                    userId = 0;

                    foreach (var item in list)
                    {
                        userId = item.FUserId;
                        try
                        {
                            SendMessageUserMemberExpiredWarn(item);
                        }
                        catch (Exception ex)
                        {

                            LogHelper.LogObj(new LogDefinition(LogLevel.Error, "DoWorkUserMemberExpiredWarn:执行会员已经过期1天的提醒,userId=" + userId), ex, item);
                        }

                    }

                }
            } while (userId > 0);

        }

        private void SendMessageUserMemberExpiredWarn(ExpiresMemberInfoDTO dto)
        {
            var list = new List<long>() { dto.FUserId };
            var userInfoList = _userBLL.GetUserConfigsByUserId(list);
            var _logBll = FactoryContainer.Create<ILogBll>();
            _logBll.SetDataRoute(new DataRouteModel() { NodeId = _userBLL.Context.DataRoute.NodeId });
            foreach (var item in userInfoList)
            {
                var sendDate = dto.FStopTime.ToString("yyyy-MM-dd");
                var isExist = _logBll.ExistWarnLog(item.FUserId, Log_WarnType.UserMemberExpiredWarn, sendDate);
                // 增加判断: 并且不是订阅的情况才发送"你的会员已经过期"邮件
                var isSubscription = SubscriptionSkuCodeArray.Contains(dto.FProductSkuId);
                if (!isExist && isSubscription == false)
                {
                    LogHelper.LogObj(new LogDefinition(LogLevel.Verbose, "SendMessageUserMemberExpiredWarn:执行会员1天过期的提醒"), dto);
                    new Task(() =>
                    {
                        ////发送邮件通知
                        bool isSuccss = false;

                        //发送 追踪数不足，提醒邮件
                        isSuccss = MessageHelper.SendMessage(new MessageModel()
                        {

                            MessageType = MessageTemplateType.UserMemberExpiredWarn,
                            TemplateData = new
                            {
                                StopTime = dto.FStopTime.ToString("yyyy-MM-dd"),
                                CanTrackNum = dto.CanTrackNum,
                                NickName = string.IsNullOrWhiteSpace(item.FNickName) ? "" : item.FNickName

                            }.MySerializeToJson(),
                            UserId = new UserInfoExt()
                            {
                                FUserId = item.FUserId,
                                FUserRole = (item.FUserRole),
                                FNickname = item.FNickName,
                                FEmail = item.FEmail,
                                FLanguage = item.FLanguage
                            }
                        });

                        if (isSuccss)
                        {
                            _logBll.AddUserWarnLog(Log_WarnType.UserMemberExpiredWarn, item.FUserId, item.FNickName, item.FEmail, YQTrackV6.Common.Utils.SerializeExtend.MySerializeToJson(dto));
                        }

                    }).Start();

                }
                else
                {
                    LogHelper.LogObj(new LogDefinition(LogLevel.Debug, "会员过期"), new
                    {
                        isExist,
                        isSubscription
                    });
                }
            }
        }

    }
}
