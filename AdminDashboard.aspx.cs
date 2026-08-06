using System;
using System.Data.SqlClient;
using System.Web.UI;

public partial class AdminDashboard : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null)
        {
            Response.Redirect("AdminLogin.aspx");
            return;
        }

        lblAdminName.Text = Session["AdminFullName"] as string ?? Session["AdminUsername"] as string;

        if (!IsPostBack)
        {
            LoadStats();
        }
    }

    private void LoadStats()
    {
        // Exception handling: a DB outage here must degrade gracefully, not crash the page.
        try
        {
            // Single round trip: one query, six aggregate counts.
            var dt = DBHelper.ExecuteQuery(
                @"SELECT
                    COUNT(*) AS TotalStudents,
                    SUM(CASE WHEN AccountStatus = 'Active' THEN 1 ELSE 0 END) AS ActiveStudents,
                    SUM(CASE WHEN AccountStatus = 'Inactive' THEN 1 ELSE 0 END) AS InactiveStudents,
                    SUM(CASE WHEN ApprovalStatus = 'Pending' THEN 1 ELSE 0 END) AS PendingApplications,
                    SUM(CASE WHEN ApprovalStatus = 'Approved' THEN 1 ELSE 0 END) AS ApprovedApplications,
                    SUM(CASE WHEN ApprovalStatus = 'Rejected' THEN 1 ELSE 0 END) AS RejectedApplications
                  FROM Students");

            if (dt.Rows.Count == 0) return;

            var row = dt.Rows[0];
            lblTotalStudents.Text = row["TotalStudents"].ToString();
            lblActiveStudents.Text = row["ActiveStudents"] == DBNull.Value ? "0" : row["ActiveStudents"].ToString();
            lblInactiveStudents.Text = row["InactiveStudents"] == DBNull.Value ? "0" : row["InactiveStudents"].ToString();
            lblPendingApplications.Text = row["PendingApplications"] == DBNull.Value ? "0" : row["PendingApplications"].ToString();
            lblApprovedApplications.Text = row["ApprovedApplications"] == DBNull.Value ? "0" : row["ApprovedApplications"].ToString();
            lblRejectedApplications.Text = row["RejectedApplications"] == DBNull.Value ? "0" : row["RejectedApplications"].ToString();
        }
        catch (SqlException)
        {
            SetStatsFallback();
        }
        catch (Exception)
        {
            SetStatsFallback();
        }
    }

    private void SetStatsFallback()
    {
        // Never let a DB hiccup crash the dashboard -- just show a clear placeholder.
        lblTotalStudents.Text = "-";
        lblActiveStudents.Text = "-";
        lblInactiveStudents.Text = "-";
        lblPendingApplications.Text = "-";
        lblApprovedApplications.Text = "-";
        lblRejectedApplications.Text = "-";
    }

    protected void btnRefreshStats_Click(object sender, EventArgs e)
    {
        LoadStats();
    }

    protected void btnManageCandidates_Click(object sender, EventArgs e)
    {
        Response.Redirect("ManageCandidates.aspx");
    }

    protected void btnLogout_Click(object sender, EventArgs e)
    {
        Session.Clear();
        Session.Abandon();
        Response.Redirect("AdminLogin.aspx");
    }
    protected void btnRichTextEditor_Click(object sender, EventArgs e)
    {
        Response.Redirect("RichTextEditorList.aspx");
    }
}
