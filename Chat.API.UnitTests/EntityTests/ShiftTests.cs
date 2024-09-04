using Bogus;
using Chat.API.UnitTests.Builders;
using ChatSystem.Chat.API.Layers.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.API.UnitTests.EntityTests
{
    public class ShiftTests
    {
        public ShiftTests()
        {
        }

        [Theory]
        [InlineData(9, 0, 16, 59, true)] // true
        [InlineData(17, 0, 0, 59, false)] // false
        [InlineData(12, 53, 2, 56, false)] // false
        public async Task Shift_IsDuringOfficeHours_ShouldReturnBasedOnPararms(int startHour, int startMinute, int endHour, int endMinute, bool expectedResult)
        {
            #region Arrange

            // Preperations for the test
            // 1 team
            // 1 agent teamlead, 1 agent senior, 1 agent midlevel, 1 agent junior

            Shift shift = new ShiftBuilder()
                .WithDefaults()
                .WithStartEndTime(startHour, startMinute, endHour, endMinute)
                .Build();

            #endregion

            #region Act

            // You execute the method you want to test
            var isDuringOfficeHoursShift = shift.IsDuringOfficeHours();

            #endregion

            #region Assert

            Assert.Equal(expectedResult, isDuringOfficeHoursShift);



            #endregion
        }

        [Fact]
        public async Task Shift_GetNormalConcurrentChatLimit_ShouldReturnCorrectNumber()
        {
            #region Arrange

            // Preperations for the test
            // 1 team
            // 1 agent teamlead, 1 agent senior, 1 agent midlevel, 1 agent junior

            Shift shift = new ShiftBuilder()
                .WithDefaults()
                .ClearTeams()
                .AddTeam("TEAM_A", true)
                .AddToTeam("TEAM_A", new Faker().Name.FirstName(), "TEAM_LEAD")
                .AddToTeam("TEAM_A", new Faker().Name.FirstName(), "SENIOR")
                .AddToTeam("TEAM_A", new Faker().Name.FirstName(), "MID_LEVEL")
                .AddToTeam("TEAM_A", new Faker().Name.FirstName(), "JUNIOR")
                .Build();

            #endregion

            #region Act

            // You execute the method you want to test
            var concurrentChatLimit = shift.GetNormalConcurrentChatLimit();

            #endregion

            #region Assert

            // The result I expect is: 
            // (1 (TL) * 10 * 0.5) + (1 (S) * 10 * 0.8) + (1 (M) * 10 * 0.6) + (1 (J) * 10 * 0.4)
            // 23
            // You assert the results
            Assert.Equal(23, concurrentChatLimit);
            Assert.Equal((int)(23 * 1.5), shift.GetNormalQueueLimit());

            #endregion
        }
    }
}
