namespace UIServiceInWPF
{
    partial class WebBrowserHost
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
            this.webBrowserHtml = new UIServiceInWPF.ExtendWebbrowser();
            this.SuspendLayout();
            // 
            // webBrowserHtml
            // 
            this.webBrowserHtml.AllowWebBrowserDrop = false;
            this.webBrowserHtml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserHtml.IsWebBrowserContextMenuEnabled = false;
            this.webBrowserHtml.Location = new System.Drawing.Point(0, 0);
            this.webBrowserHtml.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserHtml.Name = "webBrowserHtml";
            this.webBrowserHtml.ScriptErrorsSuppressed = true;
            this.webBrowserHtml.ScrollBarsEnabled = false;
            this.webBrowserHtml.Size = new System.Drawing.Size(284, 262);
            this.webBrowserHtml.TabIndex = 0;
            this.webBrowserHtml.WebBrowserShortcutsEnabled = false;
            // 
            // WebBrowserHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.ControlBox = false;
            this.Controls.Add(this.webBrowserHtml);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WebBrowserHost";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Load += new System.EventHandler(this.WebBrowserHost_Load);
            this.Resize += new System.EventHandler(this.WebBrowserHost_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private ExtendWebbrowser webBrowserHtml;
    }
}