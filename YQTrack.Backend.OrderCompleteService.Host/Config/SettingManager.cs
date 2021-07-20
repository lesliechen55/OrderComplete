using System;
using System.IO;
using System.Text;

using YQTrackV6.Setting;
using System.Diagnostics.CodeAnalysis;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host.Config
{
    public static class SettingManagerComplete
    {
        private static readonly string _FilePath = Path.Combine(Environment.CurrentDirectory, "setting.json");

        private static SettingConfig _settingModel;

        #region < 日志 >
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private static LogDefinition LoadError = new LogDefinition(LogLevel.Fatal, "设置加载失败");
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private static LogDefinition LoadForDB = new LogDefinition(LogLevel.Info, "设置加载成功(数据库)");
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private static LogDefinition LoadForLocal = new LogDefinition(LogLevel.Error, "设置加载成功(本地)");
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private static LogDefinition LoadForNone = new LogDefinition(LogLevel.Info, "设置加载成功(空)");
        #endregion

        static SettingManagerComplete()
        {
            // Load();
            LoadNew();
        }


        public static SettingConfig Setting
        {
            get
            {
                if (_settingModel == null)
                {
                    LoadNew();
                }
                return _settingModel;
            }
        }

        public static void Save()
        {
            try
            {
                File.WriteAllText(_FilePath,  YQTrackV6.Common.Utils.SerializeExtend.MySerializeToJson(_settingModel), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void LoadNew()
        {
            try
            {
                //Environment.SetEnvironmentVariable("YQTrackV6_SettingDB", "Data Source=192.168.1.103;Initial Catalog=YQTrackV6_Setting;User ID=sa;Password=sa17track.net",EnvironmentVariableTarget.Machine);
                // Environment.SetEnvironmentVariable("YQTrackV6_HostName", "192.168.1.103", EnvironmentVariableTarget.Machine);
                //Environment.SetEnvironmentVariable("YQTrackV6_GroupName", "192.168.1.103", EnvironmentVariableTarget.Machine);

                //YQTrackV6.Setting.SettingManager.Register<SettingConfigBase>();
                //var result = YQTrackV6.Setting.SettingManager.Load();
                ////判断读取结果
                //bool isLoad = false;
                //switch (result.ResultType)
                //{
                //    case SettingLoadResultType.LoadError:
                       
                //        LogHelper.Log(LoadError, result.Exception);
                //        break;
                //    case SettingLoadResultType.LoadFromDB:
                //        LogHelper.Log(LoadForDB);
                //        isLoad = true;
                //        break;
                //    case SettingLoadResultType.LoadFromLocal:
                //        LogHelper.Log(LoadForLocal, result.Exception);
                //        isLoad = true;
                //        break;
                //    case SettingLoadResultType.LoadFromNone:
                //    default:
                //        LogHelper.Log(LoadForNone);
                //        isLoad = true;
                //        break;
                //}
                //if (isLoad)
                //{
                    var objSetting = YQTrackV6.Setting.SettingManager.Read<SettingConfigBase>();
                    if (objSetting != null)
                    {
                        _settingModel = objSetting.OrderAutoCompleteSetting;
                    }
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(_FilePath))
                {
                    var strConfig = File.ReadAllText(_FilePath, Encoding.UTF8);
                    var data = YQTrackV6.Common.Utils.SerializeExtend.MyDeserializeFromJson<SettingConfig>(strConfig);
                    if (data == null)
                    {
                        data = new SettingConfig();
                    }
                    _settingModel = data;
                }
                else
                {
                    Save();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}