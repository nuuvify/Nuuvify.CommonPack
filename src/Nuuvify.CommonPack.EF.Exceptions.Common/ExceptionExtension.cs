using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFramework.Exceptions.Common
{
    public static class ExceptionExtension
    {
        

        public static IDictionary<string, string> CustomMessage(this CustomDbUpdateException exception)
        {
            return CustomMessage((DbUpdateException)exception);
        }

        public static IDictionary<string, string> CustomMessage(this DbUpdateException exception)
        {
            var currentAndProposed = new Dictionary<string, string>();

            PropertyValues proposedValues;
            PropertyValues databaseValues;
            string columnName;
            string currentValue;
            string databaseValue;

            foreach (var entry in exception.Entries)
            {
                proposedValues = entry.CurrentValues;
                databaseValues = entry.GetDatabaseValues();

                foreach (var property in proposedValues.Properties)
                {
                    columnName = property.Name;

                    currentValue = $"Proposed: {columnName} = {proposedValues[property]}";
                    if (databaseValues?[property] is null)
                    {
                        databaseValue = string.Empty;
                    }
                    else
                    {
                        databaseValue = $"DataBaseValue: {columnName} = {databaseValues?[property]}";
                    }

                    currentAndProposed.TryAdd(currentValue, databaseValue);
                }

            }

            return currentAndProposed;
            
        }

    }
}