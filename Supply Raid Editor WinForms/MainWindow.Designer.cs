namespace Supply_Raid_Editor
{
    partial class MainWindow
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
            factionTab = new Panel();
            factionBtn = new Button();
            characterBtn = new Button();
            itemBtn = new Button();
            SuspendLayout();
            // 
            // factionTab
            // 
            factionTab.BorderStyle = BorderStyle.FixedSingle;
            factionTab.Location = new Point(12, 121);
            factionTab.Name = "factionTab";
            factionTab.Size = new Size(678, 1100);
            factionTab.TabIndex = 0;
            // 
            // factionBtn
            // 
            factionBtn.BackColor = Color.MistyRose;
            factionBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            factionBtn.Location = new Point(12, 12);
            factionBtn.Name = "factionBtn";
            factionBtn.Size = new Size(96, 96);
            factionBtn.TabIndex = 1;
            factionBtn.Text = "Sosig Faction";
            factionBtn.UseVisualStyleBackColor = false;
            // 
            // characterBtn
            // 
            characterBtn.BackColor = Color.DarkSeaGreen;
            characterBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            characterBtn.Location = new Point(594, 12);
            characterBtn.Name = "characterBtn";
            characterBtn.Size = new Size(96, 96);
            characterBtn.TabIndex = 2;
            characterBtn.Text = "Character";
            characterBtn.UseVisualStyleBackColor = false;
            characterBtn.Click += button1_Click;
            // 
            // itemBtn
            // 
            itemBtn.Anchor = AnchorStyles.Top;
            itemBtn.BackColor = Color.MistyRose;
            itemBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            itemBtn.Location = new Point(320, 12);
            itemBtn.Name = "itemBtn";
            itemBtn.Size = new Size(96, 96);
            itemBtn.TabIndex = 3;
            itemBtn.Text = "Item Category";
            itemBtn.UseVisualStyleBackColor = false;
            // 
            // FactinTab
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(702, 1233);
            Controls.Add(itemBtn);
            Controls.Add(characterBtn);
            Controls.Add(factionBtn);
            Controls.Add(factionTab);
            Name = "FactinTab";
            Text = "Supply Raid Editor";
            ResumeLayout(false);
        }

        #endregion

        private Panel factionTab;
        private Button factionBtn;
        private Button characterBtn;
        private Button itemBtn;
    }
}