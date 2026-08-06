<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Login" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Student Login</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" href="Styles/Auth.css" />
    <link rel="icon" type="image/png" href="Images/favicon.png" />
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
    </style>
</head>
<body class="auth-body">
    <form id="form1" runat="server">

        <div class="auth-wrapper">
            <div class="auth-card">
                <div class="auth-header">
                    <div class="auth-icon">New Institute</div>
                    <h1>Student Login</h1>
                    <p>Sign in with your registered email and mobile number.</p>
                </div>

                <div class="auth-body-form">
                    <div class="form-group">
                        <label>Email Address<span class="required">*</span></label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="student@example.com"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Email is required." ValidationGroup="LoginGroup" />
                    </div>

                    <div class="form-group">
                        <label>Password (10-digit Mobile Number)<span class="required">*</span></label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter your 10-digit mobile number" MaxLength="10"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Password is required." ValidationGroup="LoginGroup" />
                    </div>

                    <div class="form-group captcha-group">
                        <label>Captcha Verification<span class="required">*</span></label>
                        <div class="captcha-row">
                            <asp:Image ID="imgCaptcha" runat="server" ImageUrl="~/CaptchaHandler.ashx" AlternateText="CAPTCHA" CssClass="captcha-img" />
                            <asp:LinkButton ID="lnkRefreshCaptcha" runat="server" CssClass="captcha-refresh"
                                OnClick="lnkRefreshCaptcha_Click" CausesValidation="false" ToolTip="Refresh CAPTCHA">&#8635;</asp:LinkButton>
                        </div>
                        <asp:TextBox ID="txtCaptcha" runat="server" CssClass="form-control" placeholder="Enter the code shown above"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCaptcha"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Please enter the CAPTCHA code." ValidationGroup="LoginGroup" />
                    </div>

                    <asp:Label ID="lblLoginStatus" runat="server" CssClass="status-message" Visible="false"></asp:Label>

                    <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-success auth-submit-btn"
                        OnClick="btnLogin_Click" ValidationGroup="LoginGroup" />
                </div>

                <div class="auth-footer">
                    <p>New student? <a href="Register.aspx">Register here &rarr;</a></p>
                    <p class="no-print"><a href="Landing.aspx">&larr; Back</a></p>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
