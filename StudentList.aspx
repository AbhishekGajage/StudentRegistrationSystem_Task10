<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StudentList.aspx.cs" Inherits="StudentList" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Registered Students</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" media="print" href="Styles/Print.css" />
    <link rel="icon" type="image/png" href="Images/favicon.png" />
    <style>
        .filter-bar { display: flex; flex-wrap: wrap; gap: 12px; align-items: flex-end; margin: 14px 0; }
        .filter-bar .field { display: flex; flex-direction: column; min-width: 160px; }
        .filter-bar label { font-size: 13px; font-weight: 600; margin-bottom: 4px; }
        .filter-bar .actions { display: flex; gap: 8px; }

        /* Fit all columns within the viewport instead of relying on horizontal
           scroll: fixed layout + explicit per-column widths (summing to 100%)
           + text wrapping so long values (like email) wrap instead of pushing
           the table wider than its container. */
        .student-grid {
            width: 100%;
            table-layout: fixed;
            border-collapse: collapse;
            font-size: 12.5px;
        }
        .student-grid th, .student-grid td {
            padding: 6px 8px;
            white-space: normal;
            vertical-align: middle;
        }
        .student-grid th {
            /* Headers should only wrap at word boundaries (e.g. "Student ID" ->
               "Student" / "ID"), never split inside a single word like "Photo"
               or "Country". */
            word-break: keep-all;
            overflow-wrap: normal;
        }
        .student-grid td {
            /* Data cells can contain long unbroken strings (emails), so these
               are allowed to break mid-word if needed to avoid overflow. */
            word-wrap: break-word;
            overflow-wrap: break-word;
        }
        .student-grid th:nth-child(1), .student-grid td:nth-child(1) { width: 6%;  text-align: center; }  /* Photo */
        .student-grid th:nth-child(2), .student-grid td:nth-child(2) { width: 7%;  }  /* Student ID */
        .student-grid th:nth-child(3), .student-grid td:nth-child(3) { width: 9%;  }  /* Name */
        .student-grid th:nth-child(4), .student-grid td:nth-child(4) { width: 12%; }  /* Email */
        .student-grid th:nth-child(5), .student-grid td:nth-child(5) { width: 8%;  }  /* Mobile */
        .student-grid th:nth-child(6), .student-grid td:nth-child(6) { width: 6%;  }  /* Gender */
        .student-grid th:nth-child(7), .student-grid td:nth-child(7) { width: 7%;  }  /* Country */
        .student-grid th:nth-child(8), .student-grid td:nth-child(8) { width: 8%;  }  /* State */
        .student-grid th:nth-child(9), .student-grid td:nth-child(9) { width: 8%;  }  /* District */
        .student-grid th:nth-child(10), .student-grid td:nth-child(10) { width: 10%; }  /* Course */
        .student-grid th:nth-child(11), .student-grid td:nth-child(11) { width: 7%;  }  /* Semester */
        .student-grid th:nth-child(12), .student-grid td:nth-child(12) { width: 12%; }  /* Registered On */

        .grid-photo {
            width: 32px;
            height: 32px;
            object-fit: cover;
            border-radius: 50%;
            display: block;
            margin: 0 auto;
        }

        /* Sort status banner, styled like the success/error banners elsewhere
           in the app (Register/Dashboard) but in a neutral informational blue. */
        .status-info {
            background: #eaf1ff;
            border: 1px solid #b6d0ff;
            color: #1d4ed8;
            padding: 10px 14px;
            border-radius: 6px;
            font-size: 14px;
        }

        .sort-arrow {
            margin-left: 4px;
            font-size: 11px;
        }

        /* Sortable columns (Name, Email, Registered On) render their header as an
           <a> link. Without this, the browser's default link color (dark blue,
           underlined) overrides the header's white text and the label becomes
           nearly invisible against the blue header background. */
        .student-grid th a,
        .student-grid th a:visited,
        .student-grid th a:hover,
        .student-grid th a:active {
            color: #fff;
            text-decoration: none;
        }
        .student-grid th a:hover {
            text-decoration: underline;
        }

        /* Only kick in horizontal scroll as a last resort on very narrow screens,
           instead of the grid always relying on it. */
        @media (max-width: 700px) {
            .table-scroll { overflow-x: auto; }
            .student-grid { table-layout: auto; width: max-content; min-width: 100%; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page-wrapper">
            <div class="top-bar no-print">
                <h1>Registered Students</h1>
                <p>All students who have completed OTP-verified registration.</p>
                <p><a href="Register.aspx" style="color:#fff;">&larr; Back to Registration Form</a></p>
            </div>

            <div class="print-header" style="display:none;">
                <h2>Registered Students</h2>
                <p>Printed on <%= DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt") %></p>
            </div>

            <div class="card">
                <div class="filter-bar no-print">
                    <div class="field">
                        <label>Student Name</label>
                        <asp:TextBox ID="txtSearchName" runat="server"></asp:TextBox>
                    </div>
                    <div class="field">
                        <label>Email Address</label>
                        <asp:TextBox ID="txtSearchEmail" runat="server"></asp:TextBox>
                    </div>
                    <div class="field">
                        <label>Mobile Number</label>
                        <asp:TextBox ID="txtSearchMobile" runat="server"></asp:TextBox>
                    </div>
                    <div class="field">
                        <label>Gender</label>
                        <asp:DropDownList ID="ddlFilterGender" runat="server">
                            <asp:ListItem Text="All" Value=""></asp:ListItem>
                            <asp:ListItem Text="Male" Value="Male"></asp:ListItem>
                            <asp:ListItem Text="Female" Value="Female"></asp:ListItem>
                            <asp:ListItem Text="Other" Value="Other"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="actions">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnResetFilters" runat="server" Text="Reset Filter" CssClass="btn btn-outline" OnClick="btnResetFilters_Click" CausesValidation="false" />
                    </div>
                </div>

                <div class="status-message-wrapper2">
                    <asp:Label ID="lblSortStatus" runat="server" CssClass="status-message status-info" Visible="false"></asp:Label>
                </div>

                <div class="grid-toolbar no-print">
                    <div>
                        <strong><asp:Label ID="lblTotalCount" runat="server"></asp:Label></strong>
                    </div>
                    <div class="btn-row" style="margin:0;">
                        <asp:Button ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-secondary"
                            OnClientClick="printStudentGrid(); return false;" />
                        <asp:Button ID="btnExportExcel" runat="server" Text="Export to Excel"
                            CssClass="btn btn-success" OnClick="btnExportExcel_Click" />
                        <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-outline"
                            OnClick="btnRefresh_Click" CausesValidation="false" />
                    </div>
                </div>

                <div class="table-scroll">
                    <asp:GridView ID="gvStudents" runat="server" CssClass="student-grid"
                        AutoGenerateColumns="false" GridLines="None" EmptyDataText="No student records found."
                        AllowSorting="true" OnSorting="gvStudents_Sorting" OnRowCreated="gvStudents_RowCreated">
                        <Columns>
                            <asp:TemplateField HeaderText="Photo">
                                <ItemTemplate>
                                    <img class="grid-photo"
                                        src='<%# ResolveUrl(string.IsNullOrEmpty(Eval("ProfilePhotoPath").ToString()) ? "~/Uploads/Students/default-avatar.png" : Eval("ProfilePhotoPath").ToString()) %>'
                                        onerror="this.onerror=null;this.src='<%# ResolveUrl("~/Uploads/Students/default-avatar.png") %>';"
                                        alt="Photo" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="StudentID" HeaderText="Student ID" />
                            <asp:BoundField DataField="FullName" HeaderText="Name" SortExpression="FullName" />
                            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                            <asp:BoundField DataField="MobileNumber" HeaderText="Mobile" />
                            <asp:BoundField DataField="Gender" HeaderText="Gender" />
                            <asp:BoundField DataField="CountryName" HeaderText="Country" />
                            <asp:BoundField DataField="StateName" HeaderText="State" />
                            <asp:BoundField DataField="DistrictName" HeaderText="District" />
                            <asp:BoundField DataField="Course" HeaderText="Course" />
                            <asp:BoundField DataField="Semester" HeaderText="Semester" />
                            <asp:BoundField DataField="RegistrationDate" HeaderText="Registered On"
                                DataFormatString="{0:dd-MMM-yyyy hh:mm tt}" SortExpression="RegistrationDate" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>

    <script src="Scripts/site.js"></script>
    <script>
        // Show the print-only header, hide the screen-only chrome, right before printing.
        window.addEventListener('beforeprint', function () {
            document.querySelector('.print-header').style.display = 'block';
        });
        window.addEventListener('afterprint', function () {
            document.querySelector('.print-header').style.display = 'none';
        });
    </script>
</body>
</html>
