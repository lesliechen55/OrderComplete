using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.Models.Enums;

using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    public class OrderCompleteCountWarnTack : ICompleteTask
    {

        private readonly bool isAsync;

        public OrderCompleteCountWarnTack()
        {
            isAsync = SettingManagerComplete.Setting.TaskAsync;
        }

        public void Execute(List<OrderCompleteItem> userDbNos = null)
        {
            if (userDbNos == null)
            {
                LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderCompleteCountWarnTack.Excecute 执行任务"));

                ExecuteSeller();

                ExecuteBuyer();

            }
            else
            {
                LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderCompleteCountWarnTack.Excecute 执行任务,指定的数据库编号和用户索引"));
                var profileBll = FactoryContainer.Create<IUserBLL>();
                profileBll.SetDataRoute(new DataRouteModel());

                Parallel.ForEach(userDbNos, (userDborderItem, UserDborderState) =>
                {
                    int startIndex = userDborderItem.UserStartIndex <= 0 ? SettingManagerComplete.Setting.StartIndex : userDborderItem.UserStartIndex;
                    int endIndex = userDborderItem.UserEndIndex <= 0 ? SettingManagerComplete.Setting.EndIndex : userDborderItem.UserEndIndex;

                    var service = new OrderAutoWarnService(profileBll);
                    service.DoWorkCompleteRecordCountNotify(userDborderItem.NodeId, userDborderItem.DbNo,
                        SettingManagerComplete.Setting.UserPageSize,
                        startIndex,
                        endIndex,
                        SettingManagerComplete.Setting.IsConnectionCount,
                        SettingManagerComplete.Setting.MaxConnectionCount,
                        SettingManagerComplete.Setting.LoopWaitSeconds,
                        SettingManagerComplete.Setting.MsgRepeatInsertDays
                        );
                });
            }
        }

        private static void ExecuteSeller()
        {
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderCompleteCountWarnTack.ExecuteSeller 执行任务Seller提醒服务"));
            var strDbSeller = Enum.GetName(typeof(YQDbType), YQDbType.Seller);
            var dbTypeRule = Sharding.DBShardingRouteFactory.GetDBTypeRule(strDbSeller);

            var nodeRules = dbTypeRule.NodeRoutes;
            var nodeCount = nodeRules.Count;
            //获取数据库节点
            foreach (var nodeItem in nodeRules)
            {

                var dbRules = nodeItem.Value.DBRules;
                var nodeId = nodeItem.Value.NodeId;
                foreach (var dbRuleItem in dbRules)
                {
                    var dbno = dbRuleItem.Value.DBNo;
                    var profileBll = FactoryContainer.Create<IUserBLL>();
                    profileBll.SetDataRoute(new DataRouteModel
                    {
                        NodeId = Convert.ToByte(nodeId),
                        DbNo = Convert.ToByte(dbno) /*, TableNo = Convert.ToByte(dto.FTableNo)*/
                    });


                    var service = new OrderAutoWarnService(profileBll);

                    service.DoWorkCompleteRecordCountNotify(nodeId, dbno,
                        SettingManagerComplete.Setting.UserPageSize,
                        SettingManagerComplete.Setting.StartIndex,
                        SettingManagerComplete.Setting.EndIndex,
                        SettingManagerComplete.Setting.IsConnectionCount,
                        SettingManagerComplete.Setting.MaxConnectionCount,
                        SettingManagerComplete.Setting.LoopWaitSeconds,
                        SettingManagerComplete.Setting.MsgRepeatInsertDays
                        );


                    foreach (var item in dbRuleItem.Value.TableRoutes)
                    {
                        //Seller单号数量不足的提醒，新版处理
                        var sellerTrackNumWarnService = new SellerTrackNumAutoWarnService(nodeId, dbno, item.TableNo);
                        // 处理seller跟踪数预警
                        sellerTrackNumWarnService.DoWorkSellerPayTrackNumWarn();
                        // 处理seller邮件数预警
                        sellerTrackNumWarnService.DoWorkSellerPayEmailNumWarn();
                    }
                }

            }
        }

        private void ExecuteBuyer()
        {
            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderCompleteCountWarnTack.ExecuteBuyer 执行任务Buyer提醒服务"));
            var strDbBuyer = Enum.GetName(typeof(YQDbType), YQDbType.Buyer);
            var dbTypeRule = Sharding.DBShardingRouteFactory.GetDBTypeRule(strDbBuyer);
            var nodeRules = dbTypeRule.NodeRoutes;
            var nodeCount = nodeRules.Count;
            //获取数据库节点
            foreach (var nodeItem in nodeRules)
            {
                var dbRules = nodeItem.Value.DBRules;
                var nodeId = nodeItem.Value.NodeId;
                foreach (var dbRuleItem in dbRules)
                {
                    var dbno = dbRuleItem.Value.DBNo;
                    //会员过期提醒处理
                    foreach (var item in dbRuleItem.Value.TableRoutes)
                    {
                        //会员过期提醒处理
                        try
                        {
                            var userMemberWarn = new UserMemberExpiresWarnService(nodeId, dbno, item.TableNo);
                            userMemberWarn.DoWorkUserMemberGoingToExpiredWarn();
                            userMemberWarn.DoWorkUserMemberExpiredWarn();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderCompleteCountWarnTack.ExecuteBuyer.发生异常"), ex);
                        }
                    }
                }
            }
        }


    }
}
