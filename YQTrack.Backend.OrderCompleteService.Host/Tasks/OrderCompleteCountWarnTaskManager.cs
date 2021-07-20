using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.Host.Config;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    public class OrderCompleteCountWarnTaskManager
    {
        private OrderCompleteTaskThread _taskThread;


        private OrderCompleteCountWarnTaskManager()
        {
            Init();
        }

        public static OrderCompleteCountWarnTaskManager Instance { get; } = new OrderCompleteCountWarnTaskManager();

        public OrderCompleteTaskThread TaskThread
        {
            get
            {
                if (_taskThread == null)
                {
                    Init();
                }
                return _taskThread;
            }
            set { _taskThread = value; }
        }

        public void Init()
        {
            var task = new OrderCompleteCountWarnTack();
           
            int day = SettingManagerComplete.Setting.WarnDay;
            string warnTime = SettingManagerComplete.Setting.WarnTime;

            _taskThread = new OrderCompleteTaskThread(task, day, warnTime);
            //_taskThread.RunOnlyOnce = true;
        }

        public void Start()
        {
            TaskThread.InitTimer();
        }

        public void Stop()
        {
            TaskThread.Dispose();
        }

        public void Run()
        {
            Task.Run(() => { TaskThread.Run(); });
        }

        public void RunManualWork(List<OrderCompleteItem> dbItemList)
        {
            Task.Run(() => { TaskThread.RunManualWork(dbItemList); });
        }

        public string GetLastRunTime()
        {
            var strTime = "-";
            if (TaskThread.StatrTime != null)
            {
                strTime = string.Format(CultureInfo.InvariantCulture, "最后一次执行时间:{0}", TaskThread.StatrTime.Value.ToString("yyyy-MM-dd hh:mm:ss"));
            }

            return strTime;
        }

        public string GetNextRunTime()
        {
            string strTime = "-";

            strTime = TaskThread.NextRunTime.ToString("yyyy-MM-dd hh:mm:ss");

            return strTime;
        }
    }
}