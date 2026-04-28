namespace ECommerceProject.Business.Models.Common;

public class ServiceValidationError
{
    public ServiceValidationError(string fieldName, string message)
    {
        FieldName = fieldName;
        Message = message;
    }

    public string FieldName { get; }
    public string Message { get; }
}
