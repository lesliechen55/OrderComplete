using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YQTrack.Backend.Enums;
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
    public class RandomSellerOrderComplateWorkService : BaseSellerOrderComplateWorkService, IOrderComplateWorkService
    {
        #region <构造方法>
        private readonly IUserBLL _userBll;
        DataRouteModel _drModel;

        public RandomSellerOrderComplateWorkService(DataRouteModel drModel, bool isAsync, int semaphoreCount)
        {
            _drModel = drModel;

            var profileBll = FactoryContainer.Create<IUserBLL>();
            profileBll.SetDataRoute(_drModel);
            _userBll = profileBll;

            InitData(isAsync, semaphoreCount);
        }
        #endregion

        public void DoComplateWork() {

        }

        public async Task DoWorkRandomComplete()
        {
            var mode = new CommonHelper().GetRandomMode();

            LogHelper.Log(new LogDefinition(LogLevel.Info, "RandomSellerOrderComplateWorkService.DoWorkRandomComplete.执行任务.nodeId={0},dbno={1},mode={2}"), _drModel.NodeId, _drModel.DbNo, mode);
            int loopPageSize = 100;
            try
            {
                var dto = new UserProfileConfigPageDTO
                {
                    NodeId = _drModel.NodeId,
                    DBNo = _drModel.DbNo,
                    StartIndex = 0,
                    PageSize = loopPageSize,
                    FUserId = 0
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

                LogHelper.Log(new LogDefinition(LogLevel.Info, "RandomSellerOrderComplateWorkService.DoWorkRandomComplete.执行任务.结束.nodeId={0},dbno={1},mode={2}"), _drModel.NodeId, _drModel.DbNo, mode);

                ExecuteAutoArchivedCurrentUsers(_drModel.DbNo);

                //重试发生错误的数据
                RetryErrorUserOrderComplete(_drModel.DbNo);

            }
            catch (Exception ex)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Info, "RandomSellerOrderComplateWorkService.DoWorkRandomComplete.发生异常"), ex);
            }
        }

    }
}
