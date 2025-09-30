using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

/// <summary>
/// Handler stub melhorado para testes com melhor tratamento de disposal e threading
/// </summary>
public class ImprovedDelegatingHandlerStub : DelegatingHandler
{
    private readonly HttpResponseMessage _fakeResponse;
    private readonly bool _disposeResponse;

    public ImprovedDelegatingHandlerStub(HttpResponseMessage responseMessage, bool disposeResponse = false)
    {
        _fakeResponse = responseMessage ?? throw new ArgumentNullException(nameof(responseMessage));
        _disposeResponse = disposeResponse;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Simular um pequeno delay para ser mais realista (opcional)
        await Task.Delay(1, cancellationToken);

        // Verificar se foi cancelado
        cancellationToken.ThrowIfCancellationRequested();

        // Clonar a resposta para evitar problemas de disposal múltiplo
        var clonedResponse = new HttpResponseMessage(_fakeResponse.StatusCode)
        {
            Content = _fakeResponse.Content,
            ReasonPhrase = _fakeResponse.ReasonPhrase,
            Version = _fakeResponse.Version
        };

        // Copiar headers
        foreach (var header in _fakeResponse.Headers)
        {
            clonedResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clonedResponse;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _disposeResponse)
        {
            _fakeResponse?.Dispose();
        }
        base.Dispose(disposing);
    }
}
