using FluentValidation;
using AppointmentSystem.Application.DTOs;

namespace AppointmentSystem.Application.Validators
{
    public class AppointmentValidator : AbstractValidator<AppointmentDto>
    {
        public AppointmentValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Başlık zorunludur.")
                .MaximumLength(200)
                .WithMessage("Başlık en fazla 200 karakter olabilir.");

            RuleFor(x => x.RequestedBy)
                .NotEmpty()
                .WithMessage("Talep eden kullanıcı adı zorunludur.");

            RuleFor(x => x.Date)
                .GreaterThanOrEqualTo(DateTime.Today.Date)
                .WithMessage("Talep tarihi bugünden önce olamaz.");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("Başlangıç saati zorunludur.");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("Bitiş saati zorunludur.")
                .GreaterThan(x => x.StartTime)
                .WithMessage("Bitiş saati, başlangıç saatinden sonra olmalı.");

            RuleFor(x => x.BranchId)
                .GreaterThan(0)
                .WithMessage("Şube seçimi zorunludur.");
        }
    }
}
