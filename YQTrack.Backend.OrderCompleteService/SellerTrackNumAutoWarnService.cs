using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YQTrack.Backend.Enums;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Message.Model.Enums;
using YQTrack.Backend.Message.Model.Models;
using YQTrack.Backend.Message.RabbitMQHelper;
using YQTrack.Backend.Models;
using YQTrack.Backend.OrderComplete.DTO;
using YQTrack.Backend.OrderComplete.Entity.Enume;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrackV6.Common.Utils;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService
{
    /// <summary>
    /// Seller 购买了查询数的。查询数不足10%的提醒
    /// </summary>
    public class SellerTrackNumAutoWarnService
    {

        #region <私有变量>

        private readonly IOrderAutoWarnBLL _orderAutoWarnBll;
        private readonly IUserBLL _userBLL;
        /// <summary>
        /// 消息日志业务对象
        /// </summary>
        private readonly ILogBll _logBll;

        private readonly int _nodeId = 1;
        private readonly int _dbno = 0;
        private int _tableNo = 0;

        #endregion

        public SellerTrackNumAutoWarnService(int nodeId, int dbno, int tableNo)
        {
            _nodeId = nodeId;
            _dbno = dbno;
            _tableNo = tableNo;

            _orderAutoWarnBll = FactoryContainer.Create<IOrderAutoWarnBLL>();
            _orderAutoWarnBll.SetDataRoute(new DataRouteModel
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

            _logBll = FactoryContainer.Create<ILogBll>();
            _logBll.SetDataRoute(_userBLL.Context.DataRoute);
        }

        /*
         * 按数据库节点和数据库编号，来循环处理当前下用的用户数据
         * */


        /// <summary>
        ///  执行Seller查询数不足10%的提醒任务
        /// </summary>
        public void DoWorkSellerPayTrackNumWarn()
        {

            long userId = 0;
            int pageSize = 100;
            try
            {
                do
                {
                    LogHelper.Log(new LogDefinition(LogLevel.Verbose, "DoWorkUserMemberGoingToExpiredWarn:执行会员7天过期的提醒,userId=" + userId));
                    var dto = new SellerpayCtrlTrackNumParamDTO() { FUserId = userId, StartIndex = 0, PageSize = pageSize };
                    var tasklist = _orderAutoWarnBll.GetSellerPayCtrlTrackNUmList(dto);

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
                            SendSellerPayTrackNumEmailWarn(item);
                            index++;
                        }


                    }
                } while (userId > 0);
            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Error, "DoWorkSellerPayTrackNumWarn.额度不足提醒发送一簇,userId=" + userId), ex);
            }
           

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        private void SendSellerPayTrackNumEmailWarn(SellerPayCtrlTrackNumDTO dto)
        {
            var list = new List<long>() { dto.FUserId };
            var dtoUserConfig = new UserProfileConfigPageDTO
            {
                NodeId = _nodeId,
                DBNo = _dbno,
                StartIndex = 0,
                PageSize = 100
            };

            dtoUserConfig.UserRoles = new List<int>() { (int)UserRoleType.Seller };
            dtoUserConfig.FUserId = dto.FUserId;

            var userInfoListTask = _userBLL.GetUserConfigsByPage(dtoUserConfig);
            var userInfoList = userInfoListTask.Result;

            foreach (var item in userInfoList)
            {

                if (item.WranConfig.IsWarnNum)  //用户是否配置了发送
                {
                    //最大的过期时间，是否发送过。没有就重新发送。
                    var sendDate = dto.FMaxStopTime.ToString("yyyy-MM-dd");
                    var isExist = _logBll.ExistWarnLog(item.FUserId, Log_WarnType.OrderTrackNumWarn, sendDate);
                    if (!isExist)
                    {
                        double warnPreNum = (double)dto.FUsedCount / (double)dto.FServiceCount;
                        double supwarnPreNum = ((double)100 - Math.Round(warnPreNum * 100, 2));  //剩余百分比
                        LogHelper.Log(new LogDefinition(LogLevel.Info, string.Format("SendSellerPayTrackNumEmailWarn 执行任务,userId={1},supwarnPreNum={0}", supwarnPreNum, item.FUserId)));
                        if (supwarnPreNum < (double)item.WranConfig.NumWarnPer)
                        {

                            LogHelper.LogObj(new LogDefinition(LogLevel.Verbose, "SendSellerPayTrackNumEmailWarn:执行单号数量提醒提醒"), dto);
                            new Task(() =>
                            {
                                ////发送邮件通知
                                bool isSuccss = false;

                                //发送 追踪数不足，提醒邮件
                                isSuccss = MessageHelper.SendMessage(new MessageModel()
                                {
                                    MessageType = MessageTemplateType.SellerLimitInsufficient,
                                    TemplateData = new
                                    {
                                        NumWarnPer = item.WranConfig.NumWarnPer,
                                        ServiceCount = dto.FServiceCount,
                                        UsedCount = dto.FUsedCount,
                                        NickName = string.IsNullOrWhiteSpace(item.FNickName) ? "" : item.FNickName,
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
                                    _logBll.AddUserWarnLog(Log_WarnType.OrderTrackNumWarn, item.FUserId, item.FNickName, item.FEmail, sendDate, YQTrackV6.Common.Utils.SerializeExtend.MySerializeToJson(dto));
                                }

                            }).Start();
                        }
                    }
                }
            }
        }


        public void DoWorkSellerPayEmailNumWarn()
        {
            long userId = 0;
            int pageSize = 100;
            try
            {
                do
                {
                    var dto = new SellerpayCtrlTrackNumParamDTO() { FUserId = userId, StartIndex = 0, PageSize = pageSize };
                    var list = _orderAutoWarnBll.GetSellerPayCtrlEmailNUmList(dto).Result;
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
                            SendSellerPayEmailNumEmailWarn(item);
                            index++;
                        }
                    }
                } while (userId > 0);
            }
            catch (Exception ex)
            {
                LogHelper.LogObj(new LogDefinition(LogLevel.Error, "DoWorkSellerPayEmailNumWarn.邮件额度不足提醒发生异常，userId=" + userId), ex);
            }
        }

        private void SendSellerPayEmailNumEmailWarn(SellerPayCtrlTrackNumDTO dto)
        {
            var dtoUserConfig = new UserProfileConfigPageDTO
            {
                NodeId = _nodeId,
                DBNo = _dbno,
                StartIndex = 0,
                PageSize = 100,
                UserRoles = new List<int> { (int)UserRoleType.Seller },
                FUserId = dto.FUserId
            };


            var userInfoListTask = _userBLL.GetUserConfigsByPage(dtoUserConfig);
            var userInfoList = userInfoListTask.Result;

            foreach (var item in userInfoList)
            {

                //if (item.WranConfig.IsWarnNum)  //用户是否配置了发送
                //{
                //最大的过期时间，是否发送过。没有就重新发送。
                var sendDate = dto.FMaxStopTime.ToString("yyyy-MM-dd");
                var isExist = _logBll.ExistWarnLog(item.FUserId, Log_WarnType.OrderTrackEmailWarn, sendDate);
                if (!isExist)
                {
                    double warnPreNum = (double)dto.FUsedCount / (double)dto.FServiceCount;
                    double supwarnPreNum = ((double)100 - Math.Round(warnPreNum * 100, 2));  //剩余百分比
                    LogHelper.Log(new LogDefinition(LogLevel.Info, string.Format("SendSellerPayEmailNumEmailWarn 执行任务,userId={1},supwarnPreNum={0}", supwarnPreNum, item.FUserId)));
                    // todo: 暂定 10% 定时的值 (因为定需求的时候考虑掉了邮件预警配置相关)
                    //if (supwarnPreNum < (double)item.WranConfig.NumWarnPer)
                    if (supwarnPreNum < 10)
                    {
                        LogHelper.LogObj(new LogDefinition(LogLevel.Verbose, "SendSellerPayEmailNumEmailWarn:执行单号数量提醒提醒"), dto);
                        new Task(() =>
                        {
                            ////发送邮件通知
                            bool isSuccess = false;
                            //发送 邮件数不足，提醒邮件
                            isSuccess = MessageHelper.SendMessage(new MessageModel()
                            {
                                // 修改为邮件数预警提醒模板
                                MessageType = MessageTemplateType.SellerQuotaInsufficient,
                                TemplateData = new
                                {
                                    NickName = item.FNickName,
                                    Proportion = "10%"
                                }.MySerializeToJson(),
                                UserId = new UserInfoExt()
                                {
                                    FUserId = item.FUserId,
                                    FUserRole = item.FUserRole,
                                    FNickname = item.FNickName,
                                    FEmail = item.FEmail,
                                    FLanguage = item.FLanguage
                                }
                            });
                            if (isSuccess)
                            {
                                _logBll.AddUserWarnLog(Log_WarnType.OrderTrackEmailWarn, item.FUserId, item.FNickName, item.FEmail, sendDate, dto.MySerializeToJson());
                            }
                        }).Start();
                    }
                }
                //}
            }
        }
    }
}
