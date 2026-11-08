using Medusa.Enap.Common.Core.Utils;

namespace Medusa.Enap.Common.Testing.TestSuite;

public static class TestDateStrings
{
    public const string Jan1 = "2024-01-01";
    public const string Jan2 = "2024-01-02";
    public const string Jan9 = "2024-01-09";
    public const string Jan10 = "2024-01-10";
    public const string Jan15 = "2024-01-15";
    public const string Jan31 = "2024-01-31";
    public const string Feb1 = "2024-02-01";
    public const string Feb2 = "2024-02-02";
    public const string Feb28 = "2024-02-28";
    public const string Feb29 = "2024-02-29";
    public const string Mar1 = "2024-03-01";
    public const string Mar3 = "2024-03-03";
    public const string Mar15 = "2024-03-15";
    public const string Mar31 = "2024-03-31";
    public const string Apr1 = "2024-04-01";
    public const string Apr30 = "2024-04-30";
    public const string May1 = "2024-05-01";
    public const string Dec31 = "2024-12-31";

    public const string Jan1In1990 = "1990-01-01";
    public const string Feb1In1990 = "1990-02-01";
}

public static class TestDates
{
    public static DateOnly Jan1 => TestDateStrings.Jan1.ParseDateOnly();
    public static DateOnly Jan2 => TestDateStrings.Jan2.ParseDateOnly();
    public static DateOnly Jan9 => TestDateStrings.Jan9.ParseDateOnly();
    public static DateOnly Jan10 => TestDateStrings.Jan10.ParseDateOnly();
    public static DateOnly Jan15 => TestDateStrings.Jan15.ParseDateOnly();
    public static DateOnly Jan31 => TestDateStrings.Jan31.ParseDateOnly();
    public static DateOnly Feb1 => TestDateStrings.Feb1.ParseDateOnly();
    public static DateOnly Feb2 => TestDateStrings.Feb2.ParseDateOnly();
    public static DateOnly Feb28 => TestDateStrings.Feb28.ParseDateOnly();
    public static DateOnly Feb29 => TestDateStrings.Feb29.ParseDateOnly();
    public static DateOnly Mar1 => TestDateStrings.Mar1.ParseDateOnly();
    public static DateOnly Mar3 => TestDateStrings.Mar3.ParseDateOnly();
    public static DateOnly Mar15 => TestDateStrings.Mar15.ParseDateOnly();
    public static DateOnly Mar31 => TestDateStrings.Mar31.ParseDateOnly();
    public static DateOnly Apr1 => TestDateStrings.Apr1.ParseDateOnly();
    public static DateOnly Apr30 => TestDateStrings.Apr30.ParseDateOnly();
    public static DateOnly May1 => TestDateStrings.May1.ParseDateOnly();
    public static DateOnly Dec31 => TestDateStrings.Dec31.ParseDateOnly();

    public static DateOnly Jan1In1990 => TestDateStrings.Jan1In1990.ParseDateOnly();
    public static DateOnly Feb1In1990 => TestDateStrings.Feb1In1990.ParseDateOnly();
}
