using System;

namespace Core.Utility
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string mess) : base(mess)
        {
        }
    }

    public class UserIsNull : Exception
    {
        public UserIsNull() : base()
        {
        }
    }
}