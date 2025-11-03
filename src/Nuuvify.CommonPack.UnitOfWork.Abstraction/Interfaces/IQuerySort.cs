namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

public interface IQuerySort : IQueryableCustom
{
    string Sort { get; set; }
}
