using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Nuuvify.CommonPack.Middleware.Abstraction;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Fixtures;

public abstract class BaseAppDbContextFixture : IDisposable
{

    protected Mock<IConfigurationCustom> mockIConfigurationCustom;
    protected MapperConfiguration mapperConfiguration;

    public bool PreventDisposal { get; set; }
    public DbContext Db { get; protected set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected abstract void Dispose(bool disposing);

}
