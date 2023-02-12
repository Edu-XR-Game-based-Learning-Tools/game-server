using System;

namespace Core.Utility
{
    public class InvalidSessionException : Exception
    {
        public InvalidSessionException(string mess) : base(mess)
        {
        }
    }
}