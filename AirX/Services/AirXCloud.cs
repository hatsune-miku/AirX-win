using AirX.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Data.Json;

namespace AirX.Services
{
    public static class AirXCloud
    {
        private const string BaseURL = "https://airx.eggtartc.com";

        public class UnauthorizedException : Exception { }
        public class IncorrectCredentialTypeException : Exception { }

        private static string GetParametersFromDictionary(Dictionary<string, dynamic> parameters)
        {
            StringBuilder builder = new();
            foreach (var entry in parameters)
            {
                builder.Append(entry.Key);
                builder.Append("=");
                builder.Append(entry.Value);
                builder.Append("&");
            }
            builder.Length--;
            return builder.ToString();
        }

        private static async Task<T> RequestAsync<T>(
            HttpMethod method,
            string path,
            Dictionary<string, dynamic> body,
            bool needToken
        )
        {
            var client = new HttpClient();

            HttpRequestMessage request = null;
            if (method == HttpMethod.Post)
            {
                request = new HttpRequestMessage(method, BaseURL + path);
                var jsonBody = JsonConvert.SerializeObject(body);
                var content = new StringContent(jsonBody, null, "application/json");
                request.Content = content;
            }
            else
            {
                request = new HttpRequestMessage(method, BaseURL + path + "?" + GetParametersFromDictionary(body));
            }

            if (needToken)
            {
                if (SettingsUtil.ReadCredentialType() != CredentialType.AirXToken)
                {
                    throw new IncorrectCredentialTypeException();
                }
                var credential = SettingsUtil.String(DefaultKeys.SavedCredential, "");
                request.Headers.Authorization = new("Bearer", credential);
            }

            var response = await client.SendAsync(request);
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    {
                        throw new UnauthorizedException();
                    }
                case System.Net.HttpStatusCode.OK:
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(responseString);
                    }
                default:
                    {
                        throw new HttpRequestException("Status code = " + response.StatusCode);
                    }
            }
        }

        public class LoginResponse
        {
            public bool success;
            public string message;
            public string name;
            public string token;
        }

        public class RenewResponse
        {

            public bool success;
            public string message;
            public string token;
        }

        public class GreetingsResponse
        {
            public bool success;
            public string message;
            public string name;
            public int uid;
            public DateTime validBefore;
        }

        public static async Task<LoginResponse> LoginAsync(string uid, string password, string salt = "114514")
        {
            password = HashUtil.Sha256(HashUtil.Sha256(password));
            return await RequestAsync<LoginResponse>(
                HttpMethod.Post,
                "/auth/token",
                new()
                {
                    { "uid", uid },
                    { "password", password },
                    { "salt", salt }
                },
                false
            );
        }

        public static async Task<RenewResponse> RenewAsync(string uid)
        {
            return await RequestAsync<RenewResponse>(
                HttpMethod.Post,
                "/auth/renew",
                new()
                {
                    { "uid", uid }
                },
                true
            );
        }

        public static async Task<GreetingsResponse> GreetingsAsync(string uid)
        {
            return await RequestAsync<GreetingsResponse>(
                HttpMethod.Get,
                "/api/v1/greetings",
                new()
                {
                    { "uid", uid }
                },
                true
            );
        }
    }
}
