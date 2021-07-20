using System;
using YQTrackV6.Setting;

namespace YQTrack.Backend.OrderCompleteService.Host.Config
{
    public class SettingConfig : ISettingItem
    {
        public int StartIndex { set; get; }

        public int EndIndex { set; get; }

        public string LastUpdateTime { set; get; }


        ///// <summary>
        ///// 小时计算
        ///// </summary>
        //public int Hours { set; get; } = 24;

        /// <summary>
        ///     分钟
        /// </summary>
        public int Minute { set; get; } = 60;

        public int Seconds
        {
            get { return (Minute * 60); }
        }

        /// <summary>
        ///     机房ID
        /// </summary>
        public int NodeId { get; set; } = -1;

        public int LoopWaitSeconds { set; get; } = 30;

        public bool IsConnectionCount { set; get; } = false;

        public int MaxConnectionCount { get; set; } = 100;

        public int UserPageSize { set; get; } = 100;

        public int MsgRepeatInsertDays { set; get; } = 3;


        /// <summary>
        /// 每几天提醒一次
        /// </summary>
        public int WarnDay { set; get; }

        /// <summary>
        /// 每天提醒时间 00:20 统计提醒
        /// </summary>
        public string WarnTime { set; get; }

        /// <summary>
        /// 归档订单分页大小，每次获取多少个订单处理
        /// </summary>
        public int OrderPageSize { set; get; } = 100;

        /// <summary>
        /// 线程数控制数量 默认10个
        /// </summary>
        public int SemaphoreCount { set; get; } = 10;

        /// <summary>
        /// 订单完成执行时间
        /// </summary>
        public string CompleteTime { set; get; }

        /// <summary>
        ///     任务是否异步
        /// </summary>
        public bool TaskAsync { set; get; } = false;

        public void Validate()
        {
            if (Minute < 0)
            {
                throw new InvalidOperationException("分钟数不能小于0！");
            }
            if (StartIndex < 0)
            {
                throw new InvalidOperationException("开始索引数不能小于0！");
            }
            if (EndIndex < 0)
            {
                throw new InvalidOperationException("结束索引不能小于0！");
            }
            if (LoopWaitSeconds < 0)
            {
                throw new InvalidOperationException("循环等待秒数不能小于0！");
            }
            if (MaxConnectionCount < 0)
            {
                throw new InvalidOperationException("最大控制数不能小于0！");
            }
        }

        public void CreateSample()
        {
        }

    }


    public class SettingConfigBase : SettingBase
    {
        [SettingName("OrderAutoCompleteSetting")]
        public SettingConfig OrderAutoCompleteSetting { set; get; }


        public override void Validate()
        {
            base.Validate();
            var objSetting = OrderAutoCompleteSetting;
            if (objSetting != null)
            {
                objSetting.Validate();
            }
        }
    }

    public class SeleteItem
    {
        public string Text { set; get; }

        public string Value { set; get; }
    }
}