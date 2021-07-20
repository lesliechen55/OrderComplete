using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.OrderComplete.IService.V2;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.V2.Warn;
using YQTrack.Backend.Sharding.Router;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.V2
{
    //Buyer相关提醒
    public class BuyerWarnWorkService : IWarnWorkService
    {
        #region <构造方法>
        DataRouteModel _drModel;

        bool _isAsync = false;
        int _semaphoreCount = 10;

        public BuyerWarnWorkService(DataRouteModel drModel, bool isAsync, int semaphoreCount)
        {
            _drModel = drModel;
            _isAsync = isAsync;
            _semaphoreCount = semaphoreCount;
        }
        #endregion


        public void DoWarnWork()
        {
            ExecuteBuyer();
        }

        public void ExecuteBuyer()
        {
            //会员过期提醒处理
            try
            {
                var userMemberWarn = new UserMemberExpiresWarnWorkService(_drModel);
                userMemberWarn.DoWorkUserMemberGoingToExpiredWarn();
                userMemberWarn.DoWorkUserMemberExpiredWarn();
            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Verbose, "BuyerWarnWorkService.ExecuteBuyer.发生异常"), ex);
            }
        }



        //执行选中DB编号下用户,包含邮件提醒,<注:可以不用>
        public void DoEmailWarnWork(List<OrderCompleteItem> userDbNos)
        {
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "BuyerWarnWorkService.DoEmailWarnWork 执行任务,指定的数据库编号和用户索引"));

            Parallel.ForEach(userDbNos, (userDborderItem, UserDborderState) =>
            {
                var service = new Vr.SellerWarnWorkService(new DataRouteModel(),_isAsync,_semaphoreCount);
                service.DoWorkCompleteRecordCountNotify();
            });
        }

    }
}
