
namespace Nuuvify.CommonPack.Security.JwtCredentials;

public class JwksOptions
{
    public Algorithm Algorithm { get; set; } = Algorithm.ES256;

    /// <summary>
    /// Dias em que a chave privada para geracao do token ira expirar
    /// </summary>
    /// <value></value>
    public int DaysUntilExpire { get; set; } = 90;
    public string KeyPrefix { get; set; } = $"{Environment.MachineName}_";
    public int AlgorithmsToKeep { get; set; } = 2;
    public TimeSpan CacheTime { get; set; } = TimeSpan.FromMinutes(15);



    public string Subject { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string JtiGenerator { get; set; } = Guid.NewGuid().ToString();
    public DateTime ValidatedAt { get; set; }


    /// <summary>
    /// Data da Criacao do token.
    /// Não expirar antes de 
    /// </summary>
    public DateTimeOffset NotBefore { get; set; } = DateTimeOffset.Now;

    private TimeSpan _validFor;
    /// <summary>
    /// Valido por n minutos
    /// </summary>
    /// <summary>
    /// Valido por n horas
    /// </summary>
    public TimeSpan ValidFor
    {
        get
        {
            return _validFor;
        }
        set
        {
            if (value.TotalSeconds <= 0)
                value = new TimeSpan(ValidatedAt.Hour, ValidatedAt.Minute, ValidatedAt.Second);

            if (value.TotalSeconds <= 0)
            {
                value = TimeSpan.FromHours(8);
            }
            _validFor = value;
        }
    }


    /// <summary>
    /// Data/Hora de expiracao, NotBefore + ValidFor
    /// </summary>
    public DateTimeOffset Expiration
    {
        get
        {
            var expiration = NotBefore.Add(ValidFor);
            return expiration;
        }

    }



}
