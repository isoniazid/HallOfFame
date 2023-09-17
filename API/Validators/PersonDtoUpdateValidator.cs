using FluentValidation;
using HallOfFame.DTO;

namespace HallOfFame.Validators
{
    public class PersonDtoUpdateValidator : AbstractValidator<PersonDtoUpdate>
    {
        public PersonDtoUpdateValidator()
        {
            RuleFor(person => person.Name)
                .NotEmpty().WithMessage("Имя не может быть пустым")
                .MaximumLength(50).WithMessage("Имя не может превышать 50 символов");

            RuleFor(person => person.DisplayName)
                .NotEmpty().WithMessage("Отображаемое имя не может быть пустым")
                .MaximumLength(50).WithMessage("Отображаемое имя не может превышать 50 символов");

            RuleForEach(person => person.Skills)
                .SetValidator(new SkillDtoCreateValidator())
                .Must((model, item, context) =>
            {
                var duplicateItems = model.Skills?
                    .Where(x => x.Name == item.Name)
                    .ToList();

                return duplicateItems is null || duplicateItems.Count <= 1;
            })
            .WithMessage("Элементы списка должны быть уникальны по параметру Name.");
        }
    }
}