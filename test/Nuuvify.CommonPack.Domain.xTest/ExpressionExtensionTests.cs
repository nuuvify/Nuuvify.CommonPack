using System.Globalization;
using System.Linq.Expressions;
using Nuuvify.CommonPack.Extensions;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest;

[Trait("Category", "Unit")]
public class ExpressionExtensionTests
{

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ExpressionExtension))]
    public void CombineExpressions_ComFiltro_DeveRetornarListaFiltrada()
    {

        var customers = new List<Customer>()
            {
                new() { Id = "AA1", Nome = "Fritz", Codigo = 3, Tipo = "A" },
                new() { Id = "XX1", Nome = "Giropopis", Codigo = 1, Tipo = "A" },
                new() { Id = "XA2", Nome = "Stradivarius", Codigo = 2, Tipo = "B" },
                new() { Id = "XB1", Nome = "Giropopis", Codigo = 1, Tipo = "C" },
                new() { Id = "DDD", Nome = "Fritz", Codigo = 3, Tipo = "D" },
                new() { Id = "XC3", Nome = "Fulano", Codigo = 1, Tipo = "E" },
            };

        Tipos = new string[] { "A", "E" };
        Codigos = new int[] { 1, 2 };

        var query = customers.AsQueryable()
            .Where(GetFilter())
            .OrderByDescending(p => p.Tipo)
            .ThenBy(p => p.Nome);

        var result = query.ToList();

        Assert.Equal(expected: 2, result.Count);
        Assert.Equal(expected: 1, result.Count(p => p.Tipo == "A"));
        Assert.Equal(expected: 1, result.Count(p => p.Tipo == "E"));

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(CacheTimeServiceExtension))]
    public void CacheTimeService_ComHoraComoString_DeveRetornarTimeSpanAteHoraEspecifica()
    {
        // Arrange
        const string timeString = "23:59:59";
        var now = DateTimeOffset.Now;
        var targetTime = DateTimeOffset.Parse($"{now:yyyy-MM-dd} {timeString}", CultureInfo.InvariantCulture);

        // Se a hora já passou hoje, será para amanhã
        if (targetTime <= now)
        {
            targetTime = targetTime.AddDays(1);
        }

        // Act
        var result = CacheTimeServiceExtension.ExpireAt(timeString);

        // Assert
        var expectedTimeSpan = targetTime - now;

        // Tolerância de 1 segundo para diferenças de execução
        var tolerance = TimeSpan.FromSeconds(1);
        var difference = Math.Abs((result - expectedTimeSpan).TotalSeconds);

        Assert.True(difference <= tolerance.TotalSeconds,
            $"Esperado aproximadamente {expectedTimeSpan}, mas obteve {result}. Diferença: {difference} segundos");

        // Verifica se o resultado é positivo (sempre no futuro)
        Assert.True(result > TimeSpan.Zero, "O TimeSpan retornado deve ser positivo (no futuro)");
    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(CacheTimeServiceExtension))]
    public void CacheTimeService_ComHoraPassada_DeveRetornarProximoDia()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        var pastTime = now.AddHours(-1).TimeOfDay;

        // Perto da meia-noite, now - 1h pode cair no dia anterior (ex.: 23:30).
        // Ajusta para garantir um horário que realmente já passou no dia atual.
        if (pastTime > now.TimeOfDay)
        {
            pastTime = now.TimeOfDay.Subtract(TimeSpan.FromMinutes(1));
        }

        var timeString = pastTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

        var expectedTargetTime = DateTimeOffset.Parse($"{now:yyyy-MM-dd} {timeString}", CultureInfo.InvariantCulture);
        if (expectedTargetTime <= now)
        {
            expectedTargetTime = expectedTargetTime.AddDays(1);
        }

        // Act
        var result = CacheTimeServiceExtension.ExpireAt(timeString);

        // Assert
        var expectedTimeSpan = expectedTargetTime - now;

        // Tolerância de 1 segundo para diferenças de execução
        var tolerance = TimeSpan.FromSeconds(1);
        var difference = Math.Abs((result - expectedTimeSpan).TotalSeconds);

        Assert.True(difference <= tolerance.TotalSeconds,
            $"Esperado aproximadamente {expectedTimeSpan}, mas obteve {result}. Diferença: {difference} segundos");

        // Verifica se o resultado é para o próximo dia (mais de 22 horas no futuro)
        Assert.True(result.TotalHours > 22,
            $"Para uma hora que já passou, deve retornar um TimeSpan para o próximo dia. Obteve: {result.TotalHours} horas");
    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(CacheTimeServiceExtension))]
    public void CacheTimeService_FormatacaoTimeSpan_DeveRetornarFormatoCorreto()
    {
        // Arrange
        const string timeString = "14:30:45";

        // Act
        var timeSpanResult = CacheTimeServiceExtension.ExpireAt(timeString);
        var formattedResult = timeSpanResult.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

        // Assert
        Assert.NotNull(formattedResult);
        Assert.Matches(@"^\d{2}:\d{2}:\d{2}$", formattedResult); // Formato HH:mm:ss

        // Verifica se o TimeSpan é válido e positivo
        Assert.True(timeSpanResult > TimeSpan.Zero, "O TimeSpan deve ser positivo");
        Assert.True(timeSpanResult <= TimeSpan.FromDays(1), "O TimeSpan não deve exceder 24 horas");
    }

    public string Id { get; set; }
    public int[] Codigos { get; set; }
    public string Nome { get; set; }
    public string[] Tipos { get; set; }

    public Expression<Func<Customer, bool>> GetFilter()
    {
        Expression<Func<Customer, bool>> filter = p => true;

        filter = p => p.Id != null;

        if (Codigos.NotNullOrZero())
        {
            filter = filter.CombineExpressions<Customer>(
                p => Codigos.Contains(p.Codigo));
        }

        if (Tipos.NotNullOrZero())
        {
            filter = filter.CombineExpressions<Customer>(
                p => Tipos.Contains(p.Tipo));
        }

        return filter;
    }

}



