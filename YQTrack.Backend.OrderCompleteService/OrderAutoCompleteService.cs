using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YQTrack.Backend.Enums;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.OrderComplete.DTO;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.OrderComplete.IService;
using YQTrack.Backend.OrderComplete.MQHelpr;
using YQTrack.Backend.Seller.MQData.Helper;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService
{

    /// <summary>
    ///     订单完成服务
    ///     订单标记已完成，要查询缓存中订单最后的事件，更新到数据库中（2016-04-13）
    /// </summary>
    public class OrderAutoCompleteService : IOrderAutoCompleteService, IDisposable
    {
        #region <构造方法>

        public OrderAutoCompleteService(IUserBLL userbll)
        {
            _userBll = userbll;
            _QueueErrorUserComplete = new ConcurrentQueue<UserProfileConfigDTO>();
        }


        /// <summary>
        /// 设置路由信息
        /// </summary>
        /// <param name="model"></param>
        public  void SetDataRoute(DataRouteModel model)
        {
            _userBll.SetDataRoute(model);
        }

        #endregion

        //处理订单自动完成逻辑处理
        //指定哪个用户数据库
        //查询出用户，就知道对应NodeId，DBNo，TableNo

        #region <私有变量>


        private ConcurrentQueue<UserProfileConfigDTO> _QueueErrorUserComplete;

        private Semaphore _sem = null;
        //Semaphore semaphoreArchived = new Semaphore(10, 10);
        private ConcurrentBag<OrderAutoCompleteDTO> _cbag = new ConcurrentBag<OrderAutoCompleteDTO>();

        
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

        private int _Semaphore = 5;
        //private int _SemaphoreArchived = 10;

        private int _OrderPageSize = 60;

        #endregion


        #region  <事件>

        /// <summary>
        ///  发送当前订单数量信息
        /// </summary>
        public event EventHandler<OrderCompleteMsgEventArgs> SendOrderCompleteMsgEvent;


        #endregion

        #region  <循环获取用户处理订单>

        public void Init()
        {
            _sem = new Semaphore(_Semaphore, _Semaphore);
        }

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
        ///     开始执行订单自动完成工作
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
        public void DoWork(int nodeId = 1, int dbno = 1, int pageSize = 100, int startIndex = 0, int endIndex = 0,
            bool isConnectionCount = false, int maxConnectionCount = 100, int loopWaitSeconds = 30,
            int msgRepeatInsertDays = 3, int orderPageSize = 100, int semaphoreCount = 10)
        {
            InitData(nodeId, dbno, pageSize, startIndex, endIndex, isConnectionCount, maxConnectionCount,
                loopWaitSeconds, msgRepeatInsertDays, orderPageSize, semaphoreCount);
            var loopCount = 1;
            var userCount = 0;
            try
            {
                while (_IsLoopUser)
                {
                    LogHelper.Log(new LogDefinition(LogLevel.Verbose, "DoWork 工作,loopCount={0}"), loopCount);
                    var list = GetUserConfigsByPage(true);
                    if (list.Count == 0) break;
                    userCount += list.Count;
                    ExecuteAutoCompleteAsync(list);

                    if (SendOrderCompleteMsgEvent != null)
                    {
                        SendOrderCompleteMsgEvent(this, new OrderCompleteMsgEventArgs(nodeId, dbno, userCount, _CurrentUserIndex, loopCount, _cbag.Count, _QueueErrorUserComplete.Count));
                    }
                    loopCount++;
                }

                ExecuteAutoArchivedCurrentUsers();
                //重试发生错误的数据
                RetryErrorUserOrderComplete();
            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderAutoCompleteService.DoWork.发生异常"), ex);
            }
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


        /// <summary>
        ///     订单自动完成处理
        /// </summary>
        /// <param name="userList">所有用户配置信息集合</param>
        private int ExecuteAutoCompleteAsync(IEnumerable<UserProfileConfigDTO> userList)
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
                    //控制下处理速度，达到技数的数量，休眠等待在执行
                    if (_IsConnectionCount)
                    {
                        if (_CurrentConnectionCount > _MaxConnectionCount)
                        {
                            Thread.Sleep(_LoopWaitSeconds * 1000);
                            _CurrentConnectionCount = 0;
                            _UserCount = 1;
                        }
                    }

                    _sem.WaitOne();

                    var objResult = HandleOrderAutoCompleteOrArchivedAll(item);
                    var isresult = objResult.Result;

                    _sem.Release();

                    _CurrentConnectionCount = _UserCount * 4;
                    _UserCount++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Error, "订单自动完成处理,AutoCompleteOrders,userCount={0}"), ex,
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


                //orderCompleteBll.TrackNumUnderNumWarnPerNotify += OrderCompleteBll_TrackNumUnderNumWarnPerNotify; 
                if (item.FUserRole == UserRoleType.Buyer)
                {
                    //isSuccess = await orderCompleteBll.HandleOrderAutoCompleteOrArchivedAll(item);
                }
                else
                {
                 

                    orderCompleteBll.TrackNumLackNotify += OrderCompleteBll_TrackNumLackNotify;
                    //orderCompleteBll.UpdateUserCacheTrackNum += OrderCompleteBll_UpdateUserCacheTrackNum;
                    orderCompleteBll.MoveOrderToArchivedDb += OrderCompleteBll_MoveOrderToArchivedDb;
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
                            "HandleOrderAutoCompleteOrArchivedAll 处理4种类型的订单自动完成或归档，发生异常,userId={0}",
                            item.FUserId)), ex);
            }
            return isSuccess;
        }


        /// <summary>
        ///     处理所有订单数据
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
                            "HandleOrderAutoCompleteSuccess 处理立即完成，发生异常,userId={0}",
                            item.FUserId)), ex);
            }
            return isSuccess;
        }

        /// <summary>
        /// 追踪数不足百分比（10%），邮件提醒用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OrderCompleteBll_TrackNumUnderNumWarnPerNotify(object sender, OrderAutoCompleteEventArgs e)
        {

            if (e != null && e.AutoCompleteResult != null && e.UserProfileConfig != null)
            {
               // HandleUserTrackNumNotifyCore(e.UserProfileConfig, e.AutoCompleteResult, false);
            }
        }

        #region < 事件回调方法 >
        /// <summary>
        /// 移动归档订单到归档数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OrderCompleteBll_MoveOrderToArchivedDb(object sender, OrderAutoCompleteEventArgs e)
        {

            var dto = e.AutoCompleteResult;
            LogHelper.LogObj(new LogDefinition(LogLevel.Info, "OrderCompleteBll_MoveOrderToArchivedDb"), e);
            if (dto != null)
            {
                if (dto.IsArchived == 1)
                {

                    //var autoArchivedBll = FactoryContainer.Create<IOrderAutoArchivedBll>();
                    //autoArchivedBll.SetDataRoute(new DataRouteModel
                    //{
                    //    UserRole = (byte)dto.FUserRole,
                    //    NodeId = Convert.ToByte(dto.FNodeId),
                    //    DbNo = Convert.ToByte(dto.FDbNo),
                    //    TableNo = Convert.ToByte(dto.FTableNo)
                    //});

                    //autoArchivedBll.DoWork(dto.UserId);

                    _cbag.Add(dto);
                }
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


        #region 消息队列对用户Id的处理

        public void StartMQHanler()
        {
            SellerOrderCompleteMQConsumerService.Instance.Subscribe(HanderUserOrderComplete);
        }


        private bool HanderUserOrderComplete(SellerOrderCompleteModel model)
        {
            bool result = true;
            LogHelper.LogObj(new LogDefinition(LogLevel.Info, "HanderUserOrderComplete:处理队列里用户的自动完成"), model);
            var dto = new UserProfileConfigPageDTO
            {
                NodeId = -1,
                DBNo = -1,
                StartIndex = 0,
                PageSize = 10,
                FUserId = model.FUserId
            };
            //返回集合，以后可以返回多个用户
            var userList = _userBll.GetUserConfigsByPage(dto).Result;
            var userDot = userList.FirstOrDefault();
            if (userDot != null)
            {
                bool handlerRes = false;
                if (model.FTrackInfoId > 0)
                {
                    handlerRes = HandleOrderAutoCompleteSuccess(userDot, model.FTrackInfoId).Result;
                }
                else
                {
                    handlerRes = HandleOrderAutoCompleteOrArchivedAll(userDot).Result;
                    if (!handlerRes)
                    {
                        LogHelper.LogObj(new LogDefinition(LogLevel.Error, "HanderUserOrderComplete:处理队列里用户的自动完成结果失败"), model);
                    }
                }
            }
            return result;
        }

        #endregion  


        #region < 自动归档操作，与自动完成分开执行 >


        /// <summary>
        ///   执行用户订单归档操作
        /// </summary>
        public int ExecuteAutoArchivedCurrentUsers()
        {
            var result = 0;

            LogHelper.Log(new LogDefinition(LogLevel.Info, "ExecuteAutoArchivedCurrentUsers 工作,userCount={0},_DBNo={1}"), _cbag.Count, _DBNo);
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
                                if (_IsConnectionCount)
                                {
                                    if (_CurrentConnectionCount > _MaxConnectionCount)
                                    {
                                        LogHelper.Log(
                                            new LogDefinition(LogLevel.Verbose, "控制数已满,_CurrentConnectionCount={0}，当前线程ID={1}"),
                                            _CurrentConnectionCount, Thread.CurrentThread.ManagedThreadId.ToString());
                                        Thread.Sleep(_LoopWaitSeconds * 1000);
                                        _CurrentConnectionCount = 0;
                                        _UserCount = 1;
                                    }
                                }
                                //执行归档操作
                                _sem.WaitOne();
                                LogHelper.Log(new LogDefinition(LogLevel.Info, "ExecuteAutoArchivedCurrentUsers 工作开始,UserId={0},FDbNo={1},FTableNo={2}"), dto.UserId, dto.FDbNo, dto.FTableNo);
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
                                    autoArchivedBll.DoWork(dto.UserId, _OrderPageSize);
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
                                    autoArchivedOrderBll.DoWork(dto.UserId, _OrderPageSize);
                                    if (autoArchivedOrderBll.IsError)
                                    {
                                        _cbag.Add(dto);
                                    }

                                    _sem.Release();
                                    //LogHelper.Log(new LogDefinition(LogLevel.Warn, "ExecuteAutoArchivedCurrentUsers 工作结束--,UserId={0},_DBNo={1}"), dto.UserId, _DBNo);
                                });

                                _CurrentConnectionCount = _UserCount * 10;
                                _UserCount++;

                                ////var autoArchivedBll = FactoryContainer.Create<IOrderAutoArchivedBll>();
                                ////autoArchivedBll.SetDataRoute(new DataRouteModel
                                ////{
                                ////    UserRole = (byte)dto.FUserRole,
                                ////    NodeId = Convert.ToByte(dto.FNodeId),
                                ////    DbNo = Convert.ToByte(dto.FDbNo),
                                ////    TableNo = Convert.ToByte(dto.FTableNo)
                                ////});
                                ////autoArchivedBll.DoWork(dto.UserId, _OrderPageSize);

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
                LogHelper.Log(new LogDefinition(LogLevel.Error, "订单归档操作,ExecuteAutoArchivedCurrentUsers,userCount={0}"), ex,
                    _cbag.Count);
            }
            return result;
        }

        /// <summary>
        ///   重试错误的用户自动完成操作
        /// </summary>
        public int RetryErrorUserOrderComplete()
        {
            var result = 0;

            LogHelper.Log(new LogDefinition(LogLevel.Info, "BaseSellerOrderComplateWorkService.RetryErrorUserOrderComplete 工作,UserErrorCount={0},_DBNo={1}"), _QueueErrorUserComplete.Count, _DBNo);
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

     


      
        public void Dispose()
        {
            SellerOrderCompleteMQConsumerService.Instance.UnSubscribe();
        }


        #region < 随机执行自动完成和归档操作 >

        public async Task DoWorkRandomComplete(int nodeId, int dbno, int pageSize, int startIndex, int endIndex,
            bool isConnectionCount, int maxConnectionCount, int loopWaitSeconds,
            int msgRepeatInsertDays, int orderPageSize, int semaphoreCount, int mode)
        {

            InitData(nodeId, dbno, pageSize, startIndex, endIndex, isConnectionCount, maxConnectionCount,
               loopWaitSeconds, msgRepeatInsertDays, orderPageSize, semaphoreCount);
            //根据取模值，获取用户信息，按200一个分页处理，循环用户，直到用户结束。
            //循环用户处理
            LogHelper.Log(new LogDefinition(LogLevel.Info, "DoWorkRandomComplete.执行任务.nodeId={0},dbno={1},mode={2}"), nodeId, dbno, mode);
            int loopPageSize = 100;
            try
            {
                var dto = new UserProfileConfigPageDTO
                {
                    NodeId = _NodeId,
                    DBNo = _DBNo,
                    StartIndex = 0,
                    PageSize = loopPageSize,
                    FUserId = 0,

                };
                dto.UserRoles = new List<int>() { (int)UserRoleType.Seller };
                do
                {
                    var list = await _userBll.GetUserConfigsByMode(dto, mode);
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
                LogHelper.Log(new LogDefinition(LogLevel.Info, "DoWorkRandomComplete.执行任务.结束.nodeId={0},dbno={1},mode={2}"), nodeId, dbno, mode);

                ExecuteAutoArchivedCurrentUsers();
                //重试发生错误的数据
                RetryErrorUserOrderComplete();
            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Info, "DoWorkRandomComplete.发生异常"), ex);
            }

        }

        #endregion

    }
}
