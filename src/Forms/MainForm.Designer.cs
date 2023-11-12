namespace UIXTool.Forms
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
            mnuStrip = new MenuStrip();
            mnuFile = new ToolStripMenuItem();
            mnuOpen = new ToolStripMenuItem();
            mnuAbout = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            splitContainer = new SplitContainer();
            tvContents = new TreeView();
            tabControl = new TabControl();
            tabViewer = new TabPage();
            viewerImage = new PictureBox();
            tabMeta = new TabPage();
            tabLogs = new TabPage();
            mnuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            tabControl.SuspendLayout();
            tabViewer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)viewerImage).BeginInit();
            SuspendLayout();
            // 
            // mnuStrip
            // 
            mnuStrip.Items.AddRange(new ToolStripItem[] { mnuFile, mnuAbout });
            mnuStrip.Location = new Point(0, 0);
            mnuStrip.Name = "mnuStrip";
            mnuStrip.Size = new Size(752, 24);
            mnuStrip.TabIndex = 0;
            mnuStrip.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuOpen });
            mnuFile.Name = "mnuFile";
            mnuFile.Size = new Size(37, 20);
            mnuFile.Text = "File";
            // 
            // mnuOpen
            // 
            mnuOpen.Name = "mnuOpen";
            mnuOpen.Size = new Size(103, 22);
            mnuOpen.Text = "Open";
            mnuOpen.Click += mnuOpen_Click;
            // 
            // mnuAbout
            // 
            mnuAbout.Name = "mnuAbout";
            mnuAbout.Size = new Size(52, 20);
            mnuAbout.Text = "About";
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 419);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(752, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.FixedPanel = FixedPanel.Panel1;
            splitContainer.Location = new Point(0, 24);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(tvContents);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(tabControl);
            splitContainer.Size = new Size(752, 395);
            splitContainer.SplitterDistance = 240;
            splitContainer.TabIndex = 2;
            // 
            // tvContents
            // 
            tvContents.Dock = DockStyle.Fill;
            tvContents.Location = new Point(0, 0);
            tvContents.Name = "tvContents";
            tvContents.ShowNodeToolTips = true;
            tvContents.Size = new Size(240, 395);
            tvContents.TabIndex = 0;
            tvContents.AfterSelect += tvContents_AfterSelect;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabViewer);
            tabControl.Controls.Add(tabMeta);
            tabControl.Controls.Add(tabLogs);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(508, 395);
            tabControl.TabIndex = 0;
            // 
            // tabViewer
            // 
            tabViewer.Controls.Add(viewerImage);
            tabViewer.Location = new Point(4, 24);
            tabViewer.Name = "tabViewer";
            tabViewer.Padding = new Padding(3);
            tabViewer.Size = new Size(500, 367);
            tabViewer.TabIndex = 0;
            tabViewer.Text = "Viewer";
            tabViewer.UseVisualStyleBackColor = true;
            // 
            // viewerImage
            // 
            viewerImage.BackColor = Color.White;
            viewerImage.BackgroundImageLayout = ImageLayout.Center;
            viewerImage.Dock = DockStyle.Fill;
            viewerImage.Location = new Point(3, 3);
            viewerImage.Name = "viewerImage";
            viewerImage.Size = new Size(494, 361);
            viewerImage.SizeMode = PictureBoxSizeMode.CenterImage;
            viewerImage.TabIndex = 0;
            viewerImage.TabStop = false;
            viewerImage.Paint += viewerImage_Paint;
            // 
            // tabMeta
            // 
            tabMeta.Location = new Point(4, 24);
            tabMeta.Name = "tabMeta";
            tabMeta.Padding = new Padding(3);
            tabMeta.Size = new Size(500, 367);
            tabMeta.TabIndex = 1;
            tabMeta.Text = "Meta";
            tabMeta.UseVisualStyleBackColor = true;
            // 
            // tabLogs
            // 
            tabLogs.Location = new Point(4, 24);
            tabLogs.Name = "tabLogs";
            tabLogs.Size = new Size(500, 367);
            tabLogs.TabIndex = 2;
            tabLogs.Text = "Logs";
            tabLogs.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(752, 441);
            Controls.Add(splitContainer);
            Controls.Add(statusStrip1);
            Controls.Add(mnuStrip);
            DoubleBuffered = true;
            Name = "MainForm";
            Text = "UIXTool";
            mnuStrip.ResumeLayout(false);
            mnuStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabViewer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)viewerImage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip mnuStrip;
        private ToolStripMenuItem mnuFile;
        private ToolStripMenuItem mnuAbout;
        private StatusStrip statusStrip1;
        private SplitContainer splitContainer;
        private TabControl tabControl;
        private TabPage tabViewer;
        private TabPage tabMeta;
        private TabPage tabLogs;
        private TreeView tvContents;
        private PictureBox viewerImage;
        private ToolStripMenuItem mnuOpen;
    }
}