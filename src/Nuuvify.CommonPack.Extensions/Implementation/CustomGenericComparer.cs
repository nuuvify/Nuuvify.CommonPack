namespace Nuuvify.CommonPack.Extensions.Implementation;

/// <summary>
/// Exemplo de uso com Distinct <see cref="DistinctExtension.Distinct"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public class CustomGenericComparer<T> : IEqualityComparer<T>
{
    public Func<T, T, bool> ComparerEquals { get; }
    public Func<T, int> ComparerGetHashCode { get; }
    public CustomGenericComparer(
        Func<T, T, bool> comparerEquals,
        Func<T, int> comparerGetHashCode)
    {
        ComparerEquals = comparerEquals;
        ComparerGetHashCode = comparerGetHashCode;
    }

    public bool Equals(T x, T y)
        => ComparerEquals(x, y);

    public int GetHashCode(T obj)
    {
        _ = ComparerGetHashCode(obj);
        return 0;
    }

}
