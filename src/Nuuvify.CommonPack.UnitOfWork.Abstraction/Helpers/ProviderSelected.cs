using System.Text;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction;

public static class ProviderSelected
{

    public static readonly string Oracle = "Oracle";
    public static readonly string Db2 = "Db2";
    public static readonly string SqlServer = "SqlServer";
    public static readonly string PostgreSQL = "PostgreSQL";
    public static readonly string SqLite = "SqLite";
    public static readonly string[] SuportedProviders = { Oracle, Db2, SqlServer, PostgreSQL, SqLite };

    /// <summary>
    /// Retorna uma string, separado por virgula, com todos os prividers suportados pelos metodos de extenção
    /// </summary>
    /// <returns></returns>
    public static string GetSuportedProviders()
    {
        var message = new StringBuilder();

        foreach (var item in SuportedProviders)
        {
            _ = message.Append($"{item},");
        }

        return message.ToString();
    }

    public static bool IsProviderOracle()
    {
        return ProviderName.Contains(Oracle);
    }
    public static bool IsProviderDb2()
    {
        return ProviderName.Contains(Db2) ||
            ProviderName.Contains("IBM");
    }
    public static bool IsProviderSqlServer()
    {
        return ProviderName.Contains(SqlServer);
    }
    public static bool IsProviderPostgreSQL()
    {
        return ProviderName.Contains(PostgreSQL);
    }
    public static bool IsProviderSqLite()
    {
        return ProviderName.Contains(SqLite);
    }

    /// <summary>
    /// Suported: <see cref="ProviderSelected.SuportedProviders" />
    /// </summary>
    public static string ProviderName { get; set; }

}

