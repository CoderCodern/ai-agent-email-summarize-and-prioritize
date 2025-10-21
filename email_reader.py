"""
Email Reader Module
Handles reading unread emails from IMAP server
"""

import imaplib
import email as email_lib
from email.header import decode_header
from email import utils as email_utils
from datetime import datetime
from typing import List, Dict, Optional
import logging

logger = logging.getLogger(__name__)


class EmailReader:
    """Reads unread emails from an IMAP server"""
    
    def __init__(self, email_address: str, password: str, imap_server: str, imap_port: int = 993):
        """
        Initialize EmailReader with IMAP credentials
        
        Args:
            email_address: Email address for authentication
            password: Password or app-specific password
            imap_server: IMAP server address (e.g., imap.gmail.com)
            imap_port: IMAP port (default: 993 for SSL)
        """
        self.email_address = email_address
        self.password = password
        self.imap_server = imap_server
        self.imap_port = imap_port
        self.mail = None
    
    def connect(self) -> bool:
        """
        Connect to the IMAP server
        
        Returns:
            True if connection successful, False otherwise
        """
        try:
            self.mail = imaplib.IMAP4_SSL(self.imap_server, self.imap_port)
            self.mail.login(self.email_address, self.password)
            logger.info("Successfully connected to IMAP server")
            return True
        except Exception as e:
            logger.error(f"Failed to connect to IMAP server: {e}")
            return False
    
    def disconnect(self):
        """Disconnect from the IMAP server"""
        if self.mail:
            try:
                self.mail.close()
                self.mail.logout()
                logger.info("Disconnected from IMAP server")
            except Exception as e:
                logger.error(f"Error disconnecting: {e}")
    
    def _decode_header(self, header: str) -> str:
        """
        Decode email header
        
        Args:
            header: Email header to decode
            
        Returns:
            Decoded header string
        """
        if header is None:
            return ""
        
        decoded_parts = decode_header(header)
        decoded_string = ""
        
        for part, encoding in decoded_parts:
            if isinstance(part, bytes):
                try:
                    decoded_string += part.decode(encoding or 'utf-8')
                except (UnicodeDecodeError, LookupError):
                    decoded_string += part.decode('utf-8', errors='ignore')
            else:
                decoded_string += part
        
        return decoded_string
    
    def _extract_body(self, msg) -> str:
        """
        Extract email body text
        
        Args:
            msg: Email message object
            
        Returns:
            Email body as string
        """
        body = ""
        
        if msg.is_multipart():
            for part in msg.walk():
                content_type = part.get_content_type()
                content_disposition = str(part.get("Content-Disposition"))
                
                if content_type == "text/plain" and "attachment" not in content_disposition:
                    try:
                        body = part.get_payload(decode=True).decode()
                        break
                    except (UnicodeDecodeError, AttributeError):
                        pass
        else:
            try:
                body = msg.get_payload(decode=True).decode()
            except (UnicodeDecodeError, AttributeError):
                pass
        
        return body
    
    def get_unread_emails(self, limit: Optional[int] = None) -> List[Dict]:
        """
        Fetch all unread emails
        
        Args:
            limit: Maximum number of emails to fetch (None for all)
            
        Returns:
            List of dictionaries containing email information
        """
        if not self.mail:
            logger.error("Not connected to IMAP server")
            return []
        
        try:
            # Select inbox
            self.mail.select("inbox")
            
            # Search for unread emails
            status, messages = self.mail.search(None, 'UNSEEN')
            
            if status != "OK":
                logger.error("Failed to search for unread emails")
                return []
            
            email_ids = messages[0].split()
            
            if not email_ids:
                logger.info("No unread emails found")
                return []
            
            # Apply limit if specified
            if limit:
                email_ids = email_ids[-limit:]
            
            emails = []
            
            for email_id in email_ids:
                try:
                    status, msg_data = self.mail.fetch(email_id, "(RFC822)")
                    
                    if status != "OK":
                        continue
                    
                    # Parse email
                    msg = email_lib.message_from_bytes(msg_data[0][1])
                    
                    # Extract email information
                    subject = self._decode_header(msg["Subject"])
                    from_addr = self._decode_header(msg["From"])
                    date_str = msg["Date"]
                    body = self._extract_body(msg)
                    
                    # Parse date
                    try:
                        date = email_utils.parsedate_to_datetime(date_str)
                    except (TypeError, ValueError):
                        date = datetime.now()
                    
                    email_data = {
                        "id": email_id.decode(),
                        "subject": subject,
                        "from": from_addr,
                        "date": date,
                        "body": body
                    }
                    
                    emails.append(email_data)
                    
                except Exception as e:
                    logger.error(f"Error processing email {email_id}: {e}")
                    continue
            
            logger.info(f"Retrieved {len(emails)} unread emails")
            return emails
            
        except Exception as e:
            logger.error(f"Error fetching unread emails: {e}")
            return []
