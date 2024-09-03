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
            if (StartHour >= 9 && StartMinute >= 0 && EndHour >= 14 && EndMinute < 0)
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
