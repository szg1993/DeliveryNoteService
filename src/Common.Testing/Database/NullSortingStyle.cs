namespace Common.Testing.Database;

public enum NullSortingStyle
{
    /// <summary>
    /// In ascending order, null values are sorted before non-null values
    /// </summary>
    NullFirst,
    /// <summary>
    /// In ascending order, null values are sorted after all non-null values
    /// </summary>
    NullLast
}