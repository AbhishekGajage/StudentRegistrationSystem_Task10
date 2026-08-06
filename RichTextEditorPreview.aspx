<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RichTextEditorPreview.aspx.cs" ValidateRequest="false" Inherits="RichTextEditorPreview" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Document Preview</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" href="Styles/Auth.css" />
    <link rel="icon" type="image/png" href="Images/favicon.png" />
    <style>
        .doc-preview-body { line-height: 1.6; }
        .doc-preview-body img { max-width: 100%; height: auto; }
        .doc-preview-body table { border-collapse: collapse; max-width: 100%; margin: 12px 0; }
        .doc-preview-body table td, .doc-preview-body table th { border: 1px solid #ccc; padding: 6px 10px; }
        .doc-preview-meta { color: #90a4ae; font-size: 13px; margin: -6px 0 22px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page-wrapper">
            <div class="top-bar dashboard-top-bar">
                <div>
                    <h1>Document Preview</h1>
                </div>
                <a href="RichTextEditorList.aspx" class="btn btn-outline logout-btn no-print">&larr; All Documents</a>
            </div>

            <div class="card">
                <h2><asp:Literal ID="lblTitle" runat="server" /></h2>
                <p class="doc-preview-meta"><asp:Literal ID="lblMeta" runat="server" /></p>
                <div class="doc-preview-body">
                    <asp:Literal ID="litContent" runat="server" Mode="PassThrough" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>
