namespace AstronomyPictureOfTheDayWallpaperApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            ActivateBT = new Button();
            TableLayoutMain = new TableLayoutPanel();
            DeactivateBT = new Button();
            StatusLabel = new Label();
            splitContainer1 = new SplitContainer();
            TableLayoutSettings = new TableLayoutPanel();
            label1 = new Label();
            EnterAPITxb = new TextBox();
            ValidAPIBt = new Button();
            ValidAndSaveAPIBtn = new Button();
            TableLayoutMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            TableLayoutSettings.SuspendLayout();
            SuspendLayout();
            // 
            // ActivateBT
            // 
            ActivateBT.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            ActivateBT.Location = new Point(22, 20);
            ActivateBT.Name = "ActivateBT";
            ActivateBT.Size = new Size(167, 30);
            ActivateBT.TabIndex = 0;
            ActivateBT.Text = "Activate";
            ActivateBT.UseVisualStyleBackColor = true;
            ActivateBT.Click += ActivateBT_Click;
            // 
            // TableLayoutMain
            // 
            TableLayoutMain.ColumnCount = 4;
            TableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5.14979F));
            TableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45.0520821F));
            TableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44.53125F));
            TableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5.14979F));
            TableLayoutMain.Controls.Add(ActivateBT, 1, 1);
            TableLayoutMain.Controls.Add(DeactivateBT, 2, 1);
            TableLayoutMain.Controls.Add(StatusLabel, 1, 0);
            TableLayoutMain.Dock = DockStyle.Fill;
            TableLayoutMain.Location = new Point(0, 0);
            TableLayoutMain.Name = "TableLayoutMain";
            TableLayoutMain.RowCount = 2;
            TableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            TableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TableLayoutMain.Size = new Size(384, 53);
            TableLayoutMain.TabIndex = 1;
            // 
            // DeactivateBT
            // 
            DeactivateBT.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            DeactivateBT.Location = new Point(195, 20);
            DeactivateBT.Name = "DeactivateBT";
            DeactivateBT.Size = new Size(165, 30);
            DeactivateBT.TabIndex = 1;
            DeactivateBT.Text = "Deactivate";
            DeactivateBT.UseVisualStyleBackColor = true;
            DeactivateBT.Click += DeactivateBT_Click;
            // 
            // StatusLabel
            // 
            StatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TableLayoutMain.SetColumnSpan(StatusLabel, 2);
            StatusLabel.ForeColor = Color.Red;
            StatusLabel.Location = new Point(22, 0);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(338, 17);
            StatusLabel.TabIndex = 2;
            StatusLabel.Text = "Application is not active";
            StatusLabel.TextAlign = ContentAlignment.BottomCenter;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(TableLayoutMain);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(TableLayoutSettings);
            splitContainer1.Size = new Size(384, 139);
            splitContainer1.SplitterDistance = 53;
            splitContainer1.TabIndex = 2;
            // 
            // TableLayoutSettings
            // 
            TableLayoutSettings.BackColor = SystemColors.ButtonFace;
            TableLayoutSettings.ColumnCount = 4;
            TableLayoutSettings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 19F));
            TableLayoutSettings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175F));
            TableLayoutSettings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 169F));
            TableLayoutSettings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 21F));
            TableLayoutSettings.Controls.Add(label1, 0, 0);
            TableLayoutSettings.Controls.Add(EnterAPITxb, 1, 1);
            TableLayoutSettings.Controls.Add(ValidAPIBt, 1, 2);
            TableLayoutSettings.Controls.Add(ValidAndSaveAPIBtn, 2, 2);
            TableLayoutSettings.Dock = DockStyle.Fill;
            TableLayoutSettings.Location = new Point(0, 0);
            TableLayoutSettings.Name = "TableLayoutSettings";
            TableLayoutSettings.RowCount = 3;
            TableLayoutSettings.RowStyles.Add(new RowStyle(SizeType.Percent, 35.8490562F));
            TableLayoutSettings.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            TableLayoutSettings.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
            TableLayoutSettings.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TableLayoutSettings.Size = new Size(384, 82);
            TableLayoutSettings.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            TableLayoutSettings.SetColumnSpan(label1, 4);
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(378, 16);
            label1.TabIndex = 0;
            label1.Text = "Enter your API key from APOD (if you haven't already set it)";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // EnterAPITxb
            // 
            TableLayoutSettings.SetColumnSpan(EnterAPITxb, 2);
            EnterAPITxb.Dock = DockStyle.Fill;
            EnterAPITxb.Location = new Point(22, 19);
            EnterAPITxb.Name = "EnterAPITxb";
            EnterAPITxb.Size = new Size(338, 23);
            EnterAPITxb.TabIndex = 1;
            EnterAPITxb.TextAlign = HorizontalAlignment.Center;
            // 
            // ValidAPIBt
            // 
            ValidAPIBt.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            ValidAPIBt.Location = new Point(22, 51);
            ValidAPIBt.Name = "ValidAPIBt";
            ValidAPIBt.Size = new Size(169, 23);
            ValidAPIBt.TabIndex = 2;
            ValidAPIBt.Text = "Valid API key";
            ValidAPIBt.UseVisualStyleBackColor = true;
            ValidAPIBt.Click += ValidApiBt_Click;
            // 
            // ValidAndSaveAPIBtn
            // 
            ValidAndSaveAPIBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            ValidAndSaveAPIBtn.Location = new Point(197, 51);
            ValidAndSaveAPIBtn.Name = "ValidAndSaveAPIBtn";
            ValidAndSaveAPIBtn.Size = new Size(163, 23);
            ValidAndSaveAPIBtn.TabIndex = 3;
            ValidAndSaveAPIBtn.Text = "Valid and set new API key";
            ValidAndSaveAPIBtn.UseVisualStyleBackColor = true;
            ValidAndSaveAPIBtn.Click += ValidAndSaveApiBtn_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 139);
            Controls.Add(splitContainer1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "APOD Wallpaper Manager";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            SizeChanged += MainForm_SizeChanged;
            TableLayoutMain.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            TableLayoutSettings.ResumeLayout(false);
            TableLayoutSettings.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button ActivateBT;
        private TableLayoutPanel TableLayoutMain;
        private Button DeactivateBT;
        private Label StatusLabel;
        private SplitContainer splitContainer1;
        private TableLayoutPanel TableLayoutSettings;
        private Label label1;
        private TextBox EnterAPITxb;
        private Button ValidAPIBt;
        private Button ValidAndSaveAPIBtn;
    }
}