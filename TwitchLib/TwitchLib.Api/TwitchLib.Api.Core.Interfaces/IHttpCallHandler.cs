﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api.Core.Enums;

namespace TwitchLib.Api.Core.Interfaces
{
    public interface IHttpCallHandler
    {
        Task<KeyValuePair<int, string>> GeneralRequest(string url, string method, string payload = null, ApiVersion api = ApiVersion.V5, string clientId = null, string accessToken = null, bool refreshedToken = false);
        Task PutBytes(string url, byte[] payload);
        int RequestReturnResponseCode(string url, string method, List<KeyValuePair<string, string>> getParams = null);
    }
}
