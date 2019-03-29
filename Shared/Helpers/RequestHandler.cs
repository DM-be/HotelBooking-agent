﻿using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
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
            FetchMatchingRoomsUrl = "https://us-central1-roomsbackend.cloudfunctions.net/fetchMatchingRooms";
            FetchRoomDetailUrl = "https://us-central1-roomsbackend.cloudfunctions.net/fetchRoomDetail";
        }


        // url for backend
        public string FetchMatchingRoomsUrl { get; set; }
        public string FetchRoomDetailUrl { get; set; }



        // returns general room cards // fake it for now.
        public async Task<RoomDto[]> FetchMatchingRooms(RoomRequestData predicate)
        {
            RoomDto[] roomDtos = null;
            var client = new HttpClient();
            var path = FetchMatchingRoomsUrl;
            var content = new StringContent(JsonConvert.SerializeObject(predicate), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(path, content);
            if (response.IsSuccessStatusCode)
            {
                var roomsAsStrings = await response.Content.ReadAsStringAsync();
                roomDtos = JsonConvert.DeserializeObject<RoomDto[]>(roomsAsStrings);
            }
            return roomDtos;
        }


        // fetches a detail of a room --> more RoomImages, checkin time etc etc 
        public async Task<RoomDetailDto> FetchRoomDetail(string roomId)
        {
            var roomIdObject = new
            {
                id = roomId
            };

            RoomDetailDto roomDetailDto = null;
            var client = new HttpClient();
            var path = FetchRoomDetailUrl;
            var content = new StringContent(JsonConvert.SerializeObject(roomIdObject), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(path, content);
            if (response.IsSuccessStatusCode)
            {
                var roomsAsStrings = await response.Content.ReadAsStringAsync();
                roomDetailDto = JsonConvert.DeserializeObject<RoomDetailDto>(roomsAsStrings);
            }

            return roomDetailDto;

        }
    }
}
