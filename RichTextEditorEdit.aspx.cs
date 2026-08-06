using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class RichTextEditorEdit : Page
{
    private int? DocumentId
    {
        get
        {
            int id;
            return int.TryParse(Request.QueryString["id"], out id) ? (int?)id : null;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null)
        {
            Response.Redirect("AdminLogin.aspx");
            return;
        }

        if (!IsPostBack)
        {
            if (DocumentId.HasValue)
            {
                lblPageTitle.Text = "Edit Document";
                btnSave.Text = "Update";
                LoadDocument(DocumentId.Value);
            }
            else
            {
                lblPageTitle.Text = "Create New Document";
                btnSave.Text = "Save";
            }
        }
    }

    private void LoadDocument(int id)
    {
        try
        {
            DataTable dt = DBHelper.ExecuteQuery(
                "SELECT Title, Content FROM RichTextDocuments WHERE DocumentID = @Id",
                new SqlParameter("@Id", id));

            if (dt.Rows.Count == 0)
            {
                ShowStatus("That document no longer exists.", false);
                btnSave.Enabled = false;
                return;
            }

            txtTitle.Text = dt.Rows[0]["Title"].ToString();
            txtContent.Text = dt.Rows[0]["Content"].ToString();
        }
        catch (Exception ex)
        {
            ErrorLogger.Log("RichTextEditorEdit.LoadDocument", ex);
            ShowStatus("Unable to load this document right now. Please try again.", false);
            btnSave.Enabled = false;
        }
    }

    protected void cvContent_ServerValidate(object source, ServerValidateEventArgs args)
    {
        // Server-side mirror of the client check: strip tags and see if any
        // real text remains, so "<p><br></p>" doesn't count as content.
        string stripped = Regex.Replace(txtContent.Text ?? "", "<.*?>", string.Empty).Trim();
        args.IsValid = !string.IsNullOrWhiteSpace(stripped);
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        string title = txtTitle.Text.Trim();
        string sanitizedContent;

        try
        {
            sanitizedContent = HtmlContentSanitizer.Sanitize(txtContent.Text);
        }
        catch (Exception ex)
        {
            ErrorLogger.Log("RichTextEditorEdit.Sanitize", ex);
            ShowStatus("There was a problem processing the document content. Please try again.", false);
            return;
        }

        string adminUsername = Session["AdminUsername"] as string ?? "Admin";

        try
        {
            if (DocumentId.HasValue)
            {
                int rowsAffected = DBHelper.ExecuteNonQuery(
                    @"UPDATE RichTextDocuments
                      SET Title = @Title, Content = @Content, ModifiedDate = @Now
                      WHERE DocumentID = @Id",
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Content", sanitizedContent),
                    new SqlParameter("@Now", DateTime.Now),
                    new SqlParameter("@Id", DocumentId.Value));

                ShowStatus(rowsAffected > 0 ? "Document updated successfully." : "That document no longer exists.", rowsAffected > 0);
            }
            else
            {
                DBHelper.ExecuteNonQuery(
                    @"INSERT INTO RichTextDocuments (Title, Content, CreatedDate, CreatedBy, Status)
                      VALUES (@Title, @Content, @Now, @CreatedBy, 'Draft')",
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Content", sanitizedContent),
                    new SqlParameter("@Now", DateTime.Now),
                    new SqlParameter("@CreatedBy", adminUsername));

                ShowStatus("Document created successfully.", true);
                txtTitle.Text = "";
                txtContent.Text = "";
            }
        }
        catch (SqlException ex)
        {
            ErrorLogger.Log("RichTextEditorEdit.SaveDocument.DB", ex);
            ShowStatus("A database error occurred while saving. Please try again.", false);
        }
        catch (Exception ex)
        {
            ErrorLogger.Log("RichTextEditorEdit.SaveDocument.Unexpected", ex);
            ShowStatus("An unexpected error occurred while saving. Please try again.", false);
        }
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        // Simplest reliable reset: reload the page fresh rather than trying to
        // resync TinyMCE's client-side state by hand.
        Response.Redirect(DocumentId.HasValue
            ? "RichTextEditorEdit.aspx?id=" + DocumentId.Value
            : "RichTextEditorEdit.aspx");
    }

    private void ShowStatus(string message, bool success)
    {
        lblStatus.Text = (success ? "✅ " : "⚠️ ") + message;
        lblStatus.CssClass = "status-message " + (success ? "status-success" : "status-error");
        lblStatus.Visible = true;
    }
}
