# Issue: Token Overflow per Request

## Problem
OpenAI and other LLMs have input token limits (e.g., 4,096 or 8,192 tokens per request). Long emails or concatenated content can exceed these limits, causing errors or incomplete responses.

## Solution: Chunking + Hierarchical Summarization
- **Chunking:**
  - Split long email content into smaller chunks, each within the token limit.
  - Each chunk is summarized individually.
- **Hierarchical Summarization:**
  - After all chunks are summarized, their summaries are combined and summarized again.
  - This produces a concise summary of the entire email, even if the original was too long for a single request.

## Code Example
See `AnalyzeEmail` in `EmailSummarizerService.cs`:
```csharp
// Split content into chunks
for (int i = 0; i < content.Length; i += chunkSize)
    chunks.Add(content.Substring(i, Math.Min(chunkSize, content.Length - i)));

// Summarize each chunk
foreach (var chunk in chunks)
    ... // summarize chunk

// Hierarchical summarization
var summaryContent = string.Join("\n", chunkSummaries);
var summaryEmail = new Email { ... Content = summaryContent };
var finalResult = await AnalyzeEmailSingle(summaryEmail);
```

## Benefits
- Handles arbitrarily long emails
- Prevents token overflow errors
- Ensures every email gets a summary, regardless of length
