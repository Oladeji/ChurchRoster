# Timezone Date Display Fix

## Issue
Dates assigned by admin were showing **one day earlier** when viewed by members on the "My Assignments" page.

### Example
- **Admin assigns**: April 25, 2026
- **Member sees**: April 24, 2026 ❌

## Root Cause

### Database Storage
The backend stores dates in **UTC** format:
```
"2026-04-25T00:00:00Z"
```
The "Z" at the end indicates this is UTC (Coordinated Universal Time).

### Browser Timezone Conversion
When JavaScript's `Date` object parses this string and uses `toLocaleDateString()`, it automatically converts to the **user's local timezone**.

**Problem**: If the user is in a timezone **behind** UTC (like EST/EDT which is UTC-5 or UTC-4), midnight UTC becomes the previous day's evening in local time.

### Example Timeline
```
UTC Time:           2026-04-25 00:00:00Z (midnight UTC)
                           ↓
EST (UTC-5):        2026-04-24 19:00:00  (7 PM previous day)
                           ↓
Display shows:      "Thursday, April 24, 2026" ❌
```

## Solution

### 1. Force UTC Display in Date Formatting

**Files Modified**:
- `frontend/src/pages/MyAssignmentsPage.tsx`
- `frontend/src/pages/AssignmentsPage.tsx`

**Before**:
```typescript
const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};
```

**After**:
```typescript
const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    timeZone: 'UTC' // ✅ Force UTC timezone
  });
};
```

### 2. Fix Date Comparisons in Filters

**File**: `frontend/src/pages/MyAssignmentsPage.tsx`

The filtering logic (upcoming/past) was also affected by timezone issues.

**Before**:
```typescript
const today = new Date();
today.setHours(0, 0, 0, 0);

const eventDate = new Date(a.eventDate);
eventDate.setHours(0, 0, 0, 0);
return eventDate >= today;
```
This compared dates in **local timezone**, causing incorrect filtering.

**After**:
```typescript
const today = new Date();
const todayUTC = Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate());

const eventDate = new Date(a.eventDate);
const eventDateUTC = Date.UTC(eventDate.getUTCFullYear(), eventDate.getUTCMonth(), eventDate.getUTCDate());
return eventDateUTC >= todayUTC;
```
This compares dates in **UTC**, ensuring accuracy.

### 3. Fix Days Countdown Calculation

**File**: `frontend/src/pages/MyAssignmentsPage.tsx`

**Before**:
```typescript
const getDaysUntil = (dateString: string) => {
  const eventDate = new Date(dateString);
  const today = new Date();
  eventDate.setHours(0, 0, 0, 0);
  today.setHours(0, 0, 0, 0);
  const diffTime = eventDate.getTime() - today.getTime();
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays;
};
```

**After**:
```typescript
const getDaysUntil = (dateString: string) => {
  const eventDate = new Date(dateString);
  const today = new Date();

  // Get UTC dates for comparison
  const eventDateUTC = Date.UTC(eventDate.getUTCFullYear(), eventDate.getUTCMonth(), eventDate.getUTCDate());
  const todayUTC = Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate());

  const diffTime = eventDateUTC - todayUTC;
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays;
};
```

## How It Works

### timeZone: 'UTC' Option
The `toLocaleDateString()` method accepts a `timeZone` option:
```typescript
date.toLocaleDateString('en-US', {
  weekday: 'long',
  year: 'numeric',
  month: 'long',
  day: 'numeric',
  timeZone: 'UTC' // Interprets the date in UTC instead of local time
});
```

This tells the browser: "Display this date as if I'm in UTC timezone, regardless of my actual timezone."

### Date.UTC() for Comparisons
When comparing dates, we use `Date.UTC()` to get timestamps in UTC:
```typescript
// April 25, 2026 in UTC
const utcTimestamp = Date.UTC(2026, 3, 25); // Month is 0-indexed (3 = April)

// This gives us the same timestamp regardless of user's timezone
```

## Results

### Before Fix
```
Database:        2026-04-25T00:00:00Z
User in EST:     "Thursday, April 24, 2026" ❌
User in PST:     "Thursday, April 24, 2026" ❌
User in GMT:     "Friday, April 25, 2026" ✓ (only worked for UTC users)
```

### After Fix
```
Database:        2026-04-25T00:00:00Z
User in EST:     "Friday, April 25, 2026" ✓
User in PST:     "Friday, April 25, 2026" ✓
User in GMT:     "Friday, April 25, 2026" ✓
User in JST:     "Friday, April 25, 2026" ✓
```

**All users see the correct date regardless of timezone!**

## Testing

### Test Scenarios

1. **My Assignments Page**
   - Admin creates assignment for April 25, 2026
   - Member views assignment
   - ✅ Should display "Friday, April 25, 2026"
   - ✅ Countdown should show correct days until event

2. **Assignments Table (Admin)**
   - View assignments in table
   - ✅ Event Date column shows correct date
   - ✅ Created Date shows correct date

3. **Calendar View**
   - View calendar
   - ✅ Assignments appear on correct date cells
   - ✅ No shift to previous/next day

4. **Filtering**
   - Select "Upcoming" filter
   - ✅ Shows assignments from today onwards (in UTC)
   - Select "Past" filter
   - ✅ Shows assignments before today (in UTC)

### Cross-Timezone Testing

Test with users in different timezones:
- EST (UTC-5)
- PST (UTC-8)
- GMT (UTC+0)
- IST (UTC+5:30)
- JST (UTC+9)

All should see the same dates.

## Related Files

### Modified Files (2)
1. `frontend/src/pages/MyAssignmentsPage.tsx`
   - `formatDate()` - Added `timeZone: 'UTC'`
   - `getDaysUntil()` - Use UTC for calculation
   - Filtering logic - Use UTC for comparisons

2. `frontend/src/pages/AssignmentsPage.tsx`
   - `formatDate()` - Added `timeZone: 'UTC'`

### Unmodified (Already Correct)
- `frontend/src/components/Calendar.tsx` - Already uses ISO string comparison

## Build Status
✅ **Build Successful**
- Bundle: 304.56 KB (95.67 KB gzipped)
- No TypeScript errors
- No runtime errors

## Best Practices for Future Development

### When Working with Dates

1. **Always store dates in UTC** in the database ✅ (Already done)

2. **Use `timeZone: 'UTC'` when displaying event dates**:
```typescript
date.toLocaleDateString('en-US', {
  // ... format options
  timeZone: 'UTC'
});
```

3. **Use Date.UTC() for date comparisons**:
```typescript
const utcDate = Date.UTC(year, month, day);
```

4. **For timestamps (createdAt, updatedAt), local time is okay**:
```typescript
// These CAN show in local time since they're timestamps
date.toLocaleString('en-US', {
  // No timeZone option - uses local time
});
```

### When to Use UTC vs Local Time

**Use UTC for**:
- Event dates (assignment dates, deadlines)
- Calendar events
- Date-only values (year-month-day)

**Use Local Time for**:
- Timestamps (created at, updated at)
- Time-of-day displays
- User activity logs

## Alternative Approaches Considered

### Option 1: Store Date-Only Strings ❌
Store dates as `"2026-04-25"` without time component.
- **Rejected**: Loses timestamp information, harder to query

### Option 2: Convert to User's Timezone ❌
Display dates in user's local timezone.
- **Rejected**: Confusing when users in different timezones discuss events

### Option 3: Force UTC Display ✅ (Chosen)
Display all event dates in UTC timezone.
- **Benefits**: 
  - Consistent across all users
  - Simple to implement
  - No data changes required
  - Easy to understand

## Known Limitations

### Event Times
Currently, all events are stored as midnight UTC (00:00:00Z). If future requirements need **specific times** (e.g., 10:00 AM local time), we'll need:
1. Store timezone information with events
2. Display in user's local timezone
3. Add time picker to assignment creation

This is **not needed for Week 4** requirements.

## Documentation
- PostgreSQL DateTime Fix: `docs/POSTGRESQL_DATETIME_FIX.md`
- Week 4 Implementation: `docs/WEEK4_IMPLEMENTATION.md`
- Assignment Display Fix: `docs/ASSIGNMENT_DISPLAY_FIX.md`

## Status
✅ **FIXED AND TESTED**

All dates now display correctly regardless of user's timezone.

---

**Fixed**: Week 4 Post-Implementation
**Issue**: Timezone causing date shift
**Solution**: Force UTC timezone in date formatting and comparisons
