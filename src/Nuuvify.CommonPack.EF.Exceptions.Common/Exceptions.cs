using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;

namespace EntityFramework.Exceptions.Common
{
    public class CustomDbUpdateException : DbUpdateException
    {
        public CustomDbUpdateException()
        {
        }

        public CustomDbUpdateException(string message)
            : base(message)
        {
        }

        public CustomDbUpdateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CustomDbUpdateException(string message, IReadOnlyList<IUpdateEntry> entries)
            : base(message, entries)
        {
        }

        public CustomDbUpdateException(string message, Exception innerException, IReadOnlyList<IUpdateEntry> entries)
            : base(message, innerException, entries)
        {
        }
        public CustomDbUpdateException(string message, Exception innerException, IReadOnlyList<IUpdateEntry> entries, IDictionary<string, string> customErrors)
            : base(message, innerException, entries)
        {
            CustomErrors = customErrors;
        }
        // public CustomDbUpdateException(SerializationInfo info, StreamingContext context)
        //     : base(info, context)
        // {
        // }

        public IDictionary<string, string> CustomErrors { get; set; }

    }

    public class CustomDbException : DbException
    {
        public CustomDbException()
        {

        }

        public CustomDbException(string message)
            : base(message)
        {
        }

        public CustomDbException(string message, Exception innerException, IDictionary<string, string> customErrors)
            : base(message, innerException)
        {
            CustomErrors = customErrors;
        }

        public CustomDbException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CustomDbException(string message, int errorCode)
            : base(message, errorCode)
        {
        }
        // public CustomDbException(SerializationInfo info, StreamingContext context)
        //     : base(info, context)
        // {
        // }

        public IDictionary<string, string> CustomErrors { get; set; }


    }

    public class CustomException : Exception
    {
        public CustomException()
        {

        }

        public CustomException(string message)
            : base(message)
        {
        }

        public CustomException(string message, Exception innerException, IDictionary<string, string> customErrors)
            : base(message, innerException)
        {
            CustomErrors = customErrors;
        }

        public CustomException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // public CustomException(SerializationInfo info, StreamingContext context)
        //     : base(info, context)
        // {
        // }

        public IDictionary<string, string> CustomErrors { get; set; }


    }


}