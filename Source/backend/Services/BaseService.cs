using System.ComponentModel.DataAnnotations;

namespace CoachBackend.Services;

public abstract class BaseService
{
    protected void ValidateModel(object model)
    {
        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();
        
        if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
        {
            var errors = validationResults.Select(r => r.ErrorMessage);
            throw new ValidationException(string.Join(", ", errors));
        }
    }
} 