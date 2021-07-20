using System;
using System.Threading;

using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using YQTrackV6.Log;
using YQTrack.Backend.OrderComplete.Model;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    public class OrderCompleteTaskThread : IDisposable
    {
        private readonly ICompleteTask _completeTask;
        private bool _disposed;
        private int _warnDay;
        private bool _isInitTime = false;
        private string _warnTime;
        private Timer _timer;
        private DateTime _nextRunTime;

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public OrderCompleteTaskThread(ICompleteTask completeTask,int warnDay,string exceTime)
        {
            // completeTask = new OrderCompleteTask()
            
            this._completeTask = completeTask;
            _warnDay = warnDay;
            _warnTime = exceTime;
            _isInitTime = true;
            InitSeconds();
        }

        public OrderCompleteTaskThread(ICompleteTask completeTask, int seconds)
        {
            // completeTask = new OrderCompleteTask()

            this._completeTask = completeTask;
            if (seconds < 1) seconds = 10 * 60;
            Seconds = seconds;
        }

        private void InitSeconds()
        {
            long seconds = 0;
            //计算什么时候提醒,假如每天提醒一次
            var currentDateTime = DateTime.Now;
            int currHour = currentDateTime.Hour;
            int currMinute = currentDateTime.Minute;

            if (String.IsNullOrWhiteSpace(_warnTime)) _warnTime = "00:20";
            string[] arrayTime = _warnTime.Split(new Char[] { ':' });

            int hour = 0;
            int minute = 0;

            if (arrayTime.Length > 1)
            {
                hour = Convert.ToInt32(arrayTime[0]);
                minute = Convert.ToInt32(arrayTime[1]);
            }
            if (currHour == hour && currMinute < minute)
            {
                //当前时间 小于 执行时间
                seconds = (minute - currMinute) * 60;
            }
            else {
                // 1 2 3 天
                if (_warnDay < 1)
                {
                    _warnDay = 1;
                }
                var excuteTime = currentDateTime.AddDays(_warnDay);
                //表示每天执行一次,计算warnTime什么时候执行：00:20
                var exeTime = new DateTime(excuteTime.Year, excuteTime.Month, excuteTime.Day, hour, minute+1, 0);

                TimeSpan tsResult = exeTime - currentDateTime;
                seconds = (long)tsResult.TotalSeconds;
            }
            if (seconds < 1) seconds = 10 * 60;
            Seconds = seconds;
        }

        public DateTime? StatrTime { private set; get; }

        public DateTime? EndTime { private set; get; }

        public DateTime NextRunTime
        {
            get { return _nextRunTime; }
        }

        /// <summary>
        ///     计时器 秒数
        /// </summary>
        public long Seconds { get; set; }

        /// <summary>
        ///     是否只运行一次
        /// </summary>
        public bool RunOnlyOnce { set; get; }

        /// <summary>
        ///     秒数换算为毫秒数
        /// </summary>
        public long Interval
        {
            get { return Seconds*1000; }
        }


        public void Dispose()
        {
            if ((_timer != null) && !_disposed)
            {
                lock (this)
                {
                    _timer.Dispose();
                    _timer = null;
                    _disposed = true;
                }
            }
        }

        public void InitTimer()
        {
            if (_timer == null)
            {
                _timer = new Timer(HandlerTimer, null, Interval, Interval);
                _nextRunTime = DateTime.Now.AddSeconds(Seconds);
            }
        }

        public void Run()
        {
            StatrTime = DateTime.Now;
            EndTime = null;
            if (Seconds <= 0)
            {
                return;
            }

            _completeTask.Execute();

            EndTime = DateTime.Now;

            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderCompleteTaskThread.Run 触发任务实行,开始时间:{0}，结束时间{1}"),
                StatrTime, EndTime);
        }

        public void RunManualWork(List<OrderCompleteItem> dbItemList)
        {
            StatrTime = DateTime.Now;

            _completeTask.Execute(dbItemList);

            EndTime = DateTime.Now;

            LogHelper.Log(new LogDefinition(LogLevel.Verbose, "OrderCompleteTaskThread.RunManualWork 触发任务实行,开始时间:{0}，结束时间{1}"),
                StatrTime, EndTime);
        }

        /// <summary>
        ///     定时触发方法
        /// </summary>
        /// <param name="state"></param>
        public void HandlerTimer(object state)
        {
           
            _timer.Change(-1, -1);
            Run();
            if (RunOnlyOnce)
            {
                Dispose();
            }
            else
            {
                if (_timer == null) return;
                if (_isInitTime)
                {
                    InitSeconds();
                }
               
                _timer.Change(Interval, Interval);
                _nextRunTime = DateTime.Now.AddSeconds(Seconds);
            }
        }
    }
}