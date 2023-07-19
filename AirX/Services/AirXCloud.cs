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
    /// AirXCloud类，用于：方便的和云端进行交互，提供登录、续期等功能
    /// 未来网盘的交互也在这里
    public static class AirXCloud
    {
        /// base url，指定了服务器地址，之后的每个请求的URL都基于base进行拼接
        /// 例如 `https://airx.eggtartc.com/auth/token` 就是登录的URL
        private const string BaseURL = "https://airx.eggtartc.com";

        public class UnauthorizedException : Exception { }
        public class IncorrectCredentialTypeException : Exception { }

        /// <summary>
        /// 把Dictionary转换成URL参数的形式（形如：a=1&b=2）
        /// </summary>
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

        /// <summary>
        /// Post请求的封装
        /// </summary>
        /// <typeparam name="T">泛型 T：代表任意一个C#的类，这里用于`JSON数据 -> C#类`的转换</typeparam>
        /// <param name="needToken">是否需要在请求中附上token以验证身份</param>
        /// <returns></returns>
        /// <exception cref="IncorrectCredentialTypeException">token无效的错误</exception>
        /// <exception cref="UnauthorizedException">401错误，也是token无效错误的一种</exception>
        /// <exception cref="HttpRequestException">网络异常的错误</exception>
        private static async Task<T> RequestAsync<T>(
            HttpMethod method,
            string path, /** 请求路径，如 `/auth/token` */
            Dictionary<string, dynamic> body,
            bool needToken
        )
        {
            var client = new HttpClient();

            /// 准备HTTP Post Body
            HttpRequestMessage request = null;
            if (method == HttpMethod.Post)
            {
                request = new HttpRequestMessage(method, BaseURL + path);  /** 这里实际进行了 URL拼接 */
                var jsonBody = JsonConvert.SerializeObject(body);
                var content = new StringContent(jsonBody, null, "application/json");
                request.Content = content;
            }
            else
            {
                request = new HttpRequestMessage(method, BaseURL + path + "?" + GetParametersFromDictionary(body));
            }

            /// HTTP请求头(Header)是重要的概念，参见 https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers
            if (needToken)
            {
                // Is airx token?
                /// 如果需要授权，那就把token放进header里面
                /// 这里先判断，我们存着的是不是AirX的token？还是谷歌的
                if (SettingsUtil.ReadCredentialType() != CredentialType.AirXToken)
                {
                    throw new IncorrectCredentialTypeException();
                }

                /// 正式附加token信息（格式：`Bearer <token>`)
                /// 其中`Bearer`是固定搭配
                var credential = SettingsUtil.String(Keys.SavedCredential, "");
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

                        /** DeserializeObject顾名思义用于把JSON给解析成C#对象 */
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
            public bool success;     /** 登录是否成功 */
            public string message;   /** 一段文本，如果登录成功，这段文本就是success，
                                         如果登录失败，这段文本将会是失败原因 */
            public string name;      /** 登录成功则为用户昵称，登录失败则为`nil` */
            public string token;     /** 成功则为token，参见文档token篇。失败为`nil` */
        }

        public class RenewResponse
        {

            public bool success;     /** 续期是否成功 */
            public string message;   /** 成功则为success，失败则为失败原因 */
            public string token;     /** 成功`token`失败`nil` */
        }

        public class GreetingsResponse
        {
            public bool success;            /** Token是否有效 */
            public string message;          /** 欢迎信息 */
            public string name;             /** 用户昵称 */
            public int uid;                 /** 用户UID */
            public DateTime validBefore;    /** 用户账号有效期至 */
        }

        /// 登录获得token的操作
        public static async Task<LoginResponse> LoginAsync(string uid, string password, string salt = "114514")
        {
            /// 进行2层的哈希
            password = HashUtil.Sha256(HashUtil.Sha256(password));

            // 正式进行post请求！
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

        /// Token的续期操作
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
