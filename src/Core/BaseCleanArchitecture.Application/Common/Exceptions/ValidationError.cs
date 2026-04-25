namespace BaseCleanArchitecture.Application.Common.Exceptions;


public sealed record ValidationError(string PropertyName, string ErrorMessage);
