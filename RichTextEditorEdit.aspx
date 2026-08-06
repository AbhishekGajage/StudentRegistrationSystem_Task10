<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RichTextEditorEdit.aspx.cs" ValidateRequest="false" Inherits="RichTextEditorEdit" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Rich Text Editor</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" href="Styles/Auth.css" />
    <link rel="icon" type="image/png" href="Images/favicon.png" />
    <!--
        Self-hosted TinyMCE Community build -- download the "Self Hosted" zip from
        tiny.cloud/get-tiny/self-hosted/ and extract it to Scripts/tinymce/ so this
        path resolves to Scripts/tinymce/tinymce.min.js. Self-hosted avoids the
        cloud API key requirement and matches how intlTelInput is already vendored
        locally elsewhere in this project.
    -->
    <script src="Scripts/tinymce/tinymce.min.js"></script>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <style>
        .preview-modal-overlay {
            display: none;
            position: fixed;
            inset: 0;
            background: rgba(15, 23, 42, 0.55);
            z-index: 1000;
            align-items: center;
            justify-content: center;
            padding: 24px;
        }
        .preview-modal-overlay.open { display: flex; }
        .preview-modal-box {
            background: #fff;
            border-radius: 10px;
            max-width: 900px;
            width: 100%;
            max-height: 85vh;
            display: flex;
            flex-direction: column;
            overflow: hidden;
        }
        .preview-modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 16px 22px;
            border-bottom: 1px solid #eceff1;
        }
        .preview-modal-header h3 { margin: 0; font-size: 16px; }
        .preview-modal-close { background: none; border: none; font-size: 20px; cursor: pointer; color: #666; }
        .preview-modal-body { padding: 22px; overflow-y: auto; }
        .preview-modal-body img { max-width: 100%; height: auto; }
        .preview-modal-body table { border-collapse: collapse; max-width: 100%; }
        .preview-modal-body table td, .preview-modal-body table th { border: 1px solid #ccc; padding: 6px 10px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page-wrapper">
            <div class="top-bar dashboard-top-bar">
                <div>
                    <h1><asp:Label ID="lblPageTitle" runat="server" Text="Create New Document" /></h1>
                    <p>Documents support full formatting, images, tables, and links.</p>
                </div>
                <a href="RichTextEditorList.aspx" class="btn btn-outline logout-btn no-print">&larr; All Documents</a>
            </div>

            <div class="status-message-wrapper">
                <asp:Label ID="lblStatus" runat="server" CssClass="status-message" Visible="false"></asp:Label>
            </div>

            <div class="card">
                <div class="form-group">
                    <label>Document Title<span class="required">*</span></label>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" placeholder="e.g. Admission Policy 2026" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTitle"
                        CssClass="field-error" Display="Dynamic" ErrorMessage="Document title is required." ValidationGroup="DocGroup" />
                </div>

                <div class="form-group" style="margin-top:18px;">
                    <label>Content<span class="required">*</span></label>
                    <asp:TextBox ID="txtContent" runat="server" TextMode="MultiLine" CssClass="tinymce-editor" />
                    <asp:CustomValidator ID="cvContent" runat="server" CssClass="field-error" Display="Dynamic"
                        ErrorMessage="Document content is required." OnServerValidate="cvContent_ServerValidate"
                        ValidationGroup="DocGroup" ClientValidationFunction="validateContentNotEmpty" />
                </div>

                <div class="btn-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-success"
                        ValidationGroup="DocGroup" OnClick="btnSave_Click" OnClientClick="tinymce.triggerSave();" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-outline"
                        CausesValidation="false" OnClick="btnReset_Click" />
                    <button type="button" id="btnPreview" class="btn btn-secondary">Preview</button>
                </div>
            </div>
        </div>
    </form>

    <!-- Client-side "Preview Before Saving": no server round-trip needed since
         unsaved content doesn't exist in the DB yet. Final sanitization still
         happens server-side on Save -- this is purely a WYSIWYG convenience. -->
    <div class="preview-modal-overlay" id="previewOverlay">
        <div class="preview-modal-box">
            <div class="preview-modal-header">
                <h3>Preview</h3>
                <button type="button" class="preview-modal-close" id="btnClosePreview">&times;</button>
            </div>
            <div class="preview-modal-body" id="previewBody"></div>
        </div>
    </div>

    <script>
tinymce.init({
    selector: '.tinymce-editor',
            license_key: 'gpl',
            height: 480,
            menubar: 'edit view insert format table tools',
            plugins: 'advlist autolink lists link image charmap preview anchor ' +
                     'searchreplace visualblocks code fullscreen insertdatetime ' +
                     'table help wordcount',
            toolbar:
                'undo redo | blocks fontfamily fontsize | ' +
                'bold italic underline strikethrough superscript subscript removeformat | ' +
                'forecolor backcolor | ' +
                'alignleft aligncenter alignright alignjustify | ' +
                'bullist numlist outdent indent | lineheight | ' +
                'link image table charmap hr | ' +
                'searchreplace | fullscreen preview | help',
            branding: false,

            // Image uploads (toolbar button + drag/drop/paste) POST to our handler,
            // which validates type/size server-side and returns { location: url }.
            images_upload_url: 'RichTextImageUpload.ashx',
            automatic_uploads: true,
            file_picker_types: 'image',

            // Editor-side allowlist -- a UX guardrail only. The server-side
            // HtmlContentSanitizer on Save is the actual security boundary.
            valid_elements:
                'p,br,span[style],div,strong,b,em,i,u,s,strike,sub,sup,' +
                'ul,ol,li,a[href|title|target|rel],img[src|alt|title|width|height|style],' +
                'table[style],thead,tbody,tr,td[colspan|rowspan|style],th[colspan|rowspan|style],' +
                'hr,h1,h2,h3,h4,h5,h6,blockquote,pre,code'
        });

        // Keep the required-field validator's server call in sync with what
        // TinyMCE actually contains (an editor with only "<p><br></p>" should
        // still count as empty).
        function validateContentNotEmpty(source, args) {
            var editor = tinymce.get('<%= txtContent.ClientID %>');
            var text = editor ? editor.getContent({ format: 'text' }).trim() : '';
            args.IsValid = text.length > 0;
        }

        $(function () {
            $('#btnPreview').on('click', function () {
                var editor = tinymce.get('<%= txtContent.ClientID %>');
                $('#previewBody').html(editor ? editor.getContent() : '');
                $('#previewOverlay').addClass('open');
            });
            $('#btnClosePreview, #previewOverlay').on('click', function (e) {
                if (e.target.id === 'btnClosePreview' || e.target.id === 'previewOverlay') {
                    $('#previewOverlay').removeClass('open');
                }
            });
        });
    </script>
</body>
</html>
