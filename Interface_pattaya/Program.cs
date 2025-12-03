
using Interface_pattaya.utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Interface_pattaya
{
    internal static class Program
    {
        private static LogManager _logger; // ⭐ เพิ่ม

        [STAThread]
        static void Main()
        {
            string appName = "Interface_pattaya";
            Mutex mutex;
            bool createdNew = false;

            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // มี instance อื่นทำงานอยู่แล้ว
                MessageBox.Show("โปรแกรมนี้กำลังทำงานอยู่แล้ว", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            mutex.ReleaseMutex();
            // ⭐ เพิ่ม: สร้าง logger ก่อน
            _logger = new LogManager();
            _logger.LogInfo("=== Application Starting ===");

            // ⭐ เพิ่ม: Global Exception Handlers
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                _logger?.LogError("Critical error in Main", ex);

            }
            finally
            {
                _logger?.LogInfo("=== Application Stopped ===");
            }
        }

        // ⭐ เพิ่ม: จัดการ Thread Exception
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            _logger?.LogError("Unhandled Thread Exception", e.Exception);
            _logger?.LogError($"StackTrace: {e.Exception.StackTrace}", e.Exception);


        }

        // ⭐ เพิ่ม: จัดการ Unhandled Exception
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                _logger?.LogError("Unhandled Domain Exception", ex);
                _logger?.LogError($"IsTerminating: {e.IsTerminating}", ex);
                _logger?.LogError($"StackTrace: {ex.StackTrace}", ex);


            }
        }

        // ⭐ เพิ่ม: จัดการ Async Task Exception
        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.LogError("Unobserved Task Exception", e.Exception);

            if (e.Exception.InnerException != null)
            {
                _logger?.LogError("Inner Exception", e.Exception.InnerException);
            }

            // ป้องกันไม่ให้โปรแกรมปิด
            e.SetObserved();
        }
    }
}