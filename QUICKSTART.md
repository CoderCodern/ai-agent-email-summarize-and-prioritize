# Quick Start Guide

Get started with the AI-Powered Email Summarization System in 5 minutes!

## Prerequisites

- Python 3.7 or higher
- OpenAI API key ([Get one here](https://platform.openai.com/api-keys))
- Email account with IMAP access

## Step 1: Install Dependencies

```bash
pip install -r requirements.txt
```

## Step 2: Try the Demo

See the system in action without any setup:

```bash
python demo.py
```

This will show you how the system organizes and summarizes emails.

## Step 3: Configure Your Email

Create a `.env` file in the project directory:

```bash
cp .env.example .env
```

Edit `.env` with your credentials:

```env
# OpenAI API Configuration
OPENAI_API_KEY=sk-your-actual-api-key-here

# Email Configuration
EMAIL_ADDRESS=your.email@gmail.com
EMAIL_PASSWORD=your-app-password-here
IMAP_SERVER=imap.gmail.com
IMAP_PORT=993
```

### Gmail Setup

1. Enable IMAP:
   - Go to Gmail Settings → Forwarding and POP/IMAP
   - Enable IMAP access

2. Generate App Password:
   - Visit https://myaccount.google.com/apppasswords
   - Select "Mail" and your device
   - Copy the 16-character password
   - Use this password (not your regular Gmail password) in `.env`

### Other Email Providers

**Outlook/Office 365:**
```env
IMAP_SERVER=outlook.office365.com
IMAP_PORT=993
```

**Yahoo Mail:**
```env
IMAP_SERVER=imap.mail.yahoo.com
IMAP_PORT=993
```

## Step 4: Run the Application

Process all unread emails:

```bash
python main.py
```

Or limit to the 5 most recent:

```bash
python main.py --limit 5
```

## Understanding the Output

The application provides two views:

### 1. Category View
Emails grouped by type for easy prioritization:
- 🚨 **URGENT**: Critical issues requiring immediate attention
- 📅 **MEETINGS**: Meeting invitations and reminders
- ✅ **TASKS**: Action items and to-dos
- 📢 **ANNOUNCEMENTS**: General announcements
- 👤 **PERSONAL**: Personal messages
- 🛍️ **PROMOTIONAL**: Marketing and promotional emails
- 📧 **OTHER**: Everything else

### 2. Chronological View
All emails sorted by date (newest first)

## What It Does

For each unread email, the system:

1. **Reads** the email via IMAP
2. **Summarizes** key points in 2-3 sentences using AI
3. **Categorizes** based on content
4. **Organizes** chronologically
5. **Displays** in an easy-to-read format

## Troubleshooting

### "Failed to connect to IMAP server"
- Check your email address and password
- Verify IMAP is enabled in your email settings
- For Gmail, use an App Password, not your regular password

### "Missing required configuration: openai_api_key"
- Make sure your `.env` file exists
- Check that `OPENAI_API_KEY` is set correctly
- Verify there are no spaces around the `=` sign

### "No unread emails found"
- The system only processes unread emails
- Mark some emails as unread to test
- Or try the demo: `python demo.py`

## Cost Information

The application uses OpenAI's GPT-3.5-turbo model:
- Cost: ~$0.001 per email (approximate)
- Processing 100 emails ≈ $0.10
- Processing 1000 emails ≈ $1.00

## Next Steps

- Run the application regularly to stay on top of your inbox
- Adjust the `--limit` parameter based on your needs
- Check the full [README.md](README.md) for advanced features
- Review the code to customize categorization rules

## Support

Having issues? Check:
1. Your `.env` file is properly configured
2. IMAP is enabled on your email account
3. Your OpenAI API key is valid and has credits
4. The [README.md](README.md) for detailed troubleshooting

## Security Reminder

⚠️ **Never commit your `.env` file to version control!**

The `.env` file contains sensitive credentials and is already in `.gitignore`.
