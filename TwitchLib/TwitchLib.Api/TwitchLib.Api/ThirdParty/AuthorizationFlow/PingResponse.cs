﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TwitchLib.Api.Core.Enums;

namespace TwitchLib.Api.ThirdParty.AuthorizationFlow
{
    public class PingResponse
    {
        public bool Success { get; protected set; }
        public string Id { get; protected set; }

        public int Error { get; protected set; }
        public string Message { get; protected set; }

        public List<AuthScopes> Scopes { get; protected set; }
        public string Token { get; protected set; }
        public string Refresh { get; protected set; }
        public string Username { get; protected set; }
        public string ClientId { get; protected set; }

        public PingResponse(string jsonStr)
        {
            var json = JObject.Parse(jsonStr);
            Success = bool.Parse(json.SelectToken("success").ToString());
            if(!Success)
            {
                Error = int.Parse(json.SelectToken("error").ToString());
                Message = json.SelectToken("message").ToString();
            } else
            {
                Scopes = new List<AuthScopes>();
                foreach (var scope in json.SelectToken("scopes"))
                    Scopes.Add(Core.Common.Helpers.StringToAuthScopes(scope.ToString()));
                Token = json.SelectToken("token").ToString();
                Refresh = json.SelectToken("refresh").ToString();
                Username = json.SelectToken("username").ToString();
                ClientId = json.SelectToken("client_id").ToString();
            }
        }
    }
}
