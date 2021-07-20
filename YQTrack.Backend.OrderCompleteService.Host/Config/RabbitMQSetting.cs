using System;
using System.Collections.Generic;
using YQTrackV6.RabbitMQ.Config;
using YQTrackV6.Setting;

namespace YQTrack.Backend.OrderCompleteService.Host.Config
{

    public class RabbitMQSettingCommlete : SettingBase
    {
        /// <summary>
        /// RabbitMQ配置
        /// </summary>
        [SettingName("RabbitMQ")]
        public YQTrackV6.RabbitMQ.Config.RabbitMqConfig RabbitMQConfig { get; set; }



        #region < 属性 >

        /// <summary>
        /// 初始化设置
        /// </summary>
        public static RabbitMQSettingCommlete SettingsDefault => SettingManager.Read<RabbitMQSettingCommlete>();

        #endregion

        #region < 方法 >

        /// <summary>
        /// 初始化读取设置
        /// </summary>
        public static void Register()
        {
            SettingManager.Register<RabbitMQSettingCommlete>();
        }

        #endregion





    }

 
}
