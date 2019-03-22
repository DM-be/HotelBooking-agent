using System.Net.Http;
using System.Threading.Tasks;
using HotelBot.Models.DTO;
using Newtonsoft.Json;

namespace HotelBot.Shared.Helpers
{
    public class RequestHandler
    {


        // send to backend api
        public RequestHandler()
        {
            BaseURL = "";
        }


        // url for backend
        public string BaseURL { get; set; }



        // returns general room cards // fake it for now.
        public async Task<RoomDto []> FetchMatchingRooms(dynamic predicate)
        {


            // todo: set up 
            RoomDto [] roomDtos = null;
            var client = new HttpClient();
            var path = BaseURL + ""; // set correct backend path
            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var roomsAsStrings = await response.Content.ReadAsStringAsync();
                roomDtos = JsonConvert.DeserializeObject<RoomDto []>(roomsAsStrings);
            }

            return roomDtos;
        }


        // fetches a detail of a room --> more RoomImages, checkin time etc etc 
        public async Task<RoomDetailDto> FetchRoomDetail(string id)
        {
            RoomDetailDto roomDetailDto = null;
            var client = new HttpClient();
            var path = BaseURL + id; // implement correct route
            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var roomsAsStrings = await response.Content.ReadAsStringAsync();
                roomDetailDto = JsonConvert.DeserializeObject<RoomDetailDto>(roomsAsStrings);
            }

            return roomDetailDto;

        }
    }
}
