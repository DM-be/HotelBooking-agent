namespace HotelBot.Dialogs.RoomOverview
{
    public class Booking
    {
        public int Id { get; set; }
        public string MainBookerName { get; set; }
        public bool Confirmed { get; set; } // paid for? use in cosmos db --> allow or dissallow updating booking objects 



    }
}
