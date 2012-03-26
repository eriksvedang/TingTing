using System;

namespace TingTing
{
    public class TingTingException : Exception
    {
        public TingTingException(string pMessage) : base(pMessage)
        {
        }
    }
 
    public class CantFindTingException : Exception
    {
        public CantFindTingException(string pMessage) : base(pMessage)
        {
        }
    }

    public class TingDuplicateException : Exception
    {
        public TingDuplicateException(string pMessage, Exception pInnerException) : base(pMessage, pInnerException)
        {
        }

        public TingDuplicateException(string pMessage) : base(pMessage)
        {
        }
    }
}

