namespace ViewHtml
{
    partial class FormPrintTest
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
            this.m_webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // m_webBrowser1
            // 
            this.m_webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.m_webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.m_webBrowser1.Name = "m_webBrowser1";
            this.m_webBrowser1.ScriptErrorsSuppressed = true;
            this.m_webBrowser1.Size = new System.Drawing.Size(1426, 801);
            this.m_webBrowser1.TabIndex = 0;
            // 
            // FormPrintTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1426, 801);
            this.Controls.Add(this.m_webBrowser1);
            this.Name = "FormPrintTest";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "A4 from preview";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser m_webBrowser1;
    }
}