using System.ComponentModel;

namespace SrcML_Visual_Studio_Transform_Extension
{
    partial class TransformPreviewPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.srcmlLoadWorker = new System.ComponentModel.BackgroundWorker();
            this.outputFolderComboBox = new System.Windows.Forms.ComboBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.LocationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OriginalSourceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TransformedSourceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EnabledColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.exportSourceWorker = new System.ComponentModel.BackgroundWorker();
            this.srcmlGenWorker = new System.ComponentModel.BackgroundWorker();
            this.projectFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.saveTransformDialog = new System.Windows.Forms.SaveFileDialog();
            this.inputFolderComboBox = new System.Windows.Forms.ComboBox();
            this.transformComboBox = new System.Windows.Forms.ComboBox();
            this.runQueryButton = new System.Windows.Forms.Button();
            this.runTransformButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.progressLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.messageLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.browseOutputFoldersButton = new System.Windows.Forms.Button();
            this.browseInputFoldersButton = new System.Windows.Forms.Button();
            this.queryWorker = new System.ComponentModel.BackgroundWorker();
            this.categoryTreeView = new System.Windows.Forms.TreeView();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.statusBar.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // srcmlLoadWorker
            // 
            this.srcmlLoadWorker.WorkerReportsProgress = true;
            this.srcmlLoadWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.srcmlLoadWorker_DoWork_1);
            // 
            // outputFolderComboBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.outputFolderComboBox, 3);
            this.outputFolderComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputFolderComboBox.FormattingEnabled = true;
            this.outputFolderComboBox.Location = new System.Drawing.Point(3, 31);
            this.outputFolderComboBox.Name = "outputFolderComboBox";
            this.outputFolderComboBox.Size = new System.Drawing.Size(816, 21);
            this.outputFolderComboBox.TabIndex = 4;
            this.outputFolderComboBox.Text = "Select an output directory";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LocationColumn,
            this.OriginalSourceColumn,
            this.TransformedSourceColumn,
            this.EnabledColumn});
            this.tableLayoutPanel1.SetColumnSpan(this.dataGridView1, 3);
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(225, 87);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(682, 207);
            this.dataGridView1.TabIndex = 0;
            // 
            // LocationColumn
            // 
            this.LocationColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LocationColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.LocationColumn.HeaderText = "Location";
            this.LocationColumn.Name = "LocationColumn";
            this.LocationColumn.ReadOnly = true;
            // 
            // OriginalSourceColumn
            // 
            this.OriginalSourceColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.OriginalSourceColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.OriginalSourceColumn.FillWeight = 200F;
            this.OriginalSourceColumn.HeaderText = "Original Source";
            this.OriginalSourceColumn.Name = "OriginalSourceColumn";
            this.OriginalSourceColumn.ReadOnly = true;
            this.OriginalSourceColumn.Width = 96;
            // 
            // TransformedSourceColumn
            // 
            this.TransformedSourceColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.TransformedSourceColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.TransformedSourceColumn.FillWeight = 200F;
            this.TransformedSourceColumn.HeaderText = "Transformed Source";
            this.TransformedSourceColumn.Name = "TransformedSourceColumn";
            this.TransformedSourceColumn.ReadOnly = true;
            // 
            // EnabledColumn
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            dataGridViewCellStyle4.NullValue = false;
            this.EnabledColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.EnabledColumn.FillWeight = 10F;
            this.EnabledColumn.HeaderText = "Enabled";
            this.EnabledColumn.Name = "EnabledColumn";
            this.EnabledColumn.Width = 52;
            // 
            // exportSourceWorker
            // 
            this.exportSourceWorker.WorkerReportsProgress = true;
            // 
            // saveTransformDialog
            // 
            this.saveTransformDialog.DefaultExt = "xml";
            this.saveTransformDialog.Filter = "XML Files (*.xml)|xml";
            // 
            // inputFolderComboBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.inputFolderComboBox, 3);
            this.inputFolderComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputFolderComboBox.FormattingEnabled = true;
            this.inputFolderComboBox.Location = new System.Drawing.Point(3, 3);
            this.inputFolderComboBox.Name = "inputFolderComboBox";
            this.inputFolderComboBox.Size = new System.Drawing.Size(816, 21);
            this.inputFolderComboBox.TabIndex = 3;
            this.inputFolderComboBox.Text = "Select a source directory";
            // 
            // transformComboBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.transformComboBox, 2);
            this.transformComboBox.DisplayMember = "(none)";
            this.transformComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transformComboBox.FormattingEnabled = true;
            this.transformComboBox.Location = new System.Drawing.Point(3, 59);
            this.transformComboBox.Name = "transformComboBox";
            this.transformComboBox.Size = new System.Drawing.Size(734, 21);
            this.transformComboBox.TabIndex = 11;
            this.transformComboBox.Text = "Select a transform";
            // 
            // runQueryButton
            // 
            this.runQueryButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.runQueryButton.Location = new System.Drawing.Point(743, 59);
            this.runQueryButton.Name = "runQueryButton";
            this.runQueryButton.Size = new System.Drawing.Size(76, 22);
            this.runQueryButton.TabIndex = 1;
            this.runQueryButton.Text = "Test";
            this.runQueryButton.UseVisualStyleBackColor = true;
            // 
            // runTransformButton
            // 
            this.runTransformButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.runTransformButton.Location = new System.Drawing.Point(825, 59);
            this.runTransformButton.Name = "runTransformButton";
            this.runTransformButton.Size = new System.Drawing.Size(82, 22);
            this.runTransformButton.TabIndex = 8;
            this.runTransformButton.Text = "Execute";
            this.runTransformButton.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 17);
            // 
            // progressLabel
            // 
            this.progressLabel.BackColor = System.Drawing.Color.Transparent;
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(0, 18);
            // 
            // messageLabel
            // 
            this.messageLabel.BackColor = System.Drawing.Color.Transparent;
            this.messageLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(0, 18);
            this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // browseOutputFoldersButton
            // 
            this.browseOutputFoldersButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseOutputFoldersButton.Location = new System.Drawing.Point(825, 31);
            this.browseOutputFoldersButton.Name = "browseOutputFoldersButton";
            this.browseOutputFoldersButton.Size = new System.Drawing.Size(82, 22);
            this.browseOutputFoldersButton.TabIndex = 10;
            this.browseOutputFoldersButton.Text = "Browse...";
            this.browseOutputFoldersButton.UseVisualStyleBackColor = true;
            // 
            // browseInputFoldersButton
            // 
            this.browseInputFoldersButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.browseInputFoldersButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseInputFoldersButton.Location = new System.Drawing.Point(825, 3);
            this.browseInputFoldersButton.Name = "browseInputFoldersButton";
            this.browseInputFoldersButton.Size = new System.Drawing.Size(82, 22);
            this.browseInputFoldersButton.TabIndex = 9;
            this.browseInputFoldersButton.Text = "Browse...";
            this.browseInputFoldersButton.UseVisualStyleBackColor = true;
            this.browseInputFoldersButton.Click += new System.EventHandler(this.browseInputFoldersButton_Click);
            // 
            // queryWorker
            // 
            this.queryWorker.WorkerReportsProgress = true;
            // 
            // categoryTreeView
            // 
            this.categoryTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.categoryTreeView.HideSelection = false;
            this.categoryTreeView.Location = new System.Drawing.Point(3, 87);
            this.categoryTreeView.Name = "categoryTreeView";
            this.categoryTreeView.PathSeparator = "/";
            this.categoryTreeView.ShowNodeToolTips = true;
            this.categoryTreeView.Size = new System.Drawing.Size(216, 207);
            this.categoryTreeView.TabIndex = 12;
            // 
            // statusBar
            // 
            this.statusBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.statusBar, 4);
            this.statusBar.Dock = System.Windows.Forms.DockStyle.None;
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.messageLabel,
            this.progressLabel,
            this.progressBar});
            this.statusBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusBar.Location = new System.Drawing.Point(0, 297);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(910, 23);
            this.statusBar.TabIndex = 6;
            this.statusBar.Text = "statusStrip1";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 82F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.Controls.Add(this.outputFolderComboBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.inputFolderComboBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.transformComboBox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.runQueryButton, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.runTransformButton, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.browseOutputFoldersButton, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.browseInputFoldersButton, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.statusBar, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.categoryTreeView, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(910, 320);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // TransformPreviewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TransformPreviewPanel";
            this.Size = new System.Drawing.Size(910, 320);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private BackgroundWorker srcmlLoadWorker;
        private System.Windows.Forms.ComboBox outputFolderComboBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn LocationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn OriginalSourceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TransformedSourceColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn EnabledColumn;
        private System.Windows.Forms.ComboBox inputFolderComboBox;
        private System.Windows.Forms.ComboBox transformComboBox;
        private System.Windows.Forms.Button runQueryButton;
        private System.Windows.Forms.Button runTransformButton;
        private System.Windows.Forms.Button browseOutputFoldersButton;
        private System.Windows.Forms.Button browseInputFoldersButton;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel messageLabel;
        private System.Windows.Forms.ToolStripStatusLabel progressLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.TreeView categoryTreeView;
        private BackgroundWorker exportSourceWorker;
        private BackgroundWorker srcmlGenWorker;
        private System.Windows.Forms.FolderBrowserDialog projectFolderDialog;
        private System.Windows.Forms.SaveFileDialog saveTransformDialog;
        private BackgroundWorker queryWorker;
    }
}
