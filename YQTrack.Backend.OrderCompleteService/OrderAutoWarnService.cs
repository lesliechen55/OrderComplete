using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using YQTrack.Backend.OrderComplete.IService;
using YQTrackV6.Common.Utils;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService
{

    /// <summary>
    ///     订单完成服务
    ///     订单标记已完成，要查询缓存中订单最后的事件，更新到数据库中（2016-04-13）
    /// </summary>
    public class OrderAutoWarnService : IOrderAutoWarnService,IDisposable
    {
        #region <构造方法>

        public OrderAutoWarnService(IUserBLL userbll)
        {
            _userBll = userbll;
            _QueueErrorUserComplete = new ConcurrentQueue<UserProfileConfigDTO>();
        }

        /// <summary>
        /// 设置路由信息
        /// </summary>
        /// <param name="model"></param>
        public void SetDataRoute(DataRouteModel model)
        {
            _userBll.SetDataRoute(model);
        }

        #endregion

        //处理订单自动完成逻辑处理
        //指定哪个用户数据库
        //查询出用户，就知道对应NodeId，DBNo，TableNo

        #region <私有变量>

        /// <summary>
        /// 消息日志业务对象
        /// </summary>
        private ILogBll _LogBll
        {
            get
            {
                var LogBll = FactoryContainer.Create<ILogBll>();
                LogBll.SetDataRoute(_userBll.Context.DataRoute);
                return LogBll;
            }
        }
        private ConcurrentQueue<UserProfileConfigDTO> _QueueErrorUserComplete;

        private Semaphore _sem = null;
        //Semaphore semaphoreArchived = new Semaphore(10, 10);

        ConcurrentBag<OrderAutoCompleteDTO> _cbag = new ConcurrentBag<OrderAutoCompleteDTO>();


        private readonly IUserBLL _userBll;

        private string _lastUpdateTime = string.Empty;

        /// <summary>
        ///     数据库节点,可以控制当前只查询那个数据库节点下的用户数据
        /// </summary>
        private int _NodeId = -1;

        /// <summary>
        ///     数据库编号
        /// </summary>
        private int _DBNo = -1;


        /// <summary>
        ///     用户分页大小
        /// </summary>
        private int _UserPageSize = 100;

        /// <summary>
        ///     查询用户开始位置
        /// </summary>
        private int _UserStartIndex;

        /// <summary>
        ///     查询用户的结束位置
        /// </summary>
        private int _UserEndIndex;

        /// <summary>
        ///     当前用户索引
        /// </summary>
        private int _CurrentUserIndex;

        /// <summary>
        ///     当前连接数
        /// </summary>
        private int _CurrentConnectionCount;

        /// <summary>
        ///     最大连接数
        /// </summary>
        private int _MaxConnectionCount = 100;

        /// <summary>
        ///     是否需要控制连接数
        /// </summary>
        private bool _IsConnectionCount;

        /// <summary>
        ///     循环等待秒数
        /// </summary>
        private int _LoopWaitSeconds = 60;

        /// <summary>
        ///     是否循环用户
        /// </summary>
        private bool _IsLoopUser = true;

        private int _MsgRepeatInsertDays = 3;

        private int _UserCount = 1;

        private int _Semaphore = 10;
        //private int _SemaphoreArchived = 10;

        private int _OrderPageSize = 100;

        #endregion


        #region  <事件>

        /// <summary>
        ///  发送当前订单数量信息
        /// </summary>
        //public event EventHandler<OrderCompleteMsgEventArgs> SendOrderCompleteMsgEvent;


        #endregion

        #region  <循环获取用户处理订单>

        private void InitData(int nodeId, int dbno, int pageSize, int startIndex, int endIndex, bool isConnectionCount,
            int maxConnectionCount, int loopWaitSeconds, int msgRepeatInsertDays, int orderPageSize, int semaphoreCount)
        {
            _NodeId = nodeId;
            _DBNo = dbno;
            _UserPageSize = pageSize;
            _UserStartIndex = startIndex;
            _UserEndIndex = endIndex;
            _IsConnectionCount = isConnectionCount;
            _MaxConnectionCount = maxConnectionCount;
            _LoopWaitSeconds = loopWaitSeconds;
            _MsgRepeatInsertDays = msgRepeatInsertDays;
            _OrderPageSize = orderPageSize;
            _Semaphore = semaphoreCount;


            _IsLoopUser = true;
            _UserCount = 1;
            _CurrentUserIndex = 0;
            _CurrentConnectionCount = 0;

            _sem = new Semaphore(_Semaphore, _Semaphore);
        }

       
        /// <summary>
        ///     按分页数PageSize获取用户数据
        /// </summary>
        /// <returns></returns>
        private List<UserProfileConfigDTO> GetUserConfigsByPage(bool isSeller = false)
        {
            List<UserProfileConfigDTO> list = null;


            //为0时，服务用户开始索引为当前用户查询索引
            if (_UserStartIndex > 0 && _CurrentUserIndex == 0)
            {
                _CurrentUserIndex = _UserStartIndex;
            }
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "GetUserConfigsByPage 工作,_NodeId={0},DbNo={3},_CurrentUserIndex={1},_UserPageSize={2},endIndex={4}"), _NodeId, _CurrentUserIndex, _UserPageSize, _DBNo, _UserEndIndex);
            var index = _UserPageSize;
            if (_UserEndIndex > 0 && (_UserPageSize > (_UserEndIndex - _CurrentUserIndex)))
                index = _UserEndIndex - _CurrentUserIndex;

            var dto = new UserProfileConfigPageDTO
            {
                NodeId = _NodeId,
                DBNo = _DBNo,
                StartIndex = _CurrentUserIndex,
                PageSize = index
            };
            //是否指定卖家用户操作
            if (isSeller)
            {
                dto.UserRoles = new List<int>() { (int)UserRoleType.Seller };
            }
            //假如获取用户信息失败，需要重试
            int loopCount = 5;
            do
            {
                try
                {
                    var taskResult = _userBll.GetUserConfigsByPage(dto);
                    list = taskResult.Result as List<UserProfileConfigDTO>;
                    if (list != null && list.Count > 0)
                    {
                        break;
                    }
                }
                catch// (Exception ex)
                {
                    LogHelper.Log(new LogDefinition(LogLevel.Warn, "nodeId={1},DbNo={2}获取用户重试{0}"), loopCount, _NodeId, _DBNo);
                    if (loopCount == 0)
                    {
                        LogHelper.Log(new LogDefinition(LogLevel.Error, "nodeId={1},DbNo={2}获取用户重试五次失败-{0}"), loopCount, _NodeId, _DBNo);
                    }
                }
                loopCount--;
            } while (loopCount > 0);

            if (list == null)
            {
                list = new List<UserProfileConfigDTO>();
            }
            var count = list.Count;
            //查询数与Pagesize相同，表示后面还有数据，可以继续查询
            if (count == _UserPageSize)
            {
                //控制结束索引，判断当前的查询索引是否大于结束索引。
                _CurrentUserIndex = _CurrentUserIndex + _UserPageSize;
                if (_UserEndIndex > 0 && _CurrentUserIndex >= _UserEndIndex)
                {
                    _IsLoopUser = false;
                }
            }
            else
            {
                _CurrentUserIndex = _CurrentUserIndex + count;
                _IsLoopUser = false;
            }
            return list;
        }

      

        #region < 事件回调方法 >
       

        /// <summary>
        ///     更新用户缓存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OrderCompleteBll_UpdateUserCacheTrackNum(object sender, OrderAutoCompleteEventArgs e)
        {

            var dto = e.AutoCompleteResult;
            if (dto != null)
            {

            }
        }

        /// <summary>
        /// 追踪数不足，发公告通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OrderCompleteBll_TrackNumLackNotify(object sender, OrderAutoCompleteEventArgs e)
        {


            var dto = e.AutoCompleteResult;
            if (dto != null)
            {

            }
        }

        #endregion


        #endregion



        public bool IsOldSendEmail(long userId)
        {
            bool isOk = false;
            var molNum = userId % 10;
            //if (molNum >= 7)
            //{
            //    isOk = true;
            // }
            //else
            //{
            LogHelper.Log(new LogDefinition(LogLevel.Info, "IsOldSendEmail.MessageSend.userId={0},molNum={1}"), userId, molNum);
            //}
            return isOk;
        }

        /// <summary>
        ///     处理用户跟踪数变化不带10%通知用户
        /// </summary>
        /// <param name="item">用户配置信息</param>
        /// <returns></returns>
        public bool HandleUserTrackNumNotifyCore(UserProfileConfigDTO item, OrderAutoCompleteDTO dto, bool isSend = false)
        {

            var isSuccess = false;
            if (item == null || dto == null || item.WranConfig == null)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Error, string.Format("HandleUserTrackNumNotifyCore 方法参数为null,item={0},dto={1}", item, dto)));
                return isSuccess;
            }
            //LogHelper.Log(new LogDefinition(LogLevel.Info, string.Format("OrderCompleteAppService.HandleUserTrackNumNotifyCore 执行任务 userId={0}", item.FUserId)));
            if (item.WranConfig.IsWarnNum)
            {
                try
                {
                    if (dto != null && dto.CanTrackNum > 0)
                    {
                        double warnPreNum = (double)dto.UseTrackNum / (double)dto.CanTrackNum;
                        double supwarnPreNum = ((double)100 - Math.Round(warnPreNum * 100, 2));  //剩余百分比
                        LogHelper.Log(new LogDefinition(LogLevel.Info, string.Format("HandleUserTrackNumNotifyCore 执行任务,userId={1},supwarnPreNum={0}", supwarnPreNum, item.FUserId)));
                        if (supwarnPreNum <= (double)item.WranConfig.NumWarnPer)
                        {
                            string sendDate = DateTime.Now.AddDays(0 - 3).ToString("yyyy-MM-dd");
                            //判断间隔时间，是否发送过邮件提醒
                            if (!isSend)
                            {
                                var isExist = _LogBll.ExistWarnLog(item.FUserId, Log_WarnType.OrderTrackNumWarn, sendDate);
                                if (!isExist)
                                {
                                    isSend = true;
                                }
                            }

                            if (isSend)
                            {
                                new Task(() =>
                                {
                                    ////发送邮件通知
                                    bool isSuccss = false;

                                    //发送 追踪数不足，提醒邮件
                                    isSuccss = MessageHelper.SendMessage(new EmailMessageModel()
                                    {
                                        EmailChannel = item.FUserRole == UserRoleType.Seller ? Backend.Enums.EnumChannel.Seller : Backend.Enums.EnumChannel.Buyer,
                                        MessageType = MessageTemplateType.CanTrackNum,
                                        TemplateData = new
                                        {
                                            CanTrackNum = dto.CanTrackNum,
                                            UseTrackNum = dto.UseTrackNum,
                                            UserRole = item.FUserRole == UserRoleType.Seller ? "seller" : "buyer"
                                        }.MySerializeToJson(),
                                        UserId = new UserInfoExt()
                                        {
                                            FUserId = item.FUserId,
                                            FUserRole = ((Backend.Enums.UserRoleType)(int)item.FUserRole),
                                            FNickname = item.FNickName,
                                            FEmail = item.FEmail,
                                            FLanguage = item.FLanguage
                                        }
                                    });

                                    if (isSuccss)
                                    {
                                        _LogBll.AddUserWarnLog(Log_WarnType.OrderTrackNumWarn, item.FUserId, item.FNickName, item.FEmail, YQTrackV6.Common.Utils.SerializeExtend.MySerializeToJson(dto));
                                    }

                                }).Start();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log(
                        new LogDefinition(LogLevel.Error,
                            string.Format(
                                "HandleUserTrackNumNotifyCore 处理用户追踪数不足10%的通知，发生异常,userId={0}",
                                item.FUserId)), ex);
                }
            }
            return isSuccess;
        }

        public void Dispose()
        {
            
        }



        /// <summary>
        ///   处理订阅了自动完成统计数量服务的用户，发送邮件操作
        /// </summary>
        /// <param name="nodeId">数据库节点Id</param>
        /// <param name="dbno">数据库Id</param>
        /// <param name="pageSize">用户分页大小</param>
        /// <param name="startIndex">开始查询索引位置</param>
        /// <param name="endIndex">结束查询索引位置</param>
        /// <param name="isConnectionCount">是否控制连接数</param>
        /// <param name="maxConnectionCount">最大运行连接数</param>
        /// <param name="loopWaitSeconds">等待执行时间秒</param>
        /// <param name="orderPageSize">订单每次返回个数</param>
        /// <param name="semaphoreCount">线程数量</param>
        public void DoWorkCompleteRecordCountNotify(int nodeId = 1, int dbno = 1, int pageSize = 100, int startIndex = 0, int endIndex = 0,
            bool isConnectionCount = false, int maxConnectionCount = 100, int loopWaitSeconds = 30,
            int msgRepeatInsertDays = 3, int orderPageSize = 100, int semaphoreCount = 10)
        {
            InitData(nodeId, dbno, pageSize, startIndex, endIndex, isConnectionCount, maxConnectionCount,
                loopWaitSeconds, msgRepeatInsertDays, orderPageSize, semaphoreCount);
            var loopCount = 0;

            while (_IsLoopUser)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Verbose, "DoWorkCompleteRecordCountNotify 工作,loopCount={0}"), loopCount);
                var list = GetUserConfigsByPage(true);
                ExecuteSendEmailToUserAsync(list);
                loopCount++;
            }
        }


        /// <summary>
        ///     订单完成数统计，消息通知
        /// </summary>
        /// <param name="userList">所有用户配置信息集合</param>
        private int ExecuteSendEmailToUserAsync(IEnumerable<UserProfileConfigDTO> userList)
        {
            var result = 0;
            if (userList != null && userList.Count() > 0)
            {
                //LogHelper.Log(
                //    new LogDefinition(LogLevel.Verbose, "ExecuteAutoCompleteAsync 订单自动完成处理,userCount={0},当前线程ID={0}"),
                //    userList.Count(), Thread.CurrentThread.ManagedThreadId.ToString());
                try
                {
                    userList = userList.Where(a => a.FUserRole == UserRoleType.Buyer || a.FUserRole == UserRoleType.Seller);
                    result = 1;
                    foreach (var item in userList)
                    {
                        if (_IsConnectionCount)
                        {
                            if (_CurrentConnectionCount > _MaxConnectionCount)
                            {
                                LogHelper.Log(
                                    new LogDefinition(LogLevel.Verbose, "ExecuteSendEmailToUserAsync 控制数已满,_CurrentConnectionCount={0}，当前线程ID={1}"),
                                    _CurrentConnectionCount, Thread.CurrentThread.ManagedThreadId.ToString());
                                Thread.Sleep(_LoopWaitSeconds * 1000);
                                _CurrentConnectionCount = 0;
                                _UserCount = 1;
                            }
                        }
                        _sem.WaitOne();
                        //Task.Run(() =>
                        //{
                            //是否开启自动完成提醒
                            if (item.FUserRole == UserRoleType.Seller && item.WranConfig.IsWarnComplete && !_LogBll.ExistWarnLog(item.FUserId, Log_WarnType.ChangeOrderStatus, DateTime.UtcNow.ToString("yyyy-MM-dd")))
                            {
                                var notifyresult = HandleSellerWarnNotify(item);

                            }

                        //////是否提醒点数不足
                        ////if (item.FUserRole == UserRoleType.Seller && item.WranConfig.IsWarnNum)
                        ////{
                        ////    var notifyresult = HandleUserTrackNumNotify(item);
                        ////    var result1 = notifyresult.Result;
                        ////}

                        ////是否开启包裹状态提醒
                        //if (item.FUserRole == UserRoleType.Seller && !_LogBll.ExistWarnLog(item.FUserId, Enums.UserCenter.Log_WarnType.ChangeOrderStatus, DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd")))
                        //{
                        //    var notifyresult = HandlePackageStateCountNotify(item);
                        //}

                        ////买家包裹状态提醒
                        //if (item.FUserRole == UserRoleType.Buyer)
                        //{
                        //    var notifyresult = HandleBuyyerPackageStateNotify(item);
                        //    var result2 = notifyresult.Result;
                        //}
                        _sem.Release();
                       // });

                        _CurrentConnectionCount = _UserCount * 4;
                        _UserCount++;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log(new LogDefinition(LogLevel.Error, "消息通知,订单完成数统计,ExecuteSendEmailToUserAsync,userCount={0}"), ex,
                        userList.Count());
                }
            }
            return result;
        }


        /// <summary>
        ///     卖家最近几天包括状态统计
        /// </summary>
        /// <param name="item">用户配置信息</param>
        /// <returns></returns>
        public async Task<bool> HandleSellerWarnNotify(UserProfileConfigDTO item, DateTime? currentDate = null)
        {
            var isSuccess = false;

            if (item.WranConfig.IsWarnComplete)
            {
                try
                {
                    var autoWarnBLL = FactoryContainer.Create<IOrderAutoWarnBLL>();
                    autoWarnBLL.SetDataRoute(new DataRouteModel
                    {
                        UserRole = (byte)item.FUserRole,
                        NodeId = Convert.ToByte(item.FNodeId),
                        DbNo = Convert.ToByte(item.FDbNo),
                        TableNo = Convert.ToByte(item.FTableNo)
                    });

                    if (!currentDate.HasValue)
                    {
                        currentDate = DateTime.UtcNow.AddDays(-1).Date;
                    }


                    var result = await autoWarnBLL.GetOrderAutoCompleteCount(item.FUserId, currentDate.Value);
                    var list = result as List<OrderAutoCompleteCountDTO>;
                    OrderAutoCompleteCountDTO completeCountModel = null;
                    var isSendCompleteCountWarn = false;
                    //LogHelper.Log(new LogDefinition(LogLevel.Info, "HandleUserOrderCompleteCountNotify 订单完成数量统计 ,item={0},当前线程ID={1},count={2}"), item.ToJson(), Thread.CurrentThread.ManagedThreadId.ToString(), list.Count());
                    if (list != null && list.Count() > 0)
                    {
                        completeCountModel = list[0];
                        isSendCompleteCountWarn = true;

                    }
                    else
                    {
                        completeCountModel = new OrderAutoCompleteCountDTO();
                    }

                  

                    bool isPackageStateSend = false;
                    var model_A = new PackageStateCountDTO();

                    var dtoStatCondition = new StatConditionDTO();
                    dtoStatCondition.UserId = item.FUserId;
                    dtoStatCondition.LastDays = 1; //最近几天的最新事件
                    var resultdtoStatCondition = await autoWarnBLL.GetPackageStateCount(dtoStatCondition);
                    var listdtoStatCondition = resultdtoStatCondition as List<PackageStateCountDTO>;
                    if (listdtoStatCondition != null && listdtoStatCondition.Count() > 0)
                    {
                        var statItem = listdtoStatCondition[0];
                        if (!(listdtoStatCondition.Count == 1 && (statItem.None + statItem.Shipment + statItem.NoticeLeft + statItem.DeliveryFailure + statItem.Delivered + statItem.Warn + statItem.Returned) <= 0))
                        {
                            isPackageStateSend = true;
                            model_A = statItem;
                        }
                    }

                    //没有数据，没必要发送邮件
                    if (!isPackageStateSend && !isSendCompleteCountWarn)
                    {
                        LogHelper.Log(new LogDefinition(LogLevel.Info, "HandleSellerWarnNotify 没有统计到数据，不发送邮件 ,item={0},当前线程ID={1}"), SerializeExtend.MySerializeToJson(item), Thread.CurrentThread.ManagedThreadId.ToString());
                        return true;
                    }

                    // // TODO: ClickHere 已删除 model_A.Warn_ClickHere = LanguageWcf.GetText(item.FLanguage, LanguageHelper.LanguageType.UCenterMail, ResUCenterMail.__common_clickHere);


                    isSuccess = MessageHelper.SendMessage(new EmailMessageModel()
                    {
                        EmailChannel = item.FUserRole == UserRoleType.Seller ? Backend.Enums.EnumChannel.Seller : Backend.Enums.EnumChannel.Buyer,
                        MessageType = MessageTemplateType.EmailPackageInformation,
                        //TemplateData = model_A.MySerializeToJson(),
                        TemplateData = new
                        {
                            Total_NoFound = model_A.None,
                            Total_Transit = model_A.Shipment,
                            Total_ArrivedToTake = model_A.NoticeLeft,
                            Total_DeliveryFailure = model_A.DeliveryFailure,
                            Total_SignSuccess = model_A.Delivered,
                            Total_Abnormal = model_A.Returned,
                            Total_Warn = model_A.Warn,
                            Total_Total = (model_A.None + model_A.Shipment + model_A.NoticeLeft + model_A.DeliveryFailure + model_A.Delivered + model_A.Warn + model_A.Returned),

                                //如果您不愿意继续接受此类邮件，可<a href=\"https://{0}.17track.net/noticesetting.html\">点击此处</a>
                            UserRole = "seller",

                            DisplayWarmState = isSendCompleteCountWarn,
                            UnChangeCompleteCount = completeCountModel.UnChangeCompleteCount,
                            InvalidCompleteCount = completeCountModel.InvalidCompleteCount,
                            DeliveredCompleteCount = completeCountModel.DeliveredCompleteCount,
                            CompleteArchivedCount = completeCountModel.CompleteArchivedCount,

                            //{15}天未修改的无效查询包裹
                            InvalidCompleteDays = item.CompleteConfig.InvalidTime,
                            //{0}天轨迹信息未发生变化
                            UnChangeCompleteDays = item.CompleteConfig.NoChangeTime,
                            //成功签收{0}天的包裹
                            DeliveredCompleteDays = item.CompleteConfig.SuccessTime,
                            //已完成{0}天的包裹
                            CompleteArchivedDays = item.CompleteConfig.FinishedTime,


                        }.MySerializeToJson(),

                        UserId = new UserInfoExt()
                        {
                            FUserId = item.FUserId,
                            FUserRole = ((Backend.Enums.UserRoleType)(int)item.FUserRole),
                            FNickname = item.FNickName,
                            FEmail = item.FEmail,
                            FLanguage = item.FLanguage
                        }

                    });

                    if (isSuccess)
                    {
                        _LogBll.AddUserWarnLog(Log_WarnType.ChangeOrderStatus, item.FUserId, item.FNickName, item.FEmail, SerializeExtend.MySerializeToJson(item));
                        _LogBll.AddUserWarnLog(Log_WarnType.OrderCompleteCountWarn, item.FUserId, item.FNickName, item.FEmail, SerializeExtend.MySerializeToJson(completeCountModel));
                    }
                }

                catch (Exception ex)
                {
                    LogHelper.Log(
                        new LogDefinition(LogLevel.Error,
                            string.Format(
                                "HandleSellerWarnNotify 处理卖家包裹状态每天统计通知，发生异常,userId={0}",
                                item.FUserId)), ex);
                    isSuccess = false;
                }
            }
            return isSuccess;
        }


        /// <summary>
        ///     处理用户跟踪数变化不带10%通知用户
        /// </summary>
        /// <param name="item">用户配置信息</param>
        /// <returns></returns>
        public async Task<bool> HandleUserTrackNumNotify(UserProfileConfigDTO item, string sendDate = "")
        {
            var isSuccess = false;
            //LogHelper.Log( new LogDefinition(LogLevel.Info, "HandleUserTrackNumNotify 订单数不足10% ,item={0},当前线程ID={1}"), item.ToJson(), Thread.CurrentThread.ManagedThreadId.ToString());
            if (item.WranConfig.IsWarnNum)
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(sendDate))
                    {
                        sendDate = DateTime.Now.AddDays(0 - 3).ToString("yyyy-MM-dd");
                    }

                    //判断间隔时间，是否发送过邮件提醒
                    var isExist = _LogBll.ExistWarnLog(item.FUserId, Log_WarnType.OrderTrackNumWarn, sendDate);
                    if (!isExist)
                    {
                        var orderBll = FactoryContainer.Create<IOrderAutoWarnBLL>();
                        orderBll.SetDataRoute(new DataRouteModel
                        {
                            UserRole = (byte)item.FUserRole,
                            NodeId = Convert.ToByte(item.FNodeId),
                            DbNo = Convert.ToByte(item.FDbNo),
                            TableNo = Convert.ToByte(item.FTableNo)
                        });

                        var resultModel = await  orderBll .OrderUpdateStateWaitToTracking(item.FUserId);

                        if (resultModel != null && resultModel.CanTrackNum > 0)
                        {
                            HandleUserTrackNumNotifyCore(item, resultModel, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log(
                        new LogDefinition(LogLevel.Error,
                            string.Format(
                                "HandleUserTrackNumNotify 处理用户追踪数不足10%的通知，发生异常,userId={0}",
                                item.FUserId)), ex);
                }
            }
            return isSuccess;
        }
    }
}
