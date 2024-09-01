using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatSystem.Utils.Errors
{
    public class ErrorDetails : Error
    {
        public bool Succeeded { get; set; }
        public List<Error> Errors { get; set; }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class Error
    {
        public string ErrorMessage { get; set; }
        public string ErrorExceptionMessage { get; set; }
        public string ErrorDetails { get; set; }
        public string? InnerException { get; set; }
        public string? StackTrace { get; set; }
    }
}
