using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

public partial class RichTextEditorPreview : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null)
        {
            Response.Redirect("AdminLogin.aspx");
            return;
        }

        int id;
        if (!int.TryParse(Request.QueryString["id"], out id))
        {
            lblTitle.Text = "No document specified";
            return;
        }

        try
        {
            DataTable dt = DBHelper.ExecuteQuery(
                "SELECT Title, Content, CreatedDate, ModifiedDate, CreatedBy FROM RichTextDocuments WHERE DocumentID = @Id",
                new SqlParameter("@Id", id));

            if (dt.Rows.Count == 0)
            {
                lblTitle.Text = "Document not found";
                return;
            }

            DataRow row = dt.Rows[0];
            lblTitle.Text = System.Web.HttpUtility.HtmlEncode(row["Title"].ToString());

            string meta = "Created by " + System.Web.HttpUtility.HtmlEncode(row["CreatedBy"].ToString()) +
                " on " + Convert.ToDateTime(row["CreatedDate"]).ToString("dd-MMM-yyyy hh:mm tt");

            if (row["ModifiedDate"] != DBNull.Value)
            {
                meta += " &middot; Last modified " + Convert.ToDateTime(row["ModifiedDate"]).ToString("dd-MMM-yyyy hh:mm tt");
            }
            lblMeta.Text = meta;

            // Content was already run through HtmlContentSanitizer at Save time
            // (see RichTextEditorEdit.aspx.cs), so it's safe to render as-is here.
            litContent.Text = row["Content"].ToString();
        }
        catch (Exception ex)
        {
            ErrorLogger.Log("RichTextEditorPreview.Load", ex);
            lblTitle.Text = "Unable to load this document";
            litContent.Text = "<p>Please try again in a moment.</p>";
        }
    }
}
