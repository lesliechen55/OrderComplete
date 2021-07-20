using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.DTO;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.OrderComplete.IService.V2;
using YQTrack.Backend.OrderComplete.Model;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.V2.Complate
{
    public class AutoOrderComplateWorkService : BaseSellerOrderComplateWorkService, IOrderComplateWorkService
    {
        #region <构造方法>
        private readonly IUserBLL _userBll;
        DataRouteModel _drModel;

        bool _isAsync = false;
        int _semaphoreCount = 10;

        public AutoOrderComplateWorkService(DataRouteModel drModel, bool isAsync, int semaphoreCount)
        {
            _drModel = drModel;

            var profileBll = FactoryContainer.Create<IUserBLL>();
            profileBll.SetDataRoute(_drModel);
            _userBll = profileBll;

            _isAsync = isAsync;
            _semaphoreCount = semaphoreCount;
        }
        #endregion

        public void DoComplateWork()
        {
            InitData(_isAsync, _semaphoreCount);

            GetUserConfigsByPage(_userBll, _drModel.NodeId, _drModel.DbNo, true);
        }


        public async Task DoWorkRandomComplete()
        {
            await Task.Run(null);
        }


    }
}
