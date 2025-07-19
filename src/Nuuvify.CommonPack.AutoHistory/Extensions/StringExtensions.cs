namespace Nuuvify.CommonPack.AutoHistory.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Esse metodo é copia do metodo contante na DLL Extensions, para não criar dependencia com a DLL.
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string SubstringNotNull(this string valor, int start, int length)
        {
            var newValue = string.Empty;
            var qtdCut = 0;


            if (string.IsNullOrWhiteSpace(valor))
                return newValue;

            if (start < 0 || length < 0)
                return newValue;


            qtdCut = start + length;

            if (qtdCut > valor.Length && start <= valor.Length)
            {
                newValue = valor.Substring(start);
            }
            else if (qtdCut > valor.Length && start > valor.Length)
            {
                return newValue;
            }
            else
            {
                newValue = valor.Substring(start, length);
            }


            return newValue;
        }
    }
}
