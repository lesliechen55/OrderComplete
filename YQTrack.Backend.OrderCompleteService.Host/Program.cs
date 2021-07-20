using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.OrderComplete.IService;
using YQTrack.Backend.OrderComplete.MQHelpr;
using YQTrack.Backend.OrderCompleteService.Host.V2;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            int worker, ioworker;
            ThreadPool.GetMaxThreads(out worker, out ioworker);
            ThreadPool.SetMinThreads(worker > 1024 ? 1024 : worker, ioworker > 1024 ? 1024 : ioworker);
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 1024;
            ServicePointManager.MaxServicePointIdleTime = 150000;
            ServicePointManager.UseNagleAlgorithm = false;

            YQTrackV6.Log.LogHelper.Log(new YQTrackV6.Log.LogDefinition(YQTrackV6.Log.LogLevel.Info, "开始启动"));
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            var builder = new ContainerBuilder();

            var assemblys = Directory.GetFiles(Environment.CurrentDirectory, "*.dll").Select(Assembly.LoadFile);
         
            builder.RegisterAssemblyTypes(assemblys.ToArray())
                .Where(t => typeof(IDependency).IsAssignableFrom(t))
                .AsImplementedInterfaces();
            var container = builder.Build();
            FactoryContainer.Init(container);

            //YQTrack.LanguageHelper.LanguageManage.Init(System.Web.HttpContext.Current.Server.MapPath("/ResJson"));
            // YQTrack.LanguageHelper.LanguageWcf.Init("192.168.1.200");
            SettingsHelper.Init();
            //if (args != null && args.Length > 0)
            //{
            //    YQTrackV6.Setting.SettingManager.InstanceName = "OrderCompleteService" + args[0];
            //    LogHelper.Log(new LogDefinition(LogLevel.Debug, "配置实例名称，InstanceName={0}"), YQTrackV6.Setting.SettingManager.InstanceName);
            //}

            YQTrack.Backend.UpgradeConsole.UpgradeService.Instance.InitUpgradeConfig(10, OnBeforeRestart);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new OrderComplete());
            Application.Run(new FrmOrderComplete());
        }


        public static bool OnBeforeRestart()
        {
            bool result = false;
            SellerOrderCompleteMQConsumerService.Instance.UnSubscribe();
            //OrderCompleteTaskManager.Instance.Stop();
            //OrderCompleteCountWarnTaskManager.Instance.Stop();
            AllTaskManager.Instance.Stop();
            result = true;
            Thread.Sleep(2000);
            return result;
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogHelper.Log(new LogDefinition(LogLevel.Error, "任务的未观察到的异常（TaskScheduler_UnobservedTaskException）"),
                e.Exception);
            e.Exception.Handle(error => { return true; });
            e.SetObserved();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //MessageBox.Show("抱歉，您的操作没有能够完成，请再试一次或者联系软件提供商");
            LogHelper.Log(new LogDefinition(LogLevel.Error, "线程发生异常（Application_ThreadException）"), e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //MessageBox.Show("抱歉，您的操作没有能够完成，请再试一次或者联系软件提供商");
            
            Exception ex = e.ExceptionObject as Exception;
            LogHelper.Log(new LogDefinition(LogLevel.Error, "未捕捉到异常（CurrentDomain_UnhandledException）"), ex);
        }
    }
}
