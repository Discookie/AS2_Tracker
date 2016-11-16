namespace ASh_Tracker
{
    partial class OptionsMenu
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.shouldStartup = new System.Windows.Forms.CheckBox();
            this.shouldTrayNotify = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 22);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(161, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(180, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Browse";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path to Audioshield output_log.txt:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Options:";
            // 
            // shouldStartup
            // 
            this.shouldStartup.AutoSize = true;
            this.shouldStartup.Checked = true;
            this.shouldStartup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.shouldStartup.Location = new System.Drawing.Point(15, 61);
            this.shouldStartup.Name = "shouldStartup";
            this.shouldStartup.Size = new System.Drawing.Size(96, 17);
            this.shouldStartup.TabIndex = 5;
            this.shouldStartup.Text = "Run on startup";
            this.shouldStartup.UseVisualStyleBackColor = true;
            this.shouldStartup.CheckedChanged += new System.EventHandler(this.shouldStartup_CheckedChanged);
            // 
            // shouldTrayNotify
            // 
            this.shouldTrayNotify.AutoSize = true;
            this.shouldTrayNotify.Checked = true;
            this.shouldTrayNotify.CheckState = System.Windows.Forms.CheckState.Checked;
            this.shouldTrayNotify.Location = new System.Drawing.Point(15, 80);
            this.shouldTrayNotify.Name = "shouldTrayNotify";
            this.shouldTrayNotify.Size = new System.Drawing.Size(132, 17);
            this.shouldTrayNotify.TabIndex = 6;
            this.shouldTrayNotify.Text = "Show tray notifications";
            this.shouldTrayNotify.UseVisualStyleBackColor = true;
            this.shouldTrayNotify.CheckedChanged += new System.EventHandler(this.shouldTrayNotify_CheckedChanged);
            // 
            // OptionsMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 104);
            this.Controls.Add(this.shouldTrayNotify);
            this.Controls.Add(this.shouldStartup);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "OptionsMenu";
            this.Text = "Options";
            this.Shown += new System.EventHandler(this.OptionsMenu_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox shouldStartup;
        private System.Windows.Forms.CheckBox shouldTrayNotify;
    }
}