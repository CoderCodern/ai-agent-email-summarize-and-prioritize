"""
Unit tests for email processor module
"""

import unittest
from datetime import datetime
from unittest.mock import Mock, patch, MagicMock
from email_processor import EmailProcessor


class TestEmailProcessor(unittest.TestCase):
    """Test cases for EmailProcessor class"""
    
    def setUp(self):
        """Set up test fixtures"""
        self.api_key = "test_api_key"
        self.processor = EmailProcessor(self.api_key)
    
    def test_initialization(self):
        """Test EmailProcessor initialization"""
        self.assertEqual(self.processor.api_key, self.api_key)
        self.assertIsNotNone(self.processor.client)
    
    def test_categories_defined(self):
        """Test that categories are properly defined"""
        expected_categories = ["meeting", "task", "announcement", "personal", "promotional", "urgent", "other"]
        self.assertEqual(self.processor.CATEGORIES, expected_categories)
    
    @patch('email_processor.openai.OpenAI')
    def test_summarize_email(self, mock_openai):
        """Test email summarization"""
        # Mock OpenAI response
        mock_response = MagicMock()
        mock_response.choices[0].message.content = "This is a test summary."
        mock_client = MagicMock()
        mock_client.chat.completions.create.return_value = mock_response
        
        processor = EmailProcessor(self.api_key)
        processor.client = mock_client
        
        email_data = {
            "subject": "Test Email",
            "from": "test@example.com",
            "body": "This is a test email body."
        }
        
        summary = processor.summarize_email(email_data)
        self.assertEqual(summary, "This is a test summary.")
        self.assertTrue(mock_client.chat.completions.create.called)
    
    @patch('email_processor.openai.OpenAI')
    def test_categorize_email(self, mock_openai):
        """Test email categorization"""
        # Mock OpenAI response
        mock_response = MagicMock()
        mock_response.choices[0].message.content = "meeting"
        mock_client = MagicMock()
        mock_client.chat.completions.create.return_value = mock_response
        
        processor = EmailProcessor(self.api_key)
        processor.client = mock_client
        
        email_data = {
            "subject": "Team Meeting Tomorrow",
            "body": "Let's meet at 10 AM to discuss the project."
        }
        
        category = processor.categorize_email(email_data)
        self.assertEqual(category, "meeting")
        self.assertTrue(mock_client.chat.completions.create.called)
    
    def test_sort_by_date(self):
        """Test sorting emails by date"""
        emails = [
            {"date": datetime(2025, 1, 3), "subject": "Third"},
            {"date": datetime(2025, 1, 1), "subject": "First"},
            {"date": datetime(2025, 1, 2), "subject": "Second"}
        ]
        
        # Sort oldest first
        sorted_emails = self.processor.sort_by_date(emails, reverse=False)
        self.assertEqual(sorted_emails[0]["subject"], "First")
        self.assertEqual(sorted_emails[1]["subject"], "Second")
        self.assertEqual(sorted_emails[2]["subject"], "Third")
        
        # Sort newest first
        sorted_emails = self.processor.sort_by_date(emails, reverse=True)
        self.assertEqual(sorted_emails[0]["subject"], "Third")
        self.assertEqual(sorted_emails[1]["subject"], "Second")
        self.assertEqual(sorted_emails[2]["subject"], "First")
    
    def test_group_by_category(self):
        """Test grouping emails by category"""
        emails = [
            {"category": "meeting", "subject": "Meeting 1"},
            {"category": "task", "subject": "Task 1"},
            {"category": "meeting", "subject": "Meeting 2"},
            {"category": "personal", "subject": "Personal 1"}
        ]
        
        grouped = self.processor.group_by_category(emails)
        
        self.assertEqual(len(grouped["meeting"]), 2)
        self.assertEqual(len(grouped["task"]), 1)
        self.assertEqual(len(grouped["personal"]), 1)
        self.assertIn("meeting", grouped)
        self.assertIn("task", grouped)
        self.assertIn("personal", grouped)
    
    def test_group_by_category_empty(self):
        """Test grouping empty list of emails"""
        emails = []
        grouped = self.processor.group_by_category(emails)
        self.assertEqual(grouped, {})


if __name__ == '__main__':
    unittest.main()
