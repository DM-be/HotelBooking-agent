# Hotel Booking agent

## Description
This project was built during my internship for the company Stardekk. Stardekk specializes in cloud software for the hospitality sector.
They wondered if a chatbot could be interesting to deploy for their clients. My bachelor's thesis was built around this interest.
You can view it here. 

The goal was to build a conversational agent that could be contacted on a hotel Facebook page. This agent would guide a client to book a room or to answer questions such as the hotel location or phone number. 


## Technical
The agent is a web server responding to HTTP requests. Within the body of these requests is information about sender, the text sent, channel data,... The Microsoft bot framework leverages this data and provides ways to support a natural flow of a conversation between the sender and the agent. 

LUIS is natural language understanding service provided by Microsoft. It can recognize user intents and entities. Every response in a dialog is first interpreted by LUIS, according to the recognized intents actions can be taken. For example a user booking a room and realising he/she made a mistake about the checkin date can just say "change my check-in date to next monday" and LUIS would recognize this intent, extract monday as the check-in date and make it available for the agent. 

QnA maker is another service provided by Microsoft. It matches questions with answers. A FAQ list is also implemented in the main dialog of this agent.

I go into full technical detail in my thesis, supported with code examples and schematics. It is available here. 


# Further reading
- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot basics](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [LUIS](https://luis.ai)
- [Prompt Types](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-prompts?view=azure-bot-service-4.0&tabs=javascript)
- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
- [QnA Maker](https://qnamaker.ai)



