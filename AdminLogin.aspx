<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminLogin.aspx.cs" Inherits="AdminLogin" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Admin Login</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" href="Styles/Auth.css" />
    <style>
        .auth-icon {
            display: inline-block;
            padding: 6px 16px;
            border-radius: 999px;
            background: #eef2ff;
            color: #4338ca;
            font-size: 11.5px;
            font-weight: 700;
            letter-spacing: .05em;
            text-transform: uppercase;
            margin-bottom: 14px;
        }

        /* ---- Footer ---- */
        .auth-footer { text-align: center; }
        .auth-footer p { margin: 6px 0; font-size: 13.5px; color: #6b7280; }
        .auth-footer a { color: #4338ca; font-weight: 600; text-decoration: none; }
        .auth-footer a:hover { text-decoration: underline; }
    </style>
    <link rel="icon" type="image/png" href="Images/favicon.png" />
</head>
<body class="auth-body">
    <div class="auth-wrapper">
        <div class="auth-card">
            <div class="auth-header">
                <div class="auth-icon">New Institute</div>
                <h1>Admin Login</h1>
                <p>Student Registration System &mdash; Administration.</p>
            </div>

            <div class="auth-body-form">

                <% if (!string.IsNullOrEmpty(ErrorMessage)) { %>
                    <div class="status-message status-error"><%= System.Web.HttpUtility.HtmlEncode(ErrorMessage) %></div>
                <% } %>

                <%--
                    Deliberately a plain HTML form, not an ASP.NET server form.
                    A Web Forms server form always posts back via HTTP POST (it needs POST for
                    ViewState), so there is no way to satisfy "authenticate via HTTP GET" from
                    inside that model. This form submits as a genuine browser GET request, so
                    Username/Password arrive in the query string and Page_Load authenticates
                    directly from Request.QueryString -- no postback event involved.

                    Trade-off worth knowing: GET means the credentials appear in the URL, browser
                    history, and server logs. That's inherent to doing HTTP GET auth and conflicts
                    a bit with "secure coding practices" (#7) -- flagging it, but implementing as
                    specified since GET-based auth was an explicit requirement.
                --%>
                <form method="get" action="AdminLogin.aspx" autocomplete="off">
                    <input type="hidden" name="action" value="login" />

                    <div class="form-group">
                        <label>Username<span class="required">*</span></label>
                        <input type="text" name="username" class="form-control"
                            placeholder="Enter your admin username"
                            value="<%= System.Web.HttpUtility.HtmlEncode(PostedUsername) %>" />
                        <% if (UsernameError) { %>
                            <span class="field-error">Username is required.</span>
                        <% } %>
                    </div>

                    <div class="form-group">
                        <label>Password<span class="required">*</span></label>
                        <input type="password" name="password" class="form-control" placeholder="Enter your password" />
                        <% if (PasswordError) { %>
                            <span class="field-error">Password is required.</span>
                        <% } %>
                    </div>

                    <button type="submit" class="btn btn-primary auth-submit-btn">Login</button>
                </form>
            </div>

            <div class="auth-footer">
                <p><a href="Landing.aspx">&larr; Back</a></p>
            </div>
        </div>
    </div>
</body>
</html>
