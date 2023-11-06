using System.ComponentModel;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
    /// <summary>
    /// BLOCO = Para Calcular de Diversos Lançamentos (acumulando base e valores) sem Gravar
    /// SEMACENTROS = Retirar Acentos das Mensagens de Retorno
    /// TRACE = Habilita o Log de Execução das Procedures
    /// LIMPAR = Apaga Configurações, Resultados e Lançamentos Calculados do Usuário ou Sistema Informado
    ///         (Quando não Informado Usuário e Sistema serão Excluídos de Todos Usuários e Sistemas)
    /// SEMCOMMIT = Desabilita a Confirmação Automática de Transações
    /// SEMCONTROLE = Desabilita o Controle Multiusuário de Cálculo/Gravação
    /// </summary>
    public enum FuncaoOpcao
    {
        [Description(nameof(BLOCO))]
        BLOCO = 1,
        [Description(nameof(SEMACENTOS))]
        SEMACENTOS = 2,
        [Description(nameof(TRACE))]
        TRACE = 3,
        [Description(nameof(LIMPAR))]
        LIMPAR = 4,
        [Description(nameof(SEMCOMMIT))]
        SEMCOMMIT = 5,
        [Description(nameof(SEMCONTROLE))]
        SEMCONTROLE = 6,

    }
}
