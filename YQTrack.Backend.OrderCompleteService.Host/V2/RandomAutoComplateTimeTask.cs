using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrack.Backend.OrderCompleteService.V2;
using YQTrack.Backend.OrderCompleteService.V2.Complate;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host.V2
{
    public class RandomAutoComplateTimeTask : IJob
    {
        public void Execute()
        {
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "RandomAutoComplateTimeTask.Execute 执行任务"));

            var nodeRuleList = new CommonHelper().GetNodeRulesTaskSchedule(YQDbType.Seller);
            foreach (var dbRuleItem in nodeRuleList)
            {
                var service = new RandomSellerOrderComplateWorkService(dbRuleItem, 
                                  SettingManagerComplete.Setting.TaskAsync, SettingManagerComplete.Setting.SemaphoreCount);
                var task1 = service.DoWorkRandomComplete();
                task1.Wait();
            }
        }

    }
}
