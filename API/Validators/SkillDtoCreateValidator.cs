using FluentValidation;
using HallOfFame.DTO;

namespace HallOfFame.Validators
{
    public class SkillDtoCreateValidator : AbstractValidator<SkillDtoCreate>
    {
        public SkillDtoCreateValidator()
        {
            RuleFor(skill => skill.Name)
                .NotEmpty().WithMessage("Имя навыка не может быть пустым")
                .MaximumLength(50).WithMessage("Имя навыка не может превышать 50 символов");

            RuleFor(skill => (int)skill.Level)
                .InclusiveBetween(1, 10).WithMessage("Уровень навыка должен быть в диапазоне от 1 до 10");
        }
    }
}