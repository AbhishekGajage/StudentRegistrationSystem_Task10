<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BulkRegister.aspx.cs" Inherits="BulkRegister" EnableEventValidation="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Bulk Student Registration</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" media="print" href="Styles/Print.css" />
    <link rel="stylesheet" href="Scripts/intlTelInput/css/intlTelInput.css" />
    <!-- intl-tel-input (free, open-source) for mobile validation -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.19/css/intlTelInput.min.css" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="Scripts/intlTelInput/js/intlTelInput.min.js"></script>
    <style>
        /* Only page-specific additions -- everything else (card, form-grid, form-group,
           btn, field-error, status-message, required, gender-options, photo-preview)
           comes from Site.css so this page matches Register.aspx exactly. */
        .temp-grid-wrap { margin-top: 28px; }
        .record-count { margin: 10px 0; font-weight: 600; }
        table.temp-grid { width: 100%; border-collapse: collapse; }
        table.temp-grid th, table.temp-grid td { border: 1px solid #ddd; padding: 8px; font-size: 13px; }
        table.temp-grid th { background: #f5f5f5; text-align: left; }
        table.temp-grid img.thumb { width: 36px; height: 36px; object-fit: cover; border-radius: 4px; }
        .bulk-actions { margin-top: 16px; display: flex; gap: 10px; flex-wrap: wrap; }
    </style>
    <link rel="icon" type="image/png" href="Images/favicon.png" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

        <div class="page-wrapper">
            <div class="top-bar">
                <h1>Bulk Student Registration</h1>
                <p>Add multiple students to a temporary list, then save them all at once.</p>
                <p class="no-print"><a href="Register.aspx" style="color:#fff;">&larr; Back to single registration</a></p>
                <p class="no-print"><a href="StudentList.aspx" style="color:#fff;">View all registered students &rarr;</a></p>
            </div>

            <div class="status-message-wrapper2">
                <asp:Label ID="lblStatus" runat="server" CssClass="status-message status-message-center" Visible="false"></asp:Label>
            </div>

            <div class="card">
                <h2>Add Student</h2>
                <div class="form-grid">

                    <div class="form-group">
                        <label>Full Name<span class="required">*</span></label>
                        <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" placeholder="e.g. Aarav Sharma"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFullName"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Full name is required." ValidationGroup="BulkAddGroup" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtFullName"
                            CssClass="field-error" Display="Dynamic"
                            ErrorMessage="Full Name must contain only letters and spaces (3-50 characters)."
                            ValidationExpression="^[A-Za-z\s]{3,50}$" ValidationGroup="BulkAddGroup" />
                    </div>

                    <div class="form-group">
                        <label>Email Address<span class="required">*</span></label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"
                            placeholder="student@example.com" ClientIDMode="Static" onblur="checkEmailDuplicate()"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Email is required." ValidationGroup="BulkAddGroup" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid email address."
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ValidationGroup="BulkAddGroup" />
                        <span id="emailCheckingMsg" class="field-hint" style="display:none;color:#888;font-size:12px;">Checking email&hellip;</span>
                        <span id="emailDuplicateMsg" class="field-error" style="display:none;">A student is already registered with this Email Address.</span>
                    </div>

                    <div class="form-group">
                        <label>Mobile Number<span class="required">*</span></label>
                        <input type="tel" id="txtMobile" class="form-control" />
                        <asp:HiddenField ID="hdnFullMobile" runat="server" ClientIDMode="Static" />
                        <span id="txtMobile_error" class="field-error"></span>
                        <asp:CustomValidator ID="cvMobile" runat="server"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Mobile number is required."
                            OnServerValidate="cvMobile_ServerValidate" ValidationGroup="BulkAddGroup" />
                    </div>

                    <div class="form-group">
                        <label>Country<span class="required">*</span></label>
                        <asp:DropDownList ID="ddlCountry" runat="server" CssClass="form-control" ClientIDMode="Static"
                            AutoPostBack="false" onchange="return false;">
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvCountry" runat="server" ControlToValidate="ddlCountry"
                            CssClass="field-error" Display="Dynamic" InitialValue=""
                            ErrorMessage="Please select a Country." ValidationGroup="BulkAddGroup" />
                    </div>

                    <div class="form-group">
                        <label>State<span class="required">*</span></label>
                        <asp:DropDownList ID="ddlState" runat="server" CssClass="form-control" ClientIDMode="Static" Enabled="false">
                            <asp:ListItem Text="Select State" Value=""></asp:ListItem>
                        </asp:DropDownList>
                        <!-- No RequiredFieldValidator here: State is JS-populated and never lives in
                             server ViewState, so SelectedValue matching is unreliable on postback.
                             btnAdd_Click validates it directly from Request.Form instead
                             (same fix already proven on Dashboard.aspx). -->
                    </div>

                    <div class="form-group">
                        <label>District<span class="required">*</span></label>
                        <asp:DropDownList ID="ddlDistrict" runat="server" CssClass="form-control" ClientIDMode="Static" Enabled="false">
                            <asp:ListItem Text="Select District" Value=""></asp:ListItem>
                        </asp:DropDownList>
                        <!-- Same reasoning as State above -- validated from Request.Form in code-behind. -->
                    </div>

                    <div class="form-group">
                        <label>Gender<span class="required">*</span></label>
                        <div class="gender-options">
                            <asp:RadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Text="Male" Value="Male" Selected="True" />
                                <asp:ListItem Text="Female" Value="Female" />
                                <asp:ListItem Text="Other" Value="Other" />
                            </asp:RadioButtonList>
                        </div>
                    </div>

                    <div class="form-group">
                        <label>Date of Birth<span class="required">*</span></label>
                        <asp:TextBox ID="txtDob" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvDob" runat="server" ControlToValidate="txtDob"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Date of Birth is required." ValidationGroup="BulkAddGroup" />
                        <asp:CustomValidator ID="cvDob" runat="server" ControlToValidate="txtDob"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid Date of Birth."
                            OnServerValidate="cvDob_ServerValidate" ClientValidationFunction="validateDobNotFuture" ValidationGroup="BulkAddGroup" />
                    </div>

                    <div class="form-group full-width">
                        <label>Address</label>
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvAddress" runat="server" ControlToValidate="txtAddress"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Address is required." ValidationGroup="BulkAddGroup" />
                        <asp:RegularExpressionValidator ID="revAddress" runat="server" ControlToValidate="txtAddress"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid address (5-200 characters)."
                            ValidationExpression="^(?=.*[A-Za-z])[A-Za-z0-9\s,./-]{5,200}$" ValidationGroup="BulkAddGroup" />
                    </div>

                    <div class="form-group">
                        <label>Course</label>
                        <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-control">
                            <asp:ListItem Text="B.Sc. Computer Science" Value="B.Sc. Computer Science" />
                            <asp:ListItem Text="B.Tech Information Technology" Value="B.Tech Information Technology" />
                            <asp:ListItem Text="BCA" Value="BCA" />
                            <asp:ListItem Text="MCA" Value="MCA" />
                            <asp:ListItem Text="B.Com" Value="B.Com" />
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label>Semester</label>
                        <asp:DropDownList ID="ddlSemester" runat="server" CssClass="form-control">
                            <asp:ListItem Text="Semester 1" Value="Semester 1" />
                            <asp:ListItem Text="Semester 2" Value="Semester 2" />
                            <asp:ListItem Text="Semester 3" Value="Semester 3" />
                            <asp:ListItem Text="Semester 4" Value="Semester 4" />
                            <asp:ListItem Text="Semester 5" Value="Semester 5" />
                            <asp:ListItem Text="Semester 6" Value="Semester 6" />
                            <asp:ListItem Text="Semester 7" Value="Semester 7" />
                            <asp:ListItem Text="Semester 8" Value="Semester 8" />
                        </asp:DropDownList>
                    </div>

                    <div class="form-group full-width">
                        <label>Profile Photo (JPG, JPEG, PNG &mdash; max 2 MB)</label>
                        <div style="display:flex;align-items:center;gap:16px;">
                            <img id="photoPreview" class="photo-preview" src="Uploads/Students/default-avatar.png" alt="Preview" />
                            <asp:FileUpload ID="fuProfilePhoto" runat="server"
                                onchange="previewProfilePhoto(this, 'photoPreview')" />
                        </div>
                        <asp:Label ID="lblPhotoError" runat="server" CssClass="field-error"></asp:Label>
                    </div>

                </div>

                <div class="btn-row">
                    <asp:Button ID="btnAdd" runat="server" Text="Add Record" CssClass="btn btn-primary"
                        ValidationGroup="BulkAddGroup" OnClick="btnAdd_Click" />
                </div>
                 <div class="status-message-wrapper">
                <asp:Label ID="lblAddError" runat="server" CssClass="status-message status-error" Visible="false"></asp:Label>
                   </div>
            </div>

            <div class="card temp-grid-wrap">
                <h2>Pending Records (not yet saved)</h2>
                <asp:Label ID="lblRecordCount" runat="server" CssClass="record-count" Text="0 record(s) pending"></asp:Label>

                <asp:GridView ID="gvTemp" runat="server" AutoGenerateColumns="false" CssClass="temp-grid"
                    DataKeyNames="Email" EmptyDataText="No records added yet.">
                    <Columns>
                        <asp:TemplateField HeaderText="Select">
                            <ItemTemplate>
                                <asp:CheckBox ID="chkSelect" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Photo">
                            <ItemTemplate>
                                <img class="thumb" src='<%# Eval("PhotoPath") %>' alt="" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="FullName" HeaderText="Full Name" />
                        <asp:BoundField DataField="Email" HeaderText="Email" />
                        <asp:BoundField DataField="Mobile" HeaderText="Mobile" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" />
                        <asp:BoundField DataField="Dob" HeaderText="DOB" DataFormatString="{0:dd-MMM-yyyy}" />
                        <asp:BoundField DataField="Address" HeaderText="Address" />
                        <asp:BoundField DataField="CountryName" HeaderText="Country" />
                        <asp:BoundField DataField="State" HeaderText="State" />
                        <asp:BoundField DataField="District" HeaderText="District" />
                        <asp:BoundField DataField="Course" HeaderText="Course" />
                        <asp:BoundField DataField="Semester" HeaderText="Semester" />
                    </Columns>
                </asp:GridView>

                <div class="bulk-actions">
                    <asp:Button ID="btnRemoveSelected" runat="server" Text="Remove Selected Record"
                        CssClass="btn btn-outline" CausesValidation="false" OnClick="btnRemoveSelected_Click" />
                    <asp:Button ID="btnClearAll" runat="server" Text="Clear All Records"
                        CssClass="btn btn-outline" CausesValidation="false" OnClick="btnClearAll_Click"
                        OnClientClick="return confirm('Clear all pending records? This cannot be undone.');" />
                    <asp:Button ID="btnSaveAll" runat="server" Text="Save All"
                        CssClass="btn btn-success" CausesValidation="false" OnClick="btnSaveAll_Click" />
                </div>
            </div>
        </div>
    </form>

    <script src="Scripts/site.js"></script>
    <script src="Scripts/cascading-dropdown.js"></script>
    <script>
$(function() {
    initCascadingDropdowns('ddlCountry', 'ddlState', 'ddlDistrict');
    initMobileValidation('txtMobile');

    // Keep the hidden field updated with the full E.164 number before every postback.
    $('form').on('submit', function() {
        $('#hdnFullMobile').val(getFullMobileNumber());
    });
});

function validateDobNotFuture(sender, args) {
    if (!args.Value) { args.IsValid = false; return; }
    var selectedDate = new Date(args.Value);
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    var hundredYearsAgo = new Date();
    hundredYearsAgo.setFullYear(today.getFullYear() - 100);
    args.IsValid = selectedDate <= today && selectedDate >= hundredYearsAgo;
}

// Live duplicate-email check, same as Register.aspx.
var emailCheckToken = 0;
function checkEmailDuplicate() {
    var email = document.getElementById('txtEmail').value.trim();
    var duplicateMsg = document.getElementById('emailDuplicateMsg');
    var checkingMsg = document.getElementById('emailCheckingMsg');

    duplicateMsg.style.display = 'none';

    var emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    if (!emailPattern.test(email)) {
        return;
    }

    var thisToken = ++emailCheckToken;
    checkingMsg.style.display = 'inline';

    PageMethods.CheckEmailExists(email, function(exists) {
        if (thisToken !== emailCheckToken) return;
        checkingMsg.style.display = 'none';
        duplicateMsg.style.display = exists ? 'inline' : 'none';
    }, function() {
        if (thisToken !== emailCheckToken) return;
        checkingMsg.style.display = 'none';
    });
        }
    </script>
</body>
</html>
