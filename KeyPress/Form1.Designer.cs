namespace KeyPress
{
    partial class Form1
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
            this.displayLabel = new System.Windows.Forms.Label();
            this.releaseLabel = new System.Windows.Forms.Label();
            this.trackpadSurface = new System.Windows.Forms.Panel();
            this.trackpadDot = new System.Windows.Forms.Panel();
            this.trackpadCoordLabel = new System.Windows.Forms.Label();
            this.trackpadTitleLabel = new System.Windows.Forms.Label();
            this.touchSurface = new System.Windows.Forms.Panel();
            this.touchCoordLabel = new System.Windows.Forms.Label();
            this.touchTitleLabel = new System.Windows.Forms.Label();
            this.trackpadRawSurface = new System.Windows.Forms.Panel();
            this.trackpadRawCoordLabel = new System.Windows.Forms.Label();
            this.trackpadRawTitleLabel = new System.Windows.Forms.Label();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewCursorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewTouchScreenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewTrackpadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewTrackpadRawMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.viewKeyPressTextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewKeyReleaseTextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewResetSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.viewResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.trackpadSurface.SuspendLayout();
            this.touchSurface.SuspendLayout();
            this.trackpadRawSurface.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.boxesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // displayLabel
            // 
            this.displayLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.displayLabel.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.displayLabel.Location = new System.Drawing.Point(0, 24);
            this.displayLabel.Name = "displayLabel";
            this.displayLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 9);
            this.displayLabel.Size = new System.Drawing.Size(932, 357);
            this.displayLabel.TabIndex = 0;
            this.displayLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // releaseLabel
            // 
            this.releaseLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.releaseLabel.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.releaseLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.releaseLabel.Location = new System.Drawing.Point(0, 381);
            this.releaseLabel.Name = "releaseLabel";
            this.releaseLabel.Size = new System.Drawing.Size(932, 52);
            this.releaseLabel.TabIndex = 1;
            this.releaseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackpadSurface
            // 
            this.trackpadSurface.BackColor = System.Drawing.SystemColors.Window;
            this.trackpadSurface.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.trackpadSurface.Controls.Add(this.trackpadDot);
            this.trackpadSurface.Controls.Add(this.trackpadCoordLabel);
            this.trackpadSurface.Controls.Add(this.trackpadTitleLabel);
            this.trackpadSurface.Location = new System.Drawing.Point(233, 0);
            this.trackpadSurface.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.trackpadSurface.Name = "trackpadSurface";
            this.trackpadSurface.Size = new System.Drawing.Size(206, 104);
            this.trackpadSurface.TabIndex = 3;
            // 
            // trackpadDot
            // 
            this.trackpadDot.BackColor = System.Drawing.Color.SeaGreen;
            this.trackpadDot.Location = new System.Drawing.Point(99, 70);
            this.trackpadDot.Name = "trackpadDot";
            this.trackpadDot.Size = new System.Drawing.Size(7, 7);
            this.trackpadDot.TabIndex = 0;
            this.trackpadDot.Visible = false;
            // 
            // trackpadCoordLabel
            // 
            this.trackpadCoordLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.trackpadCoordLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.trackpadCoordLabel.Location = new System.Drawing.Point(0, 19);
            this.trackpadCoordLabel.Name = "trackpadCoordLabel";
            this.trackpadCoordLabel.Size = new System.Drawing.Size(204, 24);
            this.trackpadCoordLabel.TabIndex = 1;
            this.trackpadCoordLabel.Text = "X: 0, Y: 0";
            this.trackpadCoordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackpadTitleLabel
            // 
            this.trackpadTitleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.trackpadTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.trackpadTitleLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.trackpadTitleLabel.Location = new System.Drawing.Point(0, 0);
            this.trackpadTitleLabel.Name = "trackpadTitleLabel";
            this.trackpadTitleLabel.Size = new System.Drawing.Size(204, 19);
            this.trackpadTitleLabel.TabIndex = 2;
            this.trackpadTitleLabel.Text = "Trackpad";
            this.trackpadTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // touchSurface
            // 
            this.touchSurface.BackColor = System.Drawing.SystemColors.Window;
            this.touchSurface.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.touchSurface.Controls.Add(this.touchCoordLabel);
            this.touchSurface.Controls.Add(this.touchTitleLabel);
            this.touchSurface.Location = new System.Drawing.Point(9, 0);
            this.touchSurface.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.touchSurface.Name = "touchSurface";
            this.touchSurface.Size = new System.Drawing.Size(206, 104);
            this.touchSurface.TabIndex = 4;
            // 
            // touchCoordLabel
            // 
            this.touchCoordLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.touchCoordLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.touchCoordLabel.Location = new System.Drawing.Point(0, 19);
            this.touchCoordLabel.Name = "touchCoordLabel";
            this.touchCoordLabel.Size = new System.Drawing.Size(204, 24);
            this.touchCoordLabel.TabIndex = 0;
            this.touchCoordLabel.Text = "No touch";
            this.touchCoordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // touchTitleLabel
            // 
            this.touchTitleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.touchTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.touchTitleLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.touchTitleLabel.Location = new System.Drawing.Point(0, 0);
            this.touchTitleLabel.Name = "touchTitleLabel";
            this.touchTitleLabel.Size = new System.Drawing.Size(204, 19);
            this.touchTitleLabel.TabIndex = 1;
            this.touchTitleLabel.Text = "Touch Screen";
            this.touchTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackpadRawSurface
            // 
            this.trackpadRawSurface.BackColor = System.Drawing.SystemColors.Window;
            this.trackpadRawSurface.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.trackpadRawSurface.Controls.Add(this.trackpadRawCoordLabel);
            this.trackpadRawSurface.Controls.Add(this.trackpadRawTitleLabel);
            this.trackpadRawSurface.Location = new System.Drawing.Point(457, 0);
            this.trackpadRawSurface.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.trackpadRawSurface.Name = "trackpadRawSurface";
            this.trackpadRawSurface.Size = new System.Drawing.Size(275, 139);
            this.trackpadRawSurface.TabIndex = 5;
            // 
            // trackpadRawCoordLabel
            // 
            this.trackpadRawCoordLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.trackpadRawCoordLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.trackpadRawCoordLabel.Location = new System.Drawing.Point(0, 19);
            this.trackpadRawCoordLabel.Name = "trackpadRawCoordLabel";
            this.trackpadRawCoordLabel.Size = new System.Drawing.Size(273, 24);
            this.trackpadRawCoordLabel.TabIndex = 1;
            this.trackpadRawCoordLabel.Text = "X: 0, Y: 0";
            this.trackpadRawCoordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackpadRawTitleLabel
            // 
            this.trackpadRawTitleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.trackpadRawTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.trackpadRawTitleLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.trackpadRawTitleLabel.Location = new System.Drawing.Point(0, 0);
            this.trackpadRawTitleLabel.Name = "trackpadRawTitleLabel";
            this.trackpadRawTitleLabel.Size = new System.Drawing.Size(273, 19);
            this.trackpadRawTitleLabel.TabIndex = 2;
            this.trackpadRawTitleLabel.Text = "Trackpad Surface (raw)";
            this.trackpadRawTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.viewMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.mainMenuStrip.ShowItemToolTips = true;
            this.mainMenuStrip.Size = new System.Drawing.Size(932, 24);
            this.mainMenuStrip.TabIndex = 6;
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileMenuItem.Text = "&File";
            // 
            // refreshMenuItem
            // 
            this.refreshMenuItem.Name = "refreshMenuItem";
            this.refreshMenuItem.Size = new System.Drawing.Size(113, 22);
            this.refreshMenuItem.Text = "Refresh";
            this.refreshMenuItem.ToolTipText = "Refresh hardware / monitor connections";
            // 
            // viewMenuItem
            // 
            this.viewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewCursorMenuItem,
            this.viewTouchScreenMenuItem,
            this.viewTrackpadMenuItem,
            this.viewTrackpadRawMenuItem,
            this.viewMenuSeparator,
            this.viewKeyPressTextMenuItem,
            this.viewKeyReleaseTextMenuItem,
            this.viewResetSeparator,
            this.viewResetMenuItem});
            this.viewMenuItem.Name = "viewMenuItem";
            this.viewMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewMenuItem.Text = "&View";
            // 
            // viewCursorMenuItem
            // 
            this.viewCursorMenuItem.CheckOnClick = true;
            this.viewCursorMenuItem.Name = "viewCursorMenuItem";
            this.viewCursorMenuItem.Size = new System.Drawing.Size(194, 22);
            this.viewCursorMenuItem.Text = "Cursor";
            // 
            // viewTouchScreenMenuItem
            // 
            this.viewTouchScreenMenuItem.CheckOnClick = true;
            this.viewTouchScreenMenuItem.Name = "viewTouchScreenMenuItem";
            this.viewTouchScreenMenuItem.Size = new System.Drawing.Size(194, 22);
            this.viewTouchScreenMenuItem.Text = "Touch Screen";
            // 
            // viewTrackpadMenuItem
            // 
            this.viewTrackpadMenuItem.CheckOnClick = true;
            this.viewTrackpadMenuItem.Name = "viewTrackpadMenuItem";
            this.viewTrackpadMenuItem.Size = new System.Drawing.Size(194, 22);
            this.viewTrackpadMenuItem.Text = "Trackpad";
            // 
            // viewTrackpadRawMenuItem
            // 
            this.viewTrackpadRawMenuItem.CheckOnClick = true;
            this.viewTrackpadRawMenuItem.Name = "viewTrackpadRawMenuItem";
            this.viewTrackpadRawMenuItem.Size = new System.Drawing.Size(194, 22);
            this.viewTrackpadRawMenuItem.Text = "Trackpad Surface (raw)";
            // 
            // viewMenuSeparator
            // 
            this.viewMenuSeparator.Name = "viewMenuSeparator";
            this.viewMenuSeparator.Size = new System.Drawing.Size(191, 6);
            // 
            // viewKeyPressTextMenuItem
            // 
            this.viewKeyPressTextMenuItem.CheckOnClick = true;
            this.viewKeyPressTextMenuItem.Name = "viewKeyPressTextMenuItem";
            this.viewKeyPressTextMenuItem.Size = new System.Drawing.Size(194, 22);
            this.viewKeyPressTextMenuItem.Text = "Key Press Text";
            // 
            // viewKeyReleaseTextMenuItem
            // 
            this.viewKeyReleaseTextMenuItem.CheckOnClick = true;
            this.viewKeyReleaseTextMenuItem.Name = "viewKeyReleaseTextMenuItem";
            this.viewKeyReleaseTextMenuItem.Size = new System.Drawing.Size(194, 22);
            this.viewKeyReleaseTextMenuItem.Text = "Key Release Text";
            // 
            // viewResetSeparator
            // 
            this.viewResetSeparator.Name = "viewResetSeparator";
            this.viewResetSeparator.Size = new System.Drawing.Size(191, 6);
            // 
            // viewResetMenuItem
            // 
            this.viewResetMenuItem.Name = "viewResetMenuItem";
            this.viewResetMenuItem.Size = new System.Drawing.Size(194, 22);
            this.viewResetMenuItem.Text = "Reset";
            this.viewResetMenuItem.ToolTipText = "Reset window size and device visibility to defaults";
            // 
            // boxesPanel
            // 
            this.boxesPanel.AutoSize = true;
            this.boxesPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.boxesPanel.BackColor = System.Drawing.Color.Transparent;
            this.boxesPanel.Controls.Add(this.touchSurface);
            this.boxesPanel.Controls.Add(this.trackpadSurface);
            this.boxesPanel.Controls.Add(this.trackpadRawSurface);
            this.boxesPanel.Location = new System.Drawing.Point(0, 26);
            this.boxesPanel.Margin = new System.Windows.Forms.Padding(0);
            this.boxesPanel.Name = "boxesPanel";
            this.boxesPanel.Size = new System.Drawing.Size(741, 139);
            this.boxesPanel.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(932, 433);
            this.Controls.Add(this.boxesPanel);
            this.Controls.Add(this.displayLabel);
            this.Controls.Add(this.releaseLabel);
            this.Controls.Add(this.mainMenuStrip);
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "Form1";
            this.Text = "KeyPress";
            this.trackpadSurface.ResumeLayout(false);
            this.touchSurface.ResumeLayout(false);
            this.trackpadRawSurface.ResumeLayout(false);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.boxesPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label displayLabel;
        private Label releaseLabel;
        private Panel trackpadSurface;
        private Label trackpadTitleLabel;
        private Label trackpadCoordLabel;
        private Panel trackpadDot;
        private Panel touchSurface;
        private Label touchTitleLabel;
        private Label touchCoordLabel;
        private Panel trackpadRawSurface;
        private Label trackpadRawTitleLabel;
        private Label trackpadRawCoordLabel;
        private MenuStrip mainMenuStrip;
        private ToolStripMenuItem fileMenuItem;
        private ToolStripMenuItem refreshMenuItem;
        private ToolStripMenuItem viewMenuItem;
        private ToolStripMenuItem viewCursorMenuItem;
        private ToolStripMenuItem viewTrackpadMenuItem;
        private ToolStripMenuItem viewTrackpadRawMenuItem;
        private ToolStripMenuItem viewTouchScreenMenuItem;
        private ToolStripSeparator viewMenuSeparator;
        private ToolStripMenuItem viewKeyPressTextMenuItem;
        private ToolStripMenuItem viewKeyReleaseTextMenuItem;
        private ToolStripSeparator viewResetSeparator;
        private ToolStripMenuItem viewResetMenuItem;
        private FlowLayoutPanel boxesPanel;
    }
}
