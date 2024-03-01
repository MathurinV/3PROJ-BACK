namespace API.ErrorsHandling;

public class GraphQlErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is { } exception) return error.WithMessage(exception.Message);

        return error;
    }
}