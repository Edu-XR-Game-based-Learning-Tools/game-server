using Core.Business;
using System;

namespace Core.Framework
{
    public class GenericServices : IGenericServices
    {
        public ClientVerificationData VerifyClient(string clientVersion)
        {
            throw new NotImplementedException();
        }

        public DateTime GetServerTime()
        {
            throw new NotImplementedException();
        }
    }
}