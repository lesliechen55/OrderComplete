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
using YQTrack.Backend.OrderCompleteService.V2;
using YQTrack.Backend.OrderCompleteService.V2.Complate;
using YQTrack.Backend.OrderCompleteService.V2.Warn;
using YQTrack.Backend.Sharding.Router;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Vr
{
    //Seller的相关提醒
    public class SellerWarnWorkService : BaseSellerOrderComplateWorkService, IWarnWorkService
    {
        #region <构造方法>
        private readonly IUserBLL _userBll;
        DataRouteModel _drModel;

        bool _isAsync = false;
        int _semaphoreCount = 10;

        public SellerWarnWorkService(DataRouteModel drModel, bool isAsync, int semaphoreCount)
        {
            _drModel = drModel;

            var profileBll = FactoryContainer.Create<IUserBLL>();
            profileBll.SetDataRoute(_drModel);
            _userBll = profileBll;

            _isAsync = isAsync;
            _semaphoreCount = semaphoreCount;
        }
        #endregion

        public void DoWarnWork()
        {
            DoWorkCompleteRecordCountNotify();
            SellerTrackNumAutoWarn();
        }


        private void SellerTrackNumAutoWarn()
        {
            //Seller单号数量不足的提醒，新版处理
            var sellerTrackNumWarnService = new SellerTrackNumAutoWarnWorkService(_drModel);

            //处理seller跟踪数预警
            sellerTrackNumWarnService.DoWorkSellerPayTrackNumWarn();

            //处理seller邮件数预警
            sellerTrackNumWarnService.DoWorkSellerPayEmailNumWarn();
        }

        public void DoWorkCompleteRecordCountNotify()
        {
            InitData(_isAsync, _semaphoreCount);

            GetUserConfigsByPageEmail(_userBll, _drModel.NodeId, _drModel.DbNo, true);//增加三个个参数
        }

    }
}
