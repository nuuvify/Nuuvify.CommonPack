using System.Text;
using EntityFramework.Exceptions.Common;
using IBM.Data.Db2;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace EntityFramework.Exceptions.Db2;

public class Db2ExceptionProcessorStateManager : ExceptionProcessorStateManager<DB2Exception>
{
    private const int CannotInsertNull = 1400;
    private const int UniqueConstraintViolation = -2147467259;
    private const int IntegrityConstraintViolation = 2291;
    private const int ChildRecordFound = 2292;
    private const int NumericOverflow = 1438;
    private const int NumericOrValueError = 12899;
    private const int DB2CustomErrorCollection = 0;

    public Db2ExceptionProcessorStateManager(StateManagerDependencies dependencies)
        : base(dependencies)
    {
    }

    protected override DatabaseError? GetDatabaseError(DB2Exception dbException)
    {
        switch (dbException.ErrorCode)
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
            case DB2CustomErrorCollection:
            default:
                {
                    CustomNewMessage = GetExceptionErrors(dbException);
                    return DatabaseError.CustomException;
                }
        }
    }

    public string GetExceptionErrors(DB2Exception exception)
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
            if (!string.IsNullOrWhiteSpace(exception.Errors[i].NativeError.ToString()))
            {
                key = $"#{i}-NativeError";
                value = $"{exception.Errors[i].NativeError}";

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
            if (!string.IsNullOrWhiteSpace(exception.Errors[i].SQLState))
            {
                key = $"#{i}-SQLState";
                value = $"{exception.Errors[i].SQLState}";

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
