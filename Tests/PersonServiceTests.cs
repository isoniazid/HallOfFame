using AutoMapper;
using HallOfFame.Infrastructure;
using HallOfFame.Services.PersonService;
using HallOfFame.Validators;
using Microsoft.EntityFrameworkCore;
using Moq.EntityFrameworkCore;
using Moq;
using HallOfFame.Infrastructure.ServiceResult;
using Microsoft.Extensions.Logging;
using Bogus;
using HallOfFame.DTO;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Tests
{
    public class PersonServiceTests
    {
        private HallOfFame.Services.PersonService.PersonService _service;

        private readonly IMapper _mapper;

        private Mock<ApplicationDbContext> _contextMock = new();
        private Mock<ILogger<PersonService>> _loggerMock = new();

        private readonly CancellationToken _ctoken = new();


        //
        //Фейковые фабрики для тестовых данных:
        //

        private static long skillIds = 1;
        private static Faker<HallOfFame.Model.Skill> TestSkillGenerator = new Faker<HallOfFame.Model.Skill>()
            .CustomInstantiator(x => new HallOfFame.Model.Skill() { Id = skillIds++ })
            .RuleFor(x => x.Name, y => y.Name.JobTitle())
            .RuleFor(x => x.Level, y => (byte)new Random().Next(1, 11));

        private static long personIds = 1;



        private static Faker<HallOfFame.Model.Person> TestPersonGenerator = new Faker<HallOfFame.Model.Person>()
        .CustomInstantiator(x => new HallOfFame.Model.Person() { Id = personIds++ })
        .RuleFor(x => x.DisplayName, y => y.Internet.UserName())
        .RuleFor(x => x.Name, y => y.Name.FullName())
        .RuleFor(x => x.Skills, y => TestSkillGenerator.Generate(3).ToList());

        private static Faker<HallOfFame.DTO.SkillDtoCreate> TestSkillDtoCreateGenerator = new Faker<HallOfFame.DTO.SkillDtoCreate>()
        .RuleFor(x => x.Level, y => (byte)new Random().Next(1, 11))
        .RuleFor(x => x.Name, y => y.Name.JobTitle());

        private static Faker<HallOfFame.DTO.PersonDtoCreate> TestPersonDtoCreateGenerator = new Faker<HallOfFame.DTO.PersonDtoCreate>()
        .RuleFor(x => x.DisplayName, y => y.Internet.UserName())
        .RuleFor(x => x.Name, y => y.Name.FullName())
        .RuleFor(x => x.Skills, y => TestSkillDtoCreateGenerator.Generate(3).ToList());

        private static Faker<HallOfFame.DTO.PersonDtoUpdate> TestPersonDtoUpdateGenerator = new Faker<HallOfFame.DTO.PersonDtoUpdate>()
       .RuleFor(x => x.DisplayName, y => y.Internet.UserName())
       .RuleFor(x => x.Name, y => y.Name.FullName())
       .RuleFor(x => x.Skills, y => TestSkillDtoCreateGenerator.Generate(3).ToList());

        public PersonServiceTests()
        {
            var mapperConfig = new MapperConfiguration(x => x.AddProfile(new ApplicationProfile()));

            _mapper = new Mapper(mapperConfig);
            _service = new PersonService(_loggerMock.Object,
             _mapper, _contextMock.Object,
              new PersonDtoCreateValidator(),
               new PersonDtoUpdateValidator());
        }

        [Fact]
        public async Task GetById_ReturnsAPIError_IfDoesNotExist()
        {
            //Arrange

            _contextMock.Setup<DbSet<HallOfFame.Model.Person>>(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            long inputValue = new Random().NextInt64(long.MaxValue);

            //Act
            var result = await _service.GetByIdAsync(inputValue, _ctoken);


            //Assert
            Assert.True(result.TryPickT1(out APIError apiError, out var irrelevant));
        }

        [Fact]
        public async Task GetById_ReturnsDto_IfDoesExist()
        {
            //Arrange
            var TestPerson = TestPersonGenerator.Generate();

            var fakeResult = _mapper.Map<PersonDtoBaseInfo>(TestPerson);

            _contextMock.Setup<DbSet<HallOfFame.Model.Person>>(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>
            {
                TestPerson
            });


            long inputValue = TestPerson.Id;

            //Act
            var result = await _service.GetByIdAsync(inputValue, _ctoken);

            //Assert
            if (result.TryPickT0(out PersonDtoBaseInfo resultDto, out var irrelevant))
            {
                Assert.Equivalent(resultDto, fakeResult);
            }

            else
            {
                Assert.Fail("");
            }
        }

        [Fact]
        public async Task GetAll_ReturnsAllDtos()
        {
            //Arrange
            var number = new Random().Next(1, 101);
            var TestPersonList = TestPersonGenerator
            .Generate(number).ToList();

            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(TestPersonList.AsQueryable());

            var fakeResult = _mapper.Map<List<PersonDtoBaseInfo>>(TestPersonList);

            //act
            var result = await _service.GetAllAsync(_ctoken);


            //assert
            if (result.TryPickT0(out List<PersonDtoBaseInfo> resultList, out var irrelevant))
            {
                Assert.Equivalent(resultList, fakeResult);
            }

            else
            {
                Assert.Fail("");
            }
        }


        [Fact]
        public async Task GetAll_ReturnsEmptyWhenEmpty()
        {
            //Arrange
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            //act
            var result = await _service.GetAllAsync(_ctoken);

            //assert
            if (result.TryPickT0(out List<PersonDtoBaseInfo> resultList, out var irrelevant))
            {
                Assert.Empty(resultList);
            }
        }


        [Fact]
        public async Task DeleteById_ReturnsAPIError_IfDoesNotExist()
        {
            //Arrange
            _contextMock.Setup<DbSet<HallOfFame.Model.Person>>(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            long inputValue = new Random().NextInt64(long.MaxValue);

            //Act
            var result = await _service.DeleteAsync(inputValue, _ctoken);


            //Assert
            Assert.True(result.TryPickT1(out APIError apiError, out var irrelevant));
        }

        [Fact]
        public async Task DeleteById_Deletes_IfDoesExist()
        {
            //Arrange
            var number = new Random().Next(1, 101);

            var TestPersonList = TestPersonGenerator
            .Generate(number).ToList();

            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(TestPersonList);

            long inputValue = TestPersonList[new Random().Next(TestPersonList.Count)].Id;

            //Act
            var result = await _service.DeleteAsync(inputValue, _ctoken);


            //Assert
            if (result.TryPickT0(out long id, out var irrelevant))
            {
                Assert.Null(_contextMock.Object.Persons.Find(inputValue));
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_ValidDto_IsCreated()
        {

            // Arrange 

            var fakeDb = new List<HallOfFame.Model.Person>();

            _contextMock.Setup(m => m.Persons.AddAsync(It.IsAny<HallOfFame.Model.Person>(), default))
            .Callback<HallOfFame.Model.Person, CancellationToken>((s, token) => { fakeDb.Add(s); });

            _contextMock.Setup(c => c.SaveChangesAsync(default))
            .Returns(Task.FromResult(1))
            .Verifiable();

            var dto = TestPersonDtoCreateGenerator.Generate();
            var mappedDto = _mapper.Map<HallOfFame.Model.Person>(dto);
            mappedDto.Id = 0;


            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT0(out PersonDtoCreate resultList, out var irrelevant))
            {
                Assert.NotNull(fakeDb.FirstOrDefault(x => x.Id == mappedDto.Id));
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_InvalidDto_RepeatedSkillNames_ValError()
        {
            // Arrange 
            var dto = TestPersonDtoCreateGenerator.Generate();
            dto.Skills![1] = dto.Skills![0];
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_InvalidDto_PersonName_Zero_ValError()
        {
            // Arrange 
            var dto = TestPersonDtoCreateGenerator.Generate();
            dto.Name = "";
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_InvalidDto_PersonName_Long_ValError()
        {
            // Arrange 
            var dto = TestPersonDtoCreateGenerator.Generate();
            dto.Name = new string('a', 51);
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_InvalidDto_PersonDisplayName_Long_ValError()
        {
            // Arrange 
            var dto = TestPersonDtoCreateGenerator.Generate();
            dto.DisplayName = new string('a', 51);
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_InvalidDto_PersonDisplayName_Zero_ValError()
        {
            // Arrange 
            var dto = TestPersonDtoCreateGenerator.Generate();
            dto.DisplayName = "";
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_InvalidDto_SkillName_Zero_ValError()
        {
            // Arrange 
            var dto = TestPersonDtoCreateGenerator.Generate();
            dto.Skills![0].Name = "";
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_InvalidDto_SkillName_Long_ValError()
        {
            // Arrange 
            var dto = TestPersonDtoCreateGenerator.Generate();
            dto.Skills![0].Name = new string('a', 51);
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_InvalidDto_SkillLevel_LessThan1_ValError()
        {
            // Arrange 
            var dto = TestPersonDtoCreateGenerator.Generate();
            dto.Skills![0].Level = (byte)new Random().Next(byte.MinValue, 0);
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task CreateAsync_InvalidDto_SkillLevel_MoreThan10_ValError()
        {
            // Arrange 
            var dto = TestPersonDtoCreateGenerator.Generate();
            dto.Skills![0].Level = (byte)new Random().Next(11, byte.MaxValue);
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            // Act 
            var result = await _service.CreateAsync(dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_ValidDto_IsUpdated()
        {
            var fakeDb = new List<HallOfFame.Model.Person>() {TestPersonGenerator.Generate()};

            fakeDb[0].Id = 0;
            var skillsDbSet = fakeDb[0].Skills;

             _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(fakeDb);

            _contextMock.Setup(x => x.Skills).ReturnsDbSet(skillsDbSet);

            _contextMock.Setup(m => m.Persons.AddAsync(It.IsAny<HallOfFame.Model.Person>(), default))
            .Callback<HallOfFame.Model.Person, CancellationToken>((s, token) => { fakeDb.Add(s); });

            _contextMock.Setup(c => c.SaveChangesAsync(default))
            .Returns(Task.FromResult(1))
            .Verifiable();

            var dto = TestPersonDtoUpdateGenerator.Generate();
            var mappedDto = _mapper.Map<HallOfFame.Model.Person>(dto);
            mappedDto.Id = 0;


            // Act 
            var result = await _service.UpdateAsync(mappedDto.Id ,dto, _ctoken);

            // Assert 
            if (result.TryPickT0(out long resultId, out var irrelevant))
            {
                Assert.Equivalent(mappedDto, fakeDb[0]);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_InvalidDto_RepeatedSkillNames_ValError()
        {
            // Arrange
            var testPerson = TestPersonGenerator.Generate();
            var dto = TestPersonDtoUpdateGenerator.Generate();
            var inputId = testPerson.Id;
            dto.Skills![1].Name = dto.Skills![0].Name;
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>() { testPerson });

            // Act 
            var result = await _service.UpdateAsync(inputId, dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_InvalidDto_PersonName_Zero_ValError()
        {
            // Arrange
            var testPerson = TestPersonGenerator.Generate();
            var dto = TestPersonDtoUpdateGenerator.Generate();
            var inputId = testPerson.Id;
            dto.Name = "";
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>() { testPerson });

            // Act 
            var result = await _service.UpdateAsync(inputId, dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_InvalidDto_PersonName_Long_ValError()
        {
            // Arrange 
            var testPerson = TestPersonGenerator.Generate();
            var dto = TestPersonDtoUpdateGenerator.Generate();
            var inputId = testPerson.Id;
            dto.Name = new string('g', 51);
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>() { testPerson });

            // Act 
            var result = await _service.UpdateAsync(inputId, dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_InvalidDto_PersonDisplayName_Long_ValError()
        {
            // Arrange 
            var testPerson = TestPersonGenerator.Generate();
            var dto = TestPersonDtoUpdateGenerator.Generate();
            var inputId = testPerson.Id;
            dto.DisplayName = new string('x', 51);
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>() { testPerson });

            // Act 
            var result = await _service.UpdateAsync(inputId, dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_InvalidDto_PersonDisplayName_Zero_ValError()
        {
            // Arrange 
            var testPerson = TestPersonGenerator.Generate();
            var dto = TestPersonDtoUpdateGenerator.Generate();
            var inputId = testPerson.Id;
            dto.DisplayName = "";
            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>() { testPerson });

            // Act 
            var result = await _service.UpdateAsync(inputId, dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_InvalidDto_SkillName_Zero_ValError()
        {
            // Arrange 
            var testPerson = TestPersonGenerator.Generate();
            var dto = TestPersonDtoUpdateGenerator.Generate();
            dto.Skills![0].Name = "";
            var inputId = testPerson.Id;

            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>() { testPerson });

            // Act 
            var result = await _service.UpdateAsync(inputId, dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_InvalidDto_SkillName_Long_ValError()
        {
            // Arrange 
            var testPerson = TestPersonGenerator.Generate();
            var dto = TestPersonDtoUpdateGenerator.Generate();
            dto.Skills![0].Name = new string('a', 51);
            var inputId = testPerson.Id;

            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>() { testPerson });

            // Act 
            var result = await _service.UpdateAsync(inputId, dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_InvalidDto_SkillLevel_LessThan1_ValError()
        {
            // Arrange 
            var testPerson = TestPersonGenerator.Generate();
            var dto = TestPersonDtoUpdateGenerator.Generate();
            dto.Skills![0].Level = (byte)new Random().Next(byte.MinValue, 0);
            dto.Skills![0].Name = "";
            var inputId = testPerson.Id;

            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>() { testPerson });

            // Act 
            var result = await _service.UpdateAsync(inputId, dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_InvalidDto_SkillLevel_MoreThan10_ValError()
        {
            // Arrange 
            var testPerson = TestPersonGenerator.Generate();
            var dto = TestPersonDtoUpdateGenerator.Generate();
            dto.Skills![0].Level = (byte)new Random().Next(11, byte.MaxValue);
            dto.Skills![0].Name = "";
            var inputId = testPerson.Id;

            _contextMock.Setup(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>() { testPerson });

            // Act 
            var result = await _service.UpdateAsync(inputId, dto, _ctoken);

            // Assert 
            if (result.TryPickT2(out ValidatorError error, out var irrelevant))
            {
                Assert.IsType<ValidatorError>(error);
            }

            else Assert.Fail("");
        }

        [Fact]
        public async Task UpdateAsync_ReturnsAPIError_IfDoesNotExist()
        {
            //Arrange
            var dto = TestPersonDtoUpdateGenerator.Generate();

            _contextMock.Setup<DbSet<HallOfFame.Model.Person>>(x => x.Persons)
            .ReturnsDbSet(new List<HallOfFame.Model.Person>());

            long inputValue = new Random().NextInt64(long.MaxValue);

            //Act
            var result = await _service.UpdateAsync(inputValue, dto, _ctoken);


            //Assert
            Assert.True(result.TryPickT1(out APIError apiError, out var irrelevant));
        }
    }
}