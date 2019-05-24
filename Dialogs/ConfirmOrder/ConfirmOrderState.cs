using HotelBot.Dialogs.RoomOverview;

namespace HotelBot.Dialogs.ConfirmOrder
{
    public class ConfirmOrderState
    {
        public string Email { get; set; }
        public string Number { get; set; }
        public string FullName { get; set; }

        public RoomOverviewState RoomOverviewState { get; set; }

        public bool PaymentConfirmed { get; set; } = false;
    }
}
