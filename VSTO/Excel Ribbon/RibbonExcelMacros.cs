using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;

namespace Excel_Ribbon
{
    public partial class RibbonExcelMacros
    {
        private void RibbonExcelMacros_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void BtnOfficeMacros_Click(object sender, RibbonControlEventArgs e)
        {
            ThisAddIn.GetInstance()?.ShowWindow();
        }

        private void BtnEmbedded_Click(object sender, RibbonControlEventArgs e)
        {
            
        }
    }
}
