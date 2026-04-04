# PostgreSQL DateTime UTC Fix

## Issue
When creating assignments from the frontend, the following error occurred:
```
Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', 
only UTC is supported. Note that it's not possible to mix DateTimes with different Kinds 
in an array, range, or multirange. (Parameter 'value')
```

## Root Cause
PostgreSQL's `timestamp with time zone` column type requires all DateTime values to be in UTC (Coordinated Universal Time). When the frontend sends a date without timezone information (Kind=Unspecified), the backend was trying to save it directly to the database, causing the error.

## Solution
Updated `AssignmentService.cs` to ensure all DateTime values are converted to UTC before saving to the database or using in queries.

### Files Modified
**File**: `backend/ChurchRoster.Application/Services/AssignmentService.cs`

### Changes Made

#### 1. CreateAssignmentAsync Method
**Before**:
```csharp
EventDate = request.EventDate,
```

**After**:
```csharp
// Ensure EventDate is in UTC
var eventDateUtc = request.EventDate.Kind == DateTimeKind.Utc 
    ? request.EventDate 
    : DateTime.SpecifyKind(request.EventDate, DateTimeKind.Utc);

// Use eventDateUtc throughout the method
EventDate = eventDateUtc,
```

#### 2. GetAssignmentsByDateAsync Method
**Before**:
```csharp
.Where(a => a.EventDate.Date == eventDate.Date)
```

**After**:
```csharp
// Ensure eventDate is in UTC for comparison
var eventDateUtc = eventDate.Kind == DateTimeKind.Utc 
    ? eventDate 
    : DateTime.SpecifyKind(eventDate, DateTimeKind.Utc);

.Where(a => a.EventDate.Date == eventDateUtc.Date)
```

#### 3. ValidateAssignmentAsync Method
**Before**:
```csharp
var monthStart = new DateTime(eventDate.Year, eventDate.Month, 1);
```

**After**:
```csharp
// Ensure eventDate is in UTC
var eventDateUtc = eventDate.Kind == DateTimeKind.Utc 
    ? eventDate 
    : DateTime.SpecifyKind(eventDate, DateTimeKind.Utc);

var monthStart = new DateTime(eventDateUtc.Year, eventDateUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
```

## How It Works

### DateTime.SpecifyKind()
This method converts a DateTime value to have a specific Kind (Local, UTC, or Unspecified) without changing the actual date/time values. It's perfect for our use case because:

1. If the DateTime is already UTC, we use it as-is
2. If the DateTime is Unspecified (from JSON deserialization), we mark it as UTC
3. The actual date and time values remain unchanged

### Why This Works
When the frontend sends a date like `"2026-01-15"`, it comes through as:
- **JSON**: `"2026-01-15T00:00:00"`
- **Deserialized**: `DateTime { Year = 2026, Month = 1, Day = 15, Kind = Unspecified }`
- **After Fix**: `DateTime { Year = 2026, Month = 1, Day = 15, Kind = Utc }`
- **PostgreSQL**: Accepts it as a valid UTC timestamp

## Testing

### Before Fix
```
POST /api/assignments
{
  "taskId": 1,
  "userId": 2,
  "eventDate": "2026-01-15",
  "isOverride": false
}

❌ Error: Cannot write DateTime with Kind=Unspecified
```

### After Fix
```
POST /api/assignments
{
  "taskId": 1,
  "userId": 2,
  "eventDate": "2026-01-15",
  "isOverride": false
}

✅ Success: Assignment created with ID 1
```

## Build Status
✅ **Backend Build**: Successful
- ChurchRoster.Core: ✓
- ChurchRoster.Infrastructure: ✓
- ChurchRoster.Application: ✓

## Best Practices Going Forward

### When Working with DateTime in PostgreSQL

1. **Always use UTC** for database storage
2. **Convert to UTC** at the service layer (as done in this fix)
3. **Use DateTime.UtcNow** instead of DateTime.Now for timestamps
4. **Specify DateTimeKind.Utc** when creating DateTime objects manually

### Example:
```csharp
// ✅ Good - Explicitly UTC
var date = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
var now = DateTime.UtcNow;

// ❌ Bad - Unspecified or Local
var date = new DateTime(2026, 1, 15); // Kind = Unspecified
var now = DateTime.Now; // Kind = Local
```

## Additional Notes

### Why Not Convert in the DTO?
We could have made the DTO property setter convert to UTC, but handling it in the service layer is better because:
1. **Separation of Concerns**: DTOs should be simple data containers
2. **Flexibility**: Different services might need different handling
3. **Explicit**: The conversion is visible in the business logic
4. **Testability**: Easier to test service logic

### Frontend Considerations
The frontend doesn't need changes because:
1. JavaScript Date serializes to ISO 8601 format
2. .NET's JSON deserializer handles ISO dates correctly
3. Our fix handles the Kind conversion

## Related Documentation
- [PostgreSQL Timestamp Types](https://www.postgresql.org/docs/current/datatype-datetime.html)
- [.NET DateTime and DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/standard/datetime/choosing-between-datetime)
- [Npgsql DateTime Handling](https://www.npgsql.org/doc/types/datetime.html)

## Status
✅ **FIXED AND TESTED**

The assignment creation now works correctly with PostgreSQL's timezone requirements.

---

**Fixed**: Week 4 Implementation
**Issue**: PostgreSQL DateTime UTC requirement
**Solution**: Convert DateTime to UTC in AssignmentService
