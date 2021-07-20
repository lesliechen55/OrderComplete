using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.BLL;
using YQTrack.Backend.OrderComplete.DTO;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrack.Backend.Sharding;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    public class OrderRandomCompleteTask : ICompleteTask
    {
        //private readonly IOrderCompleteBll _orderCompleteBll;

        private Timer _timer;

        private long interval = 40 * 60 * 1000; // 执行时间


        public OrderRandomCompleteTask()
        {
           
        }

        

        public void Execute(List<OrderCompleteItem> userDbNos = null)
        {
            if (userDbNos == null)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Info, "OrderRandomCompleteTask.ExcecuteAuto.定时执行随机自动完成任务开始"));
                //启动定时器处理，随机任务处理
                _timer = new Timer(HandlerTimer, null, interval, interval);

            }
            else
            {
              
            }
        }


        /// <summary>
        ///     定时触发方法
        /// </summary>
        /// <param name="state"></param>
        public void HandlerTimer(object state)
        {
            if (_timer == null) return;

            _timer.Change(-1, -1);

            //如果是在8点到十点，这个时间段
            var currentDate = DateTime.Now;
            var hour = currentDate.Hour;
            //var mun = currentDate.Minute;
            if (hour == 8 || hour == 9)
            {
                _timer.Change(interval, interval);
                return;
            }

            try
            {
                Run();
            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Error, "HandlerTimer.异常"),ex);
            }
          

            _timer.Change(interval, interval);

        }

        private List<int> modeRunList = new List<int>();

        public void Run()
        {
            //每次定时执行一次
            //随机获取一个取模值，看是否执行过，如果没有执行过，就执行。
            //异步执行不同库的用户信息

            //根据取模获取用户信息，根据用户ID获取单号，进行自动完成和归档操作。
            LogHelper.Log(new LogDefinition(LogLevel.Info, "OrderRandomCompleteTask.Run.执行随机自动完成任务处理"));
            var strDbSeller = Enum.GetName(typeof(YQDbType), YQDbType.Seller);
            var dbTypeRule = Sharding.DBShardingRouteFactory.GetDBTypeRule(strDbSeller);

            var nodeRules = dbTypeRule.NodeRoutes;
            var nodeCount = nodeRules.Count;
            List<Task> listTask = new List<Task>();
            var mode = GetRandomMode();
            //获取数据库节点
            foreach (var nodeItem in nodeRules)
            {

                var dbRules = nodeItem.Value.DBRules;

                foreach (var dbRuleItem in dbRules)
                {
                    var profileBll = FactoryContainer.Create<IUserBLL>();
                    profileBll.SetDataRoute(new DataRouteModel
                    {
                        NodeId = Convert.ToByte(nodeItem.Value.NodeId),
                        DbNo = Convert.ToByte(dbRuleItem.Value.DBNo) /*, TableNo = Convert.ToByte(dto.FTableNo)*/
                    });

                    var service = new OrderAutoCompleteService(profileBll);

                    var task1 = service.DoWorkRandomComplete(nodeItem.Value.NodeId, dbRuleItem.Value.DBNo,
                        SettingManagerComplete.Setting.UserPageSize,
                        SettingManagerComplete.Setting.StartIndex,
                        SettingManagerComplete.Setting.EndIndex,
                        SettingManagerComplete.Setting.IsConnectionCount,
                        SettingManagerComplete.Setting.MaxConnectionCount,
                        SettingManagerComplete.Setting.LoopWaitSeconds,
                        SettingManagerComplete.Setting.MsgRepeatInsertDays,
                        SettingManagerComplete.Setting.OrderPageSize,
                        SettingManagerComplete.Setting.SemaphoreCount,
                        mode
                        );

                    task1.Wait();
                   
                    //listTask.Add(task1);

                }

            }

            Task.WaitAll(listTask.ToArray());
        }


        private int GetRandomMode()
        {
            int[] modeArray = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int aryCount = modeArray.Length;
            int mode = 0;
            do
            {
                Random random = new Random();
                var index = random.Next(0, aryCount);
                mode = modeArray[index];
                if (modeRunList.Contains(mode))
                {
                    if (modeRunList.Count == aryCount)
                    {
                        modeRunList.Clear();
                    }
                    continue;
                }
                else
                {
                    modeRunList.Add(mode);
                    break;
                }
            } while (true);

            return mode;
        }
    }
}