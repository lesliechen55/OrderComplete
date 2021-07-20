using FluentScheduler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YQTrack.Backend.OrderComplete.DTO;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrack.Backend.OrderCompleteService.Host.Schedule;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host.V2
{
    public class AllTaskManager : Registry
    {
        public static AllTaskManager Instance { get; } = new AllTaskManager();

        public void Start()
        {
            OrderCompleteTaskCoordinator.Default.Start();
            JobManager.Initialize(new AllTaskManager());//开始定时任务
        }

        public void Stop()
        {
            FluentScheduler.JobManager.Stop();//停止定时任务
            OrderCompleteTaskCoordinator.Default.Stop();
        }


        public AllTaskManager()
        {
            //让Job进行单线程跑，避免没跑完时的重复执行。(全局)
            NonReentrantAsDefault();

            #region 测试用
            //立即执行每两秒一次的计划任务。测试用 
            //Schedule<AutoComplateTimeTask>().ToRunNow().AndEvery(2).Seconds();

            ////让Job进行单线程跑，通过NonReentrant()可以防止任务并发，避免没跑完时的重复执行。(单个任务)
            //Schedule<RandomAutoComplateTimeTask>().NonReentrant().ToRunNow().AndEvery(5).Seconds();

            //Schedule<WarnTimeTask>().ToRunNow().AndEvery(1).Seconds();
            #endregion

            //随机——立即执行每40分钟一次的计划任务。
            Schedule<RandomAutoComplateTimeTask>().ToRunNow().AndEvery(40).Minutes();

            //自动完成——每天上午08: 05执行
            Schedule<AutoComplateTimeTask>().ToRunEvery(1).Days().At(08, 05);

            //提醒——每天上午08: 30执行
            Schedule<WarnTimeTask>().ToRunEvery(1).Days().At(08, 30);
        }

    }
}
