using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YQTrack.Backend.Enums;
using YQTrack.Backend.Models;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.Model;
using YQTrackV6.Log;
using System.Threading;
using YQTrack.Schedule;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrack.Backend.OrderCompleteService.V2;

namespace YQTrack.Backend.OrderCompleteService.Host.Schedule
{
    public class OrderCompleteTaskCoordinator
    {
        private readonly TaskCoordinator taskCoordinator = null;
        private readonly object lockObj = new object();

        private static readonly OrderCompleteTaskCoordinator _default = new OrderCompleteTaskCoordinator();
        public static OrderCompleteTaskCoordinator Default
        {
            get { return _default; }
        }

        private OrderCompleteTaskCoordinator()
        {
            //初始化调度任务的Key和路由信息
            InitTaskAssigned();

            var readConfig = YQTrack.Configuration.ConfigManager.GetConfig<OrderCompleteTaskCoordinatorConfig>();
            var config = readConfig.ScheduleConfig;

            LogHelper.LogObj(new LogDefinition(LogLevel.Debug, "TaskCoordinatorHandler:init"), config);

            taskCoordinator = new TaskCoordinator(config);

            taskCoordinator.OnTaskAssigned += TaskCoordinator_OnTaskAssigned;
            taskCoordinator.OnTaskCanceled += TaskCoordinator_OnTaskCanceled;
        }

        private void InitTaskAssigned()
        {
            var commonHelper = new CommonHelper();
            var sellerCount = commonHelper.GetDBRulesCount(YQDbType.Seller);
            var buyerCount = commonHelper.GetDBRulesCount(YQDbType.Buyer);

            //比较大小,谁数量多就循环哪个
            var resultCount = sellerCount > buyerCount ? sellerCount : buyerCount;
            for (int i = 0; i < resultCount; i++)
            {
                string key = $"OrderRoutes{i}";
                CommonHelper.DicRoutes.Add(key, i);//存储所有路由key
                LogHelper.Log(new LogDefinition(LogLevel.Info, $"InitTaskAssigned:路由Key:{key},index:{i}..."));
            }
        }

        private void TaskCoordinator_OnTaskCanceled(object sender, TaskAssignEventArgs e)
        {
            LogHelper.LogObj(new LogDefinition(LogLevel.Info, "TaskCoordinator_OnTaskCanceled"), new { TaskAssign = e, CommonHelper.DicTasks.Keys });

            var key = e.TaskInfo.TaskId;

            lock (lockObj)
            {
                if (CommonHelper.DicTasks.ContainsKey(key))
                {
                    CommonHelper.DicTasks.Remove(key);

                    LogHelper.LogObj(new LogDefinition(LogLevel.Info, "TaskCoordinator_OnTaskCanceled_End"), CommonHelper.DicTasks.Keys);
                }
                else
                {
                    LogHelper.LogObj(new LogDefinition(LogLevel.Debug, "TaskCoordinator_OnTaskCanceled:不存在Task的TaskId="), e);
                }
            }
        }


        /// <summary>
        /// 任务调度到这里。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskCoordinator_OnTaskAssigned(object sender, TaskAssignEventArgs e)
        {
            //任务分配到了，根据key,找到任务bll层，启动任务
            LogHelper.LogObj(new LogDefinition(LogLevel.Debug, "TaskCoordinator_OnTaskAssigned"), new { TaskAssign = e, CommonHelper.DicTasks.Keys });

            if (CommonHelper.DicTasks.ContainsKey(e.TaskInfo.TaskId))
            {
                LogHelper.LogObj(new LogDefinition(LogLevel.Error, "TaskCoordinator_OnTaskAssigned:存在任务Id"), e);
                return;
            }

            // 判断任务是否，在运行
            lock (lockObj)
            {
                if (CommonHelper.DicTasks.ContainsKey(e.TaskInfo.TaskId))
                {
                    LogHelper.LogObj(new LogDefinition(LogLevel.Error, "再次TaskCoordinator_OnTaskAssigned:存在任务Id"), e);
                    return;
                }

                if (CommonHelper.DicRoutes.ContainsKey(e.TaskInfo.TaskId))
                {
                    int routeData = CommonHelper.DicRoutes[e.TaskInfo.TaskId];
                    LogHelper.LogObj(new LogDefinition(LogLevel.Info, $"TaskCoordinator_OnTaskAssigned:存在任务Id{e.TaskInfo.TaskId},路由Id{routeData}"), e);
                    CommonHelper.DicTasks.Add(e.TaskInfo.TaskId, routeData);
                }
            }
        }


        public void Start()
        {
            LogHelper.Log(new LogDefinition(LogLevel.Debug, "TaskCoordinatorHandler:Start..."), CommonHelper.DicRoutes.Keys);
            taskCoordinator.Register(CommonHelper.DicRoutes.Keys);
        }

        public void Stop()
        {
            LogHelper.Log(new LogDefinition(LogLevel.Info, "TaskCoordinatorHandler:Stop..."));
            taskCoordinator.UnRegister();

            //while (CommonHelper.DicTasks.Values.Any(t => t.Status == TaskStatus.Running))
            //{
            //    Task.Delay(10000).Wait();
            //}
        }
    }
}
