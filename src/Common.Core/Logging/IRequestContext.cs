namespace Common.Core.Logging;

public interface IRequestContext
{
    LogParams ToLogParams();
}