using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

        public DataService(string connectionString, string apiUrl, LogManager logger = null)
        {
            _connectionString = connectionString;
            _apiUrl = apiUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _logger = logger ?? new LogManager();
        }

        public async Task<(int success, int failed, List<string> errors)> ProcessAndSendDataAsync()
        {
            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();
            var currentDate = DateTime.Now.ToString("yyyyMMdd");

            // ✅ FIX: เปลี่ยนจาก ISNULL ไป IS NULL
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
                ORDER BY f_prescriptionnohis, f_seq";

            _logger?.LogInfo($"📥 Starting to process prescriptions for date: {currentDate}");

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _logger?.LogInfo($"✓ Database connection opened");

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CurrentDate", currentDate);
                        command.CommandTimeout = 60;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string prescriptionNo = "";
                                string prescriptionDateFormatted = "";

                                try
                                {
                                    var referenceCode = reader["f_referenceCode"]?.ToString() ?? "";
                                    prescriptionNo = reader["f_prescriptionnohis"]?.ToString() ?? "";
                                    var seq = reader["f_seq"]?.ToString() ?? "0";
                                    var prescriptionDate = reader["f_prescriptiondate"]?.ToString() ?? "";

                                    prescriptionDateFormatted = ExtractDate(prescriptionDate);

                                    if (string.IsNullOrEmpty(prescriptionNo))
                                    {
                                        _logger?.LogWarning("⚠️ Skipped: prescriptionNo is empty");
                                        failedCount++;
                                        continue;
                                    }

                                    _logger?.LogInfo($"Processing: Ref={referenceCode}, Rx={prescriptionNo}, Seq={seq}");

                                    var freetext1 = reader["f_freetext1"]?.ToString() ?? "";
                                    var freetext2 = reader["f_freetext2"]?.ToString() ?? "";

                                    var freetext1Parts = freetext1.Split('^');
                                    var freetext2Parts = freetext2.Split('^');

                               

                                    var orderCreateDate = CombineDateTime(
                                        reader["f_ordercreatedate"]?.ToString(),
                                        reader["f_ordercreatetime"]?.ToString()
                                    );
                                    var orderAcceptDate = CombineDateTime(
                                        reader["f_orderacceptdate"]?.ToString(),
                                        reader["f_orderaccepttime"]?.ToString()
                                    );

                                    var sex = ProcessSex(reader["f_sex"]?.ToString());

                                    var prnValue = reader["f_PRN"]?.ToString();
                                    var prn = ProcessPRN(prnValue, 1);
                                    var stat = ProcessPRN(prnValue, 2);

                                    var prescriptionBody = new PrescriptionBodyRequest
                                    {
                                        UniqID = $"{referenceCode}-{currentDate}",
                                        f_prescriptionno = prescriptionNo,
                                        f_seq = int.TryParse(seq, out int seqVal) ? seqVal : 0,
                                        f_seqmax = int.TryParse(reader["f_seqmax"]?.ToString(), out int seqmax) ? seqmax : 0,
                                        f_prescriptiondate = prescriptionDateFormatted,
                                        f_ordercreatedate = orderCreateDate,
                                        f_ordertargetdate = reader["f_ordertargetdate"]?.ToString(),
                                        f_ordertargettime = reader["f_ordertargettime"]?.ToString(),
                                        f_doctorcode = reader["f_doctorcode"]?.ToString(),
                                        f_doctorname = reader["f_doctorname"]?.ToString(),
                                        f_useracceptby = reader["f_useracceptby"]?.ToString(),
                                        f_orderacceptdate = orderAcceptDate,
                                        f_orderacceptfromip = reader["f_orderacceptfromip"]?.ToString(),
                                        f_pharmacylocationcode = reader["f_pharmacylocationpackcode"]?.ToString(),
                                        f_pharmacylocationdesc = reader["f_pharmacylocationpackdesc"]?.ToString(),
                                        f_prioritycode = reader["f_prioritycode"]?.ToString(),
                                        f_prioritydesc = reader["f_prioritydesc"]?.ToString(),
                                        f_hn = reader["f_hn"]?.ToString(),
                                        f_an = reader["f_en"]?.ToString(),
                                        f_vn = null,
                                        f_title = null,
                                        f_patientname = reader["f_patientname"]?.ToString(),
                                        f_sex = sex,
                                        f_patientdob = reader["f_patientdob"]?.ToString(),
                                        f_wardcode = reader["f_wardcode"]?.ToString(),
                                        f_warddesc = reader["f_warddesc"]?.ToString(),
                                        f_roomcode = reader["f_roomcode"]?.ToString(),
                                        f_roomdesc = reader["f_roomdesc"]?.ToString(),
                                        f_bedcode = reader["f_bedcode"]?.ToString(),
                                        f_beddesc = reader["f_bedcode"]?.ToString(),
                                        f_right = null,
                                        f_drugallergy = reader["f_freetext4"]?.ToString(),
                                        f_diagnosis = null,
                                        f_orderitemcode = freetext2Parts.Length > 0 ? freetext2Parts[0] : "",
                                        f_orderitemname = reader["f_orderitemname"]?.ToString(),
                                        f_orderitemnameTH = reader["f_orderitemnameTH"]?.ToString(),
                                        f_orderitemnamegeneric = reader["f_orderitemgenericname"]?.ToString(),
                                        f_orderqty = int.TryParse(reader["f_orderqty"]?.ToString(), out int qty) ? qty : 0,
                                        f_orderunitcode = reader["f_orderunitcode"]?.ToString(),
                                        f_orderunitdesc = reader["f_orderunitdesc"]?.ToString(),
                                        f_dosage = int.TryParse(reader["f_dosage"]?.ToString(), out int dosage) ? dosage : 0,
                                        f_dosageunit = reader["f_dosageunit"]?.ToString(),
                                        f_dosagetext = null,
                                        f_drugformcode = null,
                                        f_drugformdesc = null,
                                        f_HAD = reader["f_heighAlertDrug"]?.ToString() ?? "0",
                                        f_narcoticFlg = reader["f_narcoticdrug"]?.ToString() ?? "0",
                                        f_psychotropic = reader["f_psychotropicDrug"]?.ToString() ?? "0",
                                        f_binlocation = freetext2Parts.Length > 1 ? freetext2Parts[1] : "",
                                        f_itemidentify = null,
                                        f_itemlotno = reader["f_itemlotcode"]?.ToString(),
                                        f_itemlotexpire = reader["f_itemlotexpire"]?.ToString(),
                                        f_instructioncode = reader["f_instructioncode"]?.ToString(),
                                        f_instructiondesc = reader["f_instructiondesc"]?.ToString(),
                                        f_frequencycode = reader["f_frequencycode"]?.ToString(),
                                        f_frequencydesc = reader["f_frequencydesc"]?.ToString(),
                                        f_timecode = null,
                                        f_timedesc = null,
                                        f_frequencytime = reader["f_frequencyTime"]?.ToString(),
                                        f_dosagedispense = reader["f_dosagedispense"]?.ToString(),
                                        f_dayofweek = null,
                                        f_noteprocessing = reader["f_noteprocessing"]?.ToString(),
                                        f_prn = prn,
                                        f_stat = stat,
                                        f_comment = reader["f_comment"]?.ToString(),
                                        f_tomachineno = int.TryParse(reader["f_tomachineno"]?.ToString(), out int machine) ? machine : 0,
                                        f_ipd_order_recordno = reader["f_ipdpt_record_no"]?.ToString(),
                                        f_status = reader["f_status"]?.ToString(),
                                        f_remark = freetext1Parts.Length > 0 ? freetext2Parts[0] : "",
                                        f_durationtext = freetext1Parts.Length > 1 ? freetext2Parts[1] : "",
                                        f_labeltext = freetext2Parts.Length > 2 ? freetext2Parts[2] : "",
                                        f_dosagedispense_compare = freetext1Parts.Length > 2 ? freetext2Parts[2] : ""
                                    };

                                    var (apiSuccess, apiMessage) = await SendToApiWithRetryAsync(prescriptionBody);

                                    if (apiSuccess)
                                    {
                                        successCount++;
                                        _logger?.LogInfo($"✅ API Success - Rx: {prescriptionNo}, Seq: {seq}");
                                        await UpdateDispenseStatusAsync(prescriptionNo, prescriptionDateFormatted, "1");
                                    }
                                    else
                                    {
                                        failedCount++;
                                        _logger?.LogWarning($"❌ API Failed - Rx: {prescriptionNo}, Message: {apiMessage}");
                                        await UpdateDispenseStatusAsync(prescriptionNo, prescriptionDateFormatted, "3");
                                        errors.Add($"Prescription {prescriptionNo}: {apiMessage}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    failedCount++;
                                    _logger?.LogError($"❌ Row Processing Error - Rx: {prescriptionNo}", ex);

                                    if (!string.IsNullOrEmpty(prescriptionNo) && !string.IsNullOrEmpty(prescriptionDateFormatted))
                                    {
                                        try
                                        {
                                            await UpdateDispenseStatusAsync(prescriptionNo, prescriptionDateFormatted, "3");
                                            _logger?.LogInfo($"✓ Status updated to 3 (Exception) - Rx: {prescriptionNo}");
                                        }
                                        catch (Exception updateEx)
                                        {
                                            _logger?.LogError($"❌ Failed to update status on exception - Rx: {prescriptionNo}", updateEx);
                                        }
                                    }

                                    errors.Add($"Processing error for {prescriptionNo}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                _logger?.LogError("❌ MySQL Database Error", ex);
                errors.Add($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Critical Error in ProcessAndSendDataAsync", ex);
                errors.Add($"Critical error: {ex.Message}");
            }

            _logger?.LogInfo($"📊 Processing Complete - Success: {successCount}, Failed: {failedCount}, Total: {successCount + failedCount}");

            return (successCount, failedCount, errors);
        }

        public async Task<(bool success, string message)> SendToApiWithRetryAsync(PrescriptionBodyRequest prescription, int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger?.LogInfo($"📤 API Attempt {attempt}/{maxRetries} - Rx: {prescription.f_prescriptionno}");
                    var (success, message) = await SendToApiAsync(prescription);

                    if (success)
                    {
                        _logger?.LogInfo($"✅ API Success on attempt {attempt}");
                        return (true, message);
                    }

                    _logger?.LogWarning($"⚠️ Attempt {attempt} failed: {message}");

                    if (attempt < maxRetries)
                    {
                        _logger?.LogInfo($"⏳ Waiting 5 seconds before retry...");
                        await Task.Delay(5000);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"⚠️ Attempt {attempt} exception: {ex.Message}");
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(5000);
                    }
                }
            }

            _logger?.LogError($"❌ API Failed after {maxRetries} attempts - Rx: {prescription.f_prescriptionno}");
            return (false, $"Failed after {maxRetries} retry attempts");
        }

        public async Task<(bool success, string message)> SendToApiAsync(PrescriptionBodyRequest prescription)
        {
            try
            {
                var body = new PrescriptionBodyResponse
                {
                    data = new[] { prescription }
                };

                var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null,
                    WriteIndented = true
                });

                _logger?.LogInfo($"📤 Sending API request to: {_apiUrl}");
                _logger?.LogInfo($"Payload size: {json.Length} bytes");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogInfo($"✓ API Response (200): {responseContent.Substring(0, Math.Min(200, responseContent.Length))}");

                    return (true, responseContent);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorMsg = $"API Error {(int)response.StatusCode}: {response.ReasonPhrase}";
                    _logger?.LogWarning($"❌ {errorMsg}");
                    _logger?.LogWarning($"Response: {errorContent.Substring(0, Math.Min(500, errorContent.Length))}");

                    return (false, errorMsg);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError("❌ HTTP Request Exception", ex);
                return (false, $"HTTP Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger?.LogError("❌ Unexpected Exception in SendToApiAsync", ex);
                return (false, $"Exception: {ex.Message}");
            }
        }

        private async Task UpdateDispenseStatusAsync(string prescriptionNo, string prescriptionDate, string status)
        {
            if (string.IsNullOrEmpty(prescriptionNo) || string.IsNullOrEmpty(prescriptionDate))
            {
                _logger?.LogWarning($"⚠️ Cannot update - prescriptionNo: '{prescriptionNo}', date: '{prescriptionDate}'");
                return;
            }

            _logger?.LogInfo($"🔧 [UPDATE STATUS] Rx: {prescriptionNo}, Date: {prescriptionDate}, NewStatus: {status}");

            string query = @"
                UPDATE tb_thaneshosp_middle 
                SET f_dispensestatus_conhis = @Status
                WHERE f_prescriptionnohis = @prescriptionnohis 
                AND SUBSTRING(f_prescriptiondate, 1, 8) = @prescriptiondate";

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@prescriptionnohis", prescriptionNo);
                        command.Parameters.AddWithValue("@prescriptiondate", prescriptionDate);
                        command.Parameters.AddWithValue("@Status", status);
                        command.CommandTimeout = 30;

                        _logger?.LogInfo($"📝 Executing UPDATE query with parameters:");
                        _logger?.LogInfo($"   @prescriptionnohis = '{prescriptionNo}'");
                        _logger?.LogInfo($"   @prescriptiondate = '{prescriptionDate}'");
                        _logger?.LogInfo($"   @Status = '{status}'");

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            _logger?.LogInfo($"✅ [DB UPDATE SUCCESS] Rx={prescriptionNo}, Status={status}, Rows Affected={rowsAffected}");
                        }
                        else
                        {
                            _logger?.LogWarning($"⚠️ [NO ROWS UPDATED] Rx={prescriptionNo}");
                            _logger?.LogWarning($"   This might mean: record not found, or prescription date doesn't match");
                            _logger?.LogWarning($"   Date used: '{prescriptionDate}'");
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                _logger?.LogError($"❌ [DB ERROR] MySQL Error updating Rx={prescriptionNo}", sqlEx);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ [DB ERROR] General Exception updating Rx={prescriptionNo}", ex);
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
                query += @"
        AND (f_hn LIKE @SearchText OR f_prescriptionnohis LIKE @SearchText)";
            }

            query += @"
        ORDER BY f_prescriptionnohis, f_seq";

            _logger?.LogInfo($"📥 Loading grid data for date: {queryDate}" +
                      (hasSearchText ? $", Search: {searchText}" : ""));

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
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
                                    _logger?.LogError("Error reading row for grid", ex);
                                }
                            }
                        }
                    }
                }

                _logger?.LogInfo($"✅ Grid data loaded: {dataList.Count} records");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error loading grid data", ex);
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

        private string CombineDateTime(string dateStr, string timeStr)
        {
            if (string.IsNullOrEmpty(dateStr) || string.IsNullOrEmpty(timeStr))
                return "";

            try
            {
                if (dateStr.Length < 8 || timeStr.Length < 4)
                    return $"{dateStr} {timeStr}";

                string year = dateStr.Substring(0, 4);
                string month = dateStr.Substring(4, 2);
                string day = dateStr.Substring(6, 2);
                string hour = timeStr.Substring(0, 2);
                string minute = timeStr.Substring(2, 2);
                string second = timeStr.Length >= 6 ? timeStr.Substring(4, 2) : "00";

                return $"{year}-{month}-{day} {hour}:{minute}:{second}";
            }
            catch
            {
                return $"{dateStr} {timeStr}";
            }
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