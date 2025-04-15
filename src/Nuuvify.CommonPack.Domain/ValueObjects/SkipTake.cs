namespace Nuuvify.CommonPack.Domain.ValueObjects;

public class SkipTake
{

    private int _skip;
    private int _take;

    /// <summary>
    /// Registros para serem desconsiderados (saltados)
    /// </summary>
    /// <example>0</example>
    public virtual int Skip
    {
        get
        {
            return _skip;
        }
        set
        {
            if (value < 0)
            {
                _skip = 0;
            }
            else
            {
                _skip = value;
            }
        }
    }

    /// <summary>
    /// Registros para serem exibidos
    /// </summary>
    /// <example>25</example>
    public virtual int Take
    {
        get
        {
            return _take;
        }
        set
        {
            if (value < 0)
            {
                _take = 0;
            }
            else
            {
                _take = value;
            }
        }
    }

    /// <summary>
    /// Retorna true caso o Take for maior que 0 <br/>
    /// Quando take = 0 os metodos que usam essa classe irão ignorar paginação
    /// </summary>
    /// <returns></returns>
    public virtual bool HasPagination()
    {
        return Take > 0;
    }

    public virtual void SetSkipTakeZero()
    {
        Skip = 0;
        Take = 0;
    }

    /// <summary>
    /// Retorna 25 caso o Take for menor ou igual a 0, caso contrario retorna o valor da propriedade Take
    /// </summary>
    /// <example>25</example>
    public virtual int MinTake()
    {
        return Take <= 0 ? 25 : Take;
    }
}
