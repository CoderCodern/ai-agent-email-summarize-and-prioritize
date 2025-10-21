#!/usr/bin/env python3
"""
AI-Powered Email Summarization and Prioritization System

This application reads unread emails, summarizes them using AI,
categorizes them, and organizes them chronologically.
"""

import os
import sys
import logging
from typing import Optional
from datetime import datetime
from dotenv import load_dotenv

from email_reader import EmailReader
from email_processor import EmailProcessor
from output_formatter import OutputFormatter

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(sys.stdout)
    ]
)

logger = logging.getLogger(__name__)


def load_config():
    """Load configuration from environment variables"""
    load_dotenv()
    
    config = {
        'openai_api_key': os.getenv('OPENAI_API_KEY'),
        'email_address': os.getenv('EMAIL_ADDRESS'),
        'email_password': os.getenv('EMAIL_PASSWORD'),
        'imap_server': os.getenv('IMAP_SERVER', 'imap.gmail.com'),
        'imap_port': int(os.getenv('IMAP_PORT', '993'))
    }
    
    # Validate required configuration
    required_fields = ['openai_api_key', 'email_address', 'email_password']
    missing_fields = [field for field in required_fields if not config.get(field)]
    
    if missing_fields:
        logger.error(f"Missing required configuration: {', '.join(missing_fields)}")
        logger.error("Please create a .env file with the required configuration (see .env.example)")
        sys.exit(1)
    
    return config


def main(limit: Optional[int] = None):
    """
    Main application function
    
    Args:
        limit: Maximum number of emails to process (None for all)
    """
    logger.info("Starting AI Email Summarization and Prioritization System")
    
    # Load configuration
    config = load_config()
    
    # Initialize components
    email_reader = EmailReader(
        email_address=config['email_address'],
        password=config['email_password'],
        imap_server=config['imap_server'],
        imap_port=config['imap_port']
    )
    
    email_processor = EmailProcessor(api_key=config['openai_api_key'])
    output_formatter = OutputFormatter()
    
    try:
        # Connect to email server
        logger.info("Connecting to email server...")
        if not email_reader.connect():
            logger.error("Failed to connect to email server")
            return
        
        # Read unread emails
        logger.info("Reading unread emails...")
        emails = email_reader.get_unread_emails(limit=limit)
        
        if not emails:
            logger.info("No unread emails found")
            return
        
        logger.info(f"Found {len(emails)} unread emails")
        
        # Process emails (summarize and categorize)
        logger.info("Processing emails with AI...")
        processed_emails = email_processor.process_emails(emails)
        
        # Sort emails chronologically (newest first)
        sorted_emails = email_processor.sort_by_date(processed_emails, reverse=True)
        
        # Group by category
        grouped_emails = email_processor.group_by_category(sorted_emails)
        
        # Display results
        print("\n" + "="*80)
        print("EMAIL SUMMARY REPORT")
        print(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print("="*80 + "\n")
        
        # Display by category
        output_formatter.display_by_category(grouped_emails)
        
        print("\n" + "="*80)
        print("CHRONOLOGICAL VIEW")
        print("="*80 + "\n")
        
        # Display chronologically
        output_formatter.display_chronological(sorted_emails)
        
        logger.info("Processing complete!")
        
    except Exception as e:
        logger.error(f"An error occurred: {e}")
        sys.exit(1)
    
    finally:
        # Disconnect from email server
        email_reader.disconnect()


if __name__ == "__main__":
    import argparse
    
    parser = argparse.ArgumentParser(
        description='AI-Powered Email Summarization and Prioritization System'
    )
    parser.add_argument(
        '--limit',
        type=int,
        default=None,
        help='Maximum number of emails to process (default: all)'
    )
    
    args = parser.parse_args()
    main(limit=args.limit)
