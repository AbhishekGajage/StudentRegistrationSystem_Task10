<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Dashboard" EnableEventValidation="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Student Dashboard</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" href="Styles/Auth.css" />
    <link rel="stylesheet" href="Scripts/intlTelInput/css/intlTelInput.css" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="Scripts/intlTelInput/js/intlTelInput.min.js"></script>
    <link rel="icon" type="image/png" href="Images/favicon.png" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

        <div class="page-wrapper">
            <div class="top-bar dashboard-top-bar">
                <div>
                    <h1>Student Dashboard</h1>
                    <p>Welcome back, <asp:Literal ID="litWelcomeName" runat="server"></asp:Literal>.</p>
                </div>
                <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="btn btn-outline logout-btn" OnClick="btnLogout_Click" CausesValidation="false" />
            </div>

            <div class="card dashboard-card">

                <div class="dashboard-profile-header">
                    <img id="photoPreview" runat="server" class="dashboard-photo" alt="Profile Photo" />
                    <div>
                        <h2><asp:Literal ID="litFullName" runat="server"></asp:Literal></h2>
                        <p class="muted"><asp:Literal ID="litStudentId" runat="server"></asp:Literal></p>
                    </div>
                </div>
                    <asp:Label ID="lblUpdateStatus" runat="server" CssClass="status-message status-message-center" Visible="false"></asp:Label>

                <!-- ============ VIEW MODE ============ -->
                <asp:Panel ID="pnlView" runat="server" CssClass="dashboard-view-grid">
                    <div class="info-item"><label>Student ID</label><span><asp:Literal ID="litViewStudentId" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>Full Name</label><span><asp:Literal ID="litViewFullName" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>Email Address</label><span><asp:Literal ID="litViewEmail" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>Mobile Number</label><span><asp:Literal ID="litViewMobile" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>Country</label><span><asp:Literal ID="litViewCountry" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>State</label><span><asp:Literal ID="litViewState" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>District</label><span><asp:Literal ID="litViewDistrict" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>Gender</label><span><asp:Literal ID="litViewGender" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>Date of Birth</label><span><asp:Literal ID="litViewDob" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>Registration Date</label><span><asp:Literal ID="litViewRegDate" runat="server"></asp:Literal></span></div>
                    <div class="info-item"><label>Last Login</label><span><asp:Literal ID="litViewLastLogin" runat="server"></asp:Literal></span></div>
                    <div class="info-item full-width"><label>Address</label><span><asp:Literal ID="litViewAddress" runat="server"></asp:Literal></span></div>

                    <div class="full-width btn-row">
                        <asp:Button ID="btnEdit" runat="server" Text="Edit Profile" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" CssClass="btn btn-outline"
                            OnClick="btnChangePassword_Click" OnClientClick="window.location.hash='changepw';" CausesValidation="false" />
                    </div>
                </asp:Panel>

                <!-- ============ EDIT MODE ============ -->
                <asp:Panel ID="pnlEdit" runat="server" CssClass="form-grid" Visible="false">

                    <asp:Label ID="lblEditModeNote" runat="server" CssClass="full-width" Visible="false"
                        Text="Your Mobile Number is used as your login password. Update it below and click Save Changes to change your password."
                        ForeColor="#555" Font-Italic="true"></asp:Label>

                    <div class="form-group">
                        <label>Student ID</label>
                        <asp:TextBox ID="txtEditStudentId" runat="server" CssClass="form-control" ReadOnly="true" Enabled="false"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label>Email Address</label>
                        <asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" ReadOnly="true" Enabled="false"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label>Full Name<span class="required">*</span></label>
                        <asp:TextBox ID="txtEditFullName" runat="server" CssClass="form-control"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditFullName" runat="server" ControlToValidate="txtEditFullName"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Full name is required." ValidationGroup="EditGroup" />
                        <asp:RegularExpressionValidator ID="revEditFullName" runat="server" ControlToValidate="txtEditFullName"
                            CssClass="field-error" Display="Dynamic"
                            ErrorMessage="Full Name must contain only letters and spaces (3-50 characters)."
                            ValidationExpression="^[A-Za-z\s]{3,50}$" ValidationGroup="EditGroup" />
                    </div>

                    <div class="form-group">
                        <label>Mobile Number<span class="required">*</span></label>
                        <asp:TextBox ID="txtEditMobileDisplay" runat="server" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                        <asp:HiddenField ID="hdnEditFullMobile" runat="server" ClientIDMode="Static" />
                        <span id="txtEditMobileDisplay_error" class="field-error"></span>
                        <asp:CustomValidator ID="cvEditMobile" runat="server"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Mobile number is required."
                            OnServerValidate="cvEditMobile_ServerValidate" ValidationGroup="EditGroup" />
                    </div>

                    <div class="form-group">
                        <label>Country<span class="required">*</span></label>
                        <asp:DropDownList ID="ddlEditCountry" runat="server" CssClass="form-control" ClientIDMode="Static"></asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label>State<span class="required">*</span></label>
                        <asp:DropDownList ID="ddlEditState" runat="server" CssClass="form-control" ClientIDMode="Static">
                            <asp:ListItem Text="Select State" Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label>District<span class="required">*</span></label>
                        <asp:DropDownList ID="ddlEditDistrict" runat="server" CssClass="form-control" ClientIDMode="Static">
                            <asp:ListItem Text="Select District" Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group full-width">
                        <label>Address<span class="required">*</span></label>
                        <asp:TextBox ID="txtEditAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditAddress" runat="server" ControlToValidate="txtEditAddress"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Address is required." ValidationGroup="EditGroup" />
                        <asp:RegularExpressionValidator ID="revEditAddress" runat="server" ControlToValidate="txtEditAddress"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid address (5-200 characters)."
                            ValidationExpression="^(?=.*[A-Za-z])[A-Za-z0-9\s,./-]{5,200}$" ValidationGroup="EditGroup" />
                    </div>

                    <div class="form-group full-width">
                        <label>Profile Photo (JPG, JPEG, PNG &mdash; max 2 MB)</label>
                        <div class="photo-upload-row">
                            <img id="editPhotoPreview" runat="server" class="photo-preview" alt="Preview" />
                            <asp:FileUpload ID="fuEditProfilePhoto" runat="server" onchange="previewProfilePhoto(this, 'editPhotoPreview')" />
                        </div>
                        <asp:Label ID="lblEditPhotoError" runat="server" CssClass="field-error"></asp:Label>
                    </div>


                    <div class="full-width btn-row">
                        <asp:Button ID="btnSaveProfile" runat="server" Text="Save Changes" CssClass="btn btn-success"
                            OnClick="btnSaveProfile_Click" ValidationGroup="EditGroup" />
                        <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-outline"
                            OnClick="btnCancelEdit_Click" CausesValidation="false" />
                    </div>
                </asp:Panel>

            </div>
        </div>
    </form>

    <script src="Scripts/site.js"></script>
    <script src="Scripts/cascading-dropdown.js"></script>
    <script>
        $(function () {
            initCascadingDropdowns('ddlEditCountry', 'ddlEditState', 'ddlEditDistrict');
            initMobileValidation('txtEditMobileDisplay');

            // Keep the hidden field updated with the full E.164 number before every postback.
            $('form').on('submit', function () {
                $('#hdnEditFullMobile').val(getFullMobileNumber());
            });

            // If Change Password was clicked, jump focus to the Mobile field.
            if (window.location.hash === '#changepw') {
                var mobileField = document.getElementById('txtEditMobileDisplay');
                if (mobileField) {
                    mobileField.focus();
                    mobileField.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            }
        });
    </script>
</body>
</html>
