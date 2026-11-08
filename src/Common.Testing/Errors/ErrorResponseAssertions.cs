using Medusa.Enap.Common.AspNetCore.ErrorHandling;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Utils;

namespace Medusa.Enap.Common.Testing.Errors;

public static class ErrorResponseAssertions
{
    public static Task<MedusaErrorResponse> AssertFieldMissingAsync(this Task<HttpResponseMessage> response, string fieldName)
        => response.Then(r => r
            .AssertBadRequestAsync(CommonErrorCodes.FieldMissing)
            .AndErrorDataAsync("Field", fieldName)
        );
}
