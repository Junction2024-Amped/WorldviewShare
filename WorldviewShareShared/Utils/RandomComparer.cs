namespace WorldviewShareShared.Utils;

public class RandomComparer<T> : IComparer<T>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Random Random = new();
    public int Compare(T? x, T? y)
    {
        return Random.Next(-1, 2);
    }
}