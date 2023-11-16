# AI Social Media Chat Processor

This Proof of Concept (PoC) project demonstrates the idea of using AI to process social media messages, with a focus on Telegram, using .NET technologies. The project includes functionality to summarize chats and send messages to users in a smart way.

## Features

- **Message Sending:** The AI-powered Telegram client can send messages to users based on user input.
- **Chat Summarization:** It can summarize the last 10 messages of a conversation.

## Getting Started

Follow the steps below to set up and run the project locally.

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Ollama CLI with llama2 model ](https://github.com/jmorganca/ollama)

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/Neo-vortex/ChatWIthTelegramPoC
   ```

2. cd in the dir :

   ```bash
   cd ChatWIthTelegramPoC
   ```
3. run the project
 
   ```bash
   dotnet run
   ```

### Configuration
Set up your Telegram API credentials, phone number and ollama url:

   ```bash
export APP_ID=your_app_id
export API_HASH=your_api_hash
export PHONE_NUMBER=your_phone_number
export OLLAMA_URL=your_ollama_api_url
```
### How it Works
* The project uses the WTelegram library for interacting with the Telegram API.
* AI responses are generated using the Ollama API, providing intelligent and context-aware replies.

### Notes
* Ensure that you have the necessary permissions and authentication for using the Telegram API.
* The Ollama API should be accessible and configured with the appropriate models.

### License
This project is licensed under the MIT License. Feel free to use, modify, and distribute the code.

### Acknowledgments
* Thanks to the developers of WTelegram and Ollama for their excellent libraries and tools.
