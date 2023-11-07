using System;
using AutoMapper;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Fixtures
{
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
}