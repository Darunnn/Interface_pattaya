using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Data;
using Interface_pattaya.Models;
using Interface_pattaya.utils;

namespace Interface_pattaya.Services
{
    public class DataService
    {
        private readonly string _connectionString;
        private readonly string _apiUrl;
        private readonly HttpClient _httpClient;
        private readonly LogManager _logger;
        private readonly int _batchSize;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

        public DataService(string connectionString, string apiUrl, LogManager logger = null, int batchSize = 100)
        {
            _connectionString = connectionString;
            _apiUrl = apiUrl;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _httpClient.DefaultRequestHeaders.ConnectionClose = false;
            _logger = logger ?? new LogManager();
            _batchSize = batchSize;
        }

        private string ToNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        // ⭐ แก้ไข: รวมวันที่และเวลาอย่างถูกต้อง
        private string CombineDateTimeOrNull(object date, object time)
        {
            var dateStr = date?.ToString()?.Trim();
            var timeStr = time?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(dateStr))
                return null;

            if (string.IsNullOrWhiteSpace(timeStr))
                return dateStr; // ส่งแค่วันที่ถ้าไม่มีเวลา

            return $"{dateStr} {timeStr}";
        }

        private PrescriptionBodyRequest BuildPrescriptionBody(IDataReader reader)
        {
            try
            {
                var seq = reader["f_seq"]?.ToString();
                var prescriptionDate = reader["f_prescriptiondate"]?.ToString();
                var prescriptionDateFormatted = ExtractDate(prescriptionDate);
                var freetext1 = reader["f_freetext1"]?.ToString();
                var freetext2 = reader["f_freetext2"]?.ToString();
                var freetext1Parts = freetext1?.Split('^') ?? Array.Empty<string>();
                var freetext2Parts = freetext2?.Split('^') ?? Array.Empty<string>();
                var sex = ProcessSex(reader["f_sex"]?.ToString());
                var prnValue = reader["f_PRN"]?.ToString();
                var prn = ProcessPRN(prnValue, 1);
                var stat = ProcessPRN(prnValue, 2);

                return new PrescriptionBodyRequest
                {
                    UniqID = ToNull(reader["f_referenceCode"]?.ToString()),
                    f_prescriptionno = ToNull(reader["f_prescriptionnohis"]?.ToString()),
                    f_seq = decimal.TryParse(seq, out decimal seqVal) ? seqVal : (decimal?)null,
                    f_seqmax = decimal.TryParse(reader["f_seqmax"]?.ToString(), out decimal seqmax) ? seqmax : (decimal?)null,
                    f_prescriptiondate = ToNull(prescriptionDateFormatted),

                    // ⭐ แก้ไขตรงนี้
                    f_ordercreatedate = ToNull(CombineDateTimeOrNull(reader["f_ordercreatedate"], reader["f_ordercreatetime"])),
                    f_ordertargetdate = ToNull(reader["f_ordertargetdate"]?.ToString()),
                    f_ordertargettime = ToNull(reader["f_ordertargettime"]?.ToString()),
                    f_doctorcode = ToNull(reader["f_doctorcode"]?.ToString()),
                    f_doctorname = ToNull(reader["f_doctorname"]?.ToString()),
                    f_useracceptby = ToNull(reader["f_useracceptby"]?.ToString()),

                    // ⭐ แก้ไขตรงนี้
                    f_orderacceptdate = ToNull(CombineDateTimeOrNull(reader["f_orderacceptdate"], reader["f_orderaccepttime"])),

                    f_orderacceptfromip = ToNull(reader["f_orderacceptfromip"]?.ToString()),
                    f_pharmacylocationcode = ToNull(reader["f_pharmacylocationpackcode"]?.ToString()),
                    f_pharmacylocationdesc = ToNull(reader["f_pharmacylocationpackdesc"]?.ToString()),
                    f_prioritycode = ToNull(reader["f_prioritycode"]?.ToString()),
                    f_prioritydesc = ToNull(reader["f_prioritydesc"]?.ToString()),
                    f_hn = ToNull(reader["f_hn"]?.ToString()),
                    f_an = ToNull(reader["f_en"]?.ToString()),
                    f_vn = null,
                    f_title = null,
                    f_patientname = ToNull(reader["f_patientname"]?.ToString()),
                    f_sex = ToNull(sex),
                    f_patientdob = ToNull(reader["f_patientdob"]?.ToString()),
                    f_wardcode = ToNull(reader["f_wardcode"]?.ToString()),
                    f_warddesc = ToNull(reader["f_warddesc"]?.ToString()),
                    f_roomcode = ToNull(reader["f_roomcode"]?.ToString()),
                    f_roomdesc = ToNull(reader["f_roomdesc"]?.ToString()),
                    f_bedcode = ToNull(reader["f_bedcode"]?.ToString()),
                    f_beddesc = ToNull(reader["f_bedcode"]?.ToString()),
                    f_right = null,
                    f_drugallergy = ToNull(reader["f_freetext4"]?.ToString()),
                    f_diagnosis = null,
                    f_orderitemcode = ToNull(freetext2Parts.Length > 0 ? freetext2Parts[0] : null),
                    f_orderitemname = ToNull(reader["f_orderitemname"]?.ToString()),
                    f_orderitemnameTH = ToNull(reader["f_orderitemnameTH"]?.ToString()),
                    f_orderitemnamegeneric = ToNull(reader["f_orderitemgenericname"]?.ToString()),
                    f_orderqty = decimal.TryParse(reader["f_orderqty"]?.ToString(), out decimal qty) ? qty : (decimal?)null,
                    f_orderunitcode = ToNull(reader["f_orderunitcode"]?.ToString()),
                    f_orderunitdesc = ToNull(reader["f_orderunitdesc"]?.ToString()),
                    f_dosage = decimal.TryParse(reader["f_dosage"]?.ToString(), out decimal dosage) ? dosage : (decimal?)null,
                    f_dosageunit = ToNull(reader["f_dosageunit"]?.ToString()),
                    f_dosagetext = null,
                    f_drugformcode = null,
                    f_drugformdesc = null,
                    f_HAD = ToNull(reader["f_heighAlertDrug"]?.ToString()) ?? "0",
                    f_narcoticFlg = ToNull(reader["f_narcoticdrug"]?.ToString()) ?? "0",
                    f_psychotropic = ToNull(reader["f_psychotropicDrug"]?.ToString()) ?? "0",
                    f_binlocation = ToNull(freetext2Parts.Length > 1 ? freetext2Parts[1] : null),
                    f_itemidentify = null,
                    f_itemlotno = ToNull(reader["f_itemlotcode"]?.ToString()),
                    f_itemlotexpire = ToNull(reader["f_itemlotexpire"]?.ToString()),
                    f_instructioncode = ToNull(reader["f_instructioncode"]?.ToString()),
                    f_instructiondesc = ToNull(reader["f_instructiondesc"]?.ToString()),
                    f_frequencycode = ToNull(reader["f_frequencycode"]?.ToString()),
                    f_frequencydesc = ToNull(reader["f_frequencydesc"]?.ToString()),
                    f_timecode = null,
                    f_timedesc = null,
                    f_frequencytime = ToNull(reader["f_frequencyTime"]?.ToString()),
                    f_dosagedispense = ToNull(reader["f_dosagedispense"]?.ToString()),
                    f_dayofweek = null,
                    f_noteprocessing = ToNull(reader["f_noteprocessing"]?.ToString()),
                    f_prn = ToNull(prn),
                    f_stat = ToNull(stat),
                    f_comment = ToNull(reader["f_comment"]?.ToString()),
                    f_tomachineno = decimal.TryParse(reader["f_tomachineno"]?.ToString(), out decimal machine) ? machine : (decimal?)null,
                    f_ipd_order_recordno = ToNull(reader["f_ipdpt_record_no"]?.ToString()),
                    f_status = ToNull(reader["f_status"]?.ToString()),
                    f_remark = ToNull(freetext1Parts.Length > 0 ? freetext1Parts[0] : null),
                    f_durationtext = ToNull(freetext1Parts.Length > 1 ? freetext1Parts[1] : null),
                    f_labeltext = ToNull(freetext2Parts.Length > 2 ? freetext2Parts[2] : null),
                    f_dosagedispense_compare = ToNull(freetext1Parts.Length > 2 ? freetext1Parts[2] : null),
                    f_ipdpt_record_no = ToNull(reader["f_ipdpt_record_no"]?.ToString())
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error building prescription body", ex);
                throw;
            }
        }

        public async Task<(int success, int failed, List<string> errors)> ProcessAndSendDataAsync()
        {
            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();
            var currentDate = DateTime.Now.ToString("yyyyMMdd");

            string query = $@"
                SELECT 
                    f_referenceCode,
                    f_prescriptionnohis,
                    f_seq,
                    f_seqmax,
                    f_prescriptiondate,
                    f_ordercreatedate,
                    f_ordercreatetime,
                    f_ordertargetdate,
                    f_ordertargettime,
                    f_doctorcode,
                    f_doctorname,
                    f_useracceptby,
                    f_orderacceptdate,
                    f_orderaccepttime,
                    f_orderacceptfromip,
                    f_pharmacylocationpackcode,
                    f_pharmacylocationpackdesc,
                    f_prioritycode,
                    f_prioritydesc,
                    f_hn,
                    f_en,
                    f_patientname,
                    f_sex,
                    f_patientdob,
                    f_wardcode,
                    f_warddesc,
                    f_roomcode,
                    f_roomdesc,
                    f_bedcode,
                    f_freetext4,
                    f_orderitemname,
                    f_orderitemnameTH,
                    f_orderitemgenericname,
                    f_orderqty,
                    f_orderunitcode,
                    f_orderunitdesc,
                    f_dosage,
                    f_dosageunit,
                    f_heighAlertDrug,
                    f_narcoticdrug,
                    f_psychotropicDrug,
                    f_itemlotcode,
                    f_itemlotexpire,
                    f_instructioncode,
                    f_instructiondesc,
                    f_frequencycode,
                    f_frequencydesc,
                    f_frequencyTime,
                    f_dosagedispense,
                    f_noteprocessing,
                    f_PRN,
                    f_comment,
                    f_tomachineno,
                    f_ipdpt_record_no,
                    f_status,
                    f_freetext1,
                    f_freetext2
                FROM tb_thaneshosp_middle
                WHERE (f_dispensestatus_conhis IS NULL 
                       OR f_dispensestatus_conhis = '' 
                       OR f_dispensestatus_conhis = '0')
                AND SUBSTRING(f_prescriptiondate, 1, 8) = @CurrentDate
                ORDER BY f_lastmodified, f_prescriptionnohis, f_seq 
                LIMIT @BatchSize";

            _logger?.LogInfo($"📥 Processing batch (Size: {_batchSize}, Date: {currentDate})");

            try
            {
                var connectionBuilder = new MySqlConnectionStringBuilder(_connectionString)
                {
                    ConnectionTimeout = 10
                };

                using (var connection = new MySqlConnection(connectionBuilder.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CurrentDate", currentDate);
                        command.Parameters.AddWithValue("@BatchSize", _batchSize);
                        command.CommandTimeout = 30;
                        command.CommandType = System.Data.CommandType.Text;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var batchList = new List<PrescriptionBodyRequest>();
                            var batchPrescriptionInfo = new List<(string prescriptionNo, string prescriptionDate)>();

                            while (await reader.ReadAsync())
                            {
                                string prescriptionNo = "";
                                string prescriptionDateFormatted = "";

                                try
                                {
                                    prescriptionNo = reader["f_prescriptionnohis"]?.ToString();
                                    var prescriptionDate = reader["f_prescriptiondate"]?.ToString();
                                    prescriptionDateFormatted = ExtractDate(prescriptionDate);

                                    if (string.IsNullOrEmpty(prescriptionNo))
                                    {
                                        failedCount++;
                                        continue;
                                    }

                                    var prescriptionBody = BuildPrescriptionBody(reader);
                                    batchList.Add(prescriptionBody);
                                    batchPrescriptionInfo.Add((prescriptionNo, prescriptionDateFormatted));

                                    if (batchList.Count >= _batchSize)
                                    {
                                        _logger?.LogInfo($"📦 Sending batch ({batchList.Count} items) - Batch full");
                                        var (batchSuccess, batchFailed) = await SendBatchToApiAsync(batchList, batchPrescriptionInfo);
                                        successCount += batchSuccess;
                                        failedCount += batchFailed;

                                        batchList.Clear();
                                        batchPrescriptionInfo.Clear();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    failedCount++;
                                    _logger?.LogError($"❌ Row Error - Rx: {prescriptionNo}", ex);

                                    if (!string.IsNullOrEmpty(prescriptionNo) && !string.IsNullOrEmpty(prescriptionDateFormatted))
                                    {
                                        await UpdateDispenseStatusAsync(prescriptionNo, prescriptionDateFormatted, "3");
                                    }
                                }
                            }

                            if (batchList.Count > 0)
                            {
                                _logger?.LogInfo($"📦 Sending batch ({batchList.Count} items) - Final");
                                var (batchSuccess, batchFailed) = await SendBatchToApiAsync(batchList, batchPrescriptionInfo);
                                successCount += batchSuccess;
                                failedCount += batchFailed;
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                _logger?.LogError("❌ MySQL Error", ex);
                errors.Add($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Critical Error", ex);
                errors.Add($"Critical error: {ex.Message}");
            }

            _logger?.LogInfo($"📊 Complete - Success: {successCount}, Failed: {failedCount}");
            return (successCount, failedCount, errors);
        }

        private async Task<(int success, int failed)> SendBatchToApiAsync(
            List<PrescriptionBodyRequest> batchList,
            List<(string prescriptionNo, string prescriptionDate)> batchInfo)
        {
            int successCount = 0;
            int failedCount = 0;

            try
            {
                var body = new PrescriptionBodyResponse
                {
                    data = batchList.ToArray()
                };

                var json = JsonSerializer.Serialize(body, _jsonOptions);

                _logger?.LogInfo($"📤 Sending {batchList.Count} items ({json.Length / 1024.0:F1} KB)");

                if (batchList.Count > 0)
                {
                    var first = batchList[0];
                    _logger?.LogInfo($"   First: Rx={first.f_prescriptionno}, HN={first.f_hn}");
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogInfo($"✅ Success - {responseContent.Substring(0, Math.Min(100, responseContent.Length))}");

                    successCount = batchList.Count;

                    // ⭐ แก้ไข: ใช้ Bulk Update แทนการ Loop
                    await UpdateBatchStatusAsync(batchInfo, "1");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogError($"❌ API Error {(int)response.StatusCode}: {errorContent.Substring(0, Math.Min(200, errorContent.Length))}");

                    failedCount = batchList.Count;
                    await UpdateBatchStatusAsync(batchInfo, "3");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ Send Exception", ex);
                failedCount = batchList.Count;
                await UpdateBatchStatusAsync(batchInfo, "3");
            }

            return (successCount, failedCount);
        }

        // ⭐ แก้ไข: ใช้ CASE WHEN แทนการ Loop
        private async Task UpdateBatchStatusAsync(
            List<(string prescriptionNo, string prescriptionDate)> batchInfo,
            string status)
        {
            if (batchInfo == null || batchInfo.Count == 0)
                return;

            try
            {
                var connectionBuilder = new MySqlConnectionStringBuilder(_connectionString)
                {
                    ConnectionTimeout = 10
                };

                using (var connection = new MySqlConnection(connectionBuilder.ConnectionString))
                {
                    await connection.OpenAsync();

                    // ⭐ สร้าง IN clause สำหรับ PrescriptionNo
                    var prescriptionNos = batchInfo.Select(x => x.prescriptionNo).Distinct().ToList();
                   

                    if (prescriptionNos.Count == 0)
                        return;

                    // ⭐ ใช้ IN clause เพื่อ Update หลาย rows พร้อมกัน
                    var inClause = string.Join(",", prescriptionNos.Select((_, i) => $"@Rx{i}"));
                   

                    string query = $@"
                        UPDATE tb_thaneshosp_middle 
                        SET f_dispensestatus_conhis = @Status
                        WHERE f_prescriptionnohis IN ({inClause})
                        AND SUBSTRING(f_prescriptiondate, 1, 8) = '{DateTime.Now.ToString("yyyyMMdd")}'";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", status);

                        for (int i = 0; i < prescriptionNos.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@Rx{i}", prescriptionNos[i]);
                        }

                       
                        command.CommandTimeout = 30;

                        var affected = await command.ExecuteNonQueryAsync();
                        _logger?.LogInfo($"✅ Updated {affected} records to status '{status}' (Expected: {batchInfo.Count})");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ Batch Update Error", ex);
            }
        }

        private async Task UpdateDispenseStatusAsync(string prescriptionNo, string prescriptionDate, string status)
        {
            if (string.IsNullOrEmpty(prescriptionNo) || string.IsNullOrEmpty(prescriptionDate))
                return;

            string query = @"
                UPDATE tb_thaneshosp_middle 
                SET f_dispensestatus_conhis = @Status
                WHERE f_prescriptionnohis = @prescriptionnohis 
                AND SUBSTRING(f_prescriptiondate, 1, 8) = @prescriptiondate";

            try
            {
                var connectionBuilder = new MySqlConnectionStringBuilder(_connectionString)
                {
                    ConnectionTimeout = 10
                };

                using (var connection = new MySqlConnection(connectionBuilder.ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@prescriptionnohis", prescriptionNo);
                        command.Parameters.AddWithValue("@prescriptiondate", prescriptionDate);
                        command.Parameters.AddWithValue("@Status", status);
                        command.CommandTimeout = 10;

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ Update Error - Rx={prescriptionNo}", ex);
            }
        }

        public async Task<List<GridViewDataModel>> GetPrescriptionDataAsync(string date = "", string searchText = "")
        {
            var dataList = new List<GridViewDataModel>();
            var queryDate = string.IsNullOrEmpty(date)
                ? DateTime.Now.ToString("yyyyMMdd")
                : date.Replace("-", "");
            bool hasSearchText = !string.IsNullOrWhiteSpace(searchText);

            string query = $@"
                SELECT 
                    f_prescriptionnohis,
                    f_seq,
                    f_seqmax,
                    f_prescriptiondate,
                    f_patientname,
                    f_hn,
                    f_orderitemnameTH,
                    f_orderqty,
                    f_orderunitdesc,
                    f_dosagedispense,
                    f_dispensestatus_conhis
                FROM tb_thaneshosp_middle
                WHERE SUBSTRING(f_prescriptiondate, 1, 8) = @QueryDate
                AND f_dispensestatus_conhis IN ('1', '3')";

            if (hasSearchText)
            {
                query += @" AND (f_hn LIKE @SearchText OR f_prescriptionnohis LIKE @SearchText OR f_referenceCode LIKE @SearchText)";
            }

            query += @" ORDER BY f_prescriptionnohis, f_seq";

            try
            {
                var connectionBuilder = new MySqlConnectionStringBuilder(_connectionString)
                {
                    ConnectionTimeout = 10
                };

                using (var connection = new MySqlConnection(connectionBuilder.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@QueryDate", queryDate);
                        if (hasSearchText)
                        {
                            command.Parameters.AddWithValue("@SearchText", "%" + searchText.Trim() + "%");
                        }
                        command.CommandTimeout = 30;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                try
                                {
                                    var status = reader["f_dispensestatus_conhis"]?.ToString() ?? "";

                                    var item = new GridViewDataModel
                                    {
                                        PrescriptionNo = reader["f_prescriptionnohis"]?.ToString() ?? "",
                                        Seq = reader["f_seq"]?.ToString() ?? "",
                                        SeqMax = reader["f_seqmax"]?.ToString() ?? "",
                                        Prescriptiondate = reader["f_prescriptiondate"]?.ToString() ?? "",
                                        PatientName = reader["f_patientname"]?.ToString() ?? "",
                                        HN = reader["f_hn"]?.ToString() ?? "",
                                        ItemNameTH = reader["f_orderitemnameTH"]?.ToString() ?? "",
                                        OrderQty = reader["f_orderqty"]?.ToString() ?? "",
                                        OrderUnit = reader["f_orderunitdesc"]?.ToString() ?? "",
                                        Dosage = reader["f_dosagedispense"]?.ToString() ?? "",
                                        Status = status,
                                    };

                                    dataList.Add(item);
                                }
                                catch (Exception ex)
                                {
                                    _logger?.LogError("Error reading row", ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error loading grid data", ex);
            }

            return dataList;
        }

        public async Task<List<PrescriptionBodyRequest>> GetFullPrescriptionDataAsync(
            List<(string prescriptionNo, string prescriptionDate)> prescriptions)
        {
            var dataList = new List<PrescriptionBodyRequest>();

            if (prescriptions == null || prescriptions.Count == 0)
            {
                return dataList;
            }

            string query = @"
                SELECT * FROM tb_thaneshosp_middle
                WHERE f_prescriptionnohis = @PrescriptionNo
                AND SUBSTRING(f_prescriptiondate, 1, 8) = @PrescriptionDate
                ORDER BY f_seq";

            try
            {
                var connectionBuilder = new MySqlConnectionStringBuilder(_connectionString)
                {
                    ConnectionTimeout = 10
                };

                using (var connection = new MySqlConnection(connectionBuilder.ConnectionString))
                {
                    await connection.OpenAsync();

                    foreach (var (prescriptionNo, prescriptionDate) in prescriptions)
                    {
                        try
                        {
                            using (var command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@PrescriptionNo", prescriptionNo);
                                command.Parameters.AddWithValue("@PrescriptionDate", prescriptionDate);
                                command.CommandTimeout = 30;

                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        try
                                        {
                                            var prescriptionBody = BuildPrescriptionBody(reader);
                                            dataList.Add(prescriptionBody);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger?.LogError($"Error parsing row for Rx={prescriptionNo}", ex);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError($"Error fetching Rx={prescriptionNo}", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error in GetFullPrescriptionDataAsync", ex);
            }

            return dataList;
        }

        private string ExtractDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return "";

            if (dateStr.Length >= 8)
                return dateStr.Substring(0, 8);

            return dateStr;
        }

        private string ProcessSex(string sex)
        {
            return string.IsNullOrEmpty(sex) ? "U" : (sex == "0" ? "M" : "F");
        }

        private string ProcessPRN(string prnValue, int type)
        {
            if (string.IsNullOrEmpty(prnValue) || !int.TryParse(prnValue, out int value))
                return "0";

            return (type == 1 && value == 1) || (type == 2 && value == 2) ? "1" : "0";
        }
    }
}