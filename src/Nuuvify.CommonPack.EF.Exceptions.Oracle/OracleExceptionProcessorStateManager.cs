using System.Text;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Oracle.ManagedDataAccess.Client;

namespace EntityFramework.Exceptions.Oracle;

public class OracleExceptionProcessorStateManager : ExceptionProcessorStateManager<OracleException>
{
    private const int CannotInsertNull = 1400;
    private const int UniqueConstraintViolation = 1;
    private const int IntegrityConstraintViolation = 2291;
    private const int ChildRecordFound = 2292;
    private const int NumericOverflow = 1438;
    private const int NumericOrValueError = 12899;
    private const int OracleCustomErrorCollection = 0;

    public OracleExceptionProcessorStateManager(StateManagerDependencies dependencies)
    : base(dependencies)
    {
    }

    protected override DatabaseError? GetDatabaseError(OracleException dbException)
    {
        switch (dbException.Number)
        {
            case IntegrityConstraintViolation:
            case ChildRecordFound:
                CustomNewMessage = GetExceptionErrors(dbException);
                return DatabaseError.ReferenceConstraint;
            case CannotInsertNull:
                CustomNewMessage = GetExceptionErrors(dbException);
                return DatabaseError.CannotInsertNull;
            case NumericOrValueError:
                CustomNewMessage = GetExceptionErrors(dbException);
                return DatabaseError.MaxLength;
            case NumericOverflow:
                CustomNewMessage = GetExceptionErrors(dbException);
                return DatabaseError.NumericOverflow;
            case UniqueConstraintViolation:
                CustomNewMessage = GetExceptionErrors(dbException);
                return DatabaseError.UniqueConstraint;
            case OracleCustomErrorCollection:
            default:
                {
                    CustomNewMessage = GetExceptionErrors(dbException);
                    return DatabaseError.CustomException;
                }
        }
    }

    public string GetExceptionErrors(OracleException exception)
    {
        var newMessage = new StringBuilder();
        string key;
        string value;

        CustomErrors ??= new Dictionary<string, string>();

        for (int i = 0; i < exception.Errors.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(exception.Errors[i].Message))
            {
                key = $"#{i}-Message";
                value = $"{exception.Errors[i].Message}";

                CustomErrors.Add(key, value);
                _ = newMessage.AppendLine($"{key}: {value}");
            }
            if (!string.IsNullOrWhiteSpace(exception.Errors[i].DataSource))
            {
                key = $"#{i}-DataSource";
                value = $"{exception.Errors[i].DataSource}";

                CustomErrors.Add(key, value);
                _ = newMessage.AppendLine($"{key}: {value}");
            }
            if (!string.IsNullOrWhiteSpace(exception.Errors[i].Source))
            {
                key = $"#{i}-Source";
                value = $"{exception.Errors[i].Source}";

                CustomErrors.Add(key, value);
                _ = newMessage.AppendLine($"{key}: {value}");
            }
            if (!string.IsNullOrWhiteSpace(exception.Errors[i].Number.ToString()))
            {
                key = $"#{i}-Number";
                value = $"{exception.Errors[i].Number}";

                CustomErrors.Add(key, value);
                _ = newMessage.AppendLine($"{key}: {value}");
            }
            if (!string.IsNullOrWhiteSpace(exception.Errors[i].ParseErrorOffset.ToString()))
            {
                key = $"#{i}-ParseError";
                value = $"{exception.Errors[i].ParseErrorOffset}";

                CustomErrors.Add(key, value);
                _ = newMessage.AppendLine($"{key}: {value}");
            }
            if (!string.IsNullOrWhiteSpace(exception.Errors[i].Procedure))
            {
                key = $"#{i}-Procedure";
                value = $"{exception.Errors[i].Procedure}";

                CustomErrors.Add(key, value);
                _ = newMessage.AppendLine($"{key}: {value}");
            }
        }

        var inner = exception?.InnerException;
        if (inner != null)
        {
            key = $"#inner-{0}-Message";
            value = $"{inner.Message}";

            _ = newMessage.AppendLine($"{key}: {value}");
        }

        return newMessage.ToString();
    }

}
