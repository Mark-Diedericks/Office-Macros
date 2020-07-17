namespace Excel_Ribbon
{
    partial class RibbonExcelMacros : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public RibbonExcelMacros()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RibbonExcelMacros));
            this.TabDeveloper = this.Factory.CreateRibbonTab();
            this.OfficeMacros = this.Factory.CreateRibbonGroup();
            this.btnOfficeMacros = this.Factory.CreateRibbonButton();
            this.btnEmbedded = this.Factory.CreateRibbonButton();
            this.TabDeveloper.SuspendLayout();
            this.OfficeMacros.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabDeveloper
            // 
            this.TabDeveloper.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.TabDeveloper.ControlId.OfficeId = "TabDeveloper";
            this.TabDeveloper.Groups.Add(this.OfficeMacros);
            this.TabDeveloper.Label = "TabDeveloper";
            this.TabDeveloper.Name = "TabDeveloper";
            // 
            // OfficeMacros
            // 
            this.OfficeMacros.Items.Add(this.btnOfficeMacros);
            this.OfficeMacros.Items.Add(this.btnEmbedded);
            this.OfficeMacros.Label = "Office Macros";
            this.OfficeMacros.Name = "OfficeMacros";
            // 
            // btnOfficeMacros
            // 
            this.btnOfficeMacros.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnOfficeMacros.Description = "Macro Editor";
            this.btnOfficeMacros.Image = ((System.Drawing.Image)(resources.GetObject("btnOfficeMacros.Image")));
            this.btnOfficeMacros.Label = "Macro Editor";
            this.btnOfficeMacros.Name = "btnOfficeMacros";
            this.btnOfficeMacros.ScreenTip = "Office Macros Editor";
            this.btnOfficeMacros.ShowImage = true;
            this.btnOfficeMacros.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BtnOfficeMacros_Click);
            // 
            // btnEmbedded
            // 
            this.btnEmbedded.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnEmbedded.Description = "Embedded Macros";
            this.btnEmbedded.Image = ((System.Drawing.Image)(resources.GetObject("btnEmbedded.Image")));
            this.btnEmbedded.Label = "Embedded Macros";
            this.btnEmbedded.Name = "btnEmbedded";
            this.btnEmbedded.ScreenTip = "Embedded Macros";
            this.btnEmbedded.ShowImage = true;
            this.btnEmbedded.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.BtnEmbedded_Click);
            // 
            // RibbonExcelMacros
            // 
            this.Name = "RibbonExcelMacros";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.TabDeveloper);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.RibbonExcelMacros_Load);
            this.TabDeveloper.ResumeLayout(false);
            this.TabDeveloper.PerformLayout();
            this.OfficeMacros.ResumeLayout(false);
            this.OfficeMacros.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab TabDeveloper;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup OfficeMacros;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnOfficeMacros;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnEmbedded;
    }

    partial class ThisRibbonCollection
    {
        internal RibbonExcelMacros RibbonExcelMacros
        {
            get { return this.GetRibbon<RibbonExcelMacros>(); }
        }
    }
}
