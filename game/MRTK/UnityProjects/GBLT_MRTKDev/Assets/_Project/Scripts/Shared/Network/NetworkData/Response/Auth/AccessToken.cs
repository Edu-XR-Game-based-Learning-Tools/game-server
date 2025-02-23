﻿using MessagePack;
using System;

namespace Shared.Network
{
    [Serializable]
    [MessagePackObject(true)]
    public class AccessToken
    {
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}
