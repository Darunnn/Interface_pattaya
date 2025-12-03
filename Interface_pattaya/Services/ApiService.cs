using Interface_pattaya.utils;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Interface_pattaya.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiEndpoint;
        private readonly LogManager _logger = new LogManager();

        public ApiService(string apiEndpoint)
        {
            _httpClient = new HttpClient();
            _apiEndpoint = apiEndpoint;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }




        // เมธอดใหม่ที่ return response content
        public string SendToMiddlewareWithResponse(object data)
        {
            _logger.LogInfo($"SendToMiddlewareWithResponse: Start, Endpoint = {_apiEndpoint}");
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                _logger.LogInfo($"SendToMiddlewareWithResponse: JSON = {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = _httpClient.PostAsync(_apiEndpoint, content).Result;

                _logger.LogInfo($"SendToMiddlewareWithResponse: Response status = {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInfo($"SendToMiddlewareWithResponse: Response content = {responseContent}");
                    return responseContent;
                }
                else
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    _logger.LogError($"API call failed: {response.StatusCode}, Content: {errorContent}");
                    throw new Exception($"API call failed: {response.StatusCode}, Content: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error calling API", ex);
                throw new Exception($"Error calling API: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
