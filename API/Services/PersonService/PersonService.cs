using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using HallOfFame.DTO;
using HallOfFame.Infrastructure;
using HallOfFame.Infrastructure.ServiceResult;
using HallOfFame.Model;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace HallOfFame.Services.PersonService
{
    public class PersonService : IPersonService
    {
        private readonly ILogger<PersonService> _logger;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        private readonly IValidator<PersonDtoCreate> _personCreateValidator;
        private readonly IValidator<PersonDtoUpdate> _personUpdateValidator;

        public PersonService(ILogger<PersonService> logger, IMapper mapper, ApplicationDbContext context,
            IValidator<PersonDtoCreate> personCreateValidator,
            IValidator<PersonDtoUpdate> personUpdateValidator)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
            _personCreateValidator = personCreateValidator;
            _personUpdateValidator = personUpdateValidator;
        }


        public async Task<OneOf<PersonDtoCreate, APIError, ValidatorError>>
        CreateAsync(PersonDtoCreate dto, CancellationToken cToken)
        {

            var validation = _personCreateValidator.Validate(dto);
            if (!validation.IsValid)
            {
                _logger.LogError("Validation Error: {Errors}", validation.Errors);
                return new ValidatorError(validation);
            }

            var personToSave = _mapper.Map<Person>(dto);

            await _context.Persons.AddAsync(personToSave, cToken);

            await _context.SaveChangesAsync(cToken);

            return dto;
        }


        public async Task<OneOf<long, APIError>>
        DeleteAsync(long Id, CancellationToken cToken)
        {
            var personToDelete = await _context.Persons.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken: cToken);

            if (personToDelete is null) return new APIError(404, "No such person");

            _context.Persons.Remove(personToDelete);

            await _context.SaveChangesAsync(cToken);

            return Id;
        }


        public async Task<OneOf<List<PersonDtoBaseInfo>, APIError, ValidatorError>>
        GetAllAsync(CancellationToken cToken)
        {
            var result = await _context
            .Persons
            .Include(x => x.Skills)
            .ToListAsync(cToken);

            return _mapper.Map<List<PersonDtoBaseInfo>>(result);
        }


        public async Task<OneOf<PersonDtoBaseInfo, APIError, ValidatorError>>
        GetByIdAsync(long Id, CancellationToken cToken)
        {
            var result = await _context.Persons.
            Include(x => x.Skills)
            .FirstOrDefaultAsync(x => x.Id == Id, cToken);

            if (result is null) return new APIError(404, "No such person");

            return _mapper.Map<PersonDtoBaseInfo>(result);
        }


        public async Task<OneOf<long, APIError, ValidatorError>>
        UpdateAsync(long Id, PersonDtoUpdate dto, CancellationToken cToken)
        {
            var validation = _personUpdateValidator.Validate(dto);
            if (!validation.IsValid)
            {
                _logger.LogError("Validation Error: {Errors}", validation.Errors);
                return new ValidatorError(validation);
            }


            var personMapped = _mapper.Map<Person>(dto);

            var personToUpdate = await _context.Persons
            .Include(x => x.Skills)
            .FirstOrDefaultAsync(x => x.Id == Id, cancellationToken: cToken);

            if (personToUpdate is null) return new APIError(404, "No such person");

            personToUpdate.DisplayName = personMapped.DisplayName;
            personToUpdate.Name = personMapped.Name;

            //если Dto пустое, то удали все навыки
            if (personMapped.Skills is null)
            {
                if (personToUpdate.Skills is not null)
                {
                    _context.Skills.RemoveRange(personToUpdate.Skills);
                    await _context.SaveChangesAsync(cToken);
                }
                return Id;
            }

            //Если dto не пустое, то...
            //Если исходно навыков не было, то добавь их
            if (personToUpdate.Skills is null)
            {
                personToUpdate.Skills = personMapped.Skills;
                await _context.SaveChangesAsync(cToken);
                return Id;
            }

            //Если исходные навыки были, то
            //Удали навыки, которых нет в DTO
            var skillsToDelete = personToUpdate.Skills?
            .Where(skillToUpdate => !personMapped.Skills
            .Any(skillMapped => skillMapped.Name == skillToUpdate.Name))
            .ToList();

            if (skillsToDelete is not null) _context.Skills.RemoveRange(skillsToDelete);


            //Присвой старым корректные значениея
            foreach (var skillToUpdate in personToUpdate.Skills!)
            {
                var matchingSkill = personMapped.Skills.FirstOrDefault(skillMapped => skillMapped.Name == skillToUpdate.Name);
                if (matchingSkill != null)
                {
                    skillToUpdate.Level = matchingSkill.Level;
                }
            }

            // Добавь новые навыки
            var newSkills = personMapped.Skills?
                .Where(skillMapped => !personToUpdate.Skills
                .Any(skillToUpdate => skillToUpdate.Name == skillMapped.Name))
                .ToList();

            if (newSkills != null)
            {
                personToUpdate.Skills.AddRange(newSkills);
            };

            await _context.SaveChangesAsync(cToken);

            return Id;
        }
    }
}