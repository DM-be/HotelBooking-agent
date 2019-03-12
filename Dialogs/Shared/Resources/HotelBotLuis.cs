// <auto-generated>
// Code generated by LUISGen .\CognitiveModels\LUIS\en\json\hotelbot.json -cs Luis.HotelBotLuis -o .\Dialogs\Shared\Resources
// Tool github: https://github.com/microsoft/botbuilder-tools
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
namespace Luis
{
    public class HotelBotLuis: IRecognizerConvert
    {
        public string Text;
        public string AlteredText;
        public enum Intent {
            Book_A_Room, 
            Call_Us, 
            Cancel, 
            Get_Directions, 
            Get_Location, 
            Greeting, 
            Help, 
            None, 
            Update_ArrivalDate, 
            Update_email, 
            Update_Leaving_Date, 
            Update_Number_Of_People
        };
        public Dictionary<Intent, IntentScore> Intents;

        public class _Entities
        {

            // Built-in entities
            public DateTimeSpec[] datetime;
            public string[] email;
            public double[] number;

            // Instance
            public class _Instance
            {
                public InstanceData[] datetime;
                public InstanceData[] email;
                public InstanceData[] number;
            }
            [JsonProperty("$instance")]
            public _Instance _instance;
        }
        public _Entities Entities;

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties {get; set; }

        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<HotelBotLuis>(JsonConvert.SerializeObject(result));
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        public (Intent intent, double score) TopIntent()
        {
            Intent maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}
