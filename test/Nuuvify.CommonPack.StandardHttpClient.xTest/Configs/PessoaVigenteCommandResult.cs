namespace Nuuvify.CommonPack.StandardHttpClient.xTest.Configs;

internal sealed class PessoaVigenteCommandResult : ICommandResultR
{
    public string CODIGO_USUAL { get; set; }
    public string CTP_COD_MATRIZ { get; set; }
    public string CTP_COD_SUBSIDIARIA { get; set; }
    public string CTP_COND_PGTO_F { get; set; }
    public string CTP_DECRETO_ESP { get; set; }
    public DateTime? CTP_DH_FIM_VINCULO { get; set; }
    public DateTime? CTP_DH_VINCULO { get; set; }
    public string CTP_EDI { get; set; }
    public string CTP_IND_VINCULADO { get; set; }
    public string CTP_TP_COMPRADOR { get; set; }
    public string IND_ARREDONDA_VLRS { get; set; }
    public string IND_CLIENTE { get; set; }
    public string IND_CONTABILISTA { get; set; }
    public string IND_ESTABELECIMENTO { get; set; }
    public string IND_FISICA_JURIDICA { get; set; }
    public string IND_FORNECEDOR { get; set; }
    public string IND_NACIONAL_ESTRANGEIRA { get; set; }
    public string IND_NAT_PESSOA_EFD { get; set; }
    public string IND_PONTO_ALFANDEGADO { get; set; }
    public string IND_PRODUTOR { get; set; }
    public string IND_TRANSPORTADOR { get; set; }
    public string MNEMONICO { get; set; }
    public string PFJ_CODIGO { get; set; }
    public string PFJ_CODIGO_ENTREGA { get; set; }
    public string BAIRRO { get; set; }
    public string CAIXA_POSTAL { get; set; }
    public string CCM { get; set; }
    public string CEP { get; set; }
    public string CEP_CP { get; set; }
    public string COMPLEMENTO { get; set; }
    public string E_MAIL { get; set; }
    public string FAX { get; set; }
    public string INSCR_ESTADUAL { get; set; }
    public string INSCR_INSS { get; set; }
    public string INSCR_SUFRAMA { get; set; }
    public string LOC_CODIGO { get; set; }
    public string LOGRADOURO { get; set; }
    public string MUN_CODIGO { get; set; }
    public string MUNICIPIO { get; set; }
    public string NIRE { get; set; }
    public string NUM_REGIME_ISS { get; set; }
    public string NUM_REGIME_NF_ELET { get; set; }
    public string NUMERO { get; set; }
    public string PAIS { get; set; }
    public string PIS { get; set; }
    public string POSTO_FISCAL { get; set; }
    public string TELEFONE1 { get; set; }
    public string TELEFONE2 { get; set; }
    public string TELEX { get; set; }
    public string UNIDADE_FEDERATIVA { get; set; }
    public string WEB_SITE { get; set; }
    public string PAIS_CODIGO { get; set; }
    public string UF_CODIGO { get; set; }
    public string CAEN_CODIGO_PRINCIPAL { get; set; }
    public string CPF_CGC { get; set; }
    public string CRT_CODIGO { get; set; }
    public string IND_BENEF_DISPENSADO_NIF { get; set; }
    public string IND_CONTR_ICMS { get; set; }
    public string IND_CONTR_IPI { get; set; }
    public string IND_REGIME_LUCRO { get; set; }
    public string IND_SIMPLES_NACIONAL { get; set; }
    public string IND_SUBSTITUTO_ICMS { get; set; }
    public string NIF { get; set; }
    public string NOME_FANTASIA { get; set; }
    public string NUM_DEPENDENTES_IR { get; set; }
    public string RAZAO_SOCIAL { get; set; }
    public string TICO_CODIGO { get; set; }
    public string VL_INSS_DEDUCAO { get; set; }
    public string VL_PENSAO { get; set; }

    public DateTime DT_INICIO_PESSOA_VIGENCIA { get; set; }
    public DateTime? DT_FIM_PESSOA_VIGENCIA { get; set; }
    public DateTime DT_INICIO_LOCALIDADE_PESSOA { get; set; }
    public DateTime? DT_FIM_LOCALIDADE_PESSOA { get; set; }
}

internal sealed class RetornoPessoGenericoFornecedores
{
    public IList<RetornoPessoaGenerico> Fornecedores { get; set; }
}
internal sealed class RetornoPessoaGenerico
{
    public string PFJ_CODIGO { get; set; }
    public string RAZAO_SOCIAL { get; set; }
    public DateTime DT_INICIO_PESSOA_VIGENCIA { get; set; }
}

internal sealed class CotacaoDolarDia
{
    public IList<MoedaCotada> Value { get; set; }
}

internal sealed class MoedaCotada
{
    public decimal CotacaoCompra { get; set; }
    public decimal CotacaoVenda { get; set; }
    public DateTime DataHoraCotacao { get; set; }
}
