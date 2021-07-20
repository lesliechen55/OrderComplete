namespace YQTrack.Backend.OrderCompleteService.Host
{
    partial class FrmOrderComplete
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmOrderComplete));
            this.btnRun = new System.Windows.Forms.Button();
            this.dgvDbList = new System.Windows.Forms.DataGridView();
            this.OrderNodeId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrderDBNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrderUserCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currentUserIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.userArchivedCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.userErrorCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cbIsAsync = new System.Windows.Forms.CheckBox();
            this.txtThreadCount = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSellerWarn = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.ckbIsWranRun = new System.Windows.Forms.CheckBox();
            this.btnSelDbRun = new System.Windows.Forms.Button();
            this.btnSetting = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.btnStart = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDbList)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(506, 24);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(98, 56);
            this.btnRun.TabIndex = 17;
            this.btnRun.Text = "自动完成";
            this.btnRun.UseVisualStyleBackColor = false;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
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
            this.dgvDbList.Location = new System.Drawing.Point(12, 150);
            this.dgvDbList.Name = "dgvDbList";
            this.dgvDbList.RowTemplate.Height = 23;
            this.dgvDbList.Size = new System.Drawing.Size(736, 274);
            this.dgvDbList.TabIndex = 18;
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
            // cbIsAsync
            // 
            this.cbIsAsync.AutoSize = true;
            this.cbIsAsync.Location = new System.Drawing.Point(22, 29);
            this.cbIsAsync.Name = "cbIsAsync";
            this.cbIsAsync.Size = new System.Drawing.Size(96, 16);
            this.cbIsAsync.TabIndex = 22;
            this.cbIsAsync.Text = "是否异步执行";
            this.cbIsAsync.UseVisualStyleBackColor = true;
            // 
            // txtThreadCount
            // 
            this.txtThreadCount.Location = new System.Drawing.Point(67, 64);
            this.txtThreadCount.Name = "txtThreadCount";
            this.txtThreadCount.Size = new System.Drawing.Size(51, 21);
            this.txtThreadCount.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 20;
            this.label2.Text = "线程数";
            // 
            // btnSellerWarn
            // 
            this.btnSellerWarn.Location = new System.Drawing.Point(628, 24);
            this.btnSellerWarn.Name = "btnSellerWarn";
            this.btnSellerWarn.Size = new System.Drawing.Size(107, 56);
            this.btnSellerWarn.TabIndex = 23;
            this.btnSellerWarn.Text = "执行提醒";
            this.btnSellerWarn.UseVisualStyleBackColor = true;
            this.btnSellerWarn.Click += new System.EventHandler(this.btnSellerWarn_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(309, 24);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 56);
            this.btnStop.TabIndex = 25;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // ckbIsWranRun
            // 
            this.ckbIsWranRun.AutoSize = true;
            this.ckbIsWranRun.Location = new System.Drawing.Point(628, 110);
            this.ckbIsWranRun.Name = "ckbIsWranRun";
            this.ckbIsWranRun.Size = new System.Drawing.Size(120, 16);
            this.ckbIsWranRun.TabIndex = 19;
            this.ckbIsWranRun.Text = "是否包含邮件提醒";
            this.ckbIsWranRun.UseVisualStyleBackColor = true;
            // 
            // btnSelDbRun
            // 
            this.btnSelDbRun.Location = new System.Drawing.Point(457, 97);
            this.btnSelDbRun.Name = "btnSelDbRun";
            this.btnSelDbRun.Size = new System.Drawing.Size(147, 29);
            this.btnSelDbRun.TabIndex = 16;
            this.btnSelDbRun.Text = "执行选中DB编号下用户";
            this.btnSelDbRun.UseVisualStyleBackColor = true;
            this.btnSelDbRun.Click += new System.EventHandler(this.btnSelDbRun_Click);
            // 
            // btnSetting
            // 
            this.btnSetting.Location = new System.Drawing.Point(22, 101);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(98, 33);
            this.btnSetting.TabIndex = 26;
            this.btnSetting.Text = "保存设置";
            this.btnSetting.UseVisualStyleBackColor = false;
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
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
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "订单自动完成服务V2版本";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(181, 24);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(81, 56);
            this.btnStart.TabIndex = 27;
            this.btnStart.Text = "启动";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // FrmOrderComplete
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 450);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnSetting);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnSellerWarn);
            this.Controls.Add(this.cbIsAsync);
            this.Controls.Add(this.txtThreadCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ckbIsWranRun);
            this.Controls.Add(this.dgvDbList);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnSelDbRun);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmOrderComplete";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自动完成服务V2版本";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmOrderComplete_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDbList)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.DataGridView dgvDbList;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderNodeId;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderDBNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderUserCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn currentUserIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn userArchivedCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn userErrorCount;
        private System.Windows.Forms.CheckBox cbIsAsync;
        private System.Windows.Forms.TextBox txtThreadCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSellerWarn;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.CheckBox ckbIsWranRun;
        private System.Windows.Forms.Button btnSelDbRun;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button btnStart;
    }
}