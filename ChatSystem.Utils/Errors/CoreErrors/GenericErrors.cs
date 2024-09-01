using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ChatSystem.Utils.Errors.CoreErrors
{
    public class GenericErrors
    {
        public static readonly CustomError IntegrationError = new CustomError(HttpStatusCode.InternalServerError, "something_went_wrong", "Something went wrong");
        public static readonly CustomError ThirdPartyFailure = new CustomError(HttpStatusCode.InternalServerError, "third_party_failure", "Third Party Failure");
        public static readonly CustomError NotFound = new CustomError(HttpStatusCode.NotFound, "not_found", "Not found");

    }
}
