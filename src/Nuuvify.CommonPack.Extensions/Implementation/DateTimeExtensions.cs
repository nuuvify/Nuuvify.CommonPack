using System;

namespace Nuuvify.CommonPack.Extensions.Implementation
{
    public static class DateTimeExtensions
    {


        public static string GetFormatRest(this DateTime dateTime, bool timeToo = false)
        {

            var dataRest = $"{dateTime.Year}-{dateTime.Month}-{dateTime.Day}";

            if (timeToo)
            {
                dataRest += $"T{dateTime.TimeOfDay}";
            }

            return dataRest;

        }


        public static string GetFormatRest(this DateTimeOffset dateTimeOffset)
        {

            var dataRest = $"{dateTimeOffset.Year}-{dateTimeOffset.Month}-{dateTimeOffset.Day}";
            dataRest += $"T{dateTimeOffset.TimeOfDay} {dateTimeOffset.Offset}";

            return dataRest;

        }



    }


}