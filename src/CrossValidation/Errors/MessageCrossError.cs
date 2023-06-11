namespace CrossValidation.Errors;

public record MessageCrossError(string Message, params object[] Parameters) :
    CompleteCrossError(Message: string.Format(Message, Parameters));