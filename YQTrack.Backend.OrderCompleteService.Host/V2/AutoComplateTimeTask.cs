using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrack.Backend.OrderCompleteService.V2;
using YQTrack.Backend.OrderCompleteService.V2.Complate;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host.V2
{
    public class AutoComplateTimeTask : IJob
    {
        public void Execute()
        {
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "AutoComplateTimeTask.Execute 执行任务"));

            var nodeRuleList = new CommonHelper().GetNodeRulesTaskSchedule(YQDbType.Seller);
            foreach (var dbRuleItem in nodeRuleList)
            {
                var service = new AutoOrderComplateWorkService(dbRuleItem,
                                  SettingManagerComplete.Setting.TaskAsync, SettingManagerComplete.Setting.SemaphoreCount);
                service.DoComplateWork();
            }
        }

        public void RunManualWork(List<OrderCompleteItem> userDbNos)
        {
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "AutoComplateTimeTask.RunManualWork 执行任务,指定的数据库编号和用户索引"));

            Parallel.ForEach(userDbNos, (userDborderItem, UserDborderState) =>
            {
                var service = new AutoOrderComplateWorkService(new DataRouteModel(),
                                 SettingManagerComplete.Setting.TaskAsync, SettingManagerComplete.Setting.SemaphoreCount);
                service.DoComplateWork();
            });
        }

    }
}
