using ChatSystem.Utils.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Utils.Exceptions
{
    public class AppException : Exception
    {
        public override string Message { get; }
        public CustomError Error { get; set; }
        public AppException(CustomError error) : base(null, null)
        {
            Message = error.Message;
            Error = error;
        }

        public AppException(CustomError error, Exception inner) : base(null, inner)
        {
            Message = error.Message;
            Error = error;
        }
    }
}
