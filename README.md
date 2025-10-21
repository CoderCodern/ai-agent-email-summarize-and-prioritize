
# AI Agent Email Summarizer and Prioritizer

## What does this project do?
This .NET 8 console app uses OpenAI to automatically summarize and classify emails as important or non-important. It is designed for efficient, scalable processing of large email datasets.

### Main Functions
- **Loads or generates mock emails** for testing
- **Summarizes each email** using OpenAI LLM
- **Classifies emails** as important or non-important
- **Saves results** to JSON files in the `Data/` folder:
  - `important-emails.json`
  - `non-important-emails.json`
- **Batch processing** and **parallel programming** are used to efficiently process large numbers of emails
- **Handles long emails** by chunking and hierarchical summarization

### What are the results?
- For each email, you get:
  - A concise summary
  - Importance classification
  - Reason for classification
  - Token/cost usage (if available)
- Results are saved in the `Data/` folder for further analysis

## How to use
1. **Clone the repository**
2. **Add your OpenAI API key** to `appsettings.Development.json` (see template in `appsettings.json`)
3. **Run the app** in Development mode:
   - Windows: `$env:DOTNET_ENVIRONMENT="Development"; dotnet run`
   - Linux/macOS: `export DOTNET_ENVIRONMENT=Development; dotnet run`
4. **Follow prompts** to generate and process emails
5. **Check the `Data/` folder** for results

## Requirements
- .NET 8 SDK
- OpenAI API key

---
For technical details and solutions to large dataset or token overflow issues, see the `docs/` folder.
