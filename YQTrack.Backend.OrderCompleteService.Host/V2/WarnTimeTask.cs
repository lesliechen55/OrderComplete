using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrack.Backend.OrderCompleteService.V2;
using YQTrack.Backend.OrderCompleteService.V2.Complate;
using YQTrack.Backend.OrderCompleteService.V2.Warn;
using YQTrack.Backend.OrderCompleteService.Vr;
using YQTrack.Backend.Sharding.Router;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host.V2
{
    public class WarnTimeTask : IJob
    {
        public void Execute()
        {
            ExecuteBuyer();
            ExecuteSeller();
        }

        private void ExecuteSeller()
        {
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "WarnTimeTask.ExecuteSeller 执行任务Seller提醒服务"));

            var nodeRuleList = new CommonHelper().GetNodeRulesTaskSchedule(YQDbType.Seller);
            foreach (var dbRuleItem in nodeRuleList)
            {
                var service = new SellerWarnWorkService(dbRuleItem, SettingManagerComplete.Setting.TaskAsync, SettingManagerComplete.Setting.SemaphoreCount);
                service.DoWarnWork();
            }
        }

        private void ExecuteBuyer()
        {
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "WarnTimeTask.ExecuteBuyer 执行任务Buyer提醒服务"));

            var nodeRuleList = new CommonHelper().GetNodeRulesTaskSchedule(YQDbType.Buyer);
            foreach (var dbRuleItem in nodeRuleList)
            {
                var buyer = new BuyerWarnWorkService(dbRuleItem, SettingManagerComplete.Setting.TaskAsync, SettingManagerComplete.Setting.SemaphoreCount);
                buyer.DoWarnWork();
            }
        }


        //邮件提醒,<注:可以不用>
        public void DoEmailWarnWork(List<OrderCompleteItem> userDbNos)
        {
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "WarnTimeTask.DoEmailWarnWork 执行任务,指定的数据库编号和用户索引"));

            Parallel.ForEach(userDbNos, (userDborderItem, UserDborderState) =>
            {
                var service = new Vr.SellerWarnWorkService(new DataRouteModel(), SettingManagerComplete.Setting.TaskAsync, SettingManagerComplete.Setting.SemaphoreCount);
                service.DoWarnWork();
            });
        }

    }
}
