using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.Sharding.Router;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.V2
{
    public class CommonHelper
    {
        public static readonly Dictionary<string, int> DicRoutes = new Dictionary<string, int>();
        public static readonly Dictionary<string, int> DicTasks = new Dictionary<string, int>();

        public static CommonHelper Instance { get; } = new CommonHelper();

        private static List<DataRouteModel> _sellRules;
        public static List<DataRouteModel> SellRules
        {
            get
            {
                if (_sellRules == null)
                {
                    _sellRules = Instance.GetNodeRules(YQDbType.Seller);
                }
                return _sellRules;
            }
            set { _sellRules = value; }
        }

        private static List<DataRouteModel> _buyRules;
        public static List<DataRouteModel> BuyRules
        {
            get
            {
                if (_buyRules == null)
                {
                    _buyRules = Instance.GetNodeRules(YQDbType.Buyer);
                }
                return _buyRules;
            }
            set { _buyRules = value; }
        }


        public List<DataRouteModel> GetNodeRules(YQDbType type)
        {
            var strDbBuyer = Enum.GetName(typeof(YQDbType), type);
            var dbTypeRule = Sharding.DBShardingRouteFactory.GetDataRouteModels(strDbBuyer);
            return dbTypeRule.OrderBy(e => e.DbNo).ToList();//排序
        }

        public List<DataRouteModel> GetNodeRulesTaskSchedule(YQDbType type)
        {
            var strDbBuyer = Enum.GetName(typeof(YQDbType), type);

            List<DataRouteModel> list = new List<DataRouteModel>();

            if (DicTasks.Values.Count <= 0) return null;

            if (strDbBuyer == "Buyer")
            {
                foreach (var item in DicTasks.Values)
                {
                    if (BuyRules.Count >= item)
                    {
                        list.Add(BuyRules[item]);

                        LogHelper.Log(new LogDefinition(LogLevel.Info, $"GetNodeRulesTaskSchedule 执行任务,Buyer:{BuyRules[item]}"));
                    }
                }
            }
            else
            {
                foreach (var item in DicTasks.Values)
                {
                    if (SellRules.Count >= item)
                    {
                        list.Add(SellRules[item]);
                        LogHelper.Log(new LogDefinition(LogLevel.Info, $"GetNodeRulesTaskSchedule 执行任务,Seller:{SellRules[item]}"));
                    }
                }
            }
            return list;
        }

        #region 暂时用GetDBRulesCount方法替代
        public NodeRouteDictionary GetNodeRoutesByType(YQDbType type)
        {
            var strDbSeller = Enum.GetName(typeof(YQDbType), type);
            var objDbSellerRule = Sharding.DBShardingRouteFactory.GetDBTypeRule(strDbSeller);
            var nodes = objDbSellerRule.NodeRoutes;
            return nodes;
        }

        public int GetDBRulesCount(NodeRouteDictionary nRoute)
        {
            int count = nRoute.ToArray()[0].Value.DBRules.Count;
            return count;
        }
        #endregion

        public int GetDBRulesCount(YQDbType type)
        {
            var strDbSeller = Enum.GetName(typeof(YQDbType), type);
            if (strDbSeller == "Buyer")
            {
                return BuyRules.Count;
            }
            return SellRules.Count;
        }


        //随机数
        public int GetRandomMode()
        {
            List<int> modeRunList = new List<int>();

            int[] modeArray = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int aryCount = modeArray.Length;
            int mode = 0;
            do
            {
                Random random = new Random();
                var index = random.Next(0, aryCount);
                mode = modeArray[index];
                if (modeRunList.Contains(mode))
                {
                    if (modeRunList.Count == aryCount)
                    {
                        modeRunList.Clear();
                    }
                    continue;
                }
                else
                {
                    modeRunList.Add(mode);
                    break;
                }
            } while (true);

            return mode;
        }

    }
}
