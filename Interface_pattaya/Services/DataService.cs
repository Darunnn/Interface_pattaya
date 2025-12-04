using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Interface_pattaya.Models;
using Microsoft.Extensions.Logging;

namespace Interface_pattaya.Services
{
    public class DataService
    {
        private readonly string _connectionString;
        private readonly string _apiUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger<DataService> _logger;

        public DataService(string connectionString, string apiUrl, ILogger<DataService> logger = null)
        {
            _connectionString = connectionString;
            _apiUrl = apiUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _logger = logger;
        }

        public async Task<(int success, int failed, List<string> errors)> ProcessAndSendDataAsync()
        {
            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();
            var currentDate = DateTime.Now.ToString("yyyyMMdd");

            string query = @"
                SELECT 
                    f_referenceCode,
                    f_prescriptionnohis,
                    f_seq,
                    f_seqmax,
                    f_prescriptiondate,
                    f_ordercreatedte,
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
                    f_freetext2
                FROM tb_thaneshosp_middle 
                WHERE (IFNULL(f_dispensestatus_conhis, '0') = '0')
                AND SUBSTRING(f_prescriptiondate, 1, 8) = @CurrentDate
                ORDER BY f_prescriptionnohis, f_seq";

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CurrentDate", currentDate);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                try
                                {
                                    // ดึงข้อมูลดิบและประมวลผล
                                    var freetext2 = reader["f_freetext2"]?.ToString() ?? "";
                                    var freetext2Parts = freetext2.Split('^');

                                    var prescriptionDate = ExtractDate(reader["f_prescriptiondate"]?.ToString());
                                    var orderCreateDate = CombineDateTime(
                                        reader["f_ordercreatedte"]?.ToString(),
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

                                    // สร้าง body request
                                    var prescriptionBody = new PrescriptionBodyRequest
                                    {
                                        UniqID = $"{reader["f_referenceCode"]?.ToString()}-{currentDate}",
                                        f_prescriptionno = reader["f_prescriptionnohis"]?.ToString(),
                                        f_seq = int.TryParse(reader["f_seq"]?.ToString(), out int seq) ? seq : 0,
                                        f_seqmax = int.TryParse(reader["f_seqmax"]?.ToString(), out int seqmax) ? seqmax : 0,
                                        f_prescriptiondate = prescriptionDate,
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
                                        f_status = reader["f_status"]?.ToString()
                                    };

                                    // ส่ง API
                                    var (success, message) = await SendToApiAsync(prescriptionBody);

                                    if (success)
                                    {
                                        successCount++;
                                        _logger?.LogInformation($"✓ Success - Prescription: {prescriptionBody.f_prescriptionno}, Seq: {prescriptionBody.f_seq}");
                                        _logger?.LogDebug($"Body: {JsonSerializer.Serialize(prescriptionBody)}");
                                        await UpdateDispenseStatusAsync(reader["f_referenceCode"]?.ToString(), "1");
                                    }
                                    else
                                    {
                                        failedCount++;
                                        _logger?.LogWarning($"✗ Failed - Prescription: {prescriptionBody.f_prescriptionno}, Seq: {prescriptionBody.f_seq}");
                                        _logger?.LogDebug($"Body: {JsonSerializer.Serialize(prescriptionBody)}");
                                        _logger?.LogError($"Error: {message}");
                                        await UpdateDispenseStatusAsync(reader["f_referenceCode"]?.ToString(), "3");
                                        errors.Add($"Prescription {reader["f_prescriptionnohis"]?.ToString()}: {message}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    failedCount++;
                                    _logger?.LogError($"✗ Processing Error - RefCode: {reader["f_referenceCode"]?.ToString()}, Error: {ex.Message}");
                                    await UpdateDispenseStatusAsync(reader["f_referenceCode"]?.ToString(), "3");
                                    errors.Add($"Processing error: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Database error: {ex.Message}");
            }

            _logger?.LogInformation($"📊 Summary - Success: {successCount}, Failed: {failedCount}, Total: {successCount + failedCount}");

            return (successCount, failedCount, errors);
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
                    WriteIndented = false
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return (true, responseContent);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"API Error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }

        private async Task UpdateDispenseStatusAsync(string referenceCode, string status)
        {
            string query = @"
                UPDATE tb_thaneshosp_middle 
                SET f_dispensestatus_conhis = @Status,
                    f_dispense_datetime = NOW()
                WHERE f_referenceCode = @ReferenceCode";

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ReferenceCode", referenceCode);
                        command.Parameters.AddWithValue("@Status", status);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating dispense status: {ex.Message}", ex);
            }
        }

        private string ExtractDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 8)
                return dateStr;
            return dateStr.Substring(0, 8);
        }

        private string CombineDateTime(string dateStr, string timeStr)
        {
            if (string.IsNullOrEmpty(dateStr) || string.IsNullOrEmpty(timeStr))
                return "";

            try
            {
                if (dateStr.Length < 8 || timeStr.Length < 6)
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
            return sex == "0" ? "M" : "F";
        }

        private string ProcessPRN(string prnValue, int type)
        {
            if (string.IsNullOrEmpty(prnValue) || !int.TryParse(prnValue, out int value))
                return "0";

            if (type == 1 && value == 1)
                return "1";
            if (type == 2 && value == 2)
                return "1";

            return "0";
        }
    }
}