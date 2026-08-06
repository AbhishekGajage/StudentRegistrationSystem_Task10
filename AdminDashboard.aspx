<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="AdminDashboard" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Admin Dashboard</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" href="Styles/Auth.css" />
    <style>
        .stat-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
            gap: 16px;
            margin: 4px 0 28px;
        }
        .stat-card {
            border-radius: 10px;
            padding: 18px 20px;
            color: #fff;
            box-shadow: 0 2px 6px rgba(0,0,0,0.12);
        }
        .stat-card .stat-value { font-size: 30px; font-weight: 700; line-height: 1.1; }
        .stat-card .stat-label { font-size: 13px; opacity: .92; margin-top: 6px; }
        .stat-total       { background: linear-gradient(135deg, #2b5fd9, #4a7bf0); }
        .stat-active      { background: linear-gradient(135deg, #16a34a, #22c55e); }
        .stat-inactive    { background: linear-gradient(135deg, #6b7280, #9ca3af); }
        .stat-pending     { background: linear-gradient(135deg, #d97706, #f59e0b); }
        .stat-approved    { background: linear-gradient(135deg, #0891b2, #06b6d4); }
        .stat-rejected    { background: linear-gradient(135deg, #dc2626, #ef4444); }
    </style>
    <link rel="icon" type="image/png" href="Images/favicon.png" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="page-wrapper">
            <div class="top-bar dashboard-top-bar">
                <div>
                    <h1>Admin Dashboard</h1>
                    <p>Welcome, <asp:Label ID="lblAdminName" runat="server"></asp:Label></p>
                </div>
                <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="btn btn-outline logout-btn no-print"
                    OnClick="btnLogout_Click" CausesValidation="false" />
            </div>

            <div class="card dashboard-card">

                <div class="stat-grid">
                    <div class="stat-card stat-total">
                        <div class="stat-value"><asp:Label ID="lblTotalStudents" runat="server">0</asp:Label></div>
                        <div class="stat-label">Total Registered Students</div>
                    </div>
                    <div class="stat-card stat-active">
                        <div class="stat-value"><asp:Label ID="lblActiveStudents" runat="server">0</asp:Label></div>
                        <div class="stat-label">Total Active Students</div>
                    </div>
                    <div class="stat-card stat-inactive">
                        <div class="stat-value"><asp:Label ID="lblInactiveStudents" runat="server">0</asp:Label></div>
                        <div class="stat-label">Total Inactive Students</div>
                    </div>
                    <div class="stat-card stat-pending">
                        <div class="stat-value"><asp:Label ID="lblPendingApplications" runat="server">0</asp:Label></div>
                        <div class="stat-label">Pending Approval Applications</div>
                    </div>
                    <div class="stat-card stat-approved">
                        <div class="stat-value"><asp:Label ID="lblApprovedApplications" runat="server">0</asp:Label></div>
                        <div class="stat-label">Approved Applications</div>
                    </div>
                    <div class="stat-card stat-rejected">
                        <div class="stat-value"><asp:Label ID="lblRejectedApplications" runat="server">0</asp:Label></div>
                        <div class="stat-label">Rejected Applications</div>
                    </div>
                </div>

                <h2>Quick Actions</h2>
                <div class="full-width btn-row">
                    <asp:Button ID="btnManageCandidates" runat="server" Text="Manage Candidates"
                        CssClass="btn btn-primary" OnClick="btnManageCandidates_Click" CausesValidation="false" />
                    <asp:Button ID="btnRichTextEditor" runat="server" Text="Rich Text Editor"
                        CssClass="btn btn-primary" OnClick="btnRichTextEditor_Click" CausesValidation="false" />
                    <asp:Button ID="btnRefreshStats" runat="server" Text="Refresh Stats"
                        CssClass="btn btn-outline" OnClick="btnRefreshStats_Click" CausesValidation="false" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>
