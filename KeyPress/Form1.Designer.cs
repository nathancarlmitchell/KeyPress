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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            displayLabel = new Label();
            releaseLabel = new Label();
            trackpadSurface = new Panel();
            trackpadDot = new Panel();
            trackpadCoordLabel = new Label();
            trackpadTitleLabel = new Label();
            touchSurface = new Panel();
            touchCoordLabel = new Label();
            touchTitleLabel = new Label();
            trackpadRawSurface = new Panel();
            trackpadRawCoordLabel = new Label();
            trackpadRawTitleLabel = new Label();
            mainMenuStrip = new MenuStrip();
            fileMenuItem = new ToolStripMenuItem();
            refreshMenuItem = new ToolStripMenuItem();
            viewMenuItem = new ToolStripMenuItem();
            viewCursorMenuItem = new ToolStripMenuItem();
            viewTouchScreenMenuItem = new ToolStripMenuItem();
            viewTrackpadMenuItem = new ToolStripMenuItem();
            viewTrackpadRawMenuItem = new ToolStripMenuItem();
            viewMenuSeparator = new ToolStripSeparator();
            viewKeyPressTextMenuItem = new ToolStripMenuItem();
            viewKeyReleaseTextMenuItem = new ToolStripMenuItem();
            viewResetSeparator = new ToolStripSeparator();
            viewResetMenuItem = new ToolStripMenuItem();
            boxesPanel = new FlowLayoutPanel();
            trackpadSurface.SuspendLayout();
            touchSurface.SuspendLayout();
            trackpadRawSurface.SuspendLayout();
            mainMenuStrip.SuspendLayout();
            boxesPanel.SuspendLayout();
            SuspendLayout();
            // 
            // displayLabel
            // 
            displayLabel.Dock = DockStyle.Fill;
            displayLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            displayLabel.Location = new Point(0, 24);
            displayLabel.Name = "displayLabel";
            displayLabel.Size = new Size(884, 437);
            displayLabel.TabIndex = 0;
            displayLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // releaseLabel
            // 
            releaseLabel.Dock = DockStyle.Bottom;
            releaseLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            releaseLabel.ForeColor = SystemColors.ControlText;
            releaseLabel.Location = new Point(0, 461);
            releaseLabel.Name = "releaseLabel";
            releaseLabel.Size = new Size(884, 100);
            releaseLabel.TabIndex = 1;
            releaseLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // trackpadSurface
            // 
            trackpadSurface.BackColor = SystemColors.Window;
            trackpadSurface.BorderStyle = BorderStyle.FixedSingle;
            trackpadSurface.Controls.Add(trackpadDot);
            trackpadSurface.Controls.Add(trackpadCoordLabel);
            trackpadSurface.Controls.Add(trackpadTitleLabel);
            trackpadSurface.Location = new Point(270, 0);
            trackpadSurface.Margin = new Padding(10, 0, 10, 0);
            trackpadSurface.Name = "trackpadSurface";
            trackpadSurface.Size = new Size(240, 120);
            trackpadSurface.TabIndex = 3;
            // 
            // trackpadDot
            // 
            trackpadDot.BackColor = Color.SeaGreen;
            trackpadDot.Location = new Point(116, 81);
            trackpadDot.Name = "trackpadDot";
            trackpadDot.Size = new Size(8, 8);
            trackpadDot.TabIndex = 0;
            trackpadDot.Visible = false;
            // 
            // trackpadCoordLabel
            // 
            trackpadCoordLabel.Dock = DockStyle.Top;
            trackpadCoordLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            trackpadCoordLabel.Location = new Point(0, 22);
            trackpadCoordLabel.Name = "trackpadCoordLabel";
            trackpadCoordLabel.Size = new Size(238, 28);
            trackpadCoordLabel.TabIndex = 1;
            trackpadCoordLabel.Text = "X: 0, Y: 0";
            trackpadCoordLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // trackpadTitleLabel
            // 
            trackpadTitleLabel.Dock = DockStyle.Top;
            trackpadTitleLabel.Font = new Font("Segoe UI", 9F);
            trackpadTitleLabel.ForeColor = SystemColors.GrayText;
            trackpadTitleLabel.Location = new Point(0, 0);
            trackpadTitleLabel.Name = "trackpadTitleLabel";
            trackpadTitleLabel.Size = new Size(238, 22);
            trackpadTitleLabel.TabIndex = 2;
            trackpadTitleLabel.Text = "Trackpad";
            trackpadTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // touchSurface
            // 
            touchSurface.BackColor = SystemColors.Window;
            touchSurface.BorderStyle = BorderStyle.FixedSingle;
            touchSurface.Controls.Add(touchCoordLabel);
            touchSurface.Controls.Add(touchTitleLabel);
            touchSurface.Location = new Point(10, 0);
            touchSurface.Margin = new Padding(10, 0, 10, 0);
            touchSurface.Name = "touchSurface";
            touchSurface.Size = new Size(240, 120);
            touchSurface.TabIndex = 4;
            // 
            // touchCoordLabel
            // 
            touchCoordLabel.Dock = DockStyle.Top;
            touchCoordLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            touchCoordLabel.Location = new Point(0, 22);
            touchCoordLabel.Name = "touchCoordLabel";
            touchCoordLabel.Size = new Size(238, 28);
            touchCoordLabel.TabIndex = 0;
            touchCoordLabel.Text = "No touch";
            touchCoordLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // touchTitleLabel
            // 
            touchTitleLabel.Dock = DockStyle.Top;
            touchTitleLabel.Font = new Font("Segoe UI", 9F);
            touchTitleLabel.ForeColor = SystemColors.GrayText;
            touchTitleLabel.Location = new Point(0, 0);
            touchTitleLabel.Name = "touchTitleLabel";
            touchTitleLabel.Size = new Size(238, 22);
            touchTitleLabel.TabIndex = 1;
            touchTitleLabel.Text = "Touch Screen";
            touchTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // trackpadRawSurface
            // 
            trackpadRawSurface.BackColor = SystemColors.Window;
            trackpadRawSurface.BorderStyle = BorderStyle.FixedSingle;
            trackpadRawSurface.Controls.Add(trackpadRawCoordLabel);
            trackpadRawSurface.Controls.Add(trackpadRawTitleLabel);
            trackpadRawSurface.Location = new Point(530, 0);
            trackpadRawSurface.Margin = new Padding(10, 0, 10, 0);
            trackpadRawSurface.Name = "trackpadRawSurface";
            trackpadRawSurface.Size = new Size(320, 160);
            trackpadRawSurface.TabIndex = 5;
            //
            // trackpadRawCoordLabel
            // 
            trackpadRawCoordLabel.Dock = DockStyle.Top;
            trackpadRawCoordLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            trackpadRawCoordLabel.Location = new Point(0, 22);
            trackpadRawCoordLabel.Name = "trackpadRawCoordLabel";
            trackpadRawCoordLabel.Size = new Size(318, 28);
            trackpadRawCoordLabel.TabIndex = 1;
            trackpadRawCoordLabel.Text = "X: 0, Y: 0";
            trackpadRawCoordLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // trackpadRawTitleLabel
            // 
            trackpadRawTitleLabel.Dock = DockStyle.Top;
            trackpadRawTitleLabel.Font = new Font("Segoe UI", 9F);
            trackpadRawTitleLabel.ForeColor = SystemColors.GrayText;
            trackpadRawTitleLabel.Location = new Point(0, 0);
            trackpadRawTitleLabel.Name = "trackpadRawTitleLabel";
            trackpadRawTitleLabel.Size = new Size(318, 22);
            trackpadRawTitleLabel.TabIndex = 2;
            trackpadRawTitleLabel.Text = "Trackpad Surface (raw)";
            trackpadRawTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileMenuItem, viewMenuItem });
            mainMenuStrip.Location = new Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.ShowItemToolTips = true;
            mainMenuStrip.Size = new Size(884, 24);
            mainMenuStrip.TabIndex = 6;
            // 
            // fileMenuItem
            // 
            fileMenuItem.DropDownItems.AddRange(new ToolStripItem[] { refreshMenuItem });
            fileMenuItem.Name = "fileMenuItem";
            fileMenuItem.Size = new Size(37, 20);
            fileMenuItem.Text = "&File";
            // 
            // refreshMenuItem
            // 
            refreshMenuItem.Name = "refreshMenuItem";
            refreshMenuItem.Size = new Size(113, 22);
            refreshMenuItem.Text = "Refresh";
            refreshMenuItem.ToolTipText = "Refresh hardware / monitor connections";
            // 
            // viewMenuItem
            // 
            viewMenuItem.DropDownItems.AddRange(new ToolStripItem[] { viewCursorMenuItem, viewTouchScreenMenuItem, viewTrackpadMenuItem, viewTrackpadRawMenuItem, viewMenuSeparator, viewKeyPressTextMenuItem, viewKeyReleaseTextMenuItem, viewResetSeparator, viewResetMenuItem });
            viewMenuItem.Name = "viewMenuItem";
            viewMenuItem.Size = new Size(44, 20);
            viewMenuItem.Text = "&View";
            // 
            // viewCursorMenuItem
            // 
            viewCursorMenuItem.CheckOnClick = true;
            viewCursorMenuItem.Name = "viewCursorMenuItem";
            viewCursorMenuItem.Size = new Size(194, 22);
            viewCursorMenuItem.Text = "Cursor";
            // 
            // viewTouchScreenMenuItem
            // 
            viewTouchScreenMenuItem.CheckOnClick = true;
            viewTouchScreenMenuItem.Name = "viewTouchScreenMenuItem";
            viewTouchScreenMenuItem.Size = new Size(194, 22);
            viewTouchScreenMenuItem.Text = "Touch Screen";
            // 
            // viewTrackpadMenuItem
            // 
            viewTrackpadMenuItem.CheckOnClick = true;
            viewTrackpadMenuItem.Name = "viewTrackpadMenuItem";
            viewTrackpadMenuItem.Size = new Size(194, 22);
            viewTrackpadMenuItem.Text = "Trackpad";
            // 
            // viewTrackpadRawMenuItem
            // 
            viewTrackpadRawMenuItem.CheckOnClick = true;
            viewTrackpadRawMenuItem.Name = "viewTrackpadRawMenuItem";
            viewTrackpadRawMenuItem.Size = new Size(194, 22);
            viewTrackpadRawMenuItem.Text = "Trackpad Surface (raw)";
            // 
            // viewMenuSeparator
            // 
            viewMenuSeparator.Name = "viewMenuSeparator";
            viewMenuSeparator.Size = new Size(191, 6);
            // 
            // viewKeyPressTextMenuItem
            // 
            viewKeyPressTextMenuItem.CheckOnClick = true;
            viewKeyPressTextMenuItem.Name = "viewKeyPressTextMenuItem";
            viewKeyPressTextMenuItem.Size = new Size(194, 22);
            viewKeyPressTextMenuItem.Text = "Key Press Text";
            // 
            // viewKeyReleaseTextMenuItem
            // 
            viewKeyReleaseTextMenuItem.CheckOnClick = true;
            viewKeyReleaseTextMenuItem.Name = "viewKeyReleaseTextMenuItem";
            viewKeyReleaseTextMenuItem.Size = new Size(194, 22);
            viewKeyReleaseTextMenuItem.Text = "Key Release Text";
            //
            // viewResetSeparator
            //
            viewResetSeparator.Name = "viewResetSeparator";
            viewResetSeparator.Size = new Size(191, 6);
            //
            // viewResetMenuItem
            //
            viewResetMenuItem.Name = "viewResetMenuItem";
            viewResetMenuItem.Size = new Size(194, 22);
            viewResetMenuItem.Text = "Reset";
            viewResetMenuItem.ToolTipText = "Reset window size and device visibility to defaults";
            //
            // boxesPanel
            // 
            boxesPanel.AutoSize = true;
            boxesPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            boxesPanel.BackColor = Color.Transparent;
            boxesPanel.Controls.Add(touchSurface);
            boxesPanel.Controls.Add(trackpadSurface);
            boxesPanel.Controls.Add(trackpadRawSurface);
            boxesPanel.Location = new Point(0, 30);
            boxesPanel.Margin = new Padding(0);
            boxesPanel.Name = "boxesPanel";
            boxesPanel.Size = new Size(860, 160);
            boxesPanel.TabIndex = 7;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 561);
            Controls.Add(boxesPanel);
            Controls.Add(displayLabel);
            Controls.Add(releaseLabel);
            Controls.Add(mainMenuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = mainMenuStrip;
            Name = "Form1";
            Text = "KeyPress";
            trackpadSurface.ResumeLayout(false);
            touchSurface.ResumeLayout(false);
            trackpadRawSurface.ResumeLayout(false);
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            boxesPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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
