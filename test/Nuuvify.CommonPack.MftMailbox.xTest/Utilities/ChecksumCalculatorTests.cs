using System.Text;
using Nuuvify.CommonPack.MftMailbox.Utilities;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Utilities;

[Trait("Category", "Unit")]
public sealed class ChecksumCalculatorTests
{
    [Fact]
    public async Task Sha256Async_ComConteudoConhecido_DeveRetornarHashEsperado()
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("abc"));

        var hash = await ChecksumCalculator.Sha256Async(stream, CancellationToken.None);

        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }

    [Fact]
    public async Task Sha256Async_ComStreamSeekable_DeveReposicionarNoInicio()
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("conteudo"));
        stream.Position = 3;

        _ = await ChecksumCalculator.Sha256Async(stream, CancellationToken.None);

        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public async Task Sha256Async_ComStreamNaoSeekable_DeveCalcularHashSemFalhar()
    {
        await using var inner = new MemoryStream(Encoding.UTF8.GetBytes("abc"));
        await using var stream = new NonSeekableReadStream(inner);

        var hash = await ChecksumCalculator.Sha256Async(stream, CancellationToken.None);

        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }

    private sealed class NonSeekableReadStream : Stream
    {
        private readonly Stream _inner;

        public NonSeekableReadStream(Stream inner)
        {
            _inner = inner;
        }

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => _inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            _inner.ReadAsync(buffer, cancellationToken);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _inner.ReadAsync(buffer, offset, count, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await _inner.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
