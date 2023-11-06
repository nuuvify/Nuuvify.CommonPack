using System;

namespace Nuuvify.CommonPack.Extensions.Implementation
{
    public static class DateTimeExtensions
    {

        /// <summary>
        /// Retorna Data e Hora para atender o padrão REST, util quando necessita passar uma data como parametro em uma api
        /// </summary>
        /// <param name="dateTime">Data/Hora que serão formatados</param>
        /// <param name="timeToo">True se deseja incluir hora no retorno</param>
        /// <returns></returns>
        public static string GetFormatRest(this DateTime dateTime, bool timeToo = false)
        {

            var dataRest = $"{dateTime.Year}-{dateTime.Month}-{dateTime.Day}";

            if (timeToo)
            {
                dataRest += $"T{dateTime.TimeOfDay}";
            }

            return dataRest;

        }

        /// <inheritdoc cref="GetFormatRest(DateTime, bool)"/>
        public static string GetFormatRest(this DateTimeOffset dateTimeOffset)
        {

            var dataRest = $"{dateTimeOffset.Year}-{dateTimeOffset.Month}-{dateTimeOffset.Day}";
            dataRest += $"T{dateTimeOffset.TimeOfDay} {dateTimeOffset.Offset}";

            return dataRest;

        }


        /// <summary>
        /// Retorna a data correspondente ao primeiro dia util do mes/ano da data atual
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DateTime GetFirstWorkingDay(this DateTime data)
        {
            var firstDayOfMonth = new DateTime(data.Year, data.Month, 1);
            var dateTime = WorkDay(firstDayOfMonth);

            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, data.Hour, data.Minute, data.Second, data.Millisecond);
        }

        /// <inheritdoc cref="GetFirstWorkingDay(DateTime)"/>
        public static DateTimeOffset GetFirstWorkingDay(this DateTimeOffset data)
        {
            var firstDayOfMonth = new DateTime(data.Year, data.Month, 1);
            var dateTime = WorkDay(firstDayOfMonth);

            return new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, data.Hour, data.Minute, data.Second, data.Millisecond, data.Offset);

        }

        private static DateTime WorkDay(DateTime firstDayOfMonth)
        {
            while (firstDayOfMonth.DayOfWeek == DayOfWeek.Saturday ||
                   firstDayOfMonth.DayOfWeek == DayOfWeek.Sunday)
            {
                firstDayOfMonth = firstDayOfMonth.AddDays(1);
            }

            return firstDayOfMonth;
        }


    }


}