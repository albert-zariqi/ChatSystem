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

        public bool IsOverNormalCapacity(int currentCapacity)
        {
            return currentCapacity >= GetShiftNormalCapacity();
        }

        public bool IsOverOverflowCapacity(int currentCapacity)
        {
            return currentCapacity >= GetShiftOverflowCapacity();
        }

        public int GetShiftNormalCapacity()
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

        public int GetShiftOverflowCapacity()
        {
            if (!IsDuringOfficeHours())
                return 0;

            var maximumConcurrencyNumber = 10;
            var juniorLevelFactor = 0.4;
            var normalCapacity = GetShiftNormalCapacity();
            var overflowAgents = Teams.Where(x => !x.IsMainTeam).Count();
            var newCapacity = (int)(normalCapacity + (overflowAgents * maximumConcurrencyNumber * juniorLevelFactor));

            return newCapacity;
        }

        #region Navigation Properties

        public List<Team> Teams { get; set; }
        #endregion


    }
}
