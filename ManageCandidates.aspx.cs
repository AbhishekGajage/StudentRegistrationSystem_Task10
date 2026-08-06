using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class ManageCandidates : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null)
        {
            Response.Redirect("AdminLogin.aspx");
            return;
        }

        if (!IsPostBack)
        {
            BindGrid();
        }
    }

    private DataTable GetFilteredCandidates()
    {
        string search = txtSearch.Text.Trim();
        string approvalFilter = ddlApprovalFilter.SelectedValue;
        string accountFilter = ddlAccountFilter.SelectedValue;

        StringBuilder sql = new StringBuilder(
            @"SELECT StudentID, FullName, Email, MobileNumber, ApprovalStatus, AccountStatus,
                     RejectionRemark, RegistrationDate
              FROM Students
              WHERE 1 = 1 ");

        var parameters = new System.Collections.Generic.List<SqlParameter>();

        if (!string.IsNullOrEmpty(search))
        {
            sql.Append(" AND (FullName LIKE @Search OR Email LIKE @Search) ");
            parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
        }
        if (!string.IsNullOrEmpty(approvalFilter))
        {
            sql.Append(" AND ApprovalStatus = @ApprovalStatus ");
            parameters.Add(new SqlParameter("@ApprovalStatus", approvalFilter));
        }
        if (!string.IsNullOrEmpty(accountFilter))
        {
            sql.Append(" AND AccountStatus = @AccountStatus ");
            parameters.Add(new SqlParameter("@AccountStatus", accountFilter));
        }

        sql.Append(" ORDER BY RegistrationDate DESC ");

        return DBHelper.ExecuteQuery(sql.ToString(), parameters.ToArray());
    }

    private void BindGrid()
    {
        DataTable dt = GetFilteredCandidates();
        gvCandidates.DataSource = dt;
        gvCandidates.DataBind();
        lblCount.Text = dt.Rows.Count + " candidate(s) found";
    }

    protected void btnFilter_Click(object sender, EventArgs e)
    {
        BindGrid();
    }

    protected void btnResetFilters_Click(object sender, EventArgs e)
    {
        txtSearch.Text = "";
        ddlApprovalFilter.SelectedIndex = 0;
        ddlAccountFilter.SelectedIndex = 0;
        BindGrid();
    }

    private void ShowStatus(string message, bool success)
    {
        lblActionStatus.Text = (success ? "✅ " : "⚠️ ") + message;
        lblActionStatus.CssClass = "status-message " + (success ? "status-success" : "status-error");
        lblActionStatus.Visible = true;
    }

    protected void gvCandidates_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string studentId = e.CommandArgument.ToString();
        string adminUsername = Session["AdminUsername"] as string ?? "Admin";

        switch (e.CommandName)
        {
            case "Approve":
                ApproveCandidate(studentId, adminUsername);
                break;
            case "Reject":
                RejectCandidate(studentId, adminUsername);
                break;
            case "Reset":
                ResetCandidate(studentId);
                break;
            case "Activate":
                SetAccountStatus(studentId, "Active");
                break;
            case "Deactivate":
                SetAccountStatus(studentId, "Inactive");
                break;
        }

        BindGrid();
    }

    private void ApproveCandidate(string studentId, string adminUsername)
    {
        DBHelper.ExecuteNonQuery(
            @"UPDATE Students
              SET ApprovalStatus = 'Approved',
                  ApprovedBy = @ApprovedBy,
                  ApprovedDate = @Now,
                  RejectionRemark = NULL,
                  RejectedBy = NULL,
                  RejectedDate = NULL,
                  LastModifiedDate = @Now
              WHERE StudentID = @StudentID",
            new SqlParameter("@ApprovedBy", adminUsername),
            new SqlParameter("@Now", DateTime.Now),
            new SqlParameter("@StudentID", studentId));

        TrySendApprovalEmail(studentId);

        ShowStatus("Application " + studentId + " approved and student notified by email.", true);
    }

    private void RejectCandidate(string studentId, string adminUsername)
    {
        // Mandatory remark, captured client-side via hidden fields. Re-validate
        // server-side so this can't be bypassed if JS is disabled/tampered with.
        string remark = hdnRejectRemark.Value.Trim();
        string remarkStudentId = hdnRejectStudentId.Value.Trim();

        if (string.IsNullOrEmpty(remark) || remarkStudentId != studentId)
        {
            ShowStatus("Rejection remark is required. Please try again.", false);
            return;
        }

        DBHelper.ExecuteNonQuery(
            @"UPDATE Students
              SET ApprovalStatus = 'Rejected',
                  RejectionRemark = @Remark,
                  RejectedBy = @RejectedBy,
                  RejectedDate = @Now,
                  ApprovedBy = NULL,
                  ApprovedDate = NULL,
                  LastModifiedDate = @Now
              WHERE StudentID = @StudentID",
            new SqlParameter("@Remark", remark),
            new SqlParameter("@RejectedBy", adminUsername),
            new SqlParameter("@Now", DateTime.Now),
            new SqlParameter("@StudentID", studentId));

        TrySendRejectionEmail(studentId, remark);

        hdnRejectRemark.Value = "";
        hdnRejectStudentId.Value = "";

        ShowStatus("Application " + studentId + " rejected and student notified by email.", true);
    }

    private void ResetCandidate(string studentId)
    {
        DBHelper.ExecuteNonQuery(
            @"UPDATE Students
              SET ApprovalStatus = 'Pending',
                  RejectionRemark = NULL,
                  RejectedBy = NULL,
                  RejectedDate = NULL,
                  LastModifiedDate = @Now
              WHERE StudentID = @StudentID",
            new SqlParameter("@Now", DateTime.Now),
            new SqlParameter("@StudentID", studentId));

        ShowStatus("Application " + studentId + " reset to Pending for re-review.", true);
    }

    private void SetAccountStatus(string studentId, string status)
    {
        DBHelper.ExecuteNonQuery(
            @"UPDATE Students
              SET AccountStatus = @Status,
                  LastModifiedDate = @Now
              WHERE StudentID = @StudentID",
            new SqlParameter("@Status", status),
            new SqlParameter("@Now", DateTime.Now),
            new SqlParameter("@StudentID", studentId));

        ShowStatus("Student " + studentId + " marked as " + status + ".", true);
    }

    private string GetStudentEmail(string studentId)
    {
        object result = DBHelper.ExecuteScalar(
            "SELECT Email FROM Students WHERE StudentID = @StudentID",
            new SqlParameter("@StudentID", studentId));
        return result?.ToString();
    }

    private string GetStudentName(string studentId)
    {
        object result = DBHelper.ExecuteScalar(
            "SELECT FullName FROM Students WHERE StudentID = @StudentID",
            new SqlParameter("@StudentID", studentId));
        return result?.ToString() ?? "Student";
    }

    // Email delivery failure should never block an approval/rejection from
    // being recorded -- the DB update already succeeded by the time these run.
    private void TrySendApprovalEmail(string studentId)
    {
        string email = GetStudentEmail(studentId);
        if (string.IsNullOrEmpty(email)) return;

        try
        {
            EmailHelper.SendApprovalEmail(email, GetStudentName(studentId), studentId);
        }
        catch
        {
            // Swallow: the approval itself already succeeded in the DB.
        }
    }

    private void TrySendRejectionEmail(string studentId, string remark)
    {
        string email = GetStudentEmail(studentId);
        if (string.IsNullOrEmpty(email)) return;

        try
        {
            EmailHelper.SendRejectionEmail(email, GetStudentName(studentId), studentId, remark);
        }
        catch
        {
            // Swallow: the rejection itself already succeeded in the DB.
        }
    }
}
