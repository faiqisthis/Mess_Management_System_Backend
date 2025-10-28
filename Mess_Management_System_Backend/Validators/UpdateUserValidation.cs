using FluentValidation;
using Mess_Management_System_Backend.Dtos;

namespace Mess_Management_System_Backend.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().MaximumLength(100);

            When(x => !string.IsNullOrEmpty(x.Email), () =>
            {
                RuleFor(x => x.Email).EmailAddress();
            });
        }
    }
}
