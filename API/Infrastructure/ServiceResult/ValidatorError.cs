namespace HallOfFame.Infrastructure.ServiceResult
{
    public class ValidatorError
    {
        public int StatusCode { get; set; }

        public Dictionary<string, string> ValidationErrors { get; set; } = new Dictionary<string, string>();

        public ValidatorError(FluentValidation.Results.ValidationResult validationResults)
        {
            StatusCode = StatusCodes.Status400BadRequest;
            foreach (var error in validationResults.Errors)
            {
                ValidationErrors.Add(error.PropertyName, error.ErrorMessage);
            }

        }
    }
}