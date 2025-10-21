# AI Agent Email Summarizer and Prioritizer

## Overview
This project is a .NET 8 console application that uses OpenAI to summarize and classify emails as important or non-important. It demonstrates dependency injection, configuration management, and modular service design.

## Features
- Loads/generates mock emails
- Summarizes emails using OpenAI
- Classifies emails as important or non-important
- Saves results to JSON files
- Console logging and error handling

## Project Structure
- `OpenAIConsole/`
	- `Configuration/` — Configuration classes (e.g., `OpenAIConfig.cs`)
	- `Data/` — Email data files (JSON)
	- `Interfaces/` — Service and data provider interfaces
	- `Models/` — Email and result models
	- `Services/` — Service implementations (OpenAI, email summarization, file I/O)
	- `Program.cs` — Application entry point


## Quick Start
1. **Clone the repository:**
	 ```powershell
	 git clone <repo-url>
	 cd ai-agent-email-summarize-and-prioritize/OpenAIConsole
	 ```
2. **Add your OpenAI API key:**
	 - Copy `appsettings.json` to `appsettings.Development.json` in the `OpenAIConsole` folder.
	 - Edit `appsettings.Development.json` and set your real OpenAI API key:
		 ```json
		 {
			 "OpenAI": {
				 "ApiKey": "sk-...your-real-key...",
				 "Model": "gpt-3.5-turbo",
				 "MaxTokens": 1000,
				 "Temperature": 0.7
			 }
		 }
		 ```
	 - **Do not** commit your real API key. The `.gitignore` already protects this file.
3. **Run the application in Development mode:**
	 ```powershell
	 $env:DOTNET_ENVIRONMENT="Development"
	 dotnet run
	 ```
	 - On Linux/macOS, use:
		 ```bash
		 export DOTNET_ENVIRONMENT=Development
		 dotnet run
		 ```
4. Enter the number of mock emails to generate when prompted.
5. The app will summarize and classify emails, saving results in the `Data/` folder.

## Extending
- Add new data providers or services by implementing the relevant interfaces in `Interfaces/`.
- Add unit tests for services and providers.

## Requirements
- .NET 8 SDK
- OpenAI API key

## Architecture
- Follows dependency injection and separation of concerns
- Uses async file I/O and robust error handling
- Console logging for traceability

---
For more details, see code comments and each folder's README (if present).
