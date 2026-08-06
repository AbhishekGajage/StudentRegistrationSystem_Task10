using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class RichTextEditorList : Page
{
    // ORDER BY can't be parameterized like a normal SQL value, so the sort
    // column has to be validated against a fixed whitelist instead -- never
    // concatenate GridView's SortExpression into SQL without this check.
    private static readonly HashSet<string> AllowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Title", "CreatedDate", "ModifiedDate"
    };

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null)
        {
            Response.Redirect("AdminLogin.aspx");
            return;
        }

        if (!IsPostBack)
        {
            ViewState["SortExpr"] = "CreatedDate";
            ViewState["SortDir"] = "DESC";
            BindGrid();
        }
    }

    private string GetSafeSortColumn(string expression)
    {
        return AllowedSortColumns.Contains(expression) ? expression : "CreatedDate";
    }

    private void BindGrid()
    {
        try
        {
            string search = txtSearch.Text.Trim();
            string sortColumn = GetSafeSortColumn(ViewState["SortExpr"] as string ?? "CreatedDate");
            string sortDirection = (ViewState["SortDir"] as string) == "ASC" ? "ASC" : "DESC";

            StringBuilder sql = new StringBuilder(
                "SELECT DocumentID, Title, CreatedDate, ModifiedDate, CreatedBy, Status FROM RichTextDocuments WHERE 1 = 1 ");
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(search))
            {
                sql.Append(" AND Title LIKE @Search ");
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }

            sql.Append(" ORDER BY ").Append(sortColumn).Append(" ").Append(sortDirection);

            DataTable dt = DBHelper.ExecuteQuery(sql.ToString(), parameters.ToArray());
            gvDocuments.DataSource = dt;
            gvDocuments.DataBind();
            lblCount.Text = dt.Rows.Count + " document(s) found";
            UpdateSortStatusMessage();
        }
        catch (Exception ex)
        {
            ErrorLogger.Log("RichTextEditorList.BindGrid", ex);
            ShowStatus("Unable to load documents right now. Please try again.", false);
        }
    }

    // Shows a plain-language banner ("Sorted by Document Title in ascending
    // order.") above the grid, matching the pattern in StudentList.aspx.cs, so
    // users don't have to infer the active sort from a tiny header arrow alone.
    private void UpdateSortStatusMessage()
    {
        string sortColumn = GetSafeSortColumn(ViewState["SortExpr"] as string ?? "CreatedDate");
        string sortDirection = (ViewState["SortDir"] as string) == "ASC" ? "ASC" : "DESC";

        string columnLabel;
        switch (sortColumn)
        {
            case "Title": columnLabel = "Document Title"; break;
            case "ModifiedDate": columnLabel = "Last Modified"; break;
            case "CreatedDate": columnLabel = "Created Date"; break;
            default: columnLabel = sortColumn; break;
        }

        string directionLabel = (sortDirection == "ASC") ? "ascending" : "descending";

        lblSortStatus.Text = "Sorted by <strong>" + columnLabel + "</strong> in " + directionLabel + " order.";
        lblSortStatus.Visible = true;
    }

    // Appends a small ▲/▼ next to whichever sortable header is currently active,
    // so the direction is visible at a glance without reading the banner text.
    // Column index matches declaration order in the markup: 0=Sr. No.,
    // 1=Document Title, 2=Created Date, 3=Last Modified, 4=Actions.
    protected void gvDocuments_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.Header) return;

        string sortColumn = GetSafeSortColumn(ViewState["SortExpr"] as string ?? "CreatedDate");
        string sortDirection = (ViewState["SortDir"] as string) == "ASC" ? "ASC" : "DESC";

        AddSortArrow(e.Row.Cells[1], "Title", sortColumn, sortDirection);
        AddSortArrow(e.Row.Cells[2], "CreatedDate", sortColumn, sortDirection);
        AddSortArrow(e.Row.Cells[3], "ModifiedDate", sortColumn, sortDirection);
    }

    private void AddSortArrow(TableCell cell, string columnSortExpr, string currentExpr, string currentDir)
    {
        if (!string.Equals(columnSortExpr, currentExpr, StringComparison.OrdinalIgnoreCase)) return;

        string arrow = (currentDir == "ASC") ? "▲" : "▼";
        cell.Controls.Add(new LiteralControl("<span class='sort-arrow'>" + arrow + "</span>"));
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        gvDocuments.PageIndex = 0;
        BindGrid();
    }

    protected void btnResetSearch_Click(object sender, EventArgs e)
    {
        txtSearch.Text = "";
        gvDocuments.PageIndex = 0;
        ViewState["SortExpr"] = "CreatedDate";
        ViewState["SortDir"] = "DESC";
        BindGrid();
    }

    protected void gvDocuments_Sorting(object sender, GridViewSortEventArgs e)
    {
        string newSort = GetSafeSortColumn(e.SortExpression);
        string currentSort = ViewState["SortExpr"] as string;
        string currentDir = ViewState["SortDir"] as string ?? "DESC";

        // Clicking the same column again flips direction; a new column starts ascending.
        ViewState["SortDir"] = (newSort == currentSort && currentDir == "ASC") ? "DESC" : "ASC";
        ViewState["SortExpr"] = newSort;

        BindGrid();
    }

    protected void gvDocuments_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvDocuments.PageIndex = e.NewPageIndex;
        BindGrid();
    }

    protected void gvDocuments_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int id;
        if (!int.TryParse(e.CommandArgument.ToString(), out id))
        {
            return;
        }

        switch (e.CommandName)
        {
            case "ViewDoc":
                Response.Redirect("RichTextEditorPreview.aspx?id=" + id);
                break;
            case "EditDoc":
                Response.Redirect("RichTextEditorEdit.aspx?id=" + id);
                break;
            case "DeleteDoc":
                DeleteDocument(id);
                BindGrid();
                break;
        }
    }

    private void DeleteDocument(int id)
    {
        try
        {
            int rowsAffected = DBHelper.ExecuteNonQuery(
                "DELETE FROM RichTextDocuments WHERE DocumentID = @Id",
                new SqlParameter("@Id", id));

            ShowStatus(rowsAffected > 0 ? "Document deleted successfully." : "That document no longer exists.", rowsAffected > 0);
        }
        catch (Exception ex)
        {
            ErrorLogger.Log("RichTextEditorList.DeleteDocument", ex);
            ShowStatus("Unable to delete the document. Please try again.", false);
        }
    }

    private void ShowStatus(string message, bool success)
    {
        lblStatus.Text = (success ? "✅ " : "⚠️ ") + message;
        lblStatus.CssClass = "status-message " + (success ? "status-success" : "status-error");
        lblStatus.Visible = true;
    }
}
