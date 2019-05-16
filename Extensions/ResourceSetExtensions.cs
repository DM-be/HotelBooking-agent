using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;


namespace HotelBot.Extensions
{
    public static class ResourceSetExtensions
    {
        public static string GenerateRandomResponse(this ResourceSet resourceSet, string key)
        {
          
            IDictionaryEnumerator id = resourceSet.GetEnumerator();
            List<dynamic> randomResponses = new List<dynamic>();
            while (id.MoveNext())
            {
                if (id.Key.ToString().StartsWith(key))
                {
                    var obj = new
                    {
                        Key = id.Key.ToString(),
                        Value = id.Value.ToString()
                    };
                    randomResponses.Add(obj);
                }
            }
            Random random = new Random();
            return randomResponses[random.Next(0, randomResponses.Count)].Value;
        }

    }
}
