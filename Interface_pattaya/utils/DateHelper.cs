using System;
using System.Globalization;

namespace Interface_pattaya.utils
{
    public static class DateHelper
    {
        /// <summary>
        /// แปลงวันที่จาก DateTimePicker เป็นรูปแบบ yyyyMMdd โดยรองรับทั้ง พ.ศ. และ ค.ศ.
        /// </summary>
        public static string ConvertToChristianEra(DateTime date)
        {
            try
            {
                // ตรวจสอบว่าปีเป็น พ.ศ. หรือไม่ (มากกว่า 2500)
                int year = date.Year;

                if (year > 2500)
                {
                    // แปลง พ.ศ. เป็น ค.ศ. (ลบ 543)
                    year -= 543;

                    // สร้าง DateTime ใหม่ด้วยปี ค.ศ.
                    date = new DateTime(year, date.Month, date.Day);
                }

                return date.ToString("yyyyMMdd");
            }
            catch (Exception)
            {
                // ถ้าเกิดข้อผิดพลาด ให้ใช้วันที่ปัจจุบัน
                return DateTime.Now.ToString("yyyyMMdd");
            }
        }

        /// <summary>
        /// แปลงวันที่เป็นรูปแบบ yyyy-MM-dd โดยรองรับทั้ง พ.ศ. และ ค.ศ.
        /// </summary>
        public static string ConvertToChristianEraFormatted(DateTime date)
        {
            try
            {
                int year = date.Year;

                if (year > 2500)
                {
                    year -= 543;
                    date = new DateTime(year, date.Month, date.Day);
                }

                return date.ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                return DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// ดึงวันที่ปัจจุบันในรูปแบบ ค.ศ.
        /// </summary>
        public static string GetCurrentDateChristianEra()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }

        /// <summary>
        /// ตรวจสอบว่าระบบใช้ พ.ศ. หรือไม่
        /// </summary>
        public static bool IsBuddhistEraCalendar()
        {
            try
            {
                var culture = CultureInfo.CurrentCulture;
                return culture.Calendar is ThaiBuddhistCalendar;
            }
            catch
            {
                return false;
            }
        }
    }
}