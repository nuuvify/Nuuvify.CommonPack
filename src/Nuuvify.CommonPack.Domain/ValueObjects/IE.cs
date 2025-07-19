
using System;
using System.Globalization;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{

    /// <summary>
    /// Valida Inscrição Estadual por estado
    /// </summary>
    public class IE : NotifiableR
    {

        
        protected IE() { }

        public IE(string ie, string uf)
        {

            Validar(ie, uf);

        }


        public string Codigo { get; private set; }
        public string UF { get; private set; }

        public override string ToString()
        {
            return Codigo?.ToString();
        }


        private void Validar(string ie, string uf)
        {

            ie = ie?.Trim().Replace(".", "").Replace("-", "").Replace("/", "");
            ie = ie?.ToUpper();

            uf = uf?.Trim().ToUpper();

            new ValidationConcernR<IE>(this)
                .AssertNotIsNullOrWhiteSpace(x => ie, ie)
                .AssertHasMaxLength(x => ie, maxIE)
                .AssertFixedLength(x => uf, maxUF);


            var valido = IsValid();

            if (!valido)
            {
                Codigo = null;
                UF = null;
                return;
            }


            if (ie.Equals("ISENTO"))
            {
                Codigo = ie;
                UF = uf;
                return;
            }



            valido = uf switch
            {
                "AC" => ValidaAC(ie),
                "AL" => ValidaAL(ie),
                "AM" => ValidaAM(ie),
                "AP" => ValidaAP(ie),
                "BA" => ValidaBA(ie),
                "CE" => ValidaCE(ie),
                "DF" => ValidaDF(ie),
                "ES" => ValidaES(ie),
                "GO" => ValidaGO(ie),
                "MA" => ValidaMA(ie),
                "MT" => ValidaMT(ie),
                "MS" => ValidaMS(ie),
                "MG" => ValidaMG(ie),
                "PA" => ValidaPA(ie),
                "PB" => ValidaPB(ie),
                "PE" => ValidaPE(ie),
                "PI" => ValidaPI(ie),
                "PR" => ValidaPR(ie),
                "RJ" => ValidaRJ(ie),
                "RN" => ValidaRN(ie),
                "RO" => ValidaRO(ie),
                "RR" => ValidaRR(ie),
                "RS" => ValidaRS(ie),
                "SC" => ValidaSC(ie),
                "SE" => ValidaSE(ie),
                "SP" => ValidaSP(ie),
                "TO" => ValidaTO(ie),
                _ => false,
            };
            if (valido)
            {
                Codigo = ie;
                UF = uf;
            }
            else
            {
                Codigo = null;
                UF = null;
                AddNotification(nameof(IE), MsgValueObjects.ResourceManager.GetString("IEInvalido", CultureInfo.CurrentCulture)
                    .Replace("{uf}", uf)
                    .Replace("{ie}", ie));
            }

        }



        private static bool ValidaAC(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 13 || strOrigem.Substring(0, 2) != "01")
                return false;

            var strBase = strOrigem.Trim();

            if (strBase.Substring(0, 2) != "01") return false;

            var intSoma = 0;
            var intPeso = 4;
            var intValor = 0;

            for (var intPos = 1; (intPos <= 11); intPos++)
            {

                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);

                if (intPeso == 1) intPeso = 9;

                intSoma += intValor * intPeso;

                intPeso--;
            }


            intSoma = 0;
            strBase = (strOrigem.Trim() + "000000000000").Substring(0, 12);
            intPeso = 5;

            for (var intPos = 1; (intPos <= 12); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);

                if (intPeso == 1) intPeso = 9;

                intSoma += intValor * intPeso;
                intPeso--;
            }

            var intResto = (intSoma % 11);
            var strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

            var strBase2 = (strBase.Substring(0, 12) + strDigito2);

            return (strBase2 == strOrigem);
        }

        private static bool ValidaAL(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9 || strOrigem.Substring(0, 2) != "24")
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

            if ((strBase.Substring(0, 2) != "24")) return false;

            var intSoma = 0;
            var intPeso = 9;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);

                intSoma += intValor * intPeso;
                intPeso--;
            }

            intSoma = (intSoma * 10);
            var intResto = (intSoma % 11);

            var strDigito1 = ((intResto == 10) ? "0" : Convert.ToString(intResto)).Substring((((intResto == 10) ? "0" : Convert.ToString(intResto)).Length - 1));

            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;
        }

        private static bool ValidaAM(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9)
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intSoma = 0;
            var intPeso = 9;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);

                intSoma += intValor * intPeso;
                intPeso--;
            }

            var intResto = (intSoma % 11);
            var strDigito1 = intSoma < 11 ? (11 - intSoma).ToString() : ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return (strBase2 == strOrigem);

        }

        private static bool ValidaAP(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9)
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intPeso = 9;

            if (strBase.Substring(0, 2) != "03") return false;

            strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intSoma = 0;

            int intValor;
            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);

                intSoma += intValor * intPeso;
                intPeso--;
            }

            var intResto = (intSoma % 11);
            intValor = (11 - intResto);

            var strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;
        }

        private static bool ValidaBA(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;

            if (strOrigem.Length != 9 && strOrigem.Length != 8)
                return false;

            var strBase = "";

            switch (strOrigem.Length)
            {
                case 8:
                    {
                        strBase = (strOrigem.Trim() + "00000000").Substring(0, 8);
                        break;
                    }
                case 9:
                    {
                        strBase = (strOrigem.Trim() + "00000000").Substring(0, 9);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            var intSoma = 0;
            int intValor;
            var intPeso = 0;
            int intResto;
            var strDigito1 = "";
            var strDigito2 = "";
            var strBase2 = "";


            #region Validação 8 dígitos
            if (strBase.Length == 8)
            {

                if ((("0123458".IndexOf(strBase.Substring(0, 1), 0, StringComparison.OrdinalIgnoreCase) + 1) > 0))
                {

                    for (var intPos = 1; (intPos <= 6); intPos++)
                    {
                        int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);

                        if (intPos == 1) intPeso = 7;

                        intSoma += intValor * intPeso;
                        intPeso--;
                    }


                    intResto = (intSoma % 10);
                    strDigito2 = ((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Length - 1));


                    strBase2 = strBase.Substring(0, 7) + strDigito2;

                    if (strBase2 == strOrigem)
                    {

                        intSoma = 0;
                        intPeso = 0;

                        for (var intPos = 1; (intPos <= 7); intPos++)
                        {
                            int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);

                            if (intPos == 7)
                                int.TryParse(strBase.Substring((intPos), 1), out intValor);

                            if (intPos == 1) intPeso = 8;

                            intSoma += intValor * intPeso;
                            intPeso--;
                        }


                        intResto = (intSoma % 10);
                        strDigito1 = ((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 6) + strDigito1 + strDigito2);

                        return strBase2 == strOrigem;

                    }

                    return false;


                }


                if ((("679".IndexOf(strBase.Substring(0, 1), 0, StringComparison.OrdinalIgnoreCase) + 1) > 0))
                {

                    intSoma = 0;

                    for (var intPos = 1; (intPos <= 6); intPos++)
                    {
                        int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);

                        if (intPos == 1) intPeso = 7;

                        intSoma += intValor * intPeso;
                        intPeso--;
                    }


                    intResto = (intSoma % 11);
                    strDigito2 = ((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Length - 1));


                    strBase2 = strBase.Substring(0, 7) + strDigito2;

                    if (strBase2 == strOrigem)
                    {

                        intSoma = 0;
                        intPeso = 0;

                        for (var intPos = 1; (intPos <= 7); intPos++)
                        {
                            int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);

                            if (intPos == 7)
                                int.TryParse(strBase.Substring((intPos), 1), out intValor);

                            if (intPos == 1) intPeso = 8;

                            intSoma += intValor * intPeso;
                            intPeso--;
                        }


                        intResto = (intSoma % 11);
                        strDigito1 = ((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 6) + strDigito1 + strDigito2);

                        return strBase2 == strOrigem;

                    }

                    return false;

                }

            }
            #endregion


            #region Validação 9 dígitos
            if (strBase.Length == 9)
            {

                var modulo = (("0123458".IndexOf(strBase.Substring(1, 1), 0, StringComparison.OrdinalIgnoreCase) + 1) > 0) ? 10 : 11;


                intSoma = 0;


                for (var intPos = 1; (intPos <= 7); intPos++)
                {
                    int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);

                    if (intPos == 1) intPeso = 8;

                    intSoma += intValor * intPeso;
                    intPeso--;
                }

                intResto = (intSoma % modulo);

                strDigito2 = modulo == 11 ? ((intResto == 0 || intResto == 1) ? "0" : Convert.ToString((modulo - intResto))).Substring((((intResto == 0 || intResto == 1) ? "0" : Convert.ToString((modulo - intResto))).Length - 1)) : ((intResto == 0) ? "0" : Convert.ToString((modulo - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((modulo - intResto))).Length - 1));


                strBase2 = strBase.Substring(0, 8) + strDigito2;

                if (strBase2 == strOrigem)
                {

                    intSoma = 0;
                    intPeso = 0;

                    for (var intPos = 1; (intPos <= 8); intPos++)
                    {
                        int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);

                        if (intPos == 8)
                            int.TryParse(strBase.Substring((intPos), 1), out intValor);

                        if (intPos == 1) intPeso = 9;

                        intSoma += intValor * intPeso;
                        intPeso--;
                    }


                    intResto = (intSoma % modulo);

                    strDigito1 = modulo == 11 ? ((intResto == 0 || intResto == 1)
                        ? "0" : Convert.ToString(modulo - intResto)).Substring((((intResto == 0 || intResto == 1)
                        ? "0" : Convert.ToString((modulo - intResto))).Length - 1)) : ((intResto == 0)
                        ? "0" : Convert.ToString((modulo - intResto))).Substring((((intResto == 0)
                        ? "0" : Convert.ToString((modulo - intResto))).Length - 1));


                    strBase2 = (strBase.Substring(0, 7) + strDigito1 + strDigito2);

                    return strBase2 == strOrigem;

                }

                return false;


            }
            #endregion

            return false;

        }


        private static bool ValidaCE(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length > 9)
                return false;

            while (strOrigem.Length <= 8)
                strOrigem = "0" + strOrigem;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intSoma = 0;
            var intValor = 0;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);
                intValor = (intValor * (10 - intPos));
                intSoma = (intSoma + intValor);
            }

            var intResto = (intSoma % 11);
            intValor = (11 - intResto);

            if ((intValor > 9))
                intValor = 0;

            var strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;

        }

        private static bool ValidaDF(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 13 || strOrigem.Substring(0, 2) != "07")
                return false;

            var strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 13);

            //var intSoma = 0;
            //var intPeso = 0;
            var intValor = 0;

            //for (var intPos = 11; (intPos >= 1); intPos = (intPos + -1))
            //{
            //    int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);
            //    intValor = (intValor * intPeso);
            //    intSoma = (intSoma + intValor);
            //    intPeso = (intPeso + 1);

            //    if ((intPeso > 9))
            //        intPeso = 2;
            //}

            //var intResto = (intSoma % 11);
            //var strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));


            var intSoma = 0;
            var intPeso = 2;

            for (var intPos = 12; (intPos >= 1); intPos = (intPos + -1))
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);
                intValor = (intValor * intPeso);
                intSoma = (intSoma + intValor);
                intPeso = (intPeso + 1);

                if ((intPeso > 9))
                    intPeso = 2;
            }

            var intResto = (intSoma % 11);
            var strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            var strBase2 = (strBase.Substring(0, 12) + strDigito2);

            return strBase2 == strOrigem;
        }

        private static bool ValidaES(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9)
                return false;

            var strBase = strOrigem.Trim();
            var intSoma = 0;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                intValor = (intValor * (10 - intPos));
                intSoma = (intSoma + intValor);
            }

            var intResto = (intSoma % 11);
            var strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;

        }

        private static bool ValidaGO(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9)
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

            if ((("10,11,15".IndexOf(strBase.Substring(0, 2), 0, StringComparison.OrdinalIgnoreCase) + 1) <= 0))
                return false;

            var intSoma = 0;
            var strDigito1 = "";

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                intValor = (intValor * (10 - intPos));
                intSoma = (intSoma + intValor);
            }

            var intResto = (intSoma % 11);
            var intNumero = 0;

            switch (intResto)
            {
                case 0:
                    strDigito1 = "0";
                    break;

                case 1:
                    int.TryParse(strBase.Substring(0, 8), out intNumero);
                    strDigito1 = (((intNumero >= 10103105) && (intNumero <= 10119997)) ? "1" : "0").Substring(((((intNumero >= 10103105) && (intNumero <= 10119997)) ? "1" : "0").Length - 1));
                    break;

                default:
                    strDigito1 = Convert.ToString((11 - intResto)).Substring((Convert.ToString((11 - intResto)).Length - 1));
                    break;
            }

            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;
        }

        private static bool ValidaMA(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9 || strOrigem.Substring(0, 2) != "12")
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intSoma = 0;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                intValor = (intValor * (10 - intPos));
                intSoma = (intSoma + intValor);
            }

            var intResto = (intSoma % 11);
            var strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;

        }

        private static bool ValidaMT(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length < 9 || strOrigem.Length > 11)
                return false;

            while (strOrigem.Length < 11)
                strOrigem = "0" + strOrigem;


            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 11);
            var d01 = Convert.ToInt32(strBase.Substring(0, 1));
            var d02 = Convert.ToInt32(strBase.Substring(1, 1));
            var d03 = Convert.ToInt32(strBase.Substring(2, 1));
            var d04 = Convert.ToInt32(strBase.Substring(3, 1));
            var d05 = Convert.ToInt32(strBase.Substring(4, 1));
            var d06 = Convert.ToInt32(strBase.Substring(5, 1));
            var d07 = Convert.ToInt32(strBase.Substring(6, 1));
            var d08 = Convert.ToInt32(strBase.Substring(7, 1));
            var d09 = Convert.ToInt32(strBase.Substring(8, 1));
            var d10 = Convert.ToInt32(strBase.Substring(9, 1));
            var dfinal = Convert.ToInt32(strBase.Substring(10, 1));

            var ds = 3 * d01 + 2 * d02 + 9 * d03 + 8 * d04 + 7 * d05 +
                     6 * d06 + 5 * d07 + 4 * d08 + 3 * d09 + 2 * d10;

            var aux1 = (ds / 11);
            aux1 = aux1 * 11;
            var aux2 = ds - aux1;

            int digVerificador;
            digVerificador = aux2 == 0 || aux2 == 1 ? 0 : 11 - aux2;


            return (dfinal == digVerificador);
        }

        private static bool ValidaMS(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9 || strOrigem.Substring(0, 2) != "28")
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intSoma = 0;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                intValor = (intValor * (10 - intPos));
                intSoma = (intSoma + intValor);
            }

            var intResto = (intSoma % 11);
            var strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;

        }

        private static bool ValidaMG(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 13)
                return false;

            if (strOrigem.Substring(0, 2).ToUpper() == "PR")
                return true;


            var strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 13);
            var strBase2 = strBase.Substring(0, 3) + "0" + strBase.Substring(3, 9);
            var intNumero = 1;

            var intSoma = 0;
            var intValor = 0;

            for (var intPos = 0; intPos < 12; intPos++)
            {
                int.TryParse(strBase2.Substring(intPos, 1), out intValor);
                if (intValor * intNumero >= 10)
                {
                    int.TryParse(strBase2.Substring(intPos, 1), out intValor);
                    intSoma += (intValor * intNumero) - 9;
                }
                else
                {
                    int.TryParse(strBase2.Substring(intPos, 1), out intValor);
                    intSoma += intValor * intNumero;
                }

                intNumero = intNumero + 1;

                if (intNumero == 3)
                    intNumero = 1;

            }

            intNumero = (int)((Math.Floor((Convert.ToDecimal(intSoma) + 10) / 10) * 10) - intSoma);
            if (intNumero % 10 == 0)
                intNumero = 0;

            if (intNumero != Convert.ToInt32(strOrigem.Substring(11, 1)))
                return false;


            intNumero = 3;
            intSoma = 0;

            for (var intPos = 0; intPos < 12; intPos++)
            {
                int.TryParse(strOrigem.Substring(intPos, 1), out intValor);
                intSoma += intValor * intNumero;

                intNumero = intNumero - 1;
                if (intNumero == 1)
                    intNumero = 11;

            }

            intNumero = 11 - (intSoma % 11);
            if (intNumero >= 10)
                intNumero = 0;


            return intNumero == Convert.ToInt32(strOrigem.Substring(12, 1));

        }

        private static bool ValidaPA(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9 || strOrigem.Substring(0, 2) != "15")
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intSoma = 0;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                intValor = (intValor * (10 - intPos));
                intSoma = (intSoma + intValor);
            }

            var intResto = (intSoma % 11);
            var strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;

        }

        private static bool ValidaPB(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9)
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intSoma = 0;
            int intValor;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);
                intValor = (intValor * (10 - intPos));
                intSoma = (intSoma + intValor);
            }

            var intResto = (intSoma % 11);
            intValor = (11 - intResto);

            if ((intValor > 9))
                intValor = 0;

            var strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;

        }

        private static bool ValidaPE(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9)
                return false;

            var strBase = (strOrigem.Trim() + "00000000000000").Substring(0, 14);
            var intSoma = 0;
            var intPeso = 2;
            int intValor;

            for (var intPos = 7; (intPos >= 1); intPos = (intPos + -1))
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);
                intValor = (intValor * intPeso);
                intSoma = (intSoma + intValor);
                intPeso = (intPeso + 1);

                if ((intPeso > 9))
                    intPeso = 2;
            }

            var intResto = (intSoma % 11);
            intValor = (11 - intResto);

            if ((intValor >= 10))
                intValor = 0;

            if (intValor != Convert.ToInt32(strOrigem.Substring(7, 1)))
                return false;

            var strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));
            var strBase2 = (strBase.Substring(0, 7) + strDigito1);

            if (strBase2 != strOrigem.Substring(0, 8))
                return false;

            intSoma = 0;
            intPeso = 2;

            for (var intPos = 8; (intPos >= 1); intPos = (intPos + -1))
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);
                intValor = (intValor * intPeso);
                intSoma = (intSoma + intValor);
                intPeso = (intPeso + 1);

                if ((intPeso > 9))
                    intPeso = 2;
            }

            intResto = (intSoma % 11);
            intValor = (11 - intResto);

            if ((intValor >= 10))
                intValor = 0;


            return intValor.ToString() == strOrigem.Substring(8, 1);

        }

        private static bool ValidaPI(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9)
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intSoma = 0;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                intValor = (intValor * (10 - intPos));
                intSoma = (intSoma + intValor);
            }

            var intResto = (intSoma % 11);
            var strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;
        }

        private static bool ValidaPR(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 10)
                return false;

            var strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);
            var intSoma = 0;
            var intPeso = 2;
            int intValor;

            for (var intPos = 8; (intPos >= 1); intPos = (intPos + -1))
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);
                intValor = (intValor * intPeso);
                intSoma = (intSoma + intValor);
                intPeso = (intPeso + 1);

                if ((intPeso > 7))
                    intPeso = 2;
            }

            var intResto = (intSoma % 11);
            var strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);
            intSoma = 0;
            intPeso = 2;

            for (var intPos = 9; (intPos >= 1); intPos = (intPos + -1))
            {
                int.TryParse(strBase2.Substring((intPos - 1), 1), out intValor);
                intValor = (intValor * intPeso);
                intSoma = (intSoma + intValor);
                intPeso = (intPeso + 1);

                if ((intPeso > 7))
                    intPeso = 2;
            }

            intResto = (intSoma % 11);
            var strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            strBase2 = (strBase2 + strDigito2);

            return strBase2 == strOrigem;

        }

        private static bool ValidaRJ(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 8)
                return false;

            var strBase = (strOrigem.Trim() + "00000000").Substring(0, 8);
            var intSoma = 0;
            var intPeso = 2;

            for (var intPos = 7; (intPos >= 1); intPos = (intPos + -1))
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                intValor = (intValor * intPeso);
                intSoma = (intSoma + intValor);
                intPeso = (intPeso + 1);

                if ((intPeso > 7))
                    intPeso = 2;
            }

            var intResto = (intSoma % 11);
            var strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            var strBase2 = (strBase.Substring(0, 7) + strDigito1);

            return strBase2 == strOrigem;

        }

        private static bool ValidaRN(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            var strBase = "";
            switch (strOrigem.Length)
            {
                case 9:
                    {
                        strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                        break;
                    }
                case 10:
                    {
                        strBase = (strOrigem.Trim() + "000000000").Substring(0, 10);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            var intSoma = 0;

            if ((strBase.Substring(0, 2) == "20") && strBase.Length == 9)
            {

                for (var intPos = 1; (intPos <= 8); intPos++)
                {
                    int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                    intValor = (intValor * (10 - intPos));
                    intSoma = (intSoma + intValor);
                }

                intSoma = (intSoma * 10);
                var intResto = (intSoma % 11);
                var strDigito1 = ((intResto > 9) ? "0" : Convert.ToString(intResto)).Substring((((intResto > 9) ? "0" : Convert.ToString(intResto)).Length - 1));
                var strBase2 = (strBase.Substring(0, 8) + strDigito1);

                return strBase2 == strOrigem;

            }


            if (strBase.Length == 10)
            {
                intSoma = 0;

                for (var intPos = 1; (intPos <= 9); intPos++)
                {
                    int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                    intValor = (intValor * (11 - intPos));
                    intSoma = (intSoma + intValor);
                }

                intSoma = (intSoma * 10);
                var intResto = (intSoma % 11);
                var strDigito1 = ((intResto > 10) ? "0" : Convert.ToString(intResto)).Substring((((intResto > 10) ? "0" : Convert.ToString(intResto)).Length - 1));
                var strBase2 = (strBase.Substring(0, 9) + strDigito1);

                return strBase2 == strOrigem;

            }

            return false;

        }

        private static bool ValidaRO(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 14)
                return false;

            var strBase = (strOrigem.Trim() + "00000000000000").Substring(0, 14);
            var d01 = Convert.ToInt32(strBase.Substring(0, 1));
            var d02 = Convert.ToInt32(strBase.Substring(1, 1));
            var d03 = Convert.ToInt32(strBase.Substring(2, 1));
            var d04 = Convert.ToInt32(strBase.Substring(3, 1));
            var d05 = Convert.ToInt32(strBase.Substring(4, 1));
            var d06 = Convert.ToInt32(strBase.Substring(5, 1));
            var d07 = Convert.ToInt32(strBase.Substring(6, 1));
            var d08 = Convert.ToInt32(strBase.Substring(7, 1));
            var d09 = Convert.ToInt32(strBase.Substring(8, 1));
            var d10 = Convert.ToInt32(strBase.Substring(9, 1));
            var d11 = Convert.ToInt32(strBase.Substring(10, 1));
            var d12 = Convert.ToInt32(strBase.Substring(11, 1));
            var d13 = Convert.ToInt32(strBase.Substring(12, 1));
            var dfinal = Convert.ToInt32(strBase.Substring(13, 1));

            var ds = 6 * d01 + 5 * d02 + 4 * d03 + 3 * d04 + 2 * d05 +
                     9 * d06 + 8 * d07 + 7 * d08 + 6 * d09 + 5 * d10 +
                     4 * d11 + 3 * d12 + 2 * d13;

            var aux1 = (ds / 11);
            aux1 = aux1 * 11;
            var aux2 = ds - aux1;
            var digVerificador = 11 - aux2;

            int resto_do_calculo;
            resto_do_calculo = digVerificador > 9 ? digVerificador - 10 : digVerificador;


            return dfinal.Equals(resto_do_calculo);
        }

        private static bool ValidaRR(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 9 || strOrigem.Substring(0, 2) != "24")
                return false;

            var strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
            var intSoma = 0;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                intValor = intValor * intPos;
                intSoma += intValor;
            }

            var intResto = (intSoma % 9);
            var strDigito1 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));
            var strBase2 = (strBase.Substring(0, 8) + strDigito1);

            return strBase2 == strOrigem;

        }

        private static bool ValidaRS(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 10)
                return false;


            var strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);
            var intSoma = 0;
            var intPeso = 2;
            var intValor = 0;

            for (var intPos = 9; (intPos >= 1); intPos = (intPos + -1))
            {
                int.TryParse(strBase.Substring((intPos - 1), 1), out intValor);
                intValor = (intValor * intPeso);
                intSoma = (intSoma + intValor);
                intPeso = (intPeso + 1);

                if ((intPeso > 9))
                    intPeso = 2;
            }

            var intResto = (intSoma % 11);
            intValor = (11 - intResto);

            if ((intValor > 9))
                intValor = 0;

            var strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));
            var strBase2 = (strBase.Substring(0, 9) + strDigito1);

            return strBase2 == strOrigem;

        }

        private static bool ValidaSC(string inscricaoEstadual)
        {
            return ValidaPI(inscricaoEstadual);
        }

        private static bool ValidaSE(string inscricaoEstadual)
        {
            return ValidaPB(inscricaoEstadual);
                       
        }

        private static bool ValidaSP(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            string strBase;
            string strBase2;
            int intSoma;
            int intPeso;

            if ((strOrigem.Substring(0, 1) == "P"))
            {
                strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 13);
                intSoma = 0;
                intPeso = 1;

                for (var intPos = 1; (intPos <= 8); intPos++)
                {
                    int.TryParse(strBase.Substring((intPos), 1), out int intValor);
                    intValor = (intValor * intPeso);
                    intSoma = (intSoma + intValor);
                    intPeso = (intPeso + 1);

                    if ((intPeso == 2))
                        intPeso = 3;

                    if ((intPeso == 9))
                        intPeso = 10;
                }

                var intResto = (intSoma % 11);
                var strDigito1 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));
                strBase2 = (strBase.Substring(0, 9) + (strDigito1 + strBase.Substring(10, 3)));
            }
            else
            {
                strBase = (strOrigem.Trim() + "000000000000").Substring(0, 12);
                intSoma = 0;
                intPeso = 1;

                for (var intPos = 1; (intPos <= 8); intPos++)
                {
                    int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                    intValor = (intValor * intPeso);
                    intSoma = (intSoma + intValor);
                    intPeso = (intPeso + 1);

                    if ((intPeso == 2))
                        intPeso = 3;

                    if ((intPeso == 9))
                        intPeso = 10;
                }

                var intResto = (intSoma % 11);
                var strDigito1 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));
                strBase2 = (strBase.Substring(0, 8) + (strDigito1 + strBase.Substring(9, 2)));
                intSoma = 0;
                intPeso = 2;

                for (var intPos = 11; (intPos >= 1); intPos = (intPos + -1))
                {
                    int.TryParse(strBase.Substring((intPos - 1), 1), out int intValor);
                    intValor = (intValor * intPeso);
                    intSoma = (intSoma + intValor);
                    intPeso = (intPeso + 1);

                    if ((intPeso > 10))
                        intPeso = 2;
                }

                intResto = (intSoma % 11);
                var strDigito2 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));
                strBase2 = (strBase2 + strDigito2);
            }

            return strBase2 == strOrigem;

        }

        private static bool ValidaTO(string inscricaoEstadual)
        {

            var strOrigem = inscricaoEstadual;
            if (strOrigem.Length != 11)
                return false;

            var strBase = (strOrigem.Trim() + "00000000000").Substring(0, 11);
            var strBase2 = (strBase.Substring(0, 2) + strBase.Substring(4, 6));
            var intSoma = 0;

            for (var intPos = 1; (intPos <= 8); intPos++)
            {
                int.TryParse(strBase2.Substring((intPos - 1), 1), out int intValor);
                intValor = (intValor * (10 - intPos));
                intSoma = (intSoma + intValor);
            }

            var intResto = (intSoma % 11);
            var strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
            strBase2 = (strBase.Substring(0, 10) + strDigito1);

            return strBase2 == strOrigem;

        }


        public const int maxIE = 14;
        public const int maxUF = 2;

    }
}
