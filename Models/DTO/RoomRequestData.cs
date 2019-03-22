namespace HotelBot.Models.DTO
{

    // see 
    // https://reservations.cubilis.eu/270/Rooms/Select?Arrival=2019-3-22&Departure=2019-3-30&Room=&Rate=&Package=&DiscountCode=
    // also
    // {"Arrival":"2019-03-22","Departure":"2019-03-30","Room":null,"Package":null,"Rate":null,"DiscountCode":null,"Adults":null,"Children":null}

    public class RoomRequestData
    {
        public string Arrival { get; set; }
        public string Departure { get; set; }
        public string DiscountCode { get; set; }

        public string Room { get; set; } // unclear what room is --> its in the query params

        public string Package { get; set; }
        public string Rate { get; set; }

        public string Adults { get; set; }

        public string Children { get; set; }
    }
}
