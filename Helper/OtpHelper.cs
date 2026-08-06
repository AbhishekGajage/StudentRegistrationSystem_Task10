using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Handles generating, storing and validating OTP codes for
/// email verification during student registration.
/// </summary>
public static class OtpHelper
{
    private static int OtpLength =>
        int.Parse(ConfigurationManager.AppSettings["OtpLengthDigits"] ?? "6");

    private static int ExpiryMinutes =>
        int.Parse(ConfigurationManager.AppSettings["OtpExpiryMinutes"] ?? "5");

    /// <summary>Generates a numeric OTP, stores it in the database and returns it.</summary>
    public static string GenerateAndStoreOtp(string email)
    {
        string otp = GenerateNumericOtp(OtpLength);

        // Invalidate any previous unused OTPs for this email first.
        DBHelper.ExecuteNonQuery(
            "UPDATE OtpVerification SET IsUsed = 1 WHERE Email = @Email AND IsUsed = 0",
            new SqlParameter("@Email", email));

        DBHelper.ExecuteNonQuery(
            @"INSERT INTO OtpVerification (Email, OtpCode, GeneratedTime, ExpiryTime, IsUsed)
              VALUES (@Email, @Otp, GETDATE(), @Expiry, 0)",
            new SqlParameter("@Email", email),
            new SqlParameter("@Otp", otp),
            new SqlParameter("@Expiry", DateTime.Now.AddMinutes(ExpiryMinutes)));

        return otp;
    }

    /// <summary>Validates a submitted OTP against the most recent unused one for that email.</summary>
    public static bool VerifyOtp(string email, string submittedOtp)
    {
        DataTable dt = DBHelper.ExecuteQuery(
            @"SELECT TOP 1 OtpID, OtpCode, ExpiryTime FROM OtpVerification
              WHERE Email = @Email AND IsUsed = 0
              ORDER BY GeneratedTime DESC",
            new SqlParameter("@Email", email));

        if (dt.Rows.Count == 0) return false;

        DataRow row = dt.Rows[0];
        string storedOtp = row["OtpCode"].ToString();
        DateTime expiry = Convert.ToDateTime(row["ExpiryTime"]);

        if (DateTime.Now > expiry) return false;          // expired
        if (storedOtp != submittedOtp.Trim()) return false; // mismatch

        // Mark as used so it can't be replayed.
        DBHelper.ExecuteNonQuery(
            "UPDATE OtpVerification SET IsUsed = 1 WHERE OtpID = @Id",
            new SqlParameter("@Id", row["OtpID"]));

        return true;
    }

    private static string GenerateNumericOtp(int length)
    {
        Random rnd = new Random(Guid.NewGuid().GetHashCode());
        string otp = "";
        for (int i = 0; i < length; i++)
        {
            otp += rnd.Next(0, 10).ToString();
        }
        return otp;
    }
}
