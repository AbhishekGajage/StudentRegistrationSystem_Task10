# Student Registration System

An ASP.NET Web Forms (C#, .NET Framework 4.8) application for managing student
registrations, admin approval workflows, and rich-text document publishing.

## Features

- **Student Registration** — OTP-verified sign-up (email OTP via SendGrid),
  profile photo upload, country/state/district selection.
- **Admin Authentication** — session-based admin login guarding all
  back-office pages.
- **Manage Candidates** — admin review queue to approve, reject (with a
  mandatory remark), reset, activate, or deactivate student accounts.
- **Student List** — searchable/sortable grid of registered students, with
  print and Excel export support.
- **Rich Text Editor** — admin-authored documents (e.g. policies, notices)
  with a full WYSIWYG editor, inline image uploads, and a searchable/sortable
  document list.
- **HTML Sanitization** — all rich-text content is sanitized server-side
  before storage to prevent stored XSS.

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Web Forms, .NET Framework 4.8 |
| Language | C# |
| Database | SQL Server (LocalDB by default) |
| Rich Text Editor | [TinyMCE](https://www.tiny.cloud/) 8.x (self-hosted, GPL) |
| HTML Sanitization | HtmlSanitizer (AngleSharp-based) |
| Email | SendGrid API |

## Setup

### 1. Database

1. Open **SQL Server Object Explorer** (or SSMS) and connect to the instance
   referenced in `Web.config`'s `StudentDB` connection string (defaults to
   `(localdb)\MSSQLLocalDB`).
2. Run every `.sql` file under `Database/`, including
   `RichTextDocuments_Schema.sql`. Adding a `.sql` file to the project does
   **not** run it automatically — it must be executed manually.

### 2. TinyMCE (self-hosted)

This project uses the free, self-hosted (GPL) build of TinyMCE — no API key
or account required.

1. Download the self-hosted build from
   [tiny.cloud/get-tiny/self-hosted](https://www.tiny.cloud/get-tiny/self-hosted/).
2. Extract it and copy the contents of `tinymce/js/tinymce/` (the
   `tinymce.min.js` file plus the `icons/`, `models/`, `plugins/`, `skins/`,
   and `themes/` folders) into `Scripts/tinymce/` in this project, so that
   `Scripts/tinymce/tinymce.min.js` exists.
3. In Visual Studio, enable **Show All Files** in Solution Explorer, then
   right-click the new `tinymce` folder → **Include In Project**.
4. TinyMCE 6+ requires an explicit license declaration even for free/GPL
   usage. This is already set in `RichTextEditorEdit.aspx`:
   ```javascript
   tinymce.init({
       license_key: 'gpl',
       // ...
   });
   ```

### 3. Configuration (`Web.config`)

Update the following before running:

```xml
<connectionStrings>
  <add name="StudentDB" connectionString="Server=...;Database=...;..." providerName="System.Data.SqlClient" />
</connectionStrings>

<appSettings>
  <add key="SendGridApiKey" value="YOUR_KEY_HERE" />
  <add key="FromEmail" value="verified-sender@yourdomain.com" />
  <add key="AdminEmail" value="admin@yourdomain.com" />
  <add key="ProfilePhotoUploadPath" value="~/Uploads/Students/" />
  <add key="DocumentImageUploadPath" value="~/Uploads/DocumentImages/" />
  <add key="MaxDocumentImageSizeMB" value="5" />
</appSettings>
```

> **Security note:** never commit real API keys to source control. Rotate any
> key that has been shared or exposed, and consider environment-specific
> config transforms or a secrets manager for production.

### 4. NuGet Packages / Binding Redirects

Installing `HtmlSanitizer` pulls in `AngleSharp` and several BCL polyfill
packages that commonly conflict with existing binding redirects. Ensure
`Web.config`'s `<runtime><assemblyBinding>` includes redirects for:

- `AngleSharp`
- `System.Buffers`
- `System.Memory`
- `System.Numerics.Vectors`
- `System.Runtime.CompilerServices.Unsafe`

Match `newVersion` to whatever is actually referenced in the project (check
**References → [assembly] → Properties → Version** in Visual Studio) rather
than copying version numbers blindly from another machine.

### 5. Build & Run

1. `Build → Rebuild Solution`.
2. If any `.aspx` page throws `'controlName' does not exist in the current
   context`, its `.designer.cs` partner file is missing or stale — open the
   page in **Design view** once (or regenerate the designer file manually)
   and rebuild.
3. Press F5 / run under IIS Express.

## Security Notes

- **Request validation** is disabled (`ValidateRequest="false"`) on pages
  that submit rich-text HTML (`RichTextEditorEdit.aspx`), since ASP.NET's
  default validator blocks any `<`/`>` in form fields. This is safe **only**
  because `HtmlContentSanitizer` strips dangerous markup (`<script>`, event
  handler attributes, `javascript:` URLs, etc.) server-side before the
  content is stored or rendered — this sanitizer is the real security
  boundary, not TinyMCE's client-side filtering.
- **Image uploads** (`RichTextImageUpload.ashx`) require an authenticated
  admin session and validate file extension and size server-side.
- **Rejection remarks and other free-text admin actions** are parameterized
  via `SqlParameter` throughout — never string-concatenated into SQL.
- **Sortable grid columns** are validated against a fixed whitelist before
  being placed into an `ORDER BY` clause, since `ORDER BY` cannot be
  parameterized like a normal value.

## Error Logging

Unhandled exceptions in helper/service code are appended to
`App_Data/AppErrorLog.txt` via `ErrorLogger.Log(context, exception)` —
tab-separated: timestamp, calling context, exception type, and message.
Logging failures are swallowed so a broken logger never masks the original
error or crashes a request.
