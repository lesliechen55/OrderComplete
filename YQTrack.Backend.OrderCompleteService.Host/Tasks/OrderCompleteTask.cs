using System;
using System.Collections.Generic;
using System.Linq;
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
    public class OrderCompleteTask : ICompleteTask
    {
        //private readonly IOrderCompleteBll _orderCompleteBll;

       
        private readonly bool isAsync;
        /// <summary>
        ///  发送当前订单数量信息
        /// </summary>
        public event EventHandler<OrderCompleteMsgEventArgs> SendOrderCompleteMsgEvent;

        public OrderCompleteTask()
        {
            //this._orderCompleteBll = _orderCompleteBll;
            //this._orderCompleteBll.SetDataRoute(1, 1);
            //_Cache = new RedisCache.RedisService();
          
            isAsync = SettingManagerComplete.Setting.TaskAsync;
        }

        //public void Execute()
        //{
        //    Log.LogHelper.Log(new Log.LogDefinition(Log.LogLevel.Verbose, "OrderCompleteTask.Execute 执行任务"));
        //    // _orderCompleteBll.ExecuteAutoCompleteAsync(string.Empty, SettingManager.Setting.StartIndex, SettingManager.Setting.EndIndex);
        //    //LastUpdateTime =_orderCompleteBll.LastUpdateTime;

        //    _orderCompleteBll.DoWork(SettingManager.Setting.NodeId, 1,
        //        SettingManager.Setting.UserPageSize,
        //        SettingManager.Setting.StartIndex,
        //        SettingManager.Setting.EndIndex,
        //        SettingManager.Setting.IsConnectionCount,
        //        SettingManager.Setting.MaxConnectionCount,
        //        SettingManager.Setting.LoopWaitSeconds
        //        );

        //}


        public void Execute(List<OrderCompleteItem> userDbNos = null)
        {
            if (userDbNos == null)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderCompleteTask.ExcecuteAuto 执行任务"));
                
                var strDbSeller = Enum.GetName(typeof(YQDbType), YQDbType.Seller);
                var dbTypeRule =Sharding.DBShardingRouteFactory.GetDBTypeRule(strDbSeller);

                var nodeRules = dbTypeRule.NodeRoutes;
                var nodeCount = nodeRules.Count;
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
                        service.SendOrderCompleteMsgEvent += Service_SendOrderCompleteMsgEvent;
                        service.DoWork(nodeItem.Value.NodeId, dbRuleItem.Value.DBNo,
                            SettingManagerComplete.Setting.UserPageSize,
                            SettingManagerComplete.Setting.StartIndex,
                            SettingManagerComplete.Setting.EndIndex,
                            SettingManagerComplete.Setting.IsConnectionCount,
                            SettingManagerComplete.Setting.MaxConnectionCount,
                            SettingManagerComplete.Setting.LoopWaitSeconds,
                            SettingManagerComplete.Setting.MsgRepeatInsertDays,
                            SettingManagerComplete.Setting.OrderPageSize,
                            SettingManagerComplete.Setting.SemaphoreCount
                            );
                        //service.ExecuteAutoArchivedCurrentUsers();

                    }

                }
            }
            else
            {
                LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderCompleteTask.ExcecuteAuto 执行任务,指定的数据库编号和用户索引"));
                var profileBll = FactoryContainer.Create<IUserBLL>();
                profileBll.SetDataRoute(new DataRouteModel());

                Parallel.ForEach(userDbNos, (userDborderItem, UserDborderState) =>
                {
                    var service = new OrderAutoCompleteService(profileBll);
                    service.SendOrderCompleteMsgEvent += Service_SendOrderCompleteMsgEvent;
                    int startIndex = userDborderItem.UserStartIndex <= 0 ? SettingManagerComplete.Setting.StartIndex : userDborderItem.UserStartIndex;
                    int endIndex = userDborderItem.UserEndIndex <= 0 ? SettingManagerComplete.Setting.EndIndex : userDborderItem.UserEndIndex;
                    service.DoWork(userDborderItem.NodeId, userDborderItem.DbNo,
                        SettingManagerComplete.Setting.UserPageSize,
                        startIndex,
                        endIndex,
                        SettingManagerComplete.Setting.IsConnectionCount,
                        SettingManagerComplete.Setting.MaxConnectionCount,
                        SettingManagerComplete.Setting.LoopWaitSeconds,
                        SettingManagerComplete.Setting.MsgRepeatInsertDays,
                        SettingManagerComplete.Setting.OrderPageSize,
                        SettingManagerComplete.Setting.SemaphoreCount
                        );

                    //service.ExecuteAutoArchivedCurrentUsers();
                });
            }
        }

        private void Service_SendOrderCompleteMsgEvent(object sender, OrderCompleteMsgEventArgs e)
        {
            if (SendOrderCompleteMsgEvent != null)
            {
                SendOrderCompleteMsgEvent(this, e);
            }
        }
    }
}