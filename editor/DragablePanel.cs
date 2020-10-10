using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace wjw.Editor
{
    public class DragablePanel :System.Windows.Forms.Panel
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int IParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        private Button btnClose = new Button();
        private Button btnMinMax = new Button();
        private int originalWidth;
        private int originalHeight;
        private int orignalTop = 0;
        private int orignalLeft = 0;
        public bool Dragable { get; set; } = true;
        public DragablePanel()
        {
            //create the min and close button
            if(!this.DesignMode)
            {
                InitButton();
            }
        }
        private void InitButton()
        {
           // this.btnClose.BackgroundImage = global::LogMonitor.Properties.Resources.close2;
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnClose.BackColor = Color.Red;
           // this.btnClose.Location = new System.Drawing.Point(358, 4);
            this.btnClose.Size = new System.Drawing.Size(25, 25);
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += BtnClose_Click; 
            this.btnClose.Text = "X";

           // this.btnMinMax.BackgroundImage = global::LogMonitor.Properties.Resources.max;
            this.btnMinMax.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnMinMax.BackColor = Color.Red;
            this.btnMinMax.Text = "-";
            // this.btnClose.Location = new System.Drawing.Point(358, 4);
            this.btnMinMax.Size = new System.Drawing.Size(25, 25);
            this.btnMinMax.UseVisualStyleBackColor = true;
            this.btnMinMax.Click += BtnMinMax_Click;

            this.Controls.Add(btnClose);
            this.Controls.Add(btnMinMax);
        }
        private bool _isMax = false;

        private void BtnMinMax_Click(object sender, EventArgs e)
        {
            MinMax();
        }

        private void MinMax()
        {
            if (this.Parent.Parent != null)
            {
                if (!_isMax)
                {
                    _isMax = true;
                    orignalTop = this.Parent.Top;
                    orignalLeft = this.Parent.Left;
                    originalHeight = this.Parent.Height;
                    originalWidth = this.Parent.Width;
                   // btnMinMax.Image = Properties.Resources.min;
                    this.Parent.Size = new Size(this.Parent.Parent.Width, this.Parent.Parent.Height);
                    this.Parent.Top = 0;
                    this.Parent.Left = 0;

                }
                else
                {
                    _isMax = false;
                    this.Parent.Top = orignalTop;
                    this.Parent.Left = orignalLeft;
                 //   btnMinMax.Image = Properties.Resources.max;
                    this.Parent.Size = new Size(originalWidth, originalHeight);
                }
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            HideParent();
        }

        private void HideParent()
        {
            if (this.Parent != null)
                this.Parent.Visible = false;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            btnClose.Location = new Point(this.Width - btnClose.Width, (this.Height-btnClose.Height)/2);//y hardcode to 10
            btnMinMax.Location = new Point(this.Width - btnClose.Width - btnMinMax.Width - 5, (this.Height - btnMinMax.Height) / 2);
            if(!_isMax)
            {
                if (this.Parent != null)
                {
                    originalHeight = this.Parent.Height;
                    originalWidth = this.Parent.Width;
                }
            }
            base.OnSizeChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }
           

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            MinMax();
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if(Dragable)
            {
                ReleaseCapture();
                if (this.Parent != null)
                    SendMessage(this.Parent.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
                else
                    SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
            }

        }

    }
}
