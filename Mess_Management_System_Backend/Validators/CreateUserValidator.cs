using FluentValidation;
using Mess_Management_System_Backend.Dtos;

namespace Mess_Management_System_Backend.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.Role)
                .Must(r => new[] { "Student", "Teacher", "Admin" }.Contains(r))
                .WithMessage("Invalid role specified.");
        }
    }
}
