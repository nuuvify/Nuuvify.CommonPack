using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace EntityFramework.Exceptions.Common
{
    public static class ExceptionFactory
    {
        internal static Exception Create<T>(
            ExceptionProcessorStateManager<T>.DatabaseError error,
            DbUpdateException exception,
            List<InternalEntityEntry> entries,
            string customNewMessage,
            IDictionary<string, string> errors) where T : DbException
        {
            return error switch
            {
                ExceptionProcessorStateManager<T>.DatabaseError.CannotInsertNull when entries.Count > 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException, entries) {CustomErrors = errors},
                ExceptionProcessorStateManager<T>.DatabaseError.CannotInsertNull when entries.Count == 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException) { CustomErrors = errors },

                ExceptionProcessorStateManager<T>.DatabaseError.MaxLength when entries.Count > 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException, entries) { CustomErrors = errors },
                ExceptionProcessorStateManager<T>.DatabaseError.MaxLength when entries.Count == 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException) { CustomErrors = errors },

                ExceptionProcessorStateManager<T>.DatabaseError.NumericOverflow when entries.Count > 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException, entries) { CustomErrors = errors },
                ExceptionProcessorStateManager<T>.DatabaseError.NumericOverflow when entries.Count == 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException) { CustomErrors = errors },

                ExceptionProcessorStateManager<T>.DatabaseError.ReferenceConstraint when entries.Count > 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException, entries) { CustomErrors = errors },
                ExceptionProcessorStateManager<T>.DatabaseError.ReferenceConstraint when entries.Count == 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException) { CustomErrors = errors },

                ExceptionProcessorStateManager<T>.DatabaseError.UniqueConstraint when entries.Count > 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException, entries) { CustomErrors = errors },
                ExceptionProcessorStateManager<T>.DatabaseError.UniqueConstraint when entries.Count == 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException) { CustomErrors = errors },

                ExceptionProcessorStateManager<T>.DatabaseError.CustomDbUpdateException when entries.Count > 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException, entries) { CustomErrors = errors },
                ExceptionProcessorStateManager<T>.DatabaseError.CustomDbUpdateException when entries.Count == 0 => new CustomDbUpdateException($"{exception.Message} {customNewMessage}", exception.InnerException) { CustomErrors = errors },

                ExceptionProcessorStateManager<T>.DatabaseError.CustomDbException => new CustomDbException($"{exception.Message} {customNewMessage}", exception.InnerException) { CustomErrors = errors },
                               
                ExceptionProcessorStateManager<T>.DatabaseError.CustomException => new CustomException($"{exception.Message} {customNewMessage}", exception.InnerException) { CustomErrors = errors },
                _ => new CustomException($"{exception.Message} {customNewMessage}", exception.InnerException) { CustomErrors = errors },
            };
        }


    }


}