using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using YQTrack.Backend.OrderComplete.Model;
using YQTrackV6.Common.Utils;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.V2.Complate
{
    public class BaseSellerOrderComplateWorkService
    {
        #region <私有变量>
        private ConcurrentBag<OrderAutoCompleteDTO> _cbag = new ConcurrentBag<OrderAutoCompleteDTO>();

        private ConcurrentQueue<UserProfileConfigDTO> _QueueErrorUserComplete;

        private Semaphore _sem = null;

        /// <summary>
        ///     是否循环用户
        /// </summary>
        public bool _IsLoopUser = true;

        private int _Semaphore = 5;

        ///是否异步
        public bool _IsAsync;

        private int _UserCount = 1;

        /// <summary>
        ///     当前用户索引
        /// </summary>
        private int _CurrentUserIndex;

        /// <summary>
        ///     当前连接数
        /// </summary>
        private int _CurrentConnectionCount;


        #endregion

        #region 构造函数
        public BaseSellerOrderComplateWorkService()
        {
            _QueueErrorUserComplete = new ConcurrentQueue<UserProfileConfigDTO>();
        }

        public void Init()
        {
            _sem = new Semaphore(_Semaphore, _Semaphore);
        }

        public void InitData(bool isAsync, int semaphoreCount)
        {
            _IsAsync = isAsync;
            _Semaphore = semaphoreCount;

            _IsLoopUser = true;
            _UserCount = 1;
            _CurrentUserIndex = 0;
            _CurrentConnectionCount = 0;

            _sem = new Semaphore(_Semaphore, _Semaphore);
        }
        #endregion


        #region 执行选中DB编号下用户,包含邮件提醒
        /// <summary>
        ///     订单完成数统计，消息通知
        /// </summary>
        /// <param name="userList">所有用户配置信息集合</param>
        public int ExecuteSendEmailToUserAsync(IEnumerable<UserProfileConfigDTO> userList, ILogBll logBll)
        {
            var result = 0;
            if (userList != null && userList.Count() > 0)
            {
                try
                {
                    userList = userList.Where(a => a.FUserRole == UserRoleType.Buyer || a.FUserRole == UserRoleType.Seller);
                    result = 1;
                    foreach (var item in userList)
                    {
                        //是否等待执行,是不是要改成是否异步?
                        if (_IsAsync)
                        {
                            //是否开启自动完成提醒
                            if (item.FUserRole == UserRoleType.Seller && item.WranConfig.IsWarnComplete && !logBll.ExistWarnLog(item.FUserId, Log_WarnType.ChangeOrderStatus, DateTime.UtcNow.ToString("yyyy-MM-dd")))
                            {
                                _sem.WaitOne();
                                Task.Run(() =>
                                {
                                    var notifyresult = HandleSellerWarnNotify(item, logBll);
                                    _sem.Release();
                                });
                            }
                        }
                        else
                        {
                            //是否开启自动完成提醒
                            if (item.FUserRole == UserRoleType.Seller && item.WranConfig.IsWarnComplete && !logBll.ExistWarnLog(item.FUserId, Log_WarnType.ChangeOrderStatus, DateTime.UtcNow.ToString("yyyy-MM-dd")))
                            {
                                _sem.WaitOne();
                                var notifyresult = HandleSellerWarnNotify(item, logBll);
                                _sem.Release();
                            }
                        }

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
        public async Task<bool> HandleSellerWarnNotify(UserProfileConfigDTO item, ILogBll logBll, DateTime? currentDate = null)
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

                    //TODO: ClickHere 已删除 model_A.Warn_ClickHere = LanguageWcf.GetText(item.FLanguage, LanguageHelper.LanguageType.UCenterMail, ResUCenterMail.__common_clickHere);

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
                        logBll.AddUserWarnLog(Log_WarnType.ChangeOrderStatus, item.FUserId, item.FNickName, item.FEmail, SerializeExtend.MySerializeToJson(item));
                        logBll.AddUserWarnLog(Log_WarnType.OrderCompleteCountWarn, item.FUserId, item.FNickName, item.FEmail, SerializeExtend.MySerializeToJson(completeCountModel));
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
        #endregion


        #region <自动归档操作，与自动完成分开执行>
        /// <summary>
        ///   执行用户订单归档操作
        /// </summary>
        public int ExecuteAutoArchivedCurrentUsers(int dbNo)
        {
            var result = 0;

            LogHelper.Log(new LogDefinition(LogLevel.Info, "BaseSellerOrderComplateWorkService.ExecuteAutoArchivedCurrentUsers 工作,userCount={0},_DBNo={1}"), _cbag.Count, dbNo);
            bool isLoop = true;
            int loopCount = 0;
            int loopCountTotal = 3;
            try
            {
                int tcount = _cbag.Count; // 当前集合总数
                int index = tcount;// 当前集合总数
                do
                {
                    if (tcount == 0)
                    {
                        isLoop = true;
                        Thread.Sleep(3000);
                    }

                    result = 1;
                    if (index > 0) // 当前集合有值
                    {
                        do
                        {
                            // 获取集合对象，并移除掉。
                            OrderAutoCompleteDTO dto = null;
                            if (_cbag.TryTake(out dto))
                            {
                                //是否等待执行,是不是要改成是否异步?
                                if (_IsAsync)
                                {
                                    //执行归档操作
                                    _sem.WaitOne();

                                    LogHelper.Log(new LogDefinition(LogLevel.Info, "BaseSellerOrderComplateWorkService.ExecuteAutoArchivedCurrentUsers 工作开始,UserId={0},FDbNo={1},FTableNo={2}"), dto.UserId, dto.FDbNo, dto.FTableNo);
                                    Task.Run(() =>
                                    {
                                        var autoArchivedBll = FactoryContainer.Create<IOrderAutoArchivedBll>();
                                        autoArchivedBll.SetDataRoute(new DataRouteModel
                                        {
                                            UserRole = (byte)dto.FUserRole,
                                            NodeId = Convert.ToByte(dto.FNodeId),
                                            DbNo = Convert.ToByte(dto.FDbNo),
                                            TableNo = Convert.ToByte(dto.FTableNo)
                                        });

                                        autoArchivedBll.DoWork(dto.UserId);
                                        if (autoArchivedBll.IsError)
                                        {
                                            _cbag.Add(dto);
                                        }

                                        //针对超过 180天订单归档处理
                                        var autoArchivedOrderBll = FactoryContainer.Create<IOrderAutoArchivedOrderBll>();
                                        autoArchivedOrderBll.SetDataRoute(new DataRouteModel
                                        {
                                            UserRole = (byte)dto.FUserRole,
                                            NodeId = Convert.ToByte(dto.FNodeId),
                                            DbNo = Convert.ToByte(dto.FDbNo),
                                            TableNo = Convert.ToByte(dto.FTableNo)
                                        });

                                        autoArchivedOrderBll.DoWork(dto.UserId);
                                        if (autoArchivedOrderBll.IsError)
                                        {
                                            _cbag.Add(dto);
                                        }

                                        _sem.Release();

                                    });

                                    _CurrentConnectionCount = _UserCount * 10;
                                    _UserCount++;


                                }
                                else
                                {
                                    //执行归档操作
                                    _sem.WaitOne();

                                    LogHelper.Log(new LogDefinition(LogLevel.Info, "BaseSellerOrderComplateWorkService.ExecuteAutoArchivedCurrentUsers 工作开始,UserId={0},FDbNo={1},FTableNo={2}"), dto.UserId, dto.FDbNo, dto.FTableNo);
                                    //Task.Run(() =>
                                    //{
                                    var autoArchivedBll = FactoryContainer.Create<IOrderAutoArchivedBll>();
                                    autoArchivedBll.SetDataRoute(new DataRouteModel
                                    {
                                        UserRole = (byte)dto.FUserRole,
                                        NodeId = Convert.ToByte(dto.FNodeId),
                                        DbNo = Convert.ToByte(dto.FDbNo),
                                        TableNo = Convert.ToByte(dto.FTableNo)
                                    });

                                    autoArchivedBll.DoWork(dto.UserId);
                                    if (autoArchivedBll.IsError)
                                    {
                                        _cbag.Add(dto);
                                    }

                                    //针对超过 180天订单归档处理
                                    var autoArchivedOrderBll = FactoryContainer.Create<IOrderAutoArchivedOrderBll>();
                                    autoArchivedOrderBll.SetDataRoute(new DataRouteModel
                                    {
                                        UserRole = (byte)dto.FUserRole,
                                        NodeId = Convert.ToByte(dto.FNodeId),
                                        DbNo = Convert.ToByte(dto.FDbNo),
                                        TableNo = Convert.ToByte(dto.FTableNo)
                                    });

                                    autoArchivedOrderBll.DoWork(dto.UserId);
                                    if (autoArchivedOrderBll.IsError)
                                    {
                                        _cbag.Add(dto);
                                    }

                                    _sem.Release();

                                    //});

                                    _CurrentConnectionCount = _UserCount * 10;
                                    _UserCount++;
                                }
                            }
                            index--;
                        } while (index > 0);
                    }

                    //循环完了，看集合是否还有数据。
                    int ncount = _cbag.Count;
                    index = ncount;
                    if (ncount > 0)
                    {
                        //有数据，多循环一次，并休眠2秒
                        isLoop = true;
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        loopCount++;
                    }

                    if (loopCount == loopCountTotal)
                    {
                        isLoop = false;
                    }

                } while (isLoop);
            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Error, "BaseSellerOrderComplateWorkService.订单归档操作,ExecuteAutoArchivedCurrentUsers,userCount={0}"), ex,
                    _cbag.Count);
            }
            return result;
        }

        /// <summary>
        ///   重试错误的用户自动完成操作
        /// </summary>
        public int RetryErrorUserOrderComplete(int dbNo)
        {
            var result = 0;

            LogHelper.Log(new LogDefinition(LogLevel.Info, "BaseSellerOrderComplateWorkService.RetryErrorUserOrderComplete 工作,UserErrorCount={0},_DBNo={1}"), _QueueErrorUserComplete.Count, dbNo);
            bool isLoop = true;
            int loopCount = 0;
            int loopCountTotal = 3;
            try
            {
                int tcount = _QueueErrorUserComplete.Count; // 当前集合总数
                int index = tcount;// 当前集合总数
                do
                {
                    if (tcount == 0)
                    {
                        isLoop = true;
                        Thread.Sleep(3000);
                    }
                    result = 1;
                    if (index > 0) // 当前集合有值
                    {
                        do
                        {
                            // 获取集合对象，并移除掉。
                            UserProfileConfigDTO dto = null;
                            if (_QueueErrorUserComplete.TryDequeue(out dto))
                            {
                                LogHelper.Log(new LogDefinition(LogLevel.Info, "BaseSellerOrderComplateWorkService.RetryErrorUserOrderComplete 工作开始,UserId={0},FDbNo={1},FTableNo={2}"), dto.FUserId, dto.FDbNo, dto.FTableNo);
                                var completeResult = HandleOrderAutoCompleteOrArchivedAll(dto);
                                var isCompleteResult = completeResult.Result;
                            }
                            index--;
                        } while (index > 0);
                    }
                    //循环完了，看集合是否还有数据。
                    int ncount = _QueueErrorUserComplete.Count;
                    index = ncount;
                    if (ncount > 0)
                    {
                        //有数据，多循环一次，并休眠2秒
                        isLoop = true;
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        loopCount++;
                    }

                    if (loopCount == loopCountTotal)
                    {
                        isLoop = false;
                    }

                } while (isLoop);
            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Error, "重试错误的用户,ExecuteAutoArchivedCurrentUsers,userCount={0}"), ex,
                    _QueueErrorUserComplete.Count);
            }
            return result;
        }
        #endregion

        #region 自动完成相关
        /// <summary>
        ///     按分页数PageSize获取用户数据(自动完成,薪改动)
        /// </summary>
        /// <returns></returns>
        public void GetUserConfigsByPage(IUserBLL userBll, int nodeId, int dbNo, bool isSeller = false)
        {
            List<UserProfileConfigDTO> list = null;

            int loopPageSize = 100;
            var dto = new UserProfileConfigPageDTO
            {
                NodeId = nodeId,
                DBNo = dbNo,
                StartIndex = _CurrentUserIndex,// 0
                PageSize = loopPageSize,  //index
                FUserId = 0
            };

            //是否指定卖家用户操作
            if (isSeller)
            {
                dto.UserRoles = new List<int>() { (int)UserRoleType.Seller };
            }

            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "BaseSellerOrderComplateWorkService.DoComplateWork.GetUserConfigsByPage 工作"));
            do
            {
                var taskResult = userBll.GetUserConfigsByDataRouteModel(dto);
                list = taskResult.Result as List<UserProfileConfigDTO>;

                if (list == null || list.Count() == 0)
                {
                    break;
                }

                int count = list.Count();

                ExecuteAutoCompleteAsync(list);

                if (count != loopPageSize)
                {
                    break;
                }
                dto.FUserId = list.Max(t => t.FUserId);

            } while (true);

            LogHelper.Log(new LogDefinition(LogLevel.Info, "BaseSellerOrderComplateWorkService.DoComplateWork.GetUserConfigsByPage执行任务.结束.nodeId={0},dbno={1}"), nodeId, dbNo);

            ExecuteAutoArchivedCurrentUsers(dbNo);

            //重试发生错误的数据
            RetryErrorUserOrderComplete(dbNo);
        }


        /// <summary>
        ///     按分页数PageSize获取用户数据(提醒)
        /// </summary>
        /// <returns></returns>
        public void GetUserConfigsByPageEmail(IUserBLL userBll, int nodeId, int dbNo, bool isSeller = false)
        {
            List<UserProfileConfigDTO> list = null;

            int loopPageSize = 100;
            var dto = new UserProfileConfigPageDTO
            {
                NodeId = nodeId,
                DBNo = dbNo,
                StartIndex = _CurrentUserIndex,// 0
                PageSize = loopPageSize,  //index
                FUserId = 0
            };

            //是否指定卖家用户操作
            if (isSeller)
            {
                dto.UserRoles = new List<int>() { (int)UserRoleType.Seller };
            }

            var LogBll = FactoryContainer.Create<ILogBll>();
            LogBll.SetDataRoute(userBll.Context.DataRoute);

            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "BaseSellerOrderComplateWorkService.DoWorkCompleteRecordCountNotify.GetUserConfigsByPage 工作"));

            do
            {
                var taskResult = userBll.GetUserConfigsByDataRouteModel(dto);
                list = taskResult.Result as List<UserProfileConfigDTO>;

                if (list == null || list.Count() == 0)
                {
                    break;
                }

                int count = list.Count();

                ExecuteSendEmailToUserAsync(list, LogBll);//提醒

                if (count != loopPageSize)
                {
                    break;
                }
                dto.FUserId = list.Max(t => t.FUserId);

            } while (true);

            LogHelper.Log(new LogDefinition(LogLevel.Info, "BaseSellerOrderComplateWorkService.DoWorkCompleteRecordCountNotify.GetUserConfigsByPage执行任务.结束.nodeId={0},dbno={1}"), nodeId, dbNo);
        }


        /// <summary>
        ///   订单自动完成处理
        /// </summary>
        /// <param name="userList">所有用户配置信息集合</param>
        public int ExecuteAutoCompleteAsync(IEnumerable<UserProfileConfigDTO> userList)
        {
            var result = 0;
            if (userList == null || userList.Count() == 0)
            {
                return result;
            }

            try
            {
                result = 1;
                foreach (var item in userList)
                {
                    //是否等待执行,是不是要改成是否异步?
                    if (_IsAsync)
                    {
                        _sem.WaitOne();
                        Task.Run(() =>
                        {
                            var objResult = HandleOrderAutoCompleteOrArchivedAll(item);
                            var isresult = objResult.Result;
                            _sem.Release();
                        });
                    }
                    else
                    {
                        _sem.WaitOne();

                        var objResult = HandleOrderAutoCompleteOrArchivedAll(item);
                        var isresult = objResult.Result;

                        _sem.Release();
                    }

                    _CurrentConnectionCount = _UserCount * 4;
                    _UserCount++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Error, "订单自动完成处理,BaseSellerOrderComplateWorkService.ExecuteAutoCompleteAsync,userCount={0}"), ex,
                    userList.Count());
            }

            return result;
        }

        /// <summary>
        ///     处理所有订单数据
        /// </summary>
        /// <param name="item">用户配置信息</param>
        /// <returns></returns>
        public async Task<bool> HandleOrderAutoCompleteOrArchivedAll(UserProfileConfigDTO item)
        {
            var isSuccess = false;
            try
            {
                var orderCompleteBll = FactoryContainer.Create<IOrderAutoCompleteBll>();
                orderCompleteBll.SetDataRoute(new DataRouteModel
                {
                    UserRole = (byte)item.FUserRole,
                    NodeId = Convert.ToByte(item.FNodeId),
                    DbNo = Convert.ToByte(item.FDbNo),
                    TableNo = Convert.ToByte(item.FTableNo)
                });

                if (item.FUserRole == UserRoleType.Buyer)
                {
                    //isSuccess = await orderCompleteBll.HandleOrderAutoCompleteOrArchivedAll(item);
                }
                else
                {
                    isSuccess = await orderCompleteBll.HandleOrderAutoCompleteOrArchivedAll(item);
                }

                if (!isSuccess)
                {
                    _QueueErrorUserComplete.Enqueue(item);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(
                    new LogDefinition(LogLevel.Error,
                        string.Format(
                            "BaseSellerOrderComplateWorkService.HandleOrderAutoCompleteOrArchivedAll 处理4种类型的订单自动完成或归档，发生异常,userId={0}",
                            item.FUserId)), ex);
            }
            return isSuccess;
        }
        #endregion


        /// <summary>
        ///     处理所有订单数据(队列用)
        /// </summary>
        /// <param name="item">用户配置信息</param>
        /// <returns></returns>
        public async Task<bool> HandleOrderAutoCompleteSuccess(UserProfileConfigDTO item, long trackInfoId)
        {
            var isSuccess = false;
            try
            {
                var orderCompleteBll = FactoryContainer.Create<IOrderAutoCompleteBll>();
                orderCompleteBll.SetDataRoute(new DataRouteModel
                {
                    UserRole = (byte)item.FUserRole,
                    NodeId = Convert.ToByte(item.FNodeId),
                    DbNo = Convert.ToByte(item.FDbNo),
                    TableNo = Convert.ToByte(item.FTableNo)
                });

                isSuccess = await orderCompleteBll.HandlerOrderAutoComplete(item, trackInfoId);
            }
            catch (Exception ex)
            {
                LogHelper.Log(
                    new LogDefinition(LogLevel.Error,
                        string.Format(
                            "BaseSellerOrderComplateWorkService.HandleOrderAutoCompleteSuccess 处理立即完成，发生异常,userId={0}",
                            item.FUserId)), ex);
            }
            return isSuccess;
        }

    }
}
