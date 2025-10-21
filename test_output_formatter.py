"""
Unit tests for output formatter module
"""

import unittest
from datetime import datetime
from output_formatter import OutputFormatter


class TestOutputFormatter(unittest.TestCase):
    """Test cases for OutputFormatter class"""
    
    def setUp(self):
        """Set up test fixtures"""
        self.formatter = OutputFormatter()
    
    def test_format_date(self):
        """Test date formatting"""
        date = datetime(2025, 10, 21, 14, 30, 45)
        formatted = self.formatter.format_date(date)
        self.assertEqual(formatted, "2025-10-21 14:30:45")
    
    def test_truncate_text_short(self):
        """Test truncating text that's already short enough"""
        text = "Short text"
        truncated = self.formatter.truncate_text(text, max_length=50)
        self.assertEqual(truncated, "Short text")
    
    def test_truncate_text_long(self):
        """Test truncating long text"""
        text = "This is a very long text that needs to be truncated because it exceeds the maximum length"
        truncated = self.formatter.truncate_text(text, max_length=30)
        self.assertEqual(len(truncated), 30)
        self.assertTrue(truncated.endswith("..."))
    
    def test_generate_summary_stats(self):
        """Test generating summary statistics"""
        emails = [
            {"category": "meeting"},
            {"category": "task"},
            {"category": "meeting"},
            {"category": "personal"}
        ]
        
        stats = self.formatter.generate_summary_stats(emails)
        
        self.assertEqual(stats["total"], 4)
        self.assertEqual(stats["by_category"]["meeting"], 2)
        self.assertEqual(stats["by_category"]["task"], 1)
        self.assertEqual(stats["by_category"]["personal"], 1)
    
    def test_generate_summary_stats_empty(self):
        """Test generating statistics for empty list"""
        emails = []
        stats = self.formatter.generate_summary_stats(emails)
        
        self.assertEqual(stats["total"], 0)
        self.assertEqual(stats["by_category"], {})
    
    def test_category_display_mappings(self):
        """Test that all category display mappings are defined"""
        expected_categories = ["meeting", "task", "announcement", "personal", "promotional", "urgent", "other"]
        
        for category in expected_categories:
            self.assertIn(category, self.formatter.CATEGORY_DISPLAY)
            self.assertIsInstance(self.formatter.CATEGORY_DISPLAY[category], str)


if __name__ == '__main__':
    unittest.main()
