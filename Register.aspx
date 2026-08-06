<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="Register" EnableEventValidation="false" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Student Registration</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="Styles/Site.css" />
    <link rel="stylesheet" media="print" href="Styles/Print.css" />
    <link rel="stylesheet" href="Scripts/intlTelInput/css/intlTelInput.css" />
    <!-- intl-tel-input (free, open-source) for mobile validation -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.19/css/intlTelInput.min.css" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
   <script src="Scripts/intlTelInput/js/intlTelInput.min.js"></script>
    <link rel="icon" type="image/png" href="Images/favicon.png" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

        <div class="page-wrapper">
            <div class="top-bar">
                <h1>Student Registration</h1>
                <p>Register a new student &mdash; verify email, upload photo, and submit.</p>
                <p class="no-print"><a href="StudentList.aspx" style="color:#fff;">View all registered students &rarr;</a></p>
                <p class="no-print"><a href="Login.aspx" style="color:#fff;">Already registered? Login here &rarr;</a></p>
                <p class="no-print"><a href="BulkRegister.aspx" style="color:#fff;">Bulk Registration (add multiple students) &rarr;</a></p>
            </div>
            <div class="status-message-wrapper2">
            <asp:Label ID="lblRegisterStatus" runat="server" CssClass="status-message status-message-center" Visible="false"></asp:Label>
            </div>
            <div class="card">
                <div class="form-grid">

                    <div class="form-group">
                        <label>Full Name<span class="required">*</span></label>
                        <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" placeholder="e.g. Aarav Sharma"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFullName"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Full name is required." ValidationGroup="RegGroup" />
                        <asp:RegularExpressionValidator ID="revFullName" runat="server" ControlToValidate="txtFullName"
                            CssClass="field-error" Display="Dynamic"
                            ErrorMessage="Full Name must contain only letters and spaces (3-50 characters)."
                            ValidationExpression="^[A-Za-z\s]{3,50}$" ValidationGroup="RegGroup" />
                    </div>

                    <div class="form-group">
                        <label>Email Address<span class="required">*</span></label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"
                            placeholder="student@example.com" ClientIDMode="Static" onblur="checkEmailDuplicate()"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Email is required." ValidationGroup="RegGroup" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid email address."
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ValidationGroup="RegGroup" />
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
                            OnServerValidate="cvMobile_ServerValidate" ValidationGroup="RegGroup" />
                    </div>

                    <div class="form-group">
                        <label>Country<span class="required">*</span></label>
                        <asp:DropDownList ID="ddlCountry" runat="server" CssClass="form-control" ClientIDMode="Static"
                            AutoPostBack="false" onchange="return false;">
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvCountry" runat="server" ControlToValidate="ddlCountry"
                            CssClass="field-error" Display="Dynamic" InitialValue=""
                            ErrorMessage="Please select a Country." ValidationGroup="RegGroup" />
                    </div>

                    <div class="form-group">
                        <label>State<span class="required">*</span></label>
                        <asp:DropDownList ID="ddlState" runat="server" CssClass="form-control" ClientIDMode="Static" Enabled="false">
                            <asp:ListItem Text="Select State" Value=""></asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvState" runat="server" ControlToValidate="ddlState"
                            CssClass="field-error" Display="Dynamic" InitialValue=""
                            ErrorMessage="Please select a State." ValidationGroup="RegGroup" />
                    </div>

                    <div class="form-group">
                        <label>District<span class="required">*</span></label>
                        <asp:DropDownList ID="ddlDistrict" runat="server" CssClass="form-control" ClientIDMode="Static" Enabled="false">
                            <asp:ListItem Text="Select District" Value=""></asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvDistrict" runat="server" ControlToValidate="ddlDistrict"
                            CssClass="field-error" Display="Dynamic" InitialValue=""
                            ErrorMessage="Please select a District." ValidationGroup="RegGroup" />
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
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Date of Birth is required." ValidationGroup="RegGroup" />
                        <asp:CustomValidator ID="cvDob" runat="server" ControlToValidate="txtDob"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid Date of Birth."
                            OnServerValidate="cvDob_ServerValidate" ClientValidationFunction="validateDobNotFuture" ValidationGroup="RegGroup" />
                    </div>

                    <div class="form-group full-width">
                        <label>Address</label>
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvAddress" runat="server" ControlToValidate="txtAddress"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Address is required." ValidationGroup="RegGroup" />
                        <asp:RegularExpressionValidator ID="revAddress" runat="server" ControlToValidate="txtAddress"
                            CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid address (5-200 characters)."
                            ValidationExpression="^(?=.*[A-Za-z])[A-Za-z0-9\s,./-]{5,200}$" ValidationGroup="RegGroup" />
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
                        <div class="photo-upload-row">
                            <img id="photoPreview" class="photo-preview" src="Uploads/Students/default-avatar.png" alt="Preview" />
                            <asp:FileUpload ID="fuProfilePhoto" runat="server"
                                onchange="previewProfilePhoto(this, 'photoPreview')" />
                        </div>
                        <asp:Label ID="lblPhotoError" runat="server" CssClass="field-error"></asp:Label>
                    </div>

                    <!-- ============ OTP Verification block ============ -->
                    <div class="otp-box">
                        <label style="margin-bottom:10px;display:block;">Email Verification<span class="required">*</span></label>
                        <div class="otp-row">
                            <asp:Button ID="btnSendOtp" runat="server" Text="Send OTP" CssClass="btn btn-secondary"
                                OnClick="btnSendOtp_Click" CausesValidation="false" ClientIDMode="Static" />
                            <div class="form-group">
                                <asp:TextBox ID="txtOtp" runat="server" CssClass="form-control" placeholder="Enter 6-digit OTP" MaxLength="6"></asp:TextBox>
                            </div>
                            <asp:Button ID="btnVerifyOtp" runat="server" Text="Verify OTP" CssClass="btn btn-primary"
                                OnClick="btnVerifyOtp_Click" CausesValidation="false" />
                            <asp:Button ID="btnResendOtp" runat="server" Text="Resend OTP" CssClass="btn btn-outline"
                                OnClick="btnResendOtp_Click" CausesValidation="false" Enabled="false" />
                        </div>
                    </div>

                </div>
                <div class="status-message-wrapper">
    <asp:Label ID="lblOtpStatus" runat="server" CssClass="status-message status-message-center" Visible="false"></asp:Label>
</div>
                <div class="btn-row">
                    <asp:Button ID="btnRegister" runat="server" Text="Complete Registration"
                        CssClass="btn btn-success" OnClick="btnRegister_Click"
                        ValidationGroup="RegGroup" Enabled="false" />
                   
                </div>
            </div>
        </div>
    </form>

    <script src="Scripts/site.js"></script>
    <script src="Scripts/cascading-dropdown.js"></script>
    <script>
        $(function () {
            initCascadingDropdowns('ddlCountry', 'ddlState', 'ddlDistrict');
            initMobileValidation('txtMobile');

            // Keep the hidden field updated with the full E.164 number before every postback.
            $('form').on('submit', function () {
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

        // Task III -- Client-side duplicate-email check.
        // Fires when the Email field loses focus; calls the CheckEmailExists PageMethod
        // and, if a match is found, shows a warning and disables Send OTP so the
        // student finds out before even requesting a verification code.
        var emailCheckToken = 0;
        function checkEmailDuplicate() {
            var email = document.getElementById('txtEmail').value.trim();
            var duplicateMsg = document.getElementById('emailDuplicateMsg');
            var checkingMsg = document.getElementById('emailCheckingMsg');
            var sendOtpBtn = document.getElementById('btnSendOtp');

            duplicateMsg.style.display = 'none';

            var emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
            if (!emailPattern.test(email)) {
                return; // let the existing RegularExpressionValidator handle format errors
            }

            var thisToken = ++emailCheckToken;
            checkingMsg.style.display = 'inline';

            PageMethods.CheckEmailExists(email, function (exists) {
                if (thisToken !== emailCheckToken) return; // a newer check superseded this one
                checkingMsg.style.display = 'none';
                if (exists) {
                    duplicateMsg.style.display = 'inline';
                    sendOtpBtn.disabled = true;
                } else {
                    duplicateMsg.style.display = 'none';
                    sendOtpBtn.disabled = false;
                }
            }, function () {
                // On error, fail open -- don't block registration just because the
                // live check couldn't run; the server-side check is still authoritative.
                if (thisToken !== emailCheckToken) return;
                checkingMsg.style.display = 'none';
            });
        }
    </script>
</body>
</html>
