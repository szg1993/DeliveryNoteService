using Medusa.Enap.Common.Core.Logging;
using Medusa.Enap.Common.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;

namespace Medusa.Enap.Common.Testing.Logging;

public class NewEntityLoggerMock
{
    private readonly Mock<NewEntityLogger> _mock;

    public NewEntityLogger Logger => _mock.Object;

    public NewEntityLoggerMock()
    {
        var lf = new LoggerFactory()
           .AddSerilog(new TestSerilogLogger((e) => { }));
        var loggerMock = new Mock<MedusaLogger<NewEntityLogger>>(new Logger<NewEntityLogger>(lf));
        _mock = new Mock<NewEntityLogger>(loggerMock.Object);
    }
}
