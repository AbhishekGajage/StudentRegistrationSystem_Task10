<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageCandidates.aspx.cs" Inherits="ManageCandidates" EnableEventValidation="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Manage Candidates</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" href="Styles/Auth.css" />
    <style>
        .filter-bar { display: flex; flex-wrap: wrap; gap: 12px; align-items: flex-end; margin: 14px 0; }
        .filter-bar .field { display: flex; flex-direction: column; min-width: 160px; }
        .filter-bar label { font-size: 13px; font-weight: 600; margin-bottom: 4px; }

        .record-count { display: block; margin: 4px 0 16px; }

        .candidate-grid { width: 100%; table-layout: fixed; border-collapse: collapse; font-size: 12.5px; }
        .candidate-grid th, .candidate-grid td { word-wrap: break-word; vertical-align: middle; text-align: center; }
        .candidate-grid td { word-break: break-word; overflow-wrap: break-word; }

        .candidate-grid thead th {
            background: #f8fafc;
            color: #475569;
            font-size: 11px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: .04em;
            word-break: keep-all;
            overflow-wrap: normal;
            padding: 10px 8px;
            border-bottom: 2px solid #e2e8f0;
        }
        .candidate-grid tbody td {
            padding: 12px 8px;
            border-bottom: 1px solid #eef1f5;
        }
        .candidate-grid tbody tr:last-child td { border-bottom: none; }
        .candidate-grid tbody tr:nth-child(even) { background: #fafbfc; }
        .candidate-grid tbody tr:hover { background: #f1f5f9; }

        .candidate-grid th:nth-child(1), .candidate-grid td:nth-child(1) { width: 8%;  }  /* Student ID */
        .candidate-grid th:nth-child(2), .candidate-grid td:nth-child(2) { width: 16%; }  /* Name */
        .candidate-grid th:nth-child(3), .candidate-grid td:nth-child(3) { width: 22%; }  /* Email */
        .candidate-grid th:nth-child(4), .candidate-grid td:nth-child(4) { width: 13%; }  /* Mobile */
        .candidate-grid th:nth-child(5), .candidate-grid td:nth-child(5) { width: 11%; }  /* Approval */
        .candidate-grid th:nth-child(6), .candidate-grid td:nth-child(6) { width: 8%;  }  /* Account */
        .candidate-grid th:nth-child(7), .candidate-grid td:nth-child(7) { width: 14%; }  /* Registered On */
        .candidate-grid th:nth-child(8), .candidate-grid td:nth-child(8) { width: 8%;  text-align: center; }  /* Actions */

        .status-pill {
            display: inline-block;
            padding: 3px 10px;
            border-radius: 999px;
            font-size: 11.5px;
            font-weight: 600;
            color: #fff;
            white-space: nowrap;
        }
        .pill-pending  { background: #f59e0b; }
        .pill-approved { background: #16a34a; }
        .pill-rejected { background: #dc2626; }
        .pill-active   { background: #22c55e; }
        .pill-inactive { background: #9ca3af; }

        .remark-cell {
            position: relative;
            margin-top: 6px;
            cursor: pointer;
        }
        .remark-text {
            font-size: 11.5px;
            color: #b91c1c;
            text-align: left;
            line-height: 1.35;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        /* Custom styled tooltip for the rejection remark, replacing the plain
           browser title tooltip. Shown on hover via the data-tooltip attribute.
           Lives on .remark-cell (no overflow:hidden here) so it isn't clipped
           by the line-clamp truncation on .remark-text. */
        .remark-cell::after {
            content: attr(data-tooltip);
            position: absolute;
            left: 50%;
            bottom: 100%;
            transform: translateX(-50%) translateY(-4px) scale(0.96);
            width: max-content;
            max-width: 240px;
            background: #1f2937;
            color: #f9fafb;
            font-size: 12px;
            font-weight: 500;
            line-height: 1.45;
            text-align: left;
            padding: 9px 12px;
            border-radius: 8px;
            box-shadow: 0 10px 24px rgba(0,0,0,0.22);
            opacity: 0;
            visibility: hidden;
            transition: opacity .15s ease, transform .15s ease;
            z-index: 1000;
            pointer-events: none;
        }
        .remark-cell::before {
            content: '';
            position: absolute;
            left: 50%;
            bottom: 100%;
            transform: translateX(-50%) translateY(2px);
            border: 6px solid transparent;
            border-top-color: #1f2937;
            opacity: 0;
            visibility: hidden;
            transition: opacity .15s ease;
            z-index: 1000;
            pointer-events: none;
        }
        .remark-cell:hover::after,
        .remark-cell:hover::before {
            opacity: 1;
            visibility: visible;
        }
        .remark-cell:hover::after { transform: translateX(-50%) translateY(-10px) scale(1); }

        /* Three-dot actions menu */
        .action-menu { position: relative; display: inline-block; }
        .action-menu-toggle {
            width: 30px;
            height: 30px;
            border-radius: 6px;
            border: 1px solid #d1d5db;
            background: #fff;
            font-size: 16px;
            line-height: 1;
            letter-spacing: 1px;
            cursor: pointer;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            color: #374151;
        }
        .action-menu-toggle:hover,
        .action-menu.open .action-menu-toggle { background: #f3f4f6; border-color: #9ca3af; }

        .action-menu-list {
            display: none;
            position: fixed;
            flex-direction: column;
            min-width: 160px;
            background: #fff;
            border: 1px solid #e5e7eb;
            border-radius: 8px;
            box-shadow: 0 8px 20px rgba(0,0,0,0.15);
            padding: 6px 0;
            z-index: 1000;
        }
        .action-menu.open .action-menu-list { display: flex; }

        .action-menu-item {
            display: block;
            padding: 8px 14px;
            font-size: 13px;
            text-align: left;
            text-decoration: none;
            color: #1f2937;
            white-space: nowrap;
            background: none;
            border: none;
        }
        .action-menu-item:hover { background: #f3f4f6; }
        .action-menu-item.action-positive { color: #16a34a; }
        .action-menu-item.action-negative { color: #dc2626; }

        /* ---- Responsive ---- */
        @media (max-width: 700px) {
            /* table-layout:fixed + no min-width would just crush all 8 columns
               into the viewport width instead of scrolling. Force a sane
               minimum so .table-scroll's overflow-x:auto actually kicks in. */
            .candidate-grid { min-width: 760px; }
        }
        @media (max-width: 480px) {
            .filter-bar .field { flex: 1 1 100%; min-width: 0; }
            .filter-bar .field .btn { width: 100%; }
        }
    </style>
    <link rel="icon" type="image/png" href="Images/favicon.png" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="page-wrapper">
            <div class="top-bar dashboard-top-bar">
                <div>
                    <h1>Manage Candidates</h1>
                    <p>Review, approve, reject, and manage login access for registered students.</p>
                </div>
                <asp:Button ID="btnBackToDashboard" runat="server" Text="&larr; Back to Dashboard" CssClass="btn btn-outline logout-btn no-print"
                    CausesValidation="false" OnClientClick="window.location.href='AdminDashboard.aspx'; return false;" />
            </div>

            <div class="card dashboard-card">

                <div class="status-message-wrapper2">
                    <asp:Label ID="lblActionStatus" runat="server" CssClass="status-message" Visible="false"></asp:Label>
                </div>

                <div class="filter-bar no-print">
                    <div class="field">
                        <label>Name or Email</label>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="Search..."></asp:TextBox>
                    </div>
                    <div class="field">
                        <label>Approval Status</label>
                        <asp:DropDownList ID="ddlApprovalFilter" runat="server" CssClass="form-control">
                            <asp:ListItem Text="All" Value="" />
                            <asp:ListItem Text="Pending" Value="Pending" />
                            <asp:ListItem Text="Approved" Value="Approved" />
                            <asp:ListItem Text="Rejected" Value="Rejected" />
                        </asp:DropDownList>
                    </div>
                    <div class="field">
                        <label>Account Status</label>
                        <asp:DropDownList ID="ddlAccountFilter" runat="server" CssClass="form-control">
                            <asp:ListItem Text="All" Value="" />
                            <asp:ListItem Text="Active" Value="Active" />
                            <asp:ListItem Text="Inactive" Value="Inactive" />
                        </asp:DropDownList>
                    </div>
                    <div class="field">
                        <asp:Button ID="btnFilter" runat="server" Text="Apply Filters" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" />
                    </div>
                    <div class="field">
                        <asp:Button ID="btnResetFilters" runat="server" Text="Reset" CssClass="btn btn-outline" OnClick="btnResetFilters_Click" CausesValidation="false" />
                    </div>
                </div>

                <asp:Label ID="lblCount" runat="server" CssClass="record-count"></asp:Label>

                <div class="table-scroll">
                    <asp:GridView ID="gvCandidates" runat="server" CssClass="candidate-grid"
                        AutoGenerateColumns="false" GridLines="None" EmptyDataText="No candidates found."
                        DataKeyNames="StudentID" OnRowCommand="gvCandidates_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="StudentID" HeaderText="Student ID" />
                            <asp:BoundField DataField="FullName" HeaderText="Name" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                            <asp:BoundField DataField="MobileNumber" HeaderText="Mobile" />

                            <asp:TemplateField HeaderText="Approval">
                                <ItemTemplate>
                                    <span class='status-pill <%# "pill-" + Eval("ApprovalStatus").ToString().ToLower() %>'>
                                        <%# Eval("ApprovalStatus") %>
                                    </span>
                                    <asp:Panel ID="pnlRemark" runat="server" CssClass="remark-cell"
                                        Visible='<%# Eval("ApprovalStatus").ToString() == "Rejected" %>'
                                        data-tooltip='<%# "Reason: " + Eval("RejectionRemark") %>'>
                                        <span class="remark-text">Reason: <%# Eval("RejectionRemark") %></span>
                                    </asp:Panel>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Account">
                                <ItemTemplate>
                                    <span class='status-pill <%# "pill-" + Eval("AccountStatus").ToString().ToLower() %>'>
                                        <%# Eval("AccountStatus") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="RegistrationDate" HeaderText="Registered On" DataFormatString="{0:dd-MMM-yyyy}" />

                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <div class="action-menu">
                                        <button type="button" class="action-menu-toggle no-print"
                                            onclick="toggleActionMenu(this); return false;"
                                            aria-haspopup="true" aria-expanded="false" title="Actions">&#8942;</button>

                                        <div class="action-menu-list">
                                            <asp:LinkButton runat="server" CommandName="Approve" CommandArgument='<%# Eval("StudentID") %>'
                                                CssClass="action-menu-item action-positive"
                                                Visible='<%# Eval("ApprovalStatus").ToString() != "Approved" %>'
                                                OnClientClick="return confirm('Approve this application?');">Approve</asp:LinkButton>

                                            <asp:LinkButton runat="server" CommandName="Reject" CommandArgument='<%# Eval("StudentID") %>'
                                                CssClass="action-menu-item action-negative"
                                                Visible='<%# Eval("ApprovalStatus").ToString() != "Rejected" %>'
                                                OnClientClick='<%# "return captureRejectRemark(this, \"" + Eval("StudentID") + "\");" %>'>Reject</asp:LinkButton>

                                            <asp:LinkButton runat="server" CommandName="Reset" CommandArgument='<%# Eval("StudentID") %>'
                                                CssClass="action-menu-item"
                                                Visible='<%# Eval("ApprovalStatus").ToString() == "Rejected" %>'
                                                OnClientClick="return confirm('Reset this application back to Pending?');">Reset</asp:LinkButton>

                                            <asp:LinkButton runat="server" CommandName="Activate" CommandArgument='<%# Eval("StudentID") %>'
                                                CssClass="action-menu-item action-positive"
                                                Visible='<%# Eval("AccountStatus").ToString() == "Inactive" %>'
                                                OnClientClick="return confirm('Activate this account?');">Activate</asp:LinkButton>

                                            <asp:LinkButton runat="server" CommandName="Deactivate" CommandArgument='<%# Eval("StudentID") %>'
                                                CssClass="action-menu-item action-negative"
                                                Visible='<%# Eval("AccountStatus").ToString() == "Active" %>'
                                                OnClientClick="return confirm('Deactivate this account? The student will be unable to log in.');">Deactivate</asp:LinkButton>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <asp:HiddenField ID="hdnRejectRemark" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hdnRejectStudentId" runat="server" ClientIDMode="Static" />
    </form>

    <script>
        // Rejection requires a mandatory remark. Collect it client-side before
        // the postback fires; the server re-validates it's non-empty too.
        function captureRejectRemark(btn, studentId) {
            var remark = prompt("Enter the reason for rejecting this application (required):");
            if (remark === null) {
                return false; // user cancelled
            }
            remark = remark.trim();
            if (remark === "") {
                alert("A rejection remark is required.");
                return false;
            }
            document.getElementById('hdnRejectRemark').value = remark;
            document.getElementById('hdnRejectStudentId').value = studentId;
            return true;
        }

        // Three-dot actions menu: uses position:fixed + JS-computed coordinates
        // so the dropdown isn't clipped by the grid's horizontal-scroll wrapper.
        function closeAllActionMenus() {
            document.querySelectorAll('.action-menu.open').forEach(function (menu) {
                menu.classList.remove('open');
                var toggle = menu.querySelector('.action-menu-toggle');
                if (toggle) { toggle.setAttribute('aria-expanded', 'false'); }
            });
        }

        function toggleActionMenu(toggleBtn) {
            var menu = toggleBtn.closest('.action-menu');
            var list = menu.querySelector('.action-menu-list');
            var wasOpen = menu.classList.contains('open');

            closeAllActionMenus();

            if (!wasOpen) {
                var rect = toggleBtn.getBoundingClientRect();
                list.style.top = (rect.bottom + 4) + 'px';
                list.style.left = 'auto';
                list.style.right = (window.innerWidth - rect.right) + 'px';

                menu.classList.add('open');
                toggleBtn.setAttribute('aria-expanded', 'true');

                // Flip above the button if there isn't room below.
                var listRect = list.getBoundingClientRect();
                if (listRect.bottom > window.innerHeight) {
                    list.style.top = (rect.top - listRect.height - 4) + 'px';
                }
            }
        }

        document.addEventListener('click', function (e) {
            if (!e.target.closest('.action-menu')) {
                closeAllActionMenus();
            }
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') { closeAllActionMenus(); }
        });
        window.addEventListener('resize', closeAllActionMenus);
        document.addEventListener('scroll', function (e) {
            if (e.target === document || e.target.classList.contains('table-scroll')) {
                closeAllActionMenus();
            }
        }, true);
    </script>
</body>
</html>
