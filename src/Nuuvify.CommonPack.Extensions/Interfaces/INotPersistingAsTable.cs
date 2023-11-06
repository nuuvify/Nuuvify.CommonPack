namespace Nuuvify.CommonPack.Extensions.Interfaces
{
    /// <summary>
    /// Toda classe que implementar essa interface, sera ignorada pelo EntityFramework para ser persistida como uma tabela.
    /// Caso queira persistir alguma classe dessa biblioteca que esteja implementando essa interface, vc deve herdar a classe 
    /// em uma nova classe
    /// Exemplo: 
    ///       MINHACLASSE : Cnpj
    /// </summary>
    public interface INotPersistingAsTable
    {
        
    }


}
