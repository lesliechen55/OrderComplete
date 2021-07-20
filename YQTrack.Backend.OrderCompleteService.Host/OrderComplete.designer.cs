namespace YQTrack.Backend.OrderCompleteService.Host
{
    partial class OrderComplete
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderComplete));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.lblLogInfo = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtThreadCount = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSetting = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtRepeatInsertDays = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMaxConnectionCount = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCompleteTime = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtWaitSeconds = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cbIsConnectionCount = new System.Windows.Forms.CheckBox();
            this.txtUserEndIndex = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtUserStartIndex = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtUserPageSize = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.ckbIsWranRun = new System.Windows.Forms.CheckBox();
            this.btnSelDbRun = new System.Windows.Forms.Button();
            this.dgvDbList = new System.Windows.Forms.DataGridView();
            this.OrderNodeId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrderDBNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrderUserCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currentUserIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.userArchivedCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.userErrorCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblNextTimeComplete = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lblNextRunTimeWarn = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnRunNotify = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblWarnDayAndTime = new System.Windows.Forms.Label();
            this.lblUserPageSizeWarn = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.button5 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDbList)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(20, 177);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(98, 33);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "启动轮询";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(152, 177);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(98, 33);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "停止轮询";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(283, 177);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(98, 33);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "立即执行";
            this.btnRun.UseVisualStyleBackColor = false;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // lblLogInfo
            // 
            this.lblLogInfo.AutoSize = true;
            this.lblLogInfo.Location = new System.Drawing.Point(6, 35);
            this.lblLogInfo.Name = "lblLogInfo";
            this.lblLogInfo.Size = new System.Drawing.Size(0, 12);
            this.lblLogInfo.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblLogInfo);
            this.groupBox1.Location = new System.Drawing.Point(20, 395);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(361, 70);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "日志信息";
            // 
            // txtThreadCount
            // 
            this.txtThreadCount.Location = new System.Drawing.Point(89, 98);
            this.txtThreadCount.Name = "txtThreadCount";
            this.txtThreadCount.Size = new System.Drawing.Size(100, 21);
            this.txtThreadCount.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(42, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "线程数";
            // 
            // btnSetting
            // 
            this.btnSetting.Location = new System.Drawing.Point(581, 91);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(98, 33);
            this.btnSetting.TabIndex = 7;
            this.btnSetting.Text = "保存设置";
            this.btnSetting.UseVisualStyleBackColor = false;
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "单次用户数量";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtRepeatInsertDays);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtMaxConnectionCount);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtCompleteTime);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.txtWaitSeconds);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.cbIsConnectionCount);
            this.groupBox2.Controls.Add(this.txtUserEndIndex);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.txtUserStartIndex);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txtUserPageSize);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.btnSetting);
            this.groupBox2.Controls.Add(this.txtThreadCount);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(20, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(695, 130);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "配置信息";
            // 
            // txtRepeatInsertDays
            // 
            this.txtRepeatInsertDays.Location = new System.Drawing.Point(352, 95);
            this.txtRepeatInsertDays.Name = "txtRepeatInsertDays";
            this.txtRepeatInsertDays.Size = new System.Drawing.Size(50, 21);
            this.txtRepeatInsertDays.TabIndex = 24;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(209, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 12);
            this.label1.TabIndex = 23;
            this.label1.Text = "重复插入消息间隔（天）";
            // 
            // txtMaxConnectionCount
            // 
            this.txtMaxConnectionCount.Location = new System.Drawing.Point(467, 62);
            this.txtMaxConnectionCount.Name = "txtMaxConnectionCount";
            this.txtMaxConnectionCount.Size = new System.Drawing.Size(100, 21);
            this.txtMaxConnectionCount.TabIndex = 22;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(408, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "执行时间";
            // 
            // txtCompleteTime
            // 
            this.txtCompleteTime.Location = new System.Drawing.Point(467, 96);
            this.txtCompleteTime.Name = "txtCompleteTime";
            this.txtCompleteTime.Size = new System.Drawing.Size(100, 21);
            this.txtCompleteTime.TabIndex = 9;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(408, 66);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 12);
            this.label10.TabIndex = 21;
            this.label10.Text = "控制数量";
            // 
            // txtWaitSeconds
            // 
            this.txtWaitSeconds.Location = new System.Drawing.Point(302, 63);
            this.txtWaitSeconds.Name = "txtWaitSeconds";
            this.txtWaitSeconds.Size = new System.Drawing.Size(100, 21);
            this.txtWaitSeconds.TabIndex = 20;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(243, 68);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 19;
            this.label9.Text = "等待时间";
            // 
            // cbIsConnectionCount
            // 
            this.cbIsConnectionCount.AutoSize = true;
            this.cbIsConnectionCount.Location = new System.Drawing.Point(89, 63);
            this.cbIsConnectionCount.Name = "cbIsConnectionCount";
            this.cbIsConnectionCount.Size = new System.Drawing.Size(96, 16);
            this.cbIsConnectionCount.TabIndex = 18;
            this.cbIsConnectionCount.Text = "是否等待执行";
            this.cbIsConnectionCount.UseVisualStyleBackColor = true;
            // 
            // txtUserEndIndex
            // 
            this.txtUserEndIndex.Location = new System.Drawing.Point(467, 24);
            this.txtUserEndIndex.Name = "txtUserEndIndex";
            this.txtUserEndIndex.Size = new System.Drawing.Size(100, 21);
            this.txtUserEndIndex.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(408, 28);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 12;
            this.label6.Text = "结束索引";
            // 
            // txtUserStartIndex
            // 
            this.txtUserStartIndex.Location = new System.Drawing.Point(302, 24);
            this.txtUserStartIndex.Name = "txtUserStartIndex";
            this.txtUserStartIndex.Size = new System.Drawing.Size(100, 21);
            this.txtUserStartIndex.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(243, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "开始索引";
            // 
            // txtUserPageSize
            // 
            this.txtUserPageSize.Location = new System.Drawing.Point(89, 26);
            this.txtUserPageSize.Name = "txtUserPageSize";
            this.txtUserPageSize.Size = new System.Drawing.Size(100, 21);
            this.txtUserPageSize.TabIndex = 9;
            // 
            // timer1
            // 
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(741, 507);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button5);
            this.tabPage1.Controls.Add(this.ckbIsWranRun);
            this.tabPage1.Controls.Add(this.btnSelDbRun);
            this.tabPage1.Controls.Add(this.dgvDbList);
            this.tabPage1.Controls.Add(this.lblNextTimeComplete);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.btnStart);
            this.tabPage1.Controls.Add(this.btnStop);
            this.tabPage1.Controls.Add(this.btnRun);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(733, 481);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "自动完成任务";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // ckbIsWranRun
            // 
            this.ckbIsWranRun.AutoSize = true;
            this.ckbIsWranRun.Location = new System.Drawing.Point(562, 156);
            this.ckbIsWranRun.Name = "ckbIsWranRun";
            this.ckbIsWranRun.Size = new System.Drawing.Size(120, 16);
            this.ckbIsWranRun.TabIndex = 16;
            this.ckbIsWranRun.Text = "是否包含邮件提醒";
            this.ckbIsWranRun.UseVisualStyleBackColor = true;
            // 
            // btnSelDbRun
            // 
            this.btnSelDbRun.Location = new System.Drawing.Point(552, 178);
            this.btnSelDbRun.Name = "btnSelDbRun";
            this.btnSelDbRun.Size = new System.Drawing.Size(147, 32);
            this.btnSelDbRun.TabIndex = 15;
            this.btnSelDbRun.Text = "执行选中DB编号下用户";
            this.btnSelDbRun.UseVisualStyleBackColor = true;
            this.btnSelDbRun.Click += new System.EventHandler(this.btnSelDbRun_Click);
            // 
            // dgvDbList
            // 
            this.dgvDbList.AllowUserToAddRows = false;
            this.dgvDbList.AllowUserToDeleteRows = false;
            this.dgvDbList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDbList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.OrderNodeId,
            this.OrderDBNo,
            this.OrderUserCount,
            this.currentUserIndex,
            this.userArchivedCount,
            this.userErrorCount});
            this.dgvDbList.Location = new System.Drawing.Point(20, 227);
            this.dgvDbList.Name = "dgvDbList";
            this.dgvDbList.RowTemplate.Height = 23;
            this.dgvDbList.Size = new System.Drawing.Size(695, 162);
            this.dgvDbList.TabIndex = 14;
            // 
            // OrderNodeId
            // 
            this.OrderNodeId.HeaderText = "数据库节点";
            this.OrderNodeId.Name = "OrderNodeId";
            // 
            // OrderDBNo
            // 
            this.OrderDBNo.HeaderText = "数据库编号";
            this.OrderDBNo.Name = "OrderDBNo";
            // 
            // OrderUserCount
            // 
            this.OrderUserCount.HeaderText = "用户数";
            this.OrderUserCount.Name = "OrderUserCount";
            // 
            // currentUserIndex
            // 
            this.currentUserIndex.HeaderText = "当前索引";
            this.currentUserIndex.Name = "currentUserIndex";
            // 
            // userArchivedCount
            // 
            this.userArchivedCount.HeaderText = "归档用户数";
            this.userArchivedCount.Name = "userArchivedCount";
            // 
            // userErrorCount
            // 
            this.userErrorCount.HeaderText = "错误用户数";
            this.userErrorCount.Name = "userErrorCount";
            // 
            // lblNextTimeComplete
            // 
            this.lblNextTimeComplete.AutoSize = true;
            this.lblNextTimeComplete.Location = new System.Drawing.Point(402, 441);
            this.lblNextTimeComplete.Name = "lblNextTimeComplete";
            this.lblNextTimeComplete.Size = new System.Drawing.Size(47, 12);
            this.lblNextTimeComplete.TabIndex = 6;
            this.lblNextTimeComplete.Text = "label11";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.button4);
            this.tabPage2.Controls.Add(this.button3);
            this.tabPage2.Controls.Add(this.button2);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.txtEmail);
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.lblNextRunTimeWarn);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.btnRunNotify);
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(733, 481);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "完成统计提醒任务";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(40, 256);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 9;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(365, 379);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(177, 23);
            this.button3.TabIndex = 8;
            this.button3.Text = "测试自动完成归档";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(365, 339);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(177, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "卖家综合信息提醒";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(98, 339);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 12);
            this.label7.TabIndex = 6;
            this.label7.Text = "测试邮件地址：";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(193, 336);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(121, 21);
            this.txtEmail.TabIndex = 5;
            this.txtEmail.Text = "ksport@qq.com";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(365, 308);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(177, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "追踪数不足10%邮件提醒";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // lblNextRunTimeWarn
            // 
            this.lblNextRunTimeWarn.AutoSize = true;
            this.lblNextRunTimeWarn.Location = new System.Drawing.Point(123, 167);
            this.lblNextRunTimeWarn.Name = "lblNextRunTimeWarn";
            this.lblNextRunTimeWarn.Size = new System.Drawing.Size(47, 12);
            this.lblNextRunTimeWarn.TabIndex = 3;
            this.lblNextRunTimeWarn.Text = "label11";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(28, 167);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 12);
            this.label8.TabIndex = 2;
            this.label8.Text = "下次执行时间：";
            // 
            // btnRunNotify
            // 
            this.btnRunNotify.Location = new System.Drawing.Point(522, 167);
            this.btnRunNotify.Name = "btnRunNotify";
            this.btnRunNotify.Size = new System.Drawing.Size(107, 33);
            this.btnRunNotify.TabIndex = 1;
            this.btnRunNotify.Text = "立即执行提醒";
            this.btnRunNotify.UseVisualStyleBackColor = true;
            this.btnRunNotify.Click += new System.EventHandler(this.btnRunNotify_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblWarnDayAndTime);
            this.groupBox3.Controls.Add(this.lblUserPageSizeWarn);
            this.groupBox3.Location = new System.Drawing.Point(20, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(609, 100);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "配置信息";
            // 
            // lblWarnDayAndTime
            // 
            this.lblWarnDayAndTime.AutoSize = true;
            this.lblWarnDayAndTime.Location = new System.Drawing.Point(8, 65);
            this.lblWarnDayAndTime.Name = "lblWarnDayAndTime";
            this.lblWarnDayAndTime.Size = new System.Drawing.Size(113, 12);
            this.lblWarnDayAndTime.TabIndex = 10;
            this.lblWarnDayAndTime.Text = "每天几点执行统计：";
            // 
            // lblUserPageSizeWarn
            // 
            this.lblUserPageSizeWarn.AutoSize = true;
            this.lblUserPageSizeWarn.Location = new System.Drawing.Point(6, 26);
            this.lblUserPageSizeWarn.Name = "lblUserPageSizeWarn";
            this.lblUserPageSizeWarn.Size = new System.Drawing.Size(113, 12);
            this.lblUserPageSizeWarn.TabIndex = 9;
            this.lblUserPageSizeWarn.Text = "单次获取用户数量：";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "订单自动完成服务";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(137, 48);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(136, 22);
            this.toolStripMenuItem1.Text = "退出";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(136, 22);
            this.toolStripMenuItem2.Text = "显示主窗口";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(652, 441);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 17;
            this.button5.Text = "button5";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click_1);
            // 
            // OrderComplete
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(765, 531);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "OrderComplete";
            this.Text = "自动完成订单";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDbList)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblLogInfo;
        private System.Windows.Forms.TextBox txtThreadCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtUserEndIndex;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtUserStartIndex;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtUserPageSize;
        private System.Windows.Forms.TextBox txtMaxConnectionCount;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtWaitSeconds;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox cbIsConnectionCount;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txtRepeatInsertDays;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblUserPageSizeWarn;
        private System.Windows.Forms.Button btnRunNotify;
        private System.Windows.Forms.Label lblWarnDayAndTime;
        private System.Windows.Forms.Label lblNextRunTimeWarn;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblNextTimeComplete;
        private System.Windows.Forms.TextBox txtCompleteTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dgvDbList;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderNodeId;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderDBNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderUserCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn currentUserIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn userArchivedCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn userErrorCount;
        private System.Windows.Forms.Button btnSelDbRun;
        private System.Windows.Forms.CheckBox ckbIsWranRun;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}
