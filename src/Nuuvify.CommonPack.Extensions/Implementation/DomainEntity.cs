using System;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain
{
    /// <summary>
    /// Caso sua classe represente uma AggregateRoot, você devera herdar de <br/>
    /// AggregateRoot. Use esta classe para os agregados do root apenas <br/>
    /// As propriedades de auditoria, não podem ser alteradas, elas são populadas <br/>
    /// pelo SaveChanges no momento da persistencia. <br/>
    /// Use override nas propriedades de auditoria, com o attributo [JsonIgnore] caso queira
    /// que essas propriedades sempre sejam ignoradas em uma serialização.
    /// </summary>
    public abstract class DomainEntity : NotifiableR
    {

        protected DomainEntity()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; protected set; }
        public virtual DateTimeOffset DataCadastro { get; protected set; }
        public virtual string UsuarioCadastro { get; protected set; }
        public virtual DateTimeOffset? DataAlteracao { get; protected set; }
        public virtual string UsuarioAlteracao { get; protected set; }

        public virtual string UsuarioIdCadastro { get; protected set; }
        public virtual string UsuarioIdAlteracao { get; protected set; }


        protected void DefinirId(string Id)
        {
            if (!string.IsNullOrWhiteSpace(Id))
                this.Id = Id;
        }


        public override bool Equals(object obj)
        {
            return obj is DomainEntity domainEntity &&
                Id == domainEntity.Id;
        }

        public override int GetHashCode()
        {
            return (GetType().GetHashCode() * 666) + Id.GetHashCode();
        }

        public override string ToString()
        {
            return GetType().Name + "[Id = " + Id + "]";
        }


        public const int MaxId = 36;
        public const int MaxUsuarioCadastro = 100;
        public const int MaxUsuarioAlteracao = 100;
        public const int MaxUsuarioIdCadastro = 40;
        public const int MaxUsuarioIdAlteracao = 40;

    }
}
