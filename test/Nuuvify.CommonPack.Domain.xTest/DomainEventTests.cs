using System;
using Nuuvify.CommonPack.Domain.xTest.Entities;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest
{
    public class DomainEventTests
    {

        [Fact]
        [Trait("CommonPack.Domain", nameof(DomainEvent<string>))]
        public void DomainEvent_SourceComoClasse_DevePopularSourceId()
        {
            const string VersionEvent = "1";
            var cliente = new Cliente(987654, "Lincoln");

            var clienteEvent = new ClienteMyRoleEvent(cliente, VersionEvent, cliente);


            Assert.NotNull(clienteEvent.SourceId);
            Assert.Equal(VersionEvent, clienteEvent.Version);
            Assert.Equal(0, clienteEvent.Notifications.Count);
        }

        [Fact]
        [Trait("CommonPack.Domain", nameof(DomainEvent<string>))]
        public void DomainEvent_ComPropriedadesValidas_DevePopularSourceId()
        {
            const string VersionEvent = "Teste_837f3a9d-6685-486b-881a-a5f74e032d72";
            var cliente = new Cliente(987654, "Lincoln");

            var clienteAddedEvent = new ClienteAddedEvent(cliente, VersionEvent)
            {
                InformacaoQueEuQuero = "Teste"
            };


            Assert.NotNull(clienteAddedEvent.SourceId);
            Assert.Equal(VersionEvent, clienteAddedEvent.Version);
            Assert.Equal(0, clienteAddedEvent.Notifications.Count);

        }

        [Fact]
        [Trait("CommonPack.Domain", nameof(DomainEvent<string>))]
        public void DomainEvent_ComPropriedadesInvalidas_DeveRetornarNotificacoes()
        {
            const string VersionEvent = "Teste_12345";
            Cliente cliente = null;

            var clienteAddedEvent = new ClienteAddedEvent(cliente, VersionEvent)
            {
                InformacaoQueEuQuero = "Teste"
            };


            Assert.Null(clienteAddedEvent.SourceId);
            Assert.NotEqual(VersionEvent, clienteAddedEvent.Version);
            Assert.Equal(1, clienteAddedEvent.Notifications.Count);

        }

        [Fact]
        [Trait("CommonPack.Domain", nameof(DomainEvent<string>))]
        public void DomainEvent_ComPropriedadeNoConstrutorNullo_DeveRetornarException()
        {
            const string VersionEvent = "v12345";
            Cliente cliente = null;

            var exception = Assert.Throws<NullReferenceException>(() => 
                new ClienteUpdatedEvent(cliente, VersionEvent)
                {
                    InformacaoQueEuQuero = "Teste"
                }
            );


            Assert.Equal("Object reference not set to an instance of an object.", exception.Message);
        }

    }
}