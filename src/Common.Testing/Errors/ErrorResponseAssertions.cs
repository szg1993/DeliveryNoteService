using Common.AspNetCore.ErrorHandling;
using Common.Core.Exceptions;
using Common.Core.Utils;

namespace Common.Testing.Errors;

public static class ErrorResponseAssertions
{
    public static Task<MedusaErrorResponse> AssertFieldMissingAsync(this Task<HttpResponseMessage> response, string fieldName)
        => response.Then(r => r
            .AssertBadRequestAsync(CommonErrorCodes.FieldMissing)
            .AndErrorDataAsync("Field", fieldName)
        );
}