namespace ChatSystem.Chat.API.Layers.Domain.Entities
{
    public class Shift : BaseEntity
    {
        public string Name { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public string TimezoneId { get; set; }

        public bool IsDuringOfficeHours()
        {
            TimeOnly officeStart = new TimeOnly(9, 0);  // 9:00 AM
            TimeOnly officeEnd = new TimeOnly(16, 59);  // 4:59 PM

            TimeOnly shiftStart = new TimeOnly(StartHour, StartMinute);
            TimeOnly shiftEnd = new TimeOnly(EndHour, EndMinute);

            if (shiftStart >= officeStart && shiftEnd <= officeEnd && shiftEnd > shiftStart)
                return true;

            return false;
        }

        public bool CanAskForOverflowAgents()
        {
            return IsDuringOfficeHours();
        }

        public int GetNormalConcurrentChatLimit()
        {
            var maximumConcurrencyNumber = 10;
            var mainTeam = Teams.Where(x => x.IsMainTeam).Single();

            var teamLeadsCount = mainTeam.Agents.Count(x => x.Seniority.Name == "TEAM_LEAD");
            var seniorsCount = mainTeam.Agents.Count(x => x.Seniority.Name == "SENIOR");
            var midLevelsCount = mainTeam.Agents.Count(x => x.Seniority.Name == "MID_LEVEL");
            var juniorsCount = mainTeam.Agents.Count(x => x.Seniority.Name == "JUNIOR");

            var teamLeadFactor = mainTeam.Agents.Where(x => x.Seniority.Name == "TEAM_LEAD").Select(x => x.Seniority).Select(x => x.Factor).FirstOrDefault();
            var seniorFactor = mainTeam.Agents.Where(x => x.Seniority.Name == "SENIOR").Select(x => x.Seniority).Select(x => x.Factor).FirstOrDefault();
            var midLevelFactor = mainTeam.Agents.Where(x => x.Seniority.Name == "MID_LEVEL").Select(x => x.Seniority).Select(x => x.Factor).FirstOrDefault();
            var juniorFactor = mainTeam.Agents.Where(x => x.Seniority.Name == "JUNIOR").Select(x => x.Seniority).Select(x => x.Factor).FirstOrDefault();

            var normalCapacity = (int)(
                (teamLeadsCount * maximumConcurrencyNumber * teamLeadFactor) +
                (seniorsCount * maximumConcurrencyNumber * seniorFactor) +
                (midLevelsCount * maximumConcurrencyNumber * midLevelFactor) +
                (juniorsCount * maximumConcurrencyNumber * juniorFactor));

            return normalCapacity;
        }

        public int GetNormalQueueLimit()
        {
            return (int)(GetNormalConcurrentChatLimit() * 1.5);
        }

        public int GetOverflowQueueLimit()
        {
            return (int)(GetOverflowConcurrentChatLimit() * 1.5);
        }

        public int GetOverflowConcurrentChatLimit()
        {
            if (!IsDuringOfficeHours())
                return GetNormalConcurrentChatLimit();

            var maximumConcurrencyNumber = 10;
            var juniorLevelFactor = 0.4;
            var normalCapacity = GetNormalQueueLimit();
            var overflowAgents = Teams.Where(x => !x.IsMainTeam).Count();
            var newMaxConcurrentChats = (int)(normalCapacity + (overflowAgents * maximumConcurrencyNumber * juniorLevelFactor));
            return newMaxConcurrentChats;
        }

        public bool IsOverNormalQueueCapacity(int currentActiveSessions)
        {
            return currentActiveSessions >= GetNormalQueueLimit();
        }

        public bool IsOverOverflowQueueCapacity(int currentActiveSessions)
        {
            return currentActiveSessions >= GetOverflowQueueLimit();
        }


        #region Navigation Properties

        public List<Team> Teams { get; set; }
        #endregion
    }
}
