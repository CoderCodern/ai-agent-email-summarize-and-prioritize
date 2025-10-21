# AI-Powered Email Summarization and Prioritization System

An intelligent email management system that utilizes AI to help you stay on top of your inbox by automatically reading, summarizing, categorizing, and organizing your unread emails.

## Features

✨ **AI-Powered Email Processing**
- 📧 **Read Unread Emails**: Automatically fetches all unread emails from your inbox via IMAP
- 📝 **Smart Summarization**: Uses OpenAI GPT to generate concise 2-3 sentence summaries of each email
- 🏷️ **Automatic Categorization**: Intelligently categorizes emails into:
  - 📅 Meetings
  - ✅ Tasks
  - 📢 Announcements
  - 👤 Personal
  - 🛍️ Promotional
  - 🚨 Urgent
  - 📧 Other
- 📊 **Chronological Organization**: Displays emails sorted by date (newest first)
- 🎯 **Category Grouping**: View emails organized by category for better prioritization

## Requirements

- Python 3.7+
- OpenAI API key
- Email account with IMAP access enabled

## Installation

1. Clone the repository:
```bash
git clone https://github.com/CoderCodern/ai-agent-email-summarize-and-prioritize.git
cd ai-agent-email-summarize-and-prioritize
```

2. Install dependencies:
```bash
pip install -r requirements.txt
```

3. Set up configuration:
```bash
cp .env.example .env
```

4. Edit `.env` with your credentials:
```env
# OpenAI API Configuration
OPENAI_API_KEY=your_openai_api_key_here

# Email Configuration (IMAP)
EMAIL_ADDRESS=your_email@example.com
EMAIL_PASSWORD=your_app_password_here
IMAP_SERVER=imap.gmail.com
IMAP_PORT=993
```

### Email Setup Notes

**For Gmail:**
1. Enable IMAP in Gmail settings
2. Generate an [App Password](https://support.google.com/accounts/answer/185833)
3. Use the app password in your `.env` file

**For Other Providers:**
- Update `IMAP_SERVER` accordingly (e.g., `imap.outlook.com` for Outlook)
- Check your provider's documentation for IMAP settings

## Usage

### Basic Usage

Process all unread emails:
```bash
python main.py
```

### Limit Number of Emails

Process only the 10 most recent unread emails:
```bash
python main.py --limit 10
```

### Output

The application generates a comprehensive report with:
1. **Category View**: Emails grouped by category with summaries
2. **Chronological View**: All emails sorted by date

Example output:
```
================================================================================
EMAIL SUMMARY REPORT
Generated: 2025-10-21 03:11:20
================================================================================

📅 MEETINGS (2 emails)
--------------------------------------------------------------------------------
1.
  Subject: Team Standup - Q4 Planning
  From: manager@company.com
  Date: 2025-10-21 09:30:00
  Category: MEETING
  Summary: Team standup scheduled for tomorrow at 10 AM to discuss Q4 planning...

✅ TASKS (3 emails)
--------------------------------------------------------------------------------
1.
  Subject: Action Required: Review Pull Request #123
  From: github@notifications.com
  Date: 2025-10-21 08:15:00
  Category: TASK
  Summary: Pull request #123 needs your review before end of day...
```

## Architecture

The system consists of four main modules:

1. **`email_reader.py`**: Handles IMAP connection and fetching unread emails
2. **`email_processor.py`**: Uses OpenAI API for summarization and categorization
3. **`output_formatter.py`**: Formats and displays processed email data
4. **`main.py`**: Orchestrates the entire workflow

## How It Works

1. **Connection**: Connects to your email server via IMAP SSL
2. **Reading**: Fetches all unread emails from your inbox
3. **Processing**: For each email:
   - Generates a concise summary using GPT-3.5-turbo
   - Categorizes based on content analysis
4. **Organization**: Sorts emails chronologically (newest first)
5. **Display**: Shows organized results in both category and chronological views

## Security Best Practices

- ✅ Never commit your `.env` file (it's in `.gitignore`)
- ✅ Use app-specific passwords, not your main email password
- ✅ Restrict API key permissions to minimum required
- ✅ Keep your API keys secure and rotate them regularly

## Error Handling

The application includes robust error handling:
- Graceful handling of connection failures
- Individual email processing errors don't stop the entire workflow
- Detailed logging for troubleshooting

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License - feel free to use this project for personal or commercial purposes.

## Support

If you encounter issues:
1. Check that IMAP is enabled on your email account
2. Verify your API key is valid
3. Ensure your `.env` file is properly configured
4. Check the logs for detailed error messages

## Future Enhancements

Potential improvements:
- Support for multiple email accounts
- Email filtering and rules
- Priority scoring system
- Export to various formats (JSON, CSV, PDF)
- Web interface
- Email response suggestions
- Attachment handling
- Scheduled automatic runs
