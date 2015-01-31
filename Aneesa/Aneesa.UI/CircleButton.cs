using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aneesa.UI
{
    public partial class CircleButton : UserControl
    {
        public CircleButton()
        {
            InitializeComponent();
            OuterColor = Brushes.LightBlue;
            InnerColor = Brushes.White;
        }

        public Brush InnerColor { get; set; }
        public Brush OuterColor { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var rect = e.ClipRectangle;
            var g = e.Graphics;
            g.FillEllipse(OuterColor, 0, 0, rect.Width - 1, rect.Height - 1);
            g.DrawEllipse(Pens.Black, 0, 0, rect.Width - 1, rect.Height - 1);
            var innerPad = 10;
            g.FillEllipse(InnerColor, innerPad, innerPad, rect.Width - innerPad * 2 - 1, rect.Height - innerPad * 2 - 1);
            g.DrawEllipse(Pens.Black, innerPad, innerPad, rect.Width - innerPad * 2 - 1, rect.Height - innerPad * 2 - 1);
        }
    }
}