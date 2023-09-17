using HallOfFame.DTO;
using HallOfFame.Services.PersonService;
using Microsoft.AspNetCore.Mvc;

namespace HallOfFame.Endpoints
{
    public class PersonEndpoints
    {
        public void Define(WebApplication app)
        {
            app.MapGet("api/v1/persons", GetAll).WithTags("Person")
            .Produces(200);

            app.MapGet("api/v1/persons/{id:long}", GetById).WithTags("Person")
            .Produces(200).Produces(404);

            app.MapPost("api/v1/persons", Create).WithTags("Person")
            .Produces(200).Produces(404).Produces(400);

            app.MapPut("api/v1/persons/{id:long}", Update).WithTags("Person")
            .Produces(200).Produces(404).Produces(400);

            app.MapDelete("api/v1/persons/{id:long}", Delete).WithTags("Person")
            .Produces(200).Produces(404);
        }

        public async Task<IResult> GetAll(IPersonService service, CancellationToken cToken)
        {
            var result = await service.GetAllAsync(cToken);

            return result.Match(
            list => Results.Ok(list),
            apiError => Results.NotFound(),
            validatorError => Results.BadRequest(validatorError.ValidationErrors));
        }

        public async Task<IResult> GetById(long id, IPersonService service, CancellationToken cToken)
        {
            var result = await service.GetByIdAsync(id, cToken);

            return result.Match(
            dto => Results.Ok(dto),
            apiError => Results.NotFound(),
            validatorError => Results.BadRequest(validatorError.ValidationErrors));
        }

        public async Task<IResult> Create([FromBody] PersonDtoCreate dto, IPersonService service, CancellationToken cToken)
        {
            var result = await service.CreateAsync(dto, cToken);

            return result.Match(
            dto => Results.Ok(),
            apiError => Results.NotFound(),
            validatorError => Results.BadRequest(validatorError.ValidationErrors));
        }

        public async Task<IResult> Update(long id, [FromBody] PersonDtoUpdate dto, IPersonService service, CancellationToken cToken)
        {
            var result = await service.UpdateAsync(id, dto, cToken);

            return result.Match(
            id => Results.Ok(),
            apiError => Results.NotFound(),
            validatorError => Results.BadRequest(validatorError.ValidationErrors));
        }

        public async Task<IResult> Delete(long id, IPersonService service, CancellationToken cToken)
        {
            var result = await service.DeleteAsync(id, cToken);

            return result.Match(
            id => Results.Ok(),
            apiError => Results.NotFound());
        }
    }
}