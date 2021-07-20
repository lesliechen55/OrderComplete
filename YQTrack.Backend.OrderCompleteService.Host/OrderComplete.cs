using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

using YQTrack.Backend.OrderCompleteService.Host.Config;
using System.Linq;
using YQTrack.Backend.Factory;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.Models;
using YQTrack.Backend.OrderComplete.DTO;
using YQTrackV6.Log;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.BLL;
using YQTrack.Backend.OrderComplete.IService;
using YQTrack.Backend.OrderComplete.MQHelpr;
using YQTrack.Backend.OrderComplete.Model;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    public partial class OrderComplete : Form
    {
        private static bool _isRunning;
        private static bool _IsExit = false;
        OrderRandomCompleteTask taskRandom = new OrderRandomCompleteTask();
        public OrderComplete()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            //cbNodeId.DisplayMember = "Text";//控件显示的列名  
            //cbNodeId.ValueMember = "Value";//控件值的列名  
            //cbNodeId.Items.Add(new SeleteItem() { Text="所有",Value="-1"} );
            //int index = 0;
            //for (int i = 1; i <= 2; i++)
            //{
            //    cbNodeId.Items.Add(new　SeleteItem() {Text = "节点"+ i, Value = i.ToString() });
            //    if (SettingManager.Setting.NodeId == i) {
            //        index = i;
            //    }
            //}
            //cbNodeId.SelectedIndex = index;

            _isRunning = true;
            OrderCompleteTaskManager.Instance.Start();
            OrderCompleteCountWarnTaskManager.Instance.Start();
            btnStart.Enabled = false;
            txtThreadCount.Text = SettingManagerComplete.Setting.SemaphoreCount.ToString(CultureInfo.InvariantCulture);
            txtMaxConnectionCount.Text = SettingManagerComplete.Setting.MaxConnectionCount.ToString(CultureInfo.InvariantCulture);
            txtUserPageSize.Text = SettingManagerComplete.Setting.UserPageSize.ToString(CultureInfo.InvariantCulture);
            txtUserStartIndex.Text = SettingManagerComplete.Setting.StartIndex.ToString(CultureInfo.InvariantCulture);
            txtUserEndIndex.Text = SettingManagerComplete.Setting.EndIndex.ToString(CultureInfo.InvariantCulture);
            txtWaitSeconds.Text = SettingManagerComplete.Setting.LoopWaitSeconds.ToString(CultureInfo.InvariantCulture);
            cbIsConnectionCount.Checked = SettingManagerComplete.Setting.IsConnectionCount;
            txtRepeatInsertDays.Text = SettingManagerComplete.Setting.MsgRepeatInsertDays.ToString(CultureInfo.InvariantCulture);
            txtCompleteTime.Text = SettingManagerComplete.Setting.CompleteTime;

            lblUserPageSizeWarn.Text = lblUserPageSizeWarn.Text + SettingManagerComplete.Setting.UserPageSize.ToString();
            lblWarnDayAndTime.Text = string.Format("{0}每隔 {1} 天,在 {2} 执行", lblWarnDayAndTime.Text, SettingManagerComplete.Setting.WarnDay, SettingManagerComplete.Setting.WarnTime);

            timer1.Start();
            BindDbNode();

            //启动队列服务。


            IOrderAutoCompleteService service = FactoryContainer.Create<IOrderAutoCompleteService>();
            service.SetDataRoute(new DataRouteModel
            {
            });
            service.StartMQHanler();


            
            taskRandom.Execute();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!_isRunning)
            {
                OrderCompleteTaskManager.Instance.Init();
                OrderCompleteTaskManager.Instance.Start();
                btnStop.Enabled = true;
                btnStart.Enabled = false;
                _isRunning = true;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                OrderCompleteTaskManager.Instance.Stop();
                _isRunning = false;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            OrderCompleteTaskManager.Instance.Run();
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtThreadCount.Text))
            {
                MessageBox.Show("请输入线程控制数!");
                return;
            }

            if (string.IsNullOrEmpty(txtMaxConnectionCount.Text))
            {
                MessageBox.Show("请输入最大控制数!");
                return;
            }

            if (string.IsNullOrEmpty(txtUserPageSize.Text))
            {
                MessageBox.Show("请输入用户每次获取数量!");
                return;
            }

            if (string.IsNullOrEmpty(txtUserStartIndex.Text))
            {
                MessageBox.Show("请输入开始索引!");
                return;
            }

            if (string.IsNullOrEmpty(txtUserEndIndex.Text))
            {
                MessageBox.Show("轻输入结束索引!");
                return;
            }
            if (string.IsNullOrEmpty(txtWaitSeconds.Text))
            {
                MessageBox.Show("请输入等待休眠秒数!");
                return;
            }
            try
            {
                SettingManagerComplete.Setting.SemaphoreCount = Convert.ToInt32(txtThreadCount.Text);
                SettingManagerComplete.Setting.MaxConnectionCount = Convert.ToInt32(txtMaxConnectionCount.Text);
                SettingManagerComplete.Setting.UserPageSize = Convert.ToInt32(txtUserPageSize.Text);
                SettingManagerComplete.Setting.StartIndex = Convert.ToInt32(txtUserStartIndex.Text);
                SettingManagerComplete.Setting.EndIndex = Convert.ToInt32(txtUserEndIndex.Text);
                SettingManagerComplete.Setting.LoopWaitSeconds = Convert.ToInt32(txtWaitSeconds.Text);
                SettingManagerComplete.Setting.MsgRepeatInsertDays = Convert.ToInt32(txtRepeatInsertDays.Text);

                SettingManagerComplete.Setting.IsConnectionCount = cbIsConnectionCount.Checked;
                SettingManagerComplete.Setting.CompleteTime = txtCompleteTime.Text;
                //SettingManager.Setting.NodeId = ((SeleteItem)cbNodeId.SelectedItem).Value.ToString().ToInt32();
            }
            catch (Exception /*ex*/)
            {
                MessageBox.Show("请输入有效值!");
            }
            //SettingManager.Save();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblLogInfo.Text = OrderCompleteTaskManager.Instance.GetLastRunTime();
            lblNextTimeComplete.Text = OrderCompleteTaskManager.Instance.GetNextRunTime();

            lblNextRunTimeWarn.Text = OrderCompleteCountWarnTaskManager.Instance.GetNextRunTime();
            GetOrderCompleteMsg();
        }

        /// <summary>
        /// 立即执行提醒，用户订单完成统计数据发送到用户邮箱
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRunNotify_Click(object sender, EventArgs e)
        {
            OrderCompleteCountWarnTaskManager.Instance.Run();
            ////Task.Run(() =>
            ////{
            ////    Test();
            ////    Test2();
            ////});
            //  {"InvalidTime":15,"NoChangeTime":15,"SuccessTime":10,"FinishedTime":20,"IsWarnComplete":1,"IsWarnNum":1,"NumWarnPer":10,"NumWarnDay":1,"IsWarnPackageState":1}
        }

        public async void Test(string strEmail)
        {
            var profileBll = FactoryContainer.Create<IUserBLL>();
         
            profileBll.SetDataRoute(new DataRouteModel
            {
                //NodeId = 1,
                //DbNo = 1 /*, TableNo = Convert.ToByte(dto.FTableNo)*/
            });
        

            var model = new DataRouteModel();
            profileBll.SetDataRoute(model);
            var dto = new UserProfileConfigPageDTO
            {
                NodeId = -1,
                DBNo = -1,
                StartIndex = 0,
                PageSize = 10,
                FEmail = strEmail  //920788373@qq.com  paoshuren88@163.com
            };
            //返回集合，以后可以返回多个用户
            var userList = await profileBll.GetUserConfigsByPage(dto);
            foreach (var item in userList)
            {

                //var result2 = await service.HandleUserTrackNumNotify(item);
            }


            //var service = new OrderCompleteAppService(profileBll, _Cache);
            //var item = new DTO.User.UserProfileConfigDTO();
            //item.FUserId = 6119047324854648833;
            //item.FUserRole = Entity.User.UserRoleType.Seller;
            //item.FNickName = "";

        }

        public async void Test2(string strEmail)
        {
            var profileBll = FactoryContainer.Create<IUserBLL>();
            var _LogBll = FactoryContainer.Create<ILogBll>();
            profileBll.SetDataRoute(new DataRouteModel
            {
                //NodeId = 1,
                //DbNo = 1 /*, TableNo = Convert.ToByte(dto.FTableNo)*/
            });
          
            var model = new DataRouteModel();
            profileBll.SetDataRoute(model);
            var dto = new UserProfileConfigPageDTO
            {
                NodeId = -1,
                DBNo = -1,
                StartIndex = 0,
                PageSize = 10,
                FEmail = strEmail//920788373@qq.com  paoshuren88@163.com
            };
            //返回集合，以后可以返回多个用户
            var userList = await profileBll.GetUserConfigsByPage(dto);
            foreach (var item in userList)
            {
                //item.FConfig = "{\"InvalidTime\":15,\"NoChangeTime\":15,\"SuccessTime\":10,\"FinishedTime\":20,\"IsWarnComplete\":true,\"IsWarnNum\":false,\"NumWarnPer\":35,\"NumWarnDay\":0,\"IsWarnPackageState\":false,\"IsWarnNoneDays\":true,\"WarnNoneDays\":7,\"IsWarnUnChangeDays\":true,\"WarnUnChangeDays\":7,\"IsWarnUnDelivereDays\":false,\"WarnUnDelivereDays\":7,\"IsWarnDeliveredDays\":true,\"WarnDeliveredDays\":7}";
                //item.CompleteConfig = item.FConfig.DeserializeFromJson<ParamDTO>();
                var service = new OrderAutoWarnService(profileBll);
               
                var result = await service.HandleSellerWarnNotify(item);

            }

        }


        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
               // Test3();
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_IsExit)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                this.Dispose();

                Environment.Exit(0);
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要退出吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                _IsExit = true;

                this.Dispose();
                Environment.Exit(0);
            }
        }

        private bool _IsQueueLoop = false;
        private void GetOrderCompleteMsg()
        {
            //第一次调用循环在进行，第二次调用放弃
            //Log.LogHelper.Log(new Log.LogDefinition(Log.LogLevel.Warn, "GetOrderCompleteMsg，{0}"), OrderCompleteTaskManager.Instance.QueueOrderCompleteMsg.Count);
            if (!_IsQueueLoop)
            {
                _IsQueueLoop = true;
            }
            else
            {
                return;
            }

            int len = 0;
            var queueOrder = OrderCompleteTaskManager.Instance.QueueOrderCompleteMsg;
            if (queueOrder != null)
            {
                try
                {
                    len = queueOrder.Count;
                    if (len > 0)
                    {
                        OrderCompleteMsgEventArgs model = null;
                        for (int i = 0; i < len; i++)
                        {
                            var isOk = queueOrder.TryDequeue(out model);
                            if (isOk)
                            {
                                UpdateDbUserCount(model);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log(new LogDefinition(YQTrackV6.Log.LogLevel.Warn, "事件回调Form处理事件.ex.Message={0}"), ex.Message);
                }
            }
            _IsQueueLoop = false;
        }


        private void UpdateDbUserCount(OrderCompleteMsgEventArgs e)
        {

            var rows = dgvDbList.Rows;
            int length = rows.Count;
            for (int i = 0; i < length; i++)
            {
                var row = rows[i];
                int nodeId = Convert.ToInt32(row.Cells[0].Value);
                int dbno = Convert.ToInt32(row.Cells[1].Value);

                if (nodeId == e.NodeId && dbno == e.DbNo)
                {
                    row.Cells[2].Value = e.UserCount;
                    row.Cells[3].Value = e.UserCurrentIndex;  // Convert.ToInt32(row.Cells[3].Value) +
                    row.Cells[4].Value = e.UserArchivedCount;
                    row.Cells[5].Value = e.UserErrorCount;
                }
            }
        }

        private void BindDbNode()
        {
            var dicDbNodes = YQTrack.Backend.Sharding.DBShardingRouteFactory.DicDBTypeNodes;
            var strDbSeller = Enum.GetName(typeof(YQDbType), YQDbType.Seller);
            var objDbSelleRule = Sharding.DBShardingRouteFactory.GetDBTypeRule(strDbSeller);

            var nodes = objDbSelleRule.NodeRoutes;
            var nodeCount = nodes.Count;
            //获取数据库节点

            foreach (var nodeItem in nodes)
            {

                //数据库节点下的订单库
                var dbRules = nodeItem.Value.DBRules;
                var dbCount = dbRules.Count();
                foreach (var dbrule in dbRules)
                {
                    var row = new DataGridViewRow();
                    row.CreateCells(dgvDbList);
                    row.Cells[0].Value = nodeItem.Value.NodeId;
                    row.Cells[1].Value = dbrule.Value.DBNo;
                    row.Cells[2].Value = 0;
                    row.Cells[3].Value = 0;
                    row.Cells[4].Value = 0;
                    row.Cells[5].Value = 0;
                    //row.Cells[5].Value = item.FTableNo;
                    dgvDbList.Rows.Add(row);

                }
            }
        }

        private void btnSelDbRun_Click(object sender, EventArgs e)
        {

            var selRows = dgvDbList.SelectedRows;
            int len = selRows.Count;
            if (len == 0)
            {
                MessageBox.Show("请选择DB编号整行数据！");
                return;
            }
            List<OrderCompleteItem> dbList = new List<OrderCompleteItem>();
            for (int i = 0; i < len; i++)
            {
                var row = selRows[i];
                var dbItem = new OrderCompleteItem();
                int index = 0;
                dbItem.NodeId = Convert.ToInt32(row.Cells[index].Value);

                index++;
                dbItem.DbNo = Convert.ToInt32(row.Cells[index].Value);

                index++;
                index++;
                dbItem.UserStartIndex = Convert.ToInt32(row.Cells[index].Value);
                dbList.Add(dbItem);

            }
            Task.Run(() =>
            {
                OrderCompleteTaskManager.Instance.RunManualWork(dbList);
                if (ckbIsWranRun.Checked)
                {
                    OrderCompleteCountWarnTaskManager.Instance.RunManualWork(dbList);
                }
            });
        }

        /// <summary>
        /// 双击托盘，显示主窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要退出吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                _IsExit = true;
                Close();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Show();
            BringToFront();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string strEmail = txtEmail.Text;
            if (string.IsNullOrWhiteSpace(strEmail)) {
                MessageBox.Show("请输入测试邮件地址");
                return;
            }
            Task.Run(() =>
            {
                Test(strEmail);
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string strEmail = txtEmail.Text;
            if (string.IsNullOrWhiteSpace(strEmail))
            {
                MessageBox.Show("请输入测试邮件地址");
                return;
            }
            Task.Run(() =>
            {
                Test2(strEmail);
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string strEmail = txtEmail.Text;
            if (string.IsNullOrWhiteSpace(strEmail))
            {
                MessageBox.Show("请输入测试邮件地址");
                return;
            }
            Task.Run(() =>
            {
              
            });
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string strEmail = txtEmail.Text;
            if (string.IsNullOrWhiteSpace(strEmail))
            {
                MessageBox.Show("请输入测试邮件地址");
                return;
            }
            Task.Run(() =>
            {
                Test3AutoComplete(strEmail);
            });
        }
        public async void Test3AutoComplete(string strEmail)
        {
            var profileBll = FactoryContainer.Create<IUserBLL>();
            var _LogBll = FactoryContainer.Create<ILogBll>();
            profileBll.SetDataRoute(new DataRouteModel
            {
                //NodeId = 1,
                //DbNo = 1 /*, TableNo = Convert.ToByte(dto.FTableNo)*/
            });

            var model = new DataRouteModel();
            profileBll.SetDataRoute(model);
            var dto = new UserProfileConfigPageDTO
            {
                NodeId = -1,
                DBNo = -1,
                StartIndex = 0,
                PageSize = 10,
                FEmail = strEmail//920788373@qq.com  paoshuren88@163.com
            };
            //返回集合，以后可以返回多个用户
            var userList = await profileBll.GetUserConfigsByPage(dto);
            var userDot = userList.FirstOrDefault();
            if (userDot != null)
            {
                if (userDot.CompleteConfig == null) {
                    MessageBox.Show("配置为空不能执行");
                    return;
                }
                var service = new OrderAutoCompleteService(profileBll);
                service.Init();
                var result = await service.HandleOrderAutoCompleteOrArchivedAll(userDot);
                var result2 = service.ExecuteAutoArchivedCurrentUsers();


            }
           

        }

        private void button4_Click(object sender, EventArgs e)
        {
            SellerOrderCompleteMQProducerService.Instance.Publish(new SellerOrderCompleteModel() { FUserId = 6158546504626995201, FTrackInfoId = 6404960981654700033 });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var selRows = dgvDbList.SelectedRows;
            int len = selRows.Count;
            if (len == 0)
            {
                MessageBox.Show("请选择DB编号整行数据！");
                return;
            }
            List<OrderCompleteItem> dbList = new List<OrderCompleteItem>();
            for (int i = 0; i < len; i++)
            {
                var row = selRows[i];
                var dbItem = new OrderCompleteItem();
                int index = 0;
                dbItem.NodeId = Convert.ToInt32(row.Cells[index].Value);

                index++;
                dbItem.DbNo = Convert.ToInt32(row.Cells[index].Value);

                index++;
                index++;
                dbItem.UserStartIndex = Convert.ToInt32(row.Cells[index].Value);
                dbList.Add(dbItem);

            }
            Task.Run(() =>
            {
                OrderCompleteTaskManager.Instance.RunManualWork(dbList);
                if (ckbIsWranRun.Checked)
                {
                    OrderCompleteCountWarnTaskManager.Instance.RunManualWork(dbList);
                }
            });
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            Task.Run(() =>
            {

                for (int i = 0; i < 10; i++)
                {
                    //System.Threading.Thread.Sleep(1000);
                     Task.Run(() =>
                    {
                        var orderBll = FactoryContainer.Create<IOrderAutoWarnBLL>();
                        orderBll.SetDataRoute(new DataRouteModel
                        {
                            UserRole = 2,
                            NodeId = 1,
                            DbNo = 4,
                            TableNo = 1
                        });

                        var resultModel = orderBll.OrderUpdateStateWaitToTracking(6158546504626995201);
                        var res = resultModel.Result;
                        LogHelper.LogObj(new LogDefinition(YQTrackV6.Log.LogLevel.Notice, "OrderUpdateStateWaitToTracking1"), res);
                        if (res !=null &&  res.CanTrackNum >0)
                        {

                        }
                    });
                }


            });
        }
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    Task.Run(() =>
        //    {

        //        var nodes = DBSettingsHelper.SettingsDefault.DataConfig.NodeConfigs;
        //        var nodeCount = nodes.Count;
        //        //获取数据库节点
        //        foreach (var nodeItem in nodes)
        //        {
        //            //当前用户数据库
        //            var dbUsers = nodeItem.DBConfigs.Where(t => t.DBType == "User");
        //            foreach (var userItem in dbUsers)
        //            {
        //                var profileBll = FactoryContainer.Create<IUserProfileBll>();
        //                profileBll.SetDataRoute(new DataRouteModel
        //                {
        //                    NodeId = Convert.ToByte(nodeItem.NodeId),
        //                    DbNo = Convert.ToByte(userItem.DBNo) /*, TableNo = Convert.ToByte(dto.FTableNo)*/
        //                });
        //                //数据库节点下的订单库
        //                var dbOrders = nodeItem.DBConfigs.Where(t => t.DBType == "Order" && !t.IsArchived);
        //                var dbCount = dbOrders.Count();
        //                //循环调用订单库。
        //                var _Cache = FactoryContainer.Create<ICachingService>();
        //                foreach (var orderItem in dbOrders)
        //                {
        //                    var service = new OrderCompleteAppService(profileBll, _Cache);

        //                    service.TempDoWork(nodeItem.NodeId, orderItem.DBNo,
        //                        SettingManager.Setting.UserPageSize,
        //                        SettingManager.Setting.StartIndex,
        //                        SettingManager.Setting.EndIndex,
        //                        SettingManager.Setting.IsConnectionCount,
        //                        SettingManager.Setting.MaxConnectionCount,
        //                        SettingManager.Setting.LoopWaitSeconds,
        //                        SettingManager.Setting.MsgRepeatInsertDays
        //                        );
        //                }


        //            }

        //        }
        //    });
        //}


    }
}
