using Nuuvify.CommonPack.Domain.Implementations;
using Nuuvify.CommonPack.Domain.Interfaces;
using Nuuvify.CommonPack.Extensions;
using Nuuvify.CommonPack.MediatoR.Implementation;

namespace Nuuvify.CommonPack.Domain;

public abstract class BaseSpecification<TEntity> : NotifiableR,
    ISpecification<TEntity> where TEntity : DomainEntity
{

    /// <summary>
    /// Apenas um repositorio deve ser utilizado, se estiver precisando mais de um
    /// muito provavelmente você precisa quebrar sua especificação, ou melhorar o
    /// repositorio
    /// </summary>
    /// <param name="repository"></param>
    protected BaseSpecification(IRepositoryValidation repository)
    {
    }

    public abstract Task IsSatisfactory(TEntity entity);

    public IList<NotificationR> ValidationResult()
    {
        return Notifications.ToList();
    }
}
