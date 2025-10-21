# Issue: Large Email Dataset

## Problem
When users have thousands of emails, fetching and processing all at once will exceed both API and AI limits. This can result in:
- API rate limit errors
- Memory exhaustion
- Slow or failed processing

## Solution: Batch Processing
- **Batch Size:** Process emails in batches (recommended: 100–200 per batch, or lower for strict API limits).
- **How it works:**
  - The application splits the email list into batches.
  - Each batch is processed sequentially, but emails within a batch are summarized in parallel (asynchronously).
  - This controls the number of concurrent API requests and keeps memory usage manageable.
- **Benefits:**
  - Avoids API rate limit errors
  - Prevents memory overload
  - Allows for progress tracking and partial results

## Code Example
See `EmailSummarizerService.cs`:
```csharp
for (int i = 0; i < emails.Count; i += batchSize)
{
    var batch = emails.Skip(i).Take(batchSize).ToList();
    var tasks = batch.Select(async email => await AnalyzeEmail(email));
    var results = await Task.WhenAll(tasks);
    summarizedEmails.AddRange(results);
}
```

## When to Adjust Batch Size
- If you see API rate limit errors, reduce the batch size.
- If processing is too slow, increase batch size (within safe limits).
- Tune based on your OpenAI API plan and system resources.
