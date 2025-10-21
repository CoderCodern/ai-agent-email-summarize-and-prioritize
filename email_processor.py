"""
Email Processor Module
Handles AI-powered summarization and categorization of emails
"""

import openai
from typing import List, Dict
import logging
import json

logger = logging.getLogger(__name__)


class EmailProcessor:
    """Processes emails using AI for summarization and categorization"""
    
    CATEGORIES = [
        "meeting",
        "task",
        "announcement",
        "personal",
        "promotional",
        "urgent",
        "other"
    ]
    
    def __init__(self, api_key: str):
        """
        Initialize EmailProcessor with OpenAI API key
        
        Args:
            api_key: OpenAI API key
        """
        self.api_key = api_key
        openai.api_key = api_key
        self.client = openai.OpenAI(api_key=api_key)
    
    def summarize_email(self, email_data: Dict) -> str:
        """
        Generate a concise summary of an email
        
        Args:
            email_data: Dictionary containing email information
            
        Returns:
            Summary string
        """
        subject = email_data.get("subject", "")
        body = email_data.get("body", "")
        from_addr = email_data.get("from", "")
        
        # Truncate body if too long
        max_body_length = 2000
        if len(body) > max_body_length:
            body = body[:max_body_length] + "..."
        
        prompt = f"""Summarize the following email in 2-3 concise sentences, focusing on key information and action items:

From: {from_addr}
Subject: {subject}

Body:
{body}

Summary:"""
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-3.5-turbo",
                messages=[
                    {"role": "system", "content": "You are a helpful assistant that summarizes emails concisely."},
                    {"role": "user", "content": prompt}
                ],
                max_tokens=150,
                temperature=0.3
            )
            
            summary = response.choices[0].message.content.strip()
            logger.info(f"Generated summary for email: {subject[:50]}")
            return summary
            
        except Exception as e:
            logger.error(f"Error generating summary: {e}")
            return f"Failed to generate summary: {str(e)}"
    
    def categorize_email(self, email_data: Dict) -> str:
        """
        Categorize an email based on its content
        
        Args:
            email_data: Dictionary containing email information
            
        Returns:
            Category string (meeting, task, announcement, personal, promotional, urgent, or other)
        """
        subject = email_data.get("subject", "")
        body = email_data.get("body", "")
        
        # Truncate body if too long
        max_body_length = 1500
        if len(body) > max_body_length:
            body = body[:max_body_length] + "..."
        
        categories_str = ", ".join(self.CATEGORIES)
        
        prompt = f"""Categorize the following email into ONE of these categories: {categories_str}

Subject: {subject}
Body: {body}

Choose the most appropriate category. Respond with ONLY the category name, nothing else."""
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-3.5-turbo",
                messages=[
                    {"role": "system", "content": f"You are a helpful assistant that categorizes emails. Always respond with exactly one of these categories: {categories_str}"},
                    {"role": "user", "content": prompt}
                ],
                max_tokens=10,
                temperature=0.1
            )
            
            category = response.choices[0].message.content.strip().lower()
            
            # Validate category
            if category not in self.CATEGORIES:
                category = "other"
            
            logger.info(f"Categorized email as: {category}")
            return category
            
        except Exception as e:
            logger.error(f"Error categorizing email: {e}")
            return "other"
    
    def process_emails(self, emails: List[Dict]) -> List[Dict]:
        """
        Process multiple emails: summarize and categorize
        
        Args:
            emails: List of email dictionaries
            
        Returns:
            List of processed email dictionaries with summary and category
        """
        processed_emails = []
        
        for email_data in emails:
            try:
                # Generate summary
                summary = self.summarize_email(email_data)
                
                # Categorize email
                category = self.categorize_email(email_data)
                
                # Create processed email data
                processed_email = {
                    **email_data,
                    "summary": summary,
                    "category": category
                }
                
                processed_emails.append(processed_email)
                
            except Exception as e:
                logger.error(f"Error processing email: {e}")
                continue
        
        logger.info(f"Processed {len(processed_emails)} emails")
        return processed_emails
    
    def sort_by_date(self, emails: List[Dict], reverse: bool = False) -> List[Dict]:
        """
        Sort emails chronologically by date
        
        Args:
            emails: List of email dictionaries
            reverse: If True, sort newest first; if False, sort oldest first
            
        Returns:
            Sorted list of emails
        """
        return sorted(emails, key=lambda x: x.get("date", ""), reverse=reverse)
    
    def group_by_category(self, emails: List[Dict]) -> Dict[str, List[Dict]]:
        """
        Group emails by category
        
        Args:
            emails: List of email dictionaries with category field
            
        Returns:
            Dictionary mapping category names to lists of emails
        """
        grouped = {}
        
        for email_data in emails:
            category = email_data.get("category", "other")
            if category not in grouped:
                grouped[category] = []
            grouped[category].append(email_data)
        
        return grouped
