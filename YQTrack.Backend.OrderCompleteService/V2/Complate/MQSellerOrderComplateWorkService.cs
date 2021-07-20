using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.OrderComplete.DTO;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.OrderComplete.MQHelpr;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.V2.Complate
{
    public class MQSellerOrderComplateWorkService : BaseSellerOrderComplateWorkService
    {
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
            //var _userBll = new IUserBLL();
            var profileBll = FactoryContainer.Create<IUserBLL>();
            profileBll.SetDataRoute(new DataRouteModel { });
            var _userBll = profileBll;

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

    }
}
