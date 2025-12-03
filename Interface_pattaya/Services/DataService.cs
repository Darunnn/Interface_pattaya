using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Interface_pattaya.Models;
namespace Interface_pattaya.Services
{
  

    public class DataService
    {
        private readonly string _connectionString;
        private readonly string _apiUrl;
        private readonly HttpClient _httpClient;

        public DataService(string connectionString, string apiUrl)
        {
            _connectionString = connectionString;
            _apiUrl = apiUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<PrescriptionData>> GetPrescriptionDataAsync()
        {
            var prescriptions = new List<PrescriptionData>();
            var currentDate = DateTime.Now.ToString("yyyyMMdd");

            string query = @"
                SELECT 
                    f_referenceCode,
                    f_prescriptionnohis,
                    f_seq,
                    f_seqmax,
                    SUBSTRING(f_prescriptiondate, 1, 8) as f_prescriptiondate,
                    CONCAT(f_ordercreatedte, f_ordercreatetime) as f_ordercreatedate,
                    f_ordertargetdate,
                    f_ordertargettime,
                    f_doctorcode,
                    f_doctorname,
                    f_useracceptby,
                    CONCAT(f_orderacceptdate, f_orderaccepttime) as f_orderacceptdate,
                    f_orderacceptfromip,
                    f_pharmacylocationpackcode as f_pharmacylocationcode,
                    f_pharmacylocationpackdesc as f_pharmacylocationdesc,
                    f_prioritycode,
                    f_prioritydesc,
                    f_hn,
                    f_en as f_an,
                    f_patientname,
                    CASE WHEN f_sex = '0' THEN 'M' ELSE 'F' END as f_sex,
                    f_patientdob,
                    f_wardcode,
                    f_warddesc,
                    f_roomcode,
                    f_roomdesc,
                    f_bedcode,
                    f_bedcode as f_beddesc,
                    f_freetext4 as f_drugallergy,
                    f_orderitemname,
                    f_orderitemnameTH,
                    f_orderitemgenericname as f_orderitemnamegeneric,
                    f_orderqty,
                    f_orderunitcode,
                    f_orderunitdesc,
                    f_dosage,
                    f_dosageunit,
                    f_heighAlertDrug as f_HAD,
                    f_narcoticdrug as f_narcoticFlg,
                    f_psyhotropicDrug as f_psychotropic,
                    f_itemlotcode as f_itemlotno,
                    f_itemlotexpire,
                    f_instructioncode,
                    f_instructiondesc,
                    f_frequencycode,
                    f_frequencydesc,
                    f_frequencyTime as f_frequencytime,
                    f_dosagedispense,
                    f_noteprocessing,
                    CASE WHEN f_PRN = 1 THEN '1' ELSE '0' END as f_prn,
                    CASE WHEN f_PRN = 2 THEN '1' ELSE '0' END as f_stat,
                    f_comment,
                    f_tomachineno,
                    f_ipdpt_recode_no as f_ipd_order_recordno,
                    f_status,
                    f_freetext2
                FROM tb_thaneshosp_middle 
                WHERE (IFNULL(f_dispensestatus_conhis, '0') = '0' OR f_dispensestatus_conhis = '3')
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
                                var freetext2 = reader["f_freetext2"]?.ToString() ?? "";
                                var freetext2Parts = freetext2.Split('^');

                                var prescription = new PrescriptionData
                                {
                                    f_referenceCode = reader["f_referenceCode"]?.ToString(),
                                    f_prescriptionno = reader["f_prescriptionnohis"]?.ToString(),
                                    f_prescriptionnohis = reader["f_prescriptionnohis"]?.ToString(),
                                    f_seq = reader["f_seq"]?.ToString(),
                                    f_seqmax = reader["f_seqmax"]?.ToString(),
                                    f_prescriptiondate = reader["f_prescriptiondate"]?.ToString(),
                                    f_ordercreatedate = reader["f_ordercreatedate"]?.ToString(),
                                    f_ordertargetdate = reader["f_ordertargetdate"]?.ToString(),
                                    f_ordertargettime = reader["f_ordertargettime"]?.ToString(),
                                    f_doctorcode = reader["f_doctorcode"]?.ToString(),
                                    f_doctorname = reader["f_doctorname"]?.ToString(),
                                    f_useracceptby = reader["f_useracceptby"]?.ToString(),
                                    f_orderacceptdate = reader["f_orderacceptdate"]?.ToString(),
                                    f_orderacceptfromip = reader["f_orderacceptfromip"]?.ToString(),
                                    f_pharmacylocationcode = reader["f_pharmacylocationcode"]?.ToString(),
                                    f_pharmacylocationdesc = reader["f_pharmacylocationdesc"]?.ToString(),
                                    f_prioritycode = reader["f_prioritycode"]?.ToString(),
                                    f_prioritydesc = reader["f_prioritydesc"]?.ToString(),
                                    f_hn = reader["f_hn"]?.ToString(),
                                    f_an = reader["f_an"]?.ToString(),
                                    f_vn = null,
                                    f_title = null,
                                    f_patientname = reader["f_patientname"]?.ToString(),
                                    f_sex = reader["f_sex"]?.ToString(),
                                    f_patientdob = reader["f_patientdob"]?.ToString(),
                                    f_wardcode = reader["f_wardcode"]?.ToString(),
                                    f_warddesc = reader["f_warddesc"]?.ToString(),
                                    f_roomcode = reader["f_roomcode"]?.ToString(),
                                    f_roomdesc = reader["f_roomdesc"]?.ToString(),
                                    f_bedcode = reader["f_bedcode"]?.ToString(),
                                    f_beddesc = reader["f_beddesc"]?.ToString(),
                                    f_right = null,
                                    f_drugallergy = reader["f_drugallergy"]?.ToString(),
                                    f_diagnosis = null,
                                    f_orderitemcode = freetext2Parts.Length > 0 ? freetext2Parts[0] : "",
                                    f_orderitemname = reader["f_orderitemname"]?.ToString(),
                                    f_orderitemnameTH = reader["f_orderitemnameTH"]?.ToString(),
                                    f_orderitemnamegeneric = reader["f_orderitemnamegeneric"]?.ToString(),
                                    f_orderqty = reader["f_orderqty"]?.ToString(),
                                    f_orderunitcode = reader["f_orderunitcode"]?.ToString(),
                                    f_orderunitdesc = reader["f_orderunitdesc"]?.ToString(),
                                    f_dosage = reader["f_dosage"]?.ToString(),
                                    f_dosageunit = reader["f_dosageunit"]?.ToString(),
                                    f_dosagetext = null,
                                    f_drugformcode = null,
                                    f_drugformdesc = null,
                                    f_HAD = reader["f_HAD"]?.ToString(),
                                    f_narcoticFlg = reader["f_narcoticFlg"]?.ToString(),
                                    f_psychotropic = reader["f_psychotropic"]?.ToString(),
                                    f_binlocation = freetext2Parts.Length > 1 ? freetext2Parts[1] : "",
                                    f_itemidentify = null,
                                    f_itemlotno = reader["f_itemlotno"]?.ToString(),
                                    f_itemlotexpire = reader["f_itemlotexpire"]?.ToString(),
                                    f_instructioncode = reader["f_instructioncode"]?.ToString(),
                                    f_instructiondesc = reader["f_instructiondesc"]?.ToString(),
                                    f_frequencycode = reader["f_frequencycode"]?.ToString(),
                                    f_frequencydesc = reader["f_frequencydesc"]?.ToString(),
                                    f_timecode = null,
                                    f_timedesc = null,
                                    f_frequencytime = reader["f_frequencytime"]?.ToString(),
                                    f_dosagedispense = reader["f_dosagedispense"]?.ToString(),
                                    f_dayofweek = null,
                                    f_noteprocessing = reader["f_noteprocessing"]?.ToString(),
                                    f_prn = reader["f_prn"]?.ToString(),
                                    f_stat = reader["f_stat"]?.ToString(),
                                    f_comment = reader["f_comment"]?.ToString(),
                                    f_tomachineno = reader["f_tomachineno"]?.ToString(),
                                    f_ipd_order_recordno = reader["f_ipd_order_recordno"]?.ToString(),
                                    f_status = reader["f_status"]?.ToString(),
                                    f_remark = freetext2Parts.Length > 3 ? freetext2Parts[3] : ""
                                };

                                prescriptions.Add(prescription);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving prescription data: {ex.Message}", ex);
            }

            return prescriptions;
        }

        public async Task<(bool success, string message)> SendToApiAsync(PrescriptionData prescription)
        {
            try
            {
                var json = JsonSerializer.Serialize(prescription, new JsonSerializerOptions
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

        public async Task<(int success, int failed, List<string> errors)> ProcessAndSendDataAsync()
        {
            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();

            try
            {
                var prescriptions = await GetPrescriptionDataAsync();

                foreach (var prescription in prescriptions)
                {
                    try
                    {
                        var (success, message) = await SendToApiAsync(prescription);

                        if (success)
                        {
                            successCount++;
                            await UpdateDispenseStatusAsync(prescription.f_referenceCode, "1");
                        }
                        else
                        {
                            failedCount++;
                            await UpdateDispenseStatusAsync(prescription.f_referenceCode, "3");
                            errors.Add($"Prescription {prescription.f_prescriptionno}: {message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        await UpdateDispenseStatusAsync(prescription.f_referenceCode, "3");
                        errors.Add($"Prescription {prescription.f_prescriptionno}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Process error: {ex.Message}");
            }

            return (successCount, failedCount, errors);
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
    }
}