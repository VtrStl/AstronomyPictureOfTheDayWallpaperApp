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
            tableLayoutPanel1 = new TableLayoutPanel();
            DeactivateBT = new Button();
            StatusLabel = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // ActivateBT
            // 
            ActivateBT.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            ActivateBT.Location = new Point(22, 32);
            ActivateBT.Name = "ActivateBT";
            ActivateBT.Size = new Size(167, 34);
            ActivateBT.TabIndex = 0;
            ActivateBT.Text = "Activate";
            ActivateBT.UseVisualStyleBackColor = true;
            ActivateBT.Click += ActivateBT_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5.14979F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45.0520821F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44.53125F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5.14979F));
            tableLayoutPanel1.Controls.Add(ActivateBT, 1, 1);
            tableLayoutPanel1.Controls.Add(DeactivateBT, 2, 1);
            tableLayoutPanel1.Controls.Add(StatusLabel, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel1.Size = new Size(384, 101);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // DeactivateBT
            // 
            DeactivateBT.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            DeactivateBT.Location = new Point(195, 33);
            DeactivateBT.Name = "DeactivateBT";
            DeactivateBT.Size = new Size(165, 33);
            DeactivateBT.TabIndex = 1;
            DeactivateBT.Text = "Deactivate";
            DeactivateBT.UseVisualStyleBackColor = true;
            DeactivateBT.Click += DeactivateBT_Click;
            // 
            // StatusLabel
            // 
            StatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.SetColumnSpan(StatusLabel, 2);
            StatusLabel.ForeColor = Color.Red;
            StatusLabel.Location = new Point(22, 0);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(338, 22);
            StatusLabel.TabIndex = 2;
            StatusLabel.Text = "Application is not active";
            StatusLabel.TextAlign = ContentAlignment.BottomCenter;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 101);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "APOD Wallpaper Manager";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            SizeChanged += MainForm_SizeChanged;
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button ActivateBT;
        private TableLayoutPanel tableLayoutPanel1;
        private Button DeactivateBT;
        private Label StatusLabel;
    }
}