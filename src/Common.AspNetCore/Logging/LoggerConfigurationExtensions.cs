using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Compact;

namespace Common.AspNetCore.Logging;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration WriteToSeq(this LoggerConfiguration c, IConfigurationSection? config)
    {
        var seqUrl = config?["SeqUrl"];
        return string.IsNullOrEmpty(seqUrl)
            ? c
            : c.WriteTo.Seq(seqUrl, payloadFormatter: new RenderedCompactJsonFormatter());
    }
}