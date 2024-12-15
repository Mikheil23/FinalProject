using FluentValidation;

namespace FinalProjectV3.Helpers
{
    public class ValidationHelper
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ValidateAsync<TDto>(TDto dto)
        {
            var validator = _serviceProvider.GetService(typeof(IValidator<TDto>)) as IValidator<TDto>;
            if (validator == null)
            {
                throw new Exception($"No validator found for type {typeof(TDto).Name}");
            }

            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errorMessage); 
            }
        }
    }

}
