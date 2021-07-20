using System;
using System.Linq;
using YQTrack.Backend.Message.RabbitMQHelper;
using YQTrack.Backend.OrderComplete.Framework.Config;
using YQTrack.Backend.OrderComplete.MQHelpr;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrack.Backend.RedisCache;
using YQTrack.Backend.Seller.MQData.Helper;
using YQTrack.Backend.Seller.TrackInfo.ESHelper;
using YQTrack.Backend.Sharding;

using YQTrack.Backend.UserTrackSchedule.Order.Helper;
using YQTrackV6.Log;
using YQTrackV6.Setting;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    /// <summary>
    /// 配置操作
    /// </summary>
    public static class SettingsHelper
    {
        #region < 日志 >
        private static readonly LogDefinition LoadError = new LogDefinition(LogLevel.Fatal, "设置加载失败");
        private static readonly LogDefinition LoadForDb = new LogDefinition(LogLevel.Info, "设置加载成功(数据库)");
        private static readonly LogDefinition LoadForLocal = new LogDefinition(LogLevel.Error, "设置加载成功(本地)");
        private static readonly LogDefinition LoadForNone = new LogDefinition(LogLevel.Info, "设置加载成功(空)");
        #endregion

        #region < 属性 >
        /// <summary>
        /// 是否第一次加载
        /// </summary>
        public static bool IsLoadFirst { get; private set; } = true;

        /// <summary>
        /// 是否从数据库加载
        /// </summary>
        public static bool IsLoadForDB { get; private set; }


        #endregion

        #region < 方法 >
        /// <summary>
        /// 初始化读取设置
        /// </summary>
        /// <returns>是否读取成功</returns>
        public static bool Init()
        {
            if (IsLoadFirst)
            {
                IsLoadFirst = false;
                //注册所有设置类型
                try
                {
                    RedisCacheSettingsHelper.Register();

                    DBShardingSettingHelper.Register<SellerDBShardingSettingBase>();
                    DBShardingSettingHelper.Register<UserDBShardingSettingBase>();
                    DBShardingSettingHelper.Register<LogDBShardingSettingBase>();
                    DBShardingSettingHelper.Register<BuyerDBShardingSettingBase>();
                    DBShardingSettingHelper.Register<SellerMessageDBShardingSettingBase>();

                    YQTrackV6.Setting.SettingManager.Register<SettingConfigBase>();

                    //SettingHelper.Register();

                    //DBSettingsHelper.Register();
                    //
                    //RedisCacheSettingsHelper.Register();
                    //LanguageWcf.RegisterSetting();
                    //EmailSettings.Register();

                    //OrderPersistSettingHelper.Register();
                    //UserTrackSettingHelper.Register();
                    SellerOrderCompletMQSetting.Register();
                    SellerTrackInfoHandlerMQSetting.Register();

                    RabbitMQSettingCommlete.Register();
                    //FullTextIndexHelper.RabbitMQSetting.Register();
                    //FullTextIndexHelper.SellerElasticSearchSetting.Register();
                    //YQTrack.ECommerce.RabbitMqHelper.GatherSettingHelper.Register();

                    SellerElasticSearchSettingHelper.Register();
                    ScheduleRabbitMQSetting.Register();

                    YQTrack.Configuration.ConfigManager.Initialize<OrderCompleteTaskCoordinatorConfig>();
                }
                catch (InvalidOperationException /*ex*/)
                {

                }
                //读取设置
                if (Load())
                {
                    //    PersistConfiger.Init();
                    //    TrackCacheHelper.Init();

                    if (RabbitMQSettingCommlete.SettingsDefault.RabbitMQConfig != null)
                    {
                        RabbitMQSettingCommlete.SettingsDefault.RabbitMQConfig.ServiceName = "OrderCompleteService";
                        MessageHelper.Init(RabbitMQSettingCommlete.SettingsDefault.RabbitMQConfig);
                    }

                    var sellerDBConfig = SettingManager.Read<SellerDBShardingSettingBase>();
                    var userDBConfig = SettingManager.Read<UserDBShardingSettingBase>();
                    
                    DBShardingRouteFactory.InitDBShardingConfig(sellerDBConfig.DBShardingConfig);
                    DBShardingRouteFactory.InitDBShardingConfig(userDBConfig.DBShardingConfig);

                    var logDBConfig = SettingManager.Read<LogDBShardingSettingBase>();
                    DBShardingRouteFactory.InitDBShardingConfig(logDBConfig.DBShardingConfig);
                    var buyerDBConfig = SettingManager.Read<BuyerDBShardingSettingBase>();
                    DBShardingRouteFactory.InitDBShardingConfig(buyerDBConfig.DBShardingConfig);

                    var sellerMsgConfig = SettingManager.Read<SellerMessageDBShardingSettingBase>();
                    DBShardingRouteFactory.InitDBShardingConfig(sellerMsgConfig.DBShardingConfig);


                    //var rmqconfig = GatherSettingHelper.SettingsDefault.RabbitMq;
                    //rmqconfig.ServiceName = "Seller.OrderComplete";
                    //SendGatherHelper<DTO.Order.OrderPersistDto>.Init(rmqconfig,
                    //    GatherSettingHelper.SettingsDefault.RabbitExchange.Value,
                    //    GatherSettingHelper.SettingsDefault.RabbitNode.ToArray());

                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 读取设置
        /// </summary>
        /// <returns>是否读取成功</returns>
        public static bool Load()
        {
            //读取设置
            SettingLoadResult result = YQTrackV6.Setting.SettingManager.Load();
          
            //判断读取结果
            switch (result.ResultType)
            {
                case SettingLoadResultType.LoadError:
                    IsLoadForDB = false;
                    LogHelper.Log(LoadError, result.Exception);
                    return false;
                case SettingLoadResultType.LoadFromDB:
                    IsLoadForDB = true;
                    LogHelper.Log(LoadForDb);
                    return true;
                case SettingLoadResultType.LoadFromLocal:
                    IsLoadForDB = false;
                    LogHelper.Log(LoadForLocal, result.Exception);
                    return true;
                case SettingLoadResultType.LoadFromNone:
                default:
                    IsLoadForDB = true;
                    LogHelper.Log(LoadForNone);
                    return true;
            }
        }
        #endregion
    }
}