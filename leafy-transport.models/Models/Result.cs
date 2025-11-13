namespace leafy_transport.models.Models;

public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsCancelled { get; private set; }
    public bool IsValidationFailure => ValidationErrors?.Any() == true;
    public Dictionary<string, string[]> ValidationErrors { get; private set; }
    public IEnumerable<object> Errors { get; private set; }

    public static Result Success() => new Result { IsSuccess = true };
    public static Result Failure(IEnumerable<object> errors = null) => new Result { IsSuccess = false, Errors = errors };
    public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) => new Result { IsSuccess = false, ValidationErrors = validationErrors };
    public static Result Cancelled() => new Result { IsCancelled = true, IsSuccess = false };
}