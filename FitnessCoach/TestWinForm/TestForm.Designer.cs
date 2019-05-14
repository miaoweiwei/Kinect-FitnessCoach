namespace TestWinForm
{
    partial class TestForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button2 = new System.Windows.Forms.Button();
            this.btnSpeechRecognition1 = new System.Windows.Forms.Button();
            this.btnSpeechRecognition2 = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "语音测试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.button1);
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Controls.Add(this.btnSpeechRecognition1);
            this.flowLayoutPanel1.Controls.Add(this.btnSpeechRecognition2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(84, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(119, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "记录节点角度测试";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnSpeechRecognition1
            // 
            this.btnSpeechRecognition1.Location = new System.Drawing.Point(209, 3);
            this.btnSpeechRecognition1.Name = "btnSpeechRecognition1";
            this.btnSpeechRecognition1.Size = new System.Drawing.Size(144, 23);
            this.btnSpeechRecognition1.TabIndex = 2;
            this.btnSpeechRecognition1.Text = "Windows自带语音识别";
            this.btnSpeechRecognition1.UseVisualStyleBackColor = true;
            this.btnSpeechRecognition1.Click += new System.EventHandler(this.btnSpeechRecognition1_Click);
            // 
            // btnSpeechRecognition2
            // 
            this.btnSpeechRecognition2.Location = new System.Drawing.Point(359, 3);
            this.btnSpeechRecognition2.Name = "btnSpeechRecognition2";
            this.btnSpeechRecognition2.Size = new System.Drawing.Size(144, 23);
            this.btnSpeechRecognition2.TabIndex = 3;
            this.btnSpeechRecognition2.Text = "停止Windows语音识别";
            this.btnSpeechRecognition2.UseVisualStyleBackColor = true;
            this.btnSpeechRecognition2.Click += new System.EventHandler(this.btnSpeechRecognition2_Click);
            // 
            // TestFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "TestFrom";
            this.Text = "测试From";
            this.Load += new System.EventHandler(this.TestFrom_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnSpeechRecognition1;
        private System.Windows.Forms.Button btnSpeechRecognition2;
    }
}

