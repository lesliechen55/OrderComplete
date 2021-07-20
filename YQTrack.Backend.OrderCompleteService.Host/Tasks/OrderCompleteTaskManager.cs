using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using YQTrack.Backend.OrderComplete.DTO;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    public class OrderCompleteTaskManager
    {
        private OrderCompleteTaskThread _taskThread;

        private ConcurrentQueue<OrderCompleteMsgEventArgs> _QueueOrderCompleteMsg;

        private OrderCompleteTaskManager()
        {
            _QueueOrderCompleteMsg = new ConcurrentQueue<OrderCompleteMsgEventArgs>();
            Init();
        }

        public static OrderCompleteTaskManager Instance { get; } = new OrderCompleteTaskManager();

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
            var task = new OrderCompleteTask();
            task.SendOrderCompleteMsgEvent += Task_SendOrderCompleteMsgEvent;
            string completeTime = SettingManagerComplete.Setting.CompleteTime;
            if (string.IsNullOrWhiteSpace(completeTime))
            {
                completeTime = "8:05";
            }
            _taskThread = new OrderCompleteTaskThread(task, 1, completeTime);
            // _taskThread = new OrderCompleteTaskThread(task, SettingManager.Setting.Seconds);
            //_taskThread.RunOnlyOnce = true;
        }

        private void Task_SendOrderCompleteMsgEvent(object sender, OrderCompleteMsgEventArgs e)
        {
            LogHelper.Log(new LogDefinition(LogLevel.Info, "接收到消息nodeId={0},dbNo={1},userIndex={2},userCount={3}"), e.NodeId, e.DbNo, e.UserCurrentIndex, e.UserCount);
            _QueueOrderCompleteMsg.Enqueue(e);
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
            bool isRun = false;
            if (TaskThread.StatrTime != null)
            {
                strTime = string.Format(CultureInfo.InvariantCulture, "最后一次执行时间:{0}", TaskThread.StatrTime.Value);
                isRun = true;
            }

            if (TaskThread.EndTime != null)
            {
                isRun = false;
                strTime += Environment.NewLine + string.Format(CultureInfo.InvariantCulture, "执行结束,时间:{0}", TaskThread.EndTime.Value);
            }

            if (isRun)
            {
                strTime += Environment.NewLine + "正在执行中....";
            }

            return strTime;
        }

        public string GetNextRunTime()
        {
            string strTime = "-";

            strTime = string.Format(CultureInfo.InvariantCulture, "下次执行时间:{0}", TaskThread.NextRunTime);
            return strTime;
        }

        public ConcurrentQueue<OrderCompleteMsgEventArgs> QueueOrderCompleteMsg
        {
            get { return _QueueOrderCompleteMsg; }
        }
    }
}