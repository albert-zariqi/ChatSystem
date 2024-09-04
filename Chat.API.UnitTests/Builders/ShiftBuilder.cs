using Bogus;
using ChatSystem.Chat.API.Layers.Domain.Entities;
using Microsoft.VisualStudio.CodeCoverage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.API.UnitTests.Builders
{

    public class SeniorityLevel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Factor { get; set; }
    }

    public class ShiftBuilder
    {
        private Shift _shift = new();

        public ShiftBuilder WithDefaults()
        {
            var shiftId = Guid.NewGuid();
            var teamAId = Guid.NewGuid();
            var teamOId = Guid.NewGuid();

            var tlSeniorityId = Guid.NewGuid();
            var juniorId = Guid.NewGuid();
            var midLevelSeniorityId = Guid.NewGuid();

            _shift = new Shift
            {
                Id = shiftId,
                Name = "Office Shift",
                StartHour = 9,
                StartMinute = 0,
                EndHour = 16,
                EndMinute = 59,
                TimezoneId = "Central European Summer Time",
                Teams = new List<Team>
                {
                    new Team
                    {
                        Id = teamAId,
                        Name = "Team A",
                        ShiftId = shiftId,
                        IsMainTeam = true,
                        Agents = new List<Agent>
                        {
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = "qorb001",
                                Seniority = new Seniority
                                {
                                    Id = tlSeniorityId,
                                    Name = "SENIOR",
                                    Factor = 0.50m
                                }
                            },
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = "limo123",
                                Seniority = new Seniority
                                {
                                    Id = juniorId,
                                    Name = "JUNIOR",
                                    Factor = 0.40m
                                }
                            },
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = "ask002",
                                Seniority = new Seniority
                                {
                                    Id = midLevelSeniorityId,
                                    Name = "MID_LEVEL",
                                    Factor = 0.60m
                                }
                            },
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = "ask001",
                                Seniority = new Seniority
                                {
                                    Id = midLevelSeniorityId,
                                    Name = "MID_LEVEL",
                                    Factor = 0.60m
                                }
                            },
                        }
                    },
                    new Team
                    {
                        Id = teamOId,
                        Name = "Team O",
                        ShiftId = shiftId,
                        IsMainTeam = false,
                        Agents = new List<Agent>
                        {
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = new Faker().Name.FirstName(),
                                Seniority = new Seniority
                                {
                                    Id = juniorId,
                                    Name = "JUNIOR",
                                    Factor = 0.40m
                                }
                            },
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = new Faker().Name.FirstName(),
                                Seniority = new Seniority
                                {
                                    Id = juniorId,
                                    Name = "JUNIOR",
                                    Factor = 0.40m
                                }
                            },
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = "ask002",
                                Seniority = new Seniority
                                {
                                    Id = juniorId,
                                    Name = "JUNIOR",
                                    Factor = 0.40m
                                }
                            },
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = new Faker().Name.FirstName(),
                                Seniority = new Seniority
                                {
                                    Id = juniorId,
                                    Name = "JUNIOR",
                                    Factor = 0.40m
                                }
                            },
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = new Faker().Name.FirstName(),
                                Seniority = new Seniority
                                {
                                    Id = juniorId,
                                    Name = "JUNIOR",
                                    Factor = 0.40m
                                }
                            },
                            new Agent
                            {
                                Id = teamAId,
                                FirstName = new Faker().Name.FirstName(),
                                LastName = new Faker().Name.LastName(),
                                Username = new Faker().Name.FirstName(),
                                Seniority = new Seniority
                                {
                                    Id = juniorId,
                                    Name = "JUNIOR",
                                    Factor = 0.40m
                                }
                            },
                        }
                    }
                }
            };

            return this;
        }

        public ShiftBuilder WithId(Guid id)
        {
            _shift.Id = id;
            foreach (var item in _shift.Teams)
            {
                item.ShiftId = id;
            }
            return this;
        }

        public ShiftBuilder ClearTeams()
        {
            _shift.Teams.Clear();
            return this;
        }

        public ShiftBuilder WithStartEndTime(int startHour, int startMinute, int endHour, int endMinute)
        {
            _shift.StartHour = startHour;
            _shift.StartMinute = startMinute;
            _shift.EndMinute = endMinute;
            _shift.EndHour = endHour;
            return this;
        }

        public ShiftBuilder AddTeam(string teamName, bool isMainTeam)
        {
            _shift.Teams.Add(new Team
            {
                Id = Guid.NewGuid(),
                Name = teamName,
                ShiftId = _shift.Id,
                IsMainTeam = isMainTeam,
                Agents = new List<Agent>()
            });

            return this;
        }

        public ShiftBuilder AddToTeam(string teamName, string agentName, string seniority)
        {
            var team = _shift.Teams.Where(x => x.Name == teamName).FirstOrDefault();

            var seniorityLevel = SeniorityLevels.Where(x => x.Name == seniority).First();

            team.Agents.Add(new Agent
            {
                Id = team.Id,
                FirstName = new Faker().Name.FirstName(),
                LastName = new Faker().Name.LastName(),
                Username = agentName,
                Seniority = new Seniority
                {
                    Id = seniorityLevel.Id,
                    Name = seniorityLevel.Name,
                    Factor = seniorityLevel.Factor
                }
            });

            return this;
        }

        public Shift Build()
        {
            return _shift;
        }

        public static List<SeniorityLevel> SeniorityLevels = new List<SeniorityLevel>
        {
            new SeniorityLevel
            {
                Id = Guid.NewGuid(),
                Name = "SENIOR",
                Factor = 0.80m
            },
            new SeniorityLevel
            {
                Id = Guid.NewGuid(),
                Name = "JUNIOR",
                Factor = 0.40m
            },
            new SeniorityLevel
            {
                Id = Guid.NewGuid(),
                Name = "MID_LEVEL",
                Factor = 0.60m
            },
            new SeniorityLevel
            {
                Id = Guid.NewGuid(),
                Name = "TEAM_LEAD",
                Factor = 0.50m
            }
        };
    }
}
