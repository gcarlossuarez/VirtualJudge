# AbortController Cancellation Flow

## 🔍 How Does AbortController Work?

### Short Answer:
**You DON'T need to modify the backend sandbox** (in most cases). Cancellation works at the network level on the client side.

### Detailed Flow:

```
┌─────────────────────────────────────────────────────────────────┐
│                     CANCELLATION FLOW                           │
└─────────────────────────────────────────────────────────────────┘

                        FRONTEND (index.html)
                    ┌──────────────────────────┐
                    │  User clicks on          │
                    │  "Validate with DataSet" │
                    └────────────┬─────────────┘
                                 │
                                 ▼
                    ┌──────────────────────────┐
                    │ 1. Create AbortController│
                    │    controller = new      │
                    │    AbortController()     │
                    └────────────┬─────────────┘
                                 │
                                 ▼
                    ┌──────────────────────────┐
                    │ 2. fetch() with signal   │
                    │    fetch(url, {          │
                    │      signal: controller  │
                    │        .signal           │
                    │    })                    │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────┴─────────────┐
                    │                          │
                    ▼                          ▼
         ┌──────────────────┐      ┌──────────────────────┐
         │  CASE 1: NORMAL  │      │ CASE 2: CANCELLATION │
         └──────────────────┘      └──────────────────────┘
                    │                          │
                    │                          ▼
                    │              ┌──────────────────────────┐
                    │              │ User clicks on           │
                    │              │ "🛑 Cancel Validation"   │
                    │              └────────────┬─────────────┘
                    │                           │
                    │                           ▼
                    │              ┌──────────────────────────┐
                    │              │ 3. controller.abort()    │
                    │              └────────────┬─────────────┘
                    │                           │
                    ▼                           ▼
    ┌──────────────────────────┐  ┌──────────────────────────┐
    │ SERVER (Sandbox)         │  │ ⚡ SIGNAL SENT TO         │
    │ localhost:1100           │  │    BROWSER                │
    ├──────────────────────────┤  └────────────┬─────────────┘
    │ Receives request         │               │
    │ Starts validation        │               ▼
    │ Sends SSE events:        │  ┌──────────────────────────┐
    │   event: start           │  │ 4. Browser CLOSES the    │
    │   event: case-result     │  │    TCP/HTTP connection   │
    │   event: case-result     │  │    immediately           │
    │   ...                    │  └────────────┬─────────────┘
    │   event: complete        │               │
    └────────────┬─────────────┘               │
                 │                             │
                 ▼                             ▼
    ┌──────────────────────────┐  ┌──────────────────────────┐
    │ Frontend receives events │  │ 5. fetch() throws        │
    │ processStream() processes│  │    AbortError            │
    │ Shows results            │  └────────────┬─────────────┘
    │ Completes OK             │               │
    └──────────────────────────┘               ▼
                                   ┌──────────────────────────┐
                                   │ 6. .catch(err) captures  │
                                   │    if (err.name ===      │
                                   │      'AbortError')       │
                                   │    → Cancelled by user   │
                                   └────────────┬─────────────┘
                                                │
                                                ▼
                                   ┌──────────────────────────┐
                                   │ 7. reader.cancel()       │
                                   │    Releases resources    │
                                   └────────────┬─────────────┘
                                                │
                                                ▼
                                   ┌──────────────────────────┐
                                   │ Frontend shows:          │
                                   │ "🛑 Cancelled by user"   │
                                   │ Re-enables buttons       │
                                   └──────────────────────────┘
```

## 📊 What Happens in the Backend?

```
┌─────────────────────────────────────────────────────────────────┐
│              SERVER (SANDBOX) BEHAVIOR                          │
└─────────────────────────────────────────────────────────────────┘

SCENARIO 1: Without cancellation
─────────────────────────────────
Backend executes:
  1. Receives POST /validate-dataset
  2. Compiles code
  3. For each test case:
     - Executes code with input
     - Compares with expected output
     - Sends SSE event "case-result"
  4. Sends SSE event "complete"
  5. Closes connection

Response: ✅ 200 OK


SCENARIO 2: With cancellation (closed connection)
──────────────────────────────────────────────────
Backend executes:
  1. Receives POST /validate-dataset
  2. Compiles code
  3. For each test case:
     - Executes code with input
     - Compares with expected output
     - Tries to send SSE ───────────┐
                                     │
                                 ❌ ERROR
                         (connection closed)
                                     │
                    ┌────────────────┴───────────────┐
                    │                                │
                    ▼                                ▼
      ┌─────────────────────────┐    ┌─────────────────────────┐
      │ WITHOUT error handling: │    │ WITH error handling:    │
      ├─────────────────────────┤    ├─────────────────────────┤
      │ • Throws exception      │    │ • Detects disconnection │
      │ • May continue          │    │ • Stops the loop        │
      │   processing cases      │    │ • Releases resources    │
      │   (CPU waste)           │    │ • Exits cleanly         │
      │ • Error log             │    │ • ✅ BEST PRACTICE      │
      └─────────────────────────┘    └─────────────────────────┘
```

## 🤔 Do You Need to Modify the Backend?

### **Technically NO, it's not mandatory**, but **YES, it's recommended** for:

1. **Avoid CPU waste**: If the client cancels on case 5 of 100, the server shouldn't continue processing the remaining 95.

2. **Release resources**: Stop compilation/execution processes in progress.

3. **Logging**: Know that it was a cancellation, not an error.

### Backend Improvement Example (C#):

```csharp
// In DotNetInteractiveServer (the sandbox)

app.MapPost("/validate-dataset", async (
    HttpContext ctx,
    CancellationToken cancellationToken) => // ⬅️ IMPORTANT
{
    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync(cancellationToken);

    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var input = JsonSerializer.Deserialize<Request>(body, options);

    // ... validation code ...

    bool wantsStreaming = ctx.Request.Headers["Accept"]
        .ToString().Contains("text/event-stream");

    if (wantsStreaming)
    {
        ctx.Response.Headers["Content-Type"] = "text/event-stream";
        ctx.Response.Headers["Cache-Control"] = "no-cache";
        ctx.Response.Headers["Connection"] = "keep-alive";

        var writer = new StreamWriter(ctx.Response.Body, Encoding.UTF8, leaveOpen: true);
        await writer.FlushAsync(cancellationToken);

        try
        {
            // Send start event
            await SendSSE(writer, "start", 
                new { totalCases, problem = input.Problem }, 
                cancellationToken);
            
            int caseIndex = 0;
            foreach (var inputFile in files)
            {
                // ✅ Check if cancelled
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("🛑 Validation cancelled by client");
                    return Results.Empty; // Exit cleanly
                }
                
                caseIndex++;
                // ... process test case ...
                
                // Send result with cancellation token
                await SendSSE(writer, "case-result", resultData, cancellationToken);
            }
            
            await SendSSE(writer, "complete", 
                new { totalCases, completed = true }, 
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Client cancelled - exit cleanly
            Console.WriteLine("✅ Cancellation handled correctly");
            return Results.Empty;
        }
        catch (IOException ex) when (ex.InnerException is SocketException)
        {
            // Connection closed by client
            Console.WriteLine("🔌 Client disconnected");
            return Results.Empty;
        }
        
        await writer.FlushAsync(cancellationToken);
        return Results.Empty;
    }
    
    // ... traditional JSON mode ...
});

// Helper method with cancellation support
static async Task SendSSE(
    StreamWriter writer, 
    string eventType, 
    object data,
    CancellationToken cancellationToken = default)
{
    var json = JsonSerializer.Serialize(data);
    await writer.WriteLineAsync($"event: {eventType}");
    await writer.WriteLineAsync($"data: {json}");
    await writer.WriteLineAsync(); // Empty line = end of event
    await writer.FlushAsync(cancellationToken);
}
```

## 🎯 Summary

| Aspect | Without backend modification | With improved backend |
|--------|----------------------------|---------------------|
| **Cancels connection** | ✅ Yes | ✅ Yes |
| **Frontend sees cancellation** | ✅ Yes | ✅ Yes |
| **Backend detects disconnect** | ⚠️ When trying to write | ✅ Immediately |
| **Backend keeps processing** | ⚠️ Possibly yes | ✅ No |
| **Releases resources** | ⚠️ Eventually | ✅ Immediately |
| **Recommended for production** | ❌ No | ✅ Yes |

## 📝 Conclusion

**For your current case:**
- Cancellation will work **without modifying the backend**
- The user will see it cancelled immediately
- The backend will eventually detect the disconnection when trying to write the next SSE event

**To improve (recommended):**
- Add `CancellationToken` parameter to your sandbox endpoint
- Check `cancellationToken.IsCancellationRequested` in the loop
- Exit cleanly when cancellation is detected
- Handle `OperationCanceledException` and `IOException`

## 🔧 Additional Improvements

### 1. Connection Timeout Detection
```csharp
// Check if client is still connected
if (!ctx.Response.Body.CanWrite)
{
    Console.WriteLine("⚠️ Client disconnected");
    break;
}
```

### 2. Graceful Shutdown
```csharp
// Stop background processes when cancelled
if (currentProcess != null && !currentProcess.HasExited)
{
    currentProcess.Kill();
    Console.WriteLine("🛑 Process terminated due to cancellation");
}
```

### 3. Resource Cleanup
```csharp
using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
cts.CancelAfter(TimeSpan.FromMinutes(5)); // Max timeout

// All operations use cts.Token
```

## 📚 References

- [MDN: AbortController](https://developer.mozilla.org/en-US/docs/Web/API/AbortController)
- [ASP.NET Core: Cancellation Tokens](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-8.0#cancellation-tokens)
- [Server-Sent Events (SSE) Specification](https://html.spec.whatwg.org/multipage/server-sent-events.html)
