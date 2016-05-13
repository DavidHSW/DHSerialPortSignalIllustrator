namespace DHLineChart
{
    partial class DHLineChart
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

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.chartArea = new System.Windows.Forms.Panel();
            this.lineArea = new DHPanel();
            this.chartArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // chartArea
            // 
            this.chartArea.AutoSize = true;
            this.chartArea.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.chartArea.BackColor = System.Drawing.Color.White;
            this.chartArea.Controls.Add(this.lineArea);
            this.chartArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartArea.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.chartArea.Location = new System.Drawing.Point(0, 0);
            this.chartArea.MinimumSize = new System.Drawing.Size(50, 54);
            this.chartArea.Name = "chartArea";
            this.chartArea.Size = new System.Drawing.Size(333, 261);
            this.chartArea.TabIndex = 0;
            this.chartArea.Paint += new System.Windows.Forms.PaintEventHandler(this.chartArea_Paint);
            // 
            // lineArea
            // 
            this.lineArea.BackColor = System.Drawing.Color.Transparent;
            this.lineArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lineArea.Location = new System.Drawing.Point(0, 0);
            this.lineArea.Name = "lineArea";
            this.lineArea.Size = new System.Drawing.Size(333, 261);
            this.lineArea.TabIndex = 0;
            this.lineArea.Paint += new System.Windows.Forms.PaintEventHandler(this.lineArea_Paint);
            // 
            // DHLineChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.chartArea);
            this.Name = "DHLineChart";
            this.Size = new System.Drawing.Size(333, 261);
            this.chartArea.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel chartArea;
        private DHPanel lineArea;
    }
}
