using Bogus;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.API.Layers.Infrastructure.Data.Interceptors;
using ChatSystem.Chat.API.Layers.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using ChatSystem.Caching.CachingKeys;
using ChatSystem.Caching.Models;

namespace ChatSystem.Chat.API
{
    public class ChatSeeder
    {
        private readonly string _connectionString;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICachingService _cachingService;

        public ChatSeeder(string connectionString, ICurrentUserService currentUserService, ICachingService cachingService)
        {
            _connectionString = connectionString;
            _currentUserService = currentUserService;
            _cachingService = cachingService;
        }

        public ChatDbContext CreateContext()
        {
            // Create an instance of DbContextOptionsBuilder for InsrdDbContext
            var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();

            // Configure the options to use SQL Server with the connection string
            optionsBuilder.UseSqlServer(_connectionString);

            // Add your interceptor
            optionsBuilder.AddInterceptors(new AuditableEntityInterceptor(_currentUserService));

            // Create a new instance of InsrdDbContext with the configured options
            return new ChatDbContext(optionsBuilder.Options);
        }

        public async Task SeedDefaultCache()
        {
            // This should happen when the shift changes, and when the other shift employees clock in.
            // We will just abstract this logic here for our purposes.
            var _context = CreateContext();
            TimeOnly currentTime = TimeOnly.FromDateTime(DateTime.Now);
            var shift = await _context.Shifts
            .Where(x =>
                (x.StartHour < x.EndHour && currentTime.Hour >= x.StartHour && currentTime.Hour <= x.EndHour) ||
                (x.StartHour > x.EndHour &&
                    (currentTime.Hour >= x.StartHour || currentTime.Hour <= x.EndHour))
            )
            .Include(x => x.Teams)
            .ThenInclude(x => x.Agents)
            .ThenInclude(x => x.Seniority)
            .SingleOrDefaultAsync();

            // Shift Capacity
            var cacheKey = ShiftCachingKeys.CapacityByShift(shift.Id);
            await _cachingService.SetAsync(cacheKey, new ShiftCapacityCacheModel
            {
                CurrentActiveSessions = 0,
                OverflowAgentsRequested = false,
            });

            // Agents in Shift
            List<AgentsInShiftCacheModel> agentsInShift = shift!.Teams.Where(x => x.IsMainTeam).FirstOrDefault()!.Agents.Select(x => new AgentsInShiftCacheModel
            {
                Username = x.Username,
                Seniority = x.Seniority.Name,
                Factor = x.Seniority.Factor
            }).ToList();
            cacheKey = ShiftCachingKeys.AgentsInShift(shift.Id);
            await _cachingService.SetAsync(cacheKey, agentsInShift);

        }

        public async Task SeedDefaultData()
        {
            var _context = CreateContext();
            await _context.Database.EnsureCreatedAsync();

            await _context.Agents.ExecuteDeleteAsync();
            await _context.Teams.ExecuteDeleteAsync();
            await _context.Shifts.ExecuteDeleteAsync();
            await _context.Seniorities.ExecuteDeleteAsync();
            // Shifts
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
            var officeHoursShift = (await _context.Shifts.AddAsync(new Layers.Domain.Entities.Shift
            {
                Id = Guid.NewGuid(),
                Name = "Office Hours Shift",
                StartHour = 9,
                StartMinute = 0,
                EndHour = 16,
                EndMinute = 59,
                TimezoneId = timeZoneInfo.Id
            })).Entity;
            var eveningShift = (await _context.Shifts.AddAsync(new Layers.Domain.Entities.Shift
            {
                Id = Guid.NewGuid(),
                Name = "Evening Shift",
                StartHour = 17,
                StartMinute = 0,
                EndHour = 0,
                EndMinute = 59,
                TimezoneId = timeZoneInfo.Id
            })).Entity;
            var nightShift = (await _context.Shifts.AddAsync(new Layers.Domain.Entities.Shift
            {
                Id = Guid.NewGuid(),
                Name = "Night Shift",
                StartHour = 1,
                StartMinute = 0,
                EndHour = 8,
                EndMinute = 59,
                TimezoneId = timeZoneInfo.Id
            })).Entity;

            // Teams
            var teamA = (await _context.Teams.AddAsync(new Layers.Domain.Entities.Team
            {
                Id = Guid.NewGuid(),
                Name = "TEAM_A",
                IsMainTeam = true,
                ShiftId = officeHoursShift.Id,
            })).Entity;

            var teamB = (await _context.Teams.AddAsync(new Layers.Domain.Entities.Team
            {
                Id = Guid.NewGuid(),
                Name = "TEAM_B",
                IsMainTeam = true,
                ShiftId = eveningShift.Id
            })).Entity;

            var teamC = (await _context.Teams.AddAsync(new Layers.Domain.Entities.Team
            {
                Id = Guid.NewGuid(),
                Name = "TEAM_C",
                IsMainTeam = true,
                ShiftId = nightShift.Id
            })).Entity;

            var teamO = (await _context.Teams.AddAsync(new Layers.Domain.Entities.Team
            {
                Id = Guid.NewGuid(),
                Name = "TEAM_O",
                IsMainTeam = false,
                ShiftId = officeHoursShift.Id
            })).Entity;


            // Seniority
            var teamLeadLevel = (await _context.Seniorities.AddAsync(new Layers.Domain.Entities.Seniority
            {
                Id = Guid.NewGuid(),
                Factor = 0.5m,
                Name = "TEAM_LEAD"
            })).Entity;

            var seniorLevel = (await _context.Seniorities.AddAsync(new Layers.Domain.Entities.Seniority
            {
                Id = Guid.NewGuid(),
                Factor = 0.8m,
                Name = "SENIOR"
            })).Entity;

            var midLevelLevel = (await _context.Seniorities.AddAsync(new Layers.Domain.Entities.Seniority
            {
                Id = Guid.NewGuid(),
                Factor = 0.6m,
                Name = "MID_LEVEL"
            })).Entity;

            var juniorLevel = (await _context.Seniorities.AddAsync(new Layers.Domain.Entities.Seniority
            {
                Id = Guid.NewGuid(),
                Factor = 0.4m,
                Name = "JUNIOR"
            })).Entity;

            // Agents in Team A
            Faker faker = new Faker();
            var teamLeadA = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "qorb001",
                SeniorityId = teamLeadLevel.Id,
                TeamId = teamA.Id
            })).Entity;

            faker = new Faker();
            var midLevelA1 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "ask001",
                SeniorityId = midLevelLevel.Id,
                TeamId = teamA.Id
            })).Entity;

            faker = new Faker();
            var midLevelA2 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "ask002",
                SeniorityId = midLevelLevel.Id,
                TeamId = teamA.Id
            })).Entity;

            faker = new Faker();
            var juniorA1 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "limo123",
                SeniorityId = juniorLevel.Id,
                TeamId = teamA.Id
            })).Entity;

            // Agents in Team B
            faker = new Faker();
            var seniorb1 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "vish007",
                SeniorityId = seniorLevel.Id,
                TeamId = teamB.Id
            })).Entity;

            faker = new Faker();
            var midLevelB1 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "xeno999",
                SeniorityId = midLevelLevel.Id,
                TeamId = teamB.Id
            })).Entity;

            faker = new Faker();
            var juniorB1 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "jazz456",
                SeniorityId = juniorLevel.Id,
                TeamId = teamB.Id
            })).Entity;

            faker = new Faker();
            var juniorB2 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "zulu789",
                SeniorityId = juniorLevel.Id,
                TeamId = teamB.Id
            })).Entity;

            // Agents in Team C
            faker = new Faker();
            var midLevelC1 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "zest001",
                SeniorityId = midLevelLevel.Id,
                TeamId = teamC.Id
            })).Entity;

            faker = new Faker();
            var midLevelC2 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "morp321",
                SeniorityId = midLevelLevel.Id,
                TeamId = teamC.Id
            })).Entity;

            // Agents in Team O
            faker = new Faker();
            var juniorO1 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "kilo654",
                SeniorityId = juniorLevel.Id,
                TeamId = teamO.Id
            })).Entity;

            faker = new Faker();
            var juniorO2 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "grid345",
                SeniorityId = juniorLevel.Id,
                TeamId = teamO.Id
            })).Entity;

            faker = new Faker();
            var juniorO3 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "flux321",
                SeniorityId = juniorLevel.Id,
                TeamId = teamO.Id
            })).Entity;

            faker = new Faker();
            var juniorO4 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "rock123",
                SeniorityId = juniorLevel.Id,
                TeamId = teamO.Id
            })).Entity;

            faker = new Faker();
            var juniorO5 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "bliz789",
                SeniorityId = juniorLevel.Id,
                TeamId = teamO.Id
            })).Entity;

            faker = new Faker();
            var juniorO6 = (await _context.Agents.AddAsync(new Layers.Domain.Entities.Agent
            {
                Id = Guid.NewGuid(),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Username = "star456",
                SeniorityId = juniorLevel.Id,
                TeamId = teamO.Id
            })).Entity;

            await _context.SaveChangesAsync();
        }
    }
}
