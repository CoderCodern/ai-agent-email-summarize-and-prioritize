"""
Output Formatter Module
Handles formatting and displaying processed email information
"""

from typing import List, Dict
from datetime import datetime


class OutputFormatter:
    """Formats and displays processed email information"""
    
    # Category display names and emojis
    CATEGORY_DISPLAY = {
        "meeting": "📅 MEETINGS",
        "task": "✅ TASKS",
        "announcement": "📢 ANNOUNCEMENTS",
        "personal": "👤 PERSONAL",
        "promotional": "🛍️ PROMOTIONAL",
        "urgent": "🚨 URGENT",
        "other": "📧 OTHER"
    }
    
    def format_date(self, date: datetime) -> str:
        """
        Format date for display
        
        Args:
            date: datetime object
            
        Returns:
            Formatted date string
        """
        return date.strftime("%Y-%m-%d %H:%M:%S")
    
    def truncate_text(self, text: str, max_length: int = 80) -> str:
        """
        Truncate text to specified length
        
        Args:
            text: Text to truncate
            max_length: Maximum length
            
        Returns:
            Truncated text
        """
        if len(text) <= max_length:
            return text
        return text[:max_length-3] + "..."
    
    def display_email(self, email_data: Dict, show_body: bool = False):
        """
        Display a single email
        
        Args:
            email_data: Email dictionary
            show_body: Whether to show full body
        """
        print(f"  Subject: {email_data.get('subject', 'No Subject')}")
        print(f"  From: {email_data.get('from', 'Unknown')}")
        print(f"  Date: {self.format_date(email_data.get('date', datetime.now()))}")
        print(f"  Category: {email_data.get('category', 'other').upper()}")
        print(f"  Summary: {email_data.get('summary', 'No summary available')}")
        
        if show_body and email_data.get('body'):
            body_preview = self.truncate_text(email_data['body'], 200)
            print(f"  Body Preview: {body_preview}")
        
        print()
    
    def display_chronological(self, emails: List[Dict]):
        """
        Display emails in chronological order
        
        Args:
            emails: List of email dictionaries (should be pre-sorted)
        """
        if not emails:
            print("No emails to display")
            return
        
        for i, email_data in enumerate(emails, 1):
            print(f"{i}. [{email_data.get('category', 'other').upper()}]")
            self.display_email(email_data)
            if i < len(emails):
                print("-" * 80)
    
    def display_by_category(self, grouped_emails: Dict[str, List[Dict]]):
        """
        Display emails grouped by category
        
        Args:
            grouped_emails: Dictionary mapping categories to email lists
        """
        if not grouped_emails:
            print("No emails to display")
            return
        
        # Define priority order for categories
        category_order = ["urgent", "meeting", "task", "announcement", "personal", "promotional", "other"]
        
        for category in category_order:
            if category not in grouped_emails or not grouped_emails[category]:
                continue
            
            emails = grouped_emails[category]
            display_name = self.CATEGORY_DISPLAY.get(category, category.upper())
            
            print(f"\n{display_name} ({len(emails)} email{'s' if len(emails) != 1 else ''})")
            print("-" * 80)
            
            for i, email_data in enumerate(emails, 1):
                print(f"{i}.")
                self.display_email(email_data)
    
    def generate_summary_stats(self, emails: List[Dict]) -> Dict:
        """
        Generate summary statistics
        
        Args:
            emails: List of email dictionaries
            
        Returns:
            Dictionary with statistics
        """
        stats = {
            "total": len(emails),
            "by_category": {}
        }
        
        for email_data in emails:
            category = email_data.get("category", "other")
            stats["by_category"][category] = stats["by_category"].get(category, 0) + 1
        
        return stats
    
    def display_stats(self, emails: List[Dict]):
        """
        Display summary statistics
        
        Args:
            emails: List of email dictionaries
        """
        stats = self.generate_summary_stats(emails)
        
        print("\n" + "="*80)
        print("SUMMARY STATISTICS")
        print("="*80)
        print(f"Total Emails: {stats['total']}")
        print("\nBy Category:")
        
        for category, count in sorted(stats['by_category'].items(), key=lambda x: x[1], reverse=True):
            display_name = self.CATEGORY_DISPLAY.get(category, category.upper())
            print(f"  {display_name}: {count}")
