using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YQTrack.Backend.Factory;
using YQTrack.Backend.Models;
using YQTrack.Backend.Models.Enums;
using YQTrack.Backend.OrderComplete.IBLL;
using YQTrack.Backend.OrderComplete.Model;
using YQTrack.Backend.OrderCompleteService.Host.Config;
using YQTrack.Backend.OrderCompleteService.Host.V2;
using YQTrack.Backend.OrderCompleteService.V2.Complate;
using YQTrackV6.Log;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    public partial class FrmOrderComplete : Form
    {
        private static bool _IsExit = false;
        private static bool _isRunning;

        public FrmOrderComplete()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            txtThreadCount.Text = SettingManagerComplete.Setting.SemaphoreCount.ToString(CultureInfo.InvariantCulture);
            cbIsAsync.Checked = SettingManagerComplete.Setting.TaskAsync;//是否异步

            BindDbNode();
            _isRunning = true;

            AllTaskManager.Instance.Start();
            //Task.Run(() =>
            //{
            //    new RandomAutoComplateTimeTask().Execute();
            //});

            new MQSellerOrderComplateWorkService().StartMQHanler();//启动队列
        }


        //绑定列表
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


        //选中一行执行
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
                new AutoComplateTimeTask().RunManualWork(dbList);

                //是否包含邮件提醒
                if (ckbIsWranRun.Checked)
                {
                    Task.Run(() => { new V2.WarnTimeTask().DoEmailWarnWork(dbList); });
                }
            });
        }


        //自动完成
        private void btnRun_Click(object sender, EventArgs e)
        {
            //Schedule.OrderCompleteTaskCoordinator.Default.Start();
            new AutoComplateTimeTask().Execute();
        }

        //Seller/Buyer提醒
        private void btnSellerWarn_Click(object sender, EventArgs e)
        {
            new V2.WarnTimeTask().Execute();
        }

        //停止
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                //Schedule.OrderCompleteTaskCoordinator.Default.Stop();
                AllTaskManager.Instance.Stop();
                _isRunning = false;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtThreadCount.Text))
            {
                MessageBox.Show("请输入线程控制数!");
                return;
            }

            try
            {
                SettingManagerComplete.Setting.SemaphoreCount = Convert.ToInt32(txtThreadCount.Text);
                SettingManagerComplete.Setting.TaskAsync = cbIsAsync.Checked;
                MessageBox.Show("保存成功!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FrmOrderComplete_FormClosing(object sender, FormClosingEventArgs e)
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

        /// <summary>
        /// 双击托盘，显示主窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!_isRunning)
            {
                AllTaskManager.Instance.Start();
                btnStop.Enabled = true;
                btnStart.Enabled = false;
                _isRunning = true;
            }
        }
    }
}
