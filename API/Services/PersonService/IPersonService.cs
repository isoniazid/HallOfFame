using HallOfFame.Infrastructure.ServiceResult;
using HallOfFame.DTO;
using OneOf;

namespace HallOfFame.Services.PersonService
{
    public interface IPersonService
    {
        public Task<OneOf<List<PersonDtoBaseInfo>,APIError,ValidatorError>> GetAllAsync(CancellationToken cToken);

        public Task<OneOf<PersonDtoBaseInfo, APIError, ValidatorError>> GetByIdAsync(long Id, CancellationToken cToken);

        public Task<OneOf<PersonDtoCreate, APIError, ValidatorError>> CreateAsync(PersonDtoCreate dto, CancellationToken cToken);

        public Task<OneOf<long, APIError, ValidatorError>> UpdateAsync(long Id, PersonDtoUpdate dto, CancellationToken cToken);

        public Task<OneOf<long, APIError>> DeleteAsync(long Id, CancellationToken cToken);

    }
}