namespace Aneesa.UI
{
    partial class MainForm
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
        	this.statusCircle = new Aneesa.UI.CircleButton();
        	this.statusText = new System.Windows.Forms.Label();
        	this.SuspendLayout();
        	// 
        	// statusCircle
        	// 
        	this.statusCircle.Location = new System.Drawing.Point(108, 57);
        	this.statusCircle.Name = "statusCircle";
        	this.statusCircle.Size = new System.Drawing.Size(75, 75);
        	this.statusCircle.TabIndex = 1;
        	// 
        	// statusText
        	// 
        	this.statusText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        	this.statusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.statusText.Location = new System.Drawing.Point(12, 146);
        	this.statusText.Name = "statusText";
        	this.statusText.Size = new System.Drawing.Size(270, 20);
        	this.statusText.TabIndex = 2;
        	this.statusText.Text = "What\'s up, yo?";
        	this.statusText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        	// 
        	// MainForm
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(294, 454);
        	this.Controls.Add(this.statusText);
        	this.Controls.Add(this.statusCircle);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        	this.MaximizeBox = false;
        	this.Name = "MainForm";
        	this.Text = "Aneesa";
        	this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
        	this.Load += new System.EventHandler(this.MainForm_Load);
        	this.ResumeLayout(false);
        }

        #endregion
        private Aneesa.UI.CircleButton statusCircle;
        private System.Windows.Forms.Label statusText;
    }
}

