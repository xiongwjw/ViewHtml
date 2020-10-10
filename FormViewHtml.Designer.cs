namespace ViewHtml
{
    partial class FormViewHtml
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormViewHtml));
            this.tvList = new System.Windows.Forms.TreeView();
            this.topPannel = new System.Windows.Forms.Panel();
            this.btnPrintTest = new System.Windows.Forms.Button();
            this.btnStarteCat = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lbIds = new System.Windows.Forms.Label();
            this.btnOpenPath = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitCenter = new System.Windows.Forms.SplitContainer();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.wb = new ViewHtml.wjwWebBrowser();
            this.pnActivity = new System.Windows.Forms.Panel();
            this.lbActivity = new System.Windows.Forms.ListBox();
            this.cbLanguage = new System.Windows.Forms.ComboBox();
            this.editorLog = new wjw.editor.Editor();
            this.topPannel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitCenter)).BeginInit();
            this.splitCenter.Panel1.SuspendLayout();
            this.splitCenter.Panel2.SuspendLayout();
            this.splitCenter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnActivity.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvList
            // 
            this.tvList.BackColor = System.Drawing.Color.White;
            this.tvList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tvList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvList.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvList.ForeColor = System.Drawing.SystemColors.WindowText;
            this.tvList.Location = new System.Drawing.Point(0, 0);
            this.tvList.Name = "tvList";
            this.tvList.ShowNodeToolTips = true;
            this.tvList.Size = new System.Drawing.Size(255, 874);
            this.tvList.TabIndex = 3;
            this.tvList.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvList_BeforeSelect);
            this.tvList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvList_AfterSelect);
            this.tvList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tvList_MouseClick);
            this.tvList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tvList_MouseDoubleClick);
            // 
            // topPannel
            // 
            this.topPannel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(199)))), ((int)(((byte)(216)))));
            this.topPannel.Controls.Add(this.btnPrintTest);
            this.topPannel.Controls.Add(this.btnStarteCat);
            this.topPannel.Controls.Add(this.btnHelp);
            this.topPannel.Controls.Add(this.txtPath);
            this.topPannel.Controls.Add(this.lbIds);
            this.topPannel.Controls.Add(this.btnOpenPath);
            this.topPannel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPannel.Location = new System.Drawing.Point(0, 0);
            this.topPannel.Margin = new System.Windows.Forms.Padding(4);
            this.topPannel.Name = "topPannel";
            this.topPannel.Size = new System.Drawing.Size(2197, 65);
            this.topPannel.TabIndex = 9;
            // 
            // btnPrintTest
            // 
            this.btnPrintTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(221)))), ((int)(((byte)(221)))));
            this.btnPrintTest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnPrintTest.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnPrintTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrintTest.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrintTest.Location = new System.Drawing.Point(1642, 4);
            this.btnPrintTest.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrintTest.Name = "btnPrintTest";
            this.btnPrintTest.Size = new System.Drawing.Size(197, 48);
            this.btnPrintTest.TabIndex = 13;
            this.btnPrintTest.Text = "ViewForm";
            this.btnPrintTest.UseVisualStyleBackColor = false;
            this.btnPrintTest.Visible = false;
            this.btnPrintTest.Click += new System.EventHandler(this.btnPrintTest_Click);
            // 
            // btnStarteCat
            // 
            this.btnStarteCat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(221)))), ((int)(((byte)(221)))));
            this.btnStarteCat.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnStarteCat.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnStarteCat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStarteCat.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStarteCat.Location = new System.Drawing.Point(1437, 3);
            this.btnStarteCat.Margin = new System.Windows.Forms.Padding(4);
            this.btnStarteCat.Name = "btnStarteCat";
            this.btnStarteCat.Size = new System.Drawing.Size(197, 48);
            this.btnStarteCat.TabIndex = 12;
            this.btnStarteCat.Text = "eCat";
            this.btnStarteCat.UseVisualStyleBackColor = false;
            this.btnStarteCat.Click += new System.EventHandler(this.btnStarteCat_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(221)))), ((int)(((byte)(221)))));
            this.btnHelp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnHelp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHelp.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHelp.Location = new System.Drawing.Point(1232, 5);
            this.btnHelp.Margin = new System.Windows.Forms.Padding(4);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(197, 48);
            this.btnHelp.TabIndex = 11;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // txtPath
            // 
            this.txtPath.BackColor = System.Drawing.Color.White;
            this.txtPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPath.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPath.Location = new System.Drawing.Point(133, 13);
            this.txtPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(937, 38);
            this.txtPath.TabIndex = 9;
            this.txtPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPath_KeyDown);
            this.txtPath.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPath_KeyPress);
            // 
            // lbIds
            // 
            this.lbIds.AutoSize = true;
            this.lbIds.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbIds.ForeColor = System.Drawing.Color.Black;
            this.lbIds.Location = new System.Drawing.Point(13, 15);
            this.lbIds.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbIds.Name = "lbIds";
            this.lbIds.Size = new System.Drawing.Size(112, 31);
            this.lbIds.TabIndex = 10;
            this.lbIds.Text = "Folder:";
            // 
            // btnOpenPath
            // 
            this.btnOpenPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(221)))), ((int)(((byte)(221)))));
            this.btnOpenPath.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnOpenPath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnOpenPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenPath.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenPath.Location = new System.Drawing.Point(1078, 4);
            this.btnOpenPath.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenPath.Name = "btnOpenPath";
            this.btnOpenPath.Size = new System.Drawing.Size(146, 49);
            this.btnOpenPath.TabIndex = 8;
            this.btnOpenPath.Text = "Navigate";
            this.btnOpenPath.UseVisualStyleBackColor = false;
            this.btnOpenPath.Click += new System.EventHandler(this.btnOpenPath_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 65);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitCenter);
            this.splitContainer1.Size = new System.Drawing.Size(2197, 874);
            this.splitContainer1.SplitterDistance = 255;
            this.splitContainer1.TabIndex = 10;
            // 
            // splitCenter
            // 
            this.splitCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCenter.Location = new System.Drawing.Point(0, 0);
            this.splitCenter.Name = "splitCenter";
            this.splitCenter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitCenter.Panel1
            // 
            this.splitCenter.Panel1.Controls.Add(this.splitMain);
            // 
            // splitCenter.Panel2
            // 
            this.splitCenter.Panel2.Controls.Add(this.editorLog);
            this.splitCenter.Panel2Collapsed = true;
            this.splitCenter.Size = new System.Drawing.Size(1938, 874);
            this.splitCenter.SplitterDistance = 526;
            this.splitCenter.TabIndex = 8;
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.wb);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.pnActivity);
            this.splitMain.Panel2Collapsed = true;
            this.splitMain.Size = new System.Drawing.Size(1938, 874);
            this.splitMain.SplitterDistance = 646;
            this.splitMain.TabIndex = 7;
            // 
            // wb
            // 
            this.wb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wb.Location = new System.Drawing.Point(0, 0);
            this.wb.MinimumSize = new System.Drawing.Size(20, 21);
            this.wb.Name = "wb";
            this.wb.Size = new System.Drawing.Size(1938, 874);
            this.wb.TabIndex = 4;
            // 
            // pnActivity
            // 
            this.pnActivity.Controls.Add(this.lbActivity);
            this.pnActivity.Controls.Add(this.cbLanguage);
            this.pnActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnActivity.Location = new System.Drawing.Point(0, 0);
            this.pnActivity.Name = "pnActivity";
            this.pnActivity.Size = new System.Drawing.Size(96, 100);
            this.pnActivity.TabIndex = 6;
            // 
            // lbActivity
            // 
            this.lbActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbActivity.FormattingEnabled = true;
            this.lbActivity.ItemHeight = 31;
            this.lbActivity.Location = new System.Drawing.Point(0, 39);
            this.lbActivity.Name = "lbActivity";
            this.lbActivity.Size = new System.Drawing.Size(96, 61);
            this.lbActivity.TabIndex = 5;
            this.lbActivity.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbActivity_MouseDoubleClick);
            // 
            // cbLanguage
            // 
            this.cbLanguage.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLanguage.FormattingEnabled = true;
            this.cbLanguage.Location = new System.Drawing.Point(0, 0);
            this.cbLanguage.Name = "cbLanguage";
            this.cbLanguage.Size = new System.Drawing.Size(96, 39);
            this.cbLanguage.TabIndex = 6;
            this.cbLanguage.SelectedIndexChanged += new System.EventHandler(this.cbLanguage_SelectedIndexChanged);
            // 
            // editorLog
            // 
            this.editorLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorLog.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.editorLog.Location = new System.Drawing.Point(0, 0);
            this.editorLog.Margin = new System.Windows.Forms.Padding(2);
            this.editorLog.Name = "editorLog";
            this.editorLog.Size = new System.Drawing.Size(150, 46);
            this.editorLog.TabIndex = 0;
            this.editorLog.WrapMode = ScintillaNET.WrapMode.None;
            // 
            // FormViewHtml
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2197, 939);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.topPannel);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FormViewHtml";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "View Html";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormViewHtml_FormClosing);
            this.Load += new System.EventHandler(this.FormViewHtml_Load);
            this.Resize += new System.EventHandler(this.FormViewHtml_Resize);
            this.topPannel.ResumeLayout(false);
            this.topPannel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitCenter.Panel1.ResumeLayout(false);
            this.splitCenter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitCenter)).EndInit();
            this.splitCenter.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnActivity.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvList;
        private wjwWebBrowser wb;
        private System.Windows.Forms.Panel topPannel;
        private System.Windows.Forms.Button btnOpenPath;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label lbIds;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.ListBox lbActivity;
        private System.Windows.Forms.Button btnStarteCat;
        private System.Windows.Forms.Panel pnActivity;
        private System.Windows.Forms.ComboBox cbLanguage;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitCenter;
        private wjw.editor.Editor editorLog;
        private System.Windows.Forms.Button btnPrintTest;
    }
}