using System;
using System.IO;
using System.Web;

/// <summary>
/// Lightweight, dependency-free exception logger. Appends one line per error to
/// ~/App_Data/AppErrorLog.txt with a timestamp, the calling context (so you know
/// which operation failed), the exception type, and its message. Logging itself
/// must never be the reason a request fails, so every failure here is swallowed.
/// </summary>
public static class ErrorLogger
{
    private static readonly object LogLock = new object();

    /// <param name="context">Short label for where this happened, e.g.
    /// "RichTextEditorEdit.SaveDocument" -- makes the log actually useful for
    /// debugging instead of just a pile of stack traces.</param>
    public static void Log(string context, Exception ex)
    {
        try
        {
            string logPath = HttpContext.Current.Server.MapPath("~/App_Data/AppErrorLog.txt");
            string logDir = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            string line = string.Format(
                "{0:yyyy-MM-dd HH:mm:ss}\t{1}\t{2}\t{3}",
                DateTime.Now,
                context,
                ex.GetType().Name,
                (ex.Message ?? "").Replace("\r", " ").Replace("\n", " "));

            lock (LogLock)
            {
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
        }
        catch
        {
            // Never let a logging failure mask or replace the original error.
        }
    }
}
