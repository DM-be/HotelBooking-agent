using System;
using HotelBot.Models.DTO;

namespace HotelBot.Dialogs.RoomOverview
{
    [Serializable]
    public class SelectedRoom
    {

        public RoomDetailDto RoomDetailDto { get; set; }
        public RoomRate SelectedRate { get; set; }
        public string Id { get; } = Guid.NewGuid().ToString();
    }
}
