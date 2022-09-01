using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;

namespace FYP_Customer_Behavior_
{
    public partial class Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ReportViewer1.ProcessingMode = ProcessingMode.Local;
            ReportViewer1.LocalReport.ReportPath = Server.MapPath("Report.rdlc");
            FYPEntities fYPEntities = new FYPEntities();
            var abc = fYPEntities.AreaCityStateCountrySelect().ToList();
            ReportDataSource datasource = new ReportDataSource("AreaDataSet", abc);
            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(datasource);
        }
    }
}