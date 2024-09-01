namespace ChatSystem.Chat.API.Layers.Domain.Entities
{
    public class Shift : BaseEntity
    {
        public Guid Id { get; set; }
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
    }
}
