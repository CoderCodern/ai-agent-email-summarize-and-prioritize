#!/usr/bin/env python3
"""
Demo script showing the email summarization system with sample data
This demonstrates the functionality without requiring actual email server access
"""

import os
import sys
from datetime import datetime, timedelta
from output_formatter import OutputFormatter
from email_processor import EmailProcessor


def generate_sample_emails():
    """Generate sample email data for demonstration"""
    base_date = datetime.now()
    
    sample_emails = [
        {
            "id": "1",
            "subject": "Team Meeting Tomorrow - Q4 Planning",
            "from": "manager@company.com",
            "date": base_date - timedelta(hours=2),
            "body": "Hi Team,\n\nWe'll have our quarterly planning meeting tomorrow at 10 AM in Conference Room B. Please review the attached Q3 metrics and come prepared with your Q4 goals.\n\nAgenda:\n1. Q3 Review\n2. Q4 Goals\n3. Resource Planning\n\nSee you there!\nBest,\nManager",
            "summary": "Team meeting scheduled for tomorrow at 10 AM to discuss Q4 planning. Review Q3 metrics beforehand and prepare Q4 goals.",
            "category": "meeting"
        },
        {
            "id": "2",
            "subject": "Action Required: Review Pull Request #456",
            "from": "github@notifications.com",
            "date": base_date - timedelta(hours=5),
            "body": "CoderCodern has requested your review on Pull Request #456: 'Implement email categorization feature'.\n\nPlease review the changes and provide feedback by end of day.\n\nView Pull Request: https://github.com/example/repo/pull/456",
            "summary": "Code review needed for Pull Request #456 implementing email categorization. Review requested by end of day.",
            "category": "task"
        },
        {
            "id": "3",
            "subject": "System Maintenance Notice - Saturday 10 PM",
            "from": "it-support@company.com",
            "date": base_date - timedelta(hours=8),
            "body": "Dear Users,\n\nPlease be advised that we will be performing scheduled maintenance on our email servers this Saturday from 10 PM to 2 AM.\n\nDuring this time, email services may be intermittently unavailable.\n\nWe apologize for any inconvenience.\n\nIT Support Team",
            "summary": "Scheduled system maintenance on Saturday 10 PM to 2 AM. Email services may be intermittently unavailable during this period.",
            "category": "announcement"
        },
        {
            "id": "4",
            "subject": "Hey! Long time no see",
            "from": "friend@personal.com",
            "date": base_date - timedelta(hours=12),
            "body": "Hi!\n\nHow have you been? It's been ages since we last caught up. Would you like to grab coffee sometime this week?\n\nLet me know what works for you!\n\nCheers,\nYour Friend",
            "summary": "Personal message from a friend suggesting to meet for coffee this week. Casual catch-up invitation.",
            "category": "personal"
        },
        {
            "id": "5",
            "subject": "URGENT: Production Server Error - Immediate Action Required",
            "from": "monitoring@company.com",
            "date": base_date - timedelta(minutes=30),
            "body": "ALERT: Production server API-PROD-01 is experiencing high error rates (45% in last 5 minutes).\n\nError Type: Database Connection Timeout\nAffected Services: User Authentication, Payment Processing\n\nImmediate action required. Please investigate and resolve ASAP.\n\nDashboard: https://monitoring.company.com/alert/12345",
            "summary": "Critical production issue: API server experiencing 45% error rate due to database timeouts. Immediate investigation and resolution required.",
            "category": "urgent"
        },
        {
            "id": "6",
            "subject": "Complete Your Task: Update Documentation by Friday",
            "from": "project-manager@company.com",
            "date": base_date - timedelta(days=1),
            "body": "Hi,\n\nThis is a reminder to complete the documentation update for the new feature by end of day Friday.\n\nThe documentation should include:\n- API reference\n- Usage examples\n- Migration guide\n\nPlease let me know if you need any help.\n\nThanks!",
            "summary": "Reminder to complete documentation update by Friday, including API reference, usage examples, and migration guide.",
            "category": "task"
        },
        {
            "id": "7",
            "subject": "50% OFF - Limited Time Offer!",
            "from": "sales@retailstore.com",
            "date": base_date - timedelta(days=2),
            "body": "Don't miss out on our biggest sale of the year!\n\nGet 50% OFF on all electronics for the next 48 hours only!\n\nShop now and save big: https://retailstore.com/sale\n\nFree shipping on orders over $50!",
            "summary": "Promotional email advertising 50% off electronics sale for 48 hours with free shipping on orders over $50.",
            "category": "promotional"
        }
    ]
    
    return sample_emails


def main():
    """Run demo with sample data"""
    print("="*80)
    print("AI EMAIL SUMMARIZATION AND PRIORITIZATION SYSTEM - DEMO")
    print("="*80)
    print("\nThis demo shows how the system processes and organizes emails.")
    print("Sample data is being used for demonstration purposes.\n")
    
    # Generate sample emails
    sample_emails = generate_sample_emails()
    
    # Initialize formatter
    formatter = OutputFormatter()
    
    # Note: In real usage, EmailProcessor would be used here
    # For demo, we're using pre-processed sample data
    
    # Sort emails chronologically (newest first)
    sorted_emails = sorted(sample_emails, key=lambda x: x['date'], reverse=True)
    
    # Group by category
    grouped_emails = {}
    for email in sorted_emails:
        category = email.get('category', 'other')
        if category not in grouped_emails:
            grouped_emails[category] = []
        grouped_emails[category].append(email)
    
    # Display results
    print("\n" + "="*80)
    print("EMAIL SUMMARY REPORT")
    print(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print(f"Total Emails: {len(sample_emails)}")
    print("="*80 + "\n")
    
    # Display by category
    print("ORGANIZED BY CATEGORY")
    print("="*80)
    formatter.display_by_category(grouped_emails)
    
    print("\n" + "="*80)
    print("CHRONOLOGICAL VIEW (Newest First)")
    print("="*80 + "\n")
    
    # Display chronologically
    formatter.display_chronological(sorted_emails)
    
    # Display statistics
    formatter.display_stats(sorted_emails)
    
    print("\n" + "="*80)
    print("DEMO COMPLETE")
    print("="*80)
    print("\nTo use with real emails:")
    print("1. Set up your .env file with API keys and email credentials")
    print("2. Run: python main.py")
    print("\nFor more information, see README.md")


if __name__ == "__main__":
    main()
