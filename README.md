# Hotel Booking agent

## Description
This project was built during my internship for the company Stardekk. Stardekk specializes in cloud software for the hospitality sector.
They wondered if a chatbot could be interesting to deploy for their clients. My bachelor's <a href="https://github.com/DM-be/HotelBooking-agent/raw/master/Bachelorproef_Dennis_Morent.pdf"> thesis </a>
 was built around this interest.

The goal was to build a conversational agent that could be contacted on a hotel Facebook page. This agent would guide a client to book a room or to answer questions such as the hotel location or phone number. 

#### Quick replies
Conversations are guided with quick replies. These are text pop ups users can use to send a text message in a quick and simple way. Manually typing the same text yields the same result. Some interactions are made easier with a quick reply. For example sending your own location to get directions to the hotel. The quick replies are also used to provide suggestions on what to do next. 

### Technical
The agent is a web server responding to HTTP requests. Within the body of these requests is information about sender, the text sent, channel data,... The Microsoft bot framework leverages this data and provides ways to support a natural flow of a conversation between the sender and the agent. 

<img src="https://docs.microsoft.com/en-us/azure/bot-service/v4sdk/media/bot-builder-activity.png?view=azure-bot-service-4.0">

I go into full technical detail in my <a href="https://github.com/DM-be/HotelBooking-agent/raw/master/Bachelorproef_Dennis_Morent.pdf"> thesis</a>, supported with code examples and schematics. 

#### LUIS
LUIS is natural language understanding service provided by Microsoft. It can recognize user intents and entities. Every response in a dialog is first interpreted by LUIS, according to the recognized intents actions can be taken. For example a user booking a room and realising he/she made a mistake about the checkin date can just say "change my check-in date to next monday" and LUIS would recognize this intent, extract monday as the check-in date and make it available for the agent. 

### QnA maker
QnA maker is another service provided by Microsoft. It matches questions with answers. A FAQ list is also implemented in the main dialog of this agent.




## Features
* full natural language integration through LUIS service
* frequently asked questions integration through the use of QnA maker and service
* find available rooms between check-in and check-out dates
* view rates of a queried room
* book a selected room
* user friendly checkout for payment using Facebook Quick replies (name, number and email)
* receipt after payment with an overview of the purchase
* get directions to the hotel with Google Maps using location quick reply
* 


# Further reading
- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot basics](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [LUIS](https://luis.ai)
- [Prompt Types](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-prompts?view=azure-bot-service-4.0&tabs=javascript)
- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
- [QnA Maker](https://qnamaker.ai)



