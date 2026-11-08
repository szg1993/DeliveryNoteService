using Medusa.Enap.Common.AspNetCore;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Utils;

namespace Medusa.Enap.Common.Testing;

public static class I18nTester
{
    public static void AssertAllErrorCodesArePresent(string filePath, params Type[] errorCodeTypes)
    {
        var errorCodes = new List<string>();

        foreach (var errorCodeType in errorCodeTypes)
            errorCodes.AddRange(ErrorCodeCollector.Run(errorCodeType));

        errorCodes.AddRange(ErrorCodeCollector.Run(typeof(CommonErrorCodes)));
        errorCodes.AddRange(ErrorCodeCollector.Run(typeof(CommonWebErrorCodes)));

        string json = File.ReadAllText(filePath);

        var i18n = EnapSerializer.FromJson<Dictionary<string, string>>(json) ?? [];
        var missingErrorCodes = new List<string>();

        foreach (var errorCode in errorCodes)
        {
            if (!i18n.ContainsKey(errorCode))
                missingErrorCodes.Add(errorCode);
        }

        if (missingErrorCodes.Count > 0)
            throw new Exception($"Missing error codes from {filePath}: {string.Join("; ", missingErrorCodes)}");
    }
}