using FluentValidation;
using MuhasibPro.Business.DTOModel.SistemModel;

namespace MuhasibPro.Business.ModelValidator.SistemValidations
{
    public class KullaniciValidator : AbstractValidator<KullaniciModel>
    {
        public KullaniciValidator()
        {
            string mesaj = "Gerekli alan!";
            ClassLevelCascadeMode = CascadeMode.Continue;
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(p => p.KullaniciAdi)
               .NotEmpty()
               .WithMessage(mesaj)
               .MaximumLength(50)
               .WithMessage("En fazla 50 karakter olmalı");

            RuleFor(p => p.Adi)
               .NotEmpty()
               .WithMessage(mesaj)
               .MaximumLength(50)
               .WithMessage("En fazla 50 karakter olmalı");
        }
    }
}
