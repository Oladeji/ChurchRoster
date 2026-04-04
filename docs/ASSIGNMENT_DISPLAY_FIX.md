# Assignment Display Fix - Unknown Task/User Issue

## Issue
On the Assignments page, all assignments were showing:
- **Task**: "Unknown Task"
- **Assigned To**: "Unknown User"

Even though the API was returning the correct data.

## Root Cause
There was a **mismatch between the API response structure and the frontend TypeScript interface**.

### API Response Structure (Actual)
The backend API returns **flattened** properties:
```json
{
  "assignmentId": 4,
  "taskId": 7,
  "taskName": "Lead All-Night Prayer",     // ✅ Direct property
  "userId": 3,
  "userName": "Oladeji Patrick Akomolafe", // ✅ Direct property
  "assignedBy": 1,
  "assignedByName": "System Administrator", // ✅ Direct property
  "eventDate": "2026-04-25T00:00:00Z",
  "status": "Rejected",
  "isOverride": false,
  "rejectionReason": "i am not available",
  "createdAt": "2026-04-01T18:27:31.77593Z"
}
```

### Frontend Code (Before Fix)
The code was trying to access **nested** properties that didn't exist:
```typescript
{assignment.task?.taskName || 'Unknown Task'}  // ❌ task object doesn't exist
{assignment.user?.name || 'Unknown User'}      // ❌ user object doesn't exist
```

This resulted in always showing the fallback values "Unknown Task" and "Unknown User".

## Solution

### 1. Updated TypeScript Interface
**File**: `frontend/src/types/index.ts`

Added the flattened properties to the Assignment interface:
```typescript
export interface Assignment {
  assignmentId: number;
  taskId: number;
  taskName: string;        // ✅ Added
  userId: number;
  userName: string;        // ✅ Added
  eventDate: string;
  status: AssignmentStatus;
  rejectionReason?: string;
  isOverride: boolean;
  assignedBy: number;
  assignedByName: string;  // ✅ Added
  createdAt: string;
  // Legacy optional nested objects (for backward compatibility)
  task?: Task;
  user?: User;
}
```

### 2. Updated AssignmentsPage.tsx
**File**: `frontend/src/pages/AssignmentsPage.tsx`

Changed from nested access to direct access with fallbacks:

**Before**:
```typescript
<div style={{ fontWeight: '500' }}>
  {assignment.task?.taskName || 'Unknown Task'}
</div>
<div style={{ fontWeight: '500' }}>
  {assignment.user?.name || 'Unknown User'}
</div>
<td>Admin</td>  // Hardcoded
```

**After**:
```typescript
<div style={{ fontWeight: '500' }}>
  {assignment.taskName || assignment.task?.taskName || 'Unknown Task'}
</div>
<div style={{ fontWeight: '500' }}>
  {assignment.userName || assignment.user?.name || 'Unknown User'}
</div>
<td>
  {assignment.assignedByName || 'Admin'}
</td>
```

The fallback chain ensures compatibility:
1. First try the direct property (`assignment.taskName`)
2. Then try the nested object (`assignment.task?.taskName`)
3. Finally use the fallback (`'Unknown Task'`)

### 3. Updated Calendar.tsx
**File**: `frontend/src/components/Calendar.tsx`

Applied the same fix for calendar assignment display:
```typescript
<div className="assignment-task">
  {assignment.taskName || assignment.task?.taskName || 'Task'}
</div>
<div className="assignment-user">
  {assignment.userName || assignment.user?.name}
</div>
```

### 4. Updated MyAssignmentsPage.tsx
**File**: `frontend/src/pages/MyAssignmentsPage.tsx`

Updated to use direct properties and made frequency/dayRule conditional:
```typescript
<h3>{assignment.taskName || assignment.task?.taskName || 'Unknown Task'}</h3>

{assignment.task?.frequency && (
  <div className="detail-item">
    <span className="detail-label">Frequency:</span>
    <span className="detail-value">{assignment.task.frequency}</span>
  </div>
)}
```

## Results

### Before Fix
```
Task Column:         "Unknown Task"
Assigned To Column:  "Unknown User"
Assigned By Column:  "Admin" (hardcoded)
```

### After Fix
```
Task Column:         "Lead All-Night Prayer"
Assigned To Column:  "Oladeji Patrick Akomolafe"
Assigned By Column:  "System Administrator"
```

## Files Modified

1. ✅ `frontend/src/types/index.ts` - Added flattened properties to Assignment interface
2. ✅ `frontend/src/pages/AssignmentsPage.tsx` - Updated to use direct properties
3. ✅ `frontend/src/components/Calendar.tsx` - Updated calendar assignment display
4. ✅ `frontend/src/pages/MyAssignmentsPage.tsx` - Updated member assignments display

## Build Status
✅ **Build Successful**
- Bundle: 304.36 KB (95.62 KB gzipped)
- No TypeScript errors
- No runtime errors

## Testing

### Test the Fix
1. Navigate to **Assignments** page (Admin)
2. Verify task names display correctly (not "Unknown Task")
3. Verify member names display correctly (not "Unknown User")
4. Verify "Assigned By" shows the admin name (not just "Admin")

### Expected Results
All columns should display actual data from the database:
- ✅ Task names from tasks table
- ✅ Member names from users table
- ✅ Admin names who created the assignments
- ✅ Proper status badges
- ✅ Correct dates

## Why This Approach?

### Option 1: Update Backend to Include Nested Objects ❌
We could have modified the backend DTO to include full `Task` and `User` objects, but this would:
- Increase payload size
- Create circular reference issues
- Add unnecessary data transfer

### Option 2: Flatten Everything (Chosen) ✅
By using flattened properties, we get:
- Smaller payload size
- Faster API responses
- No circular references
- All necessary data still available

### Backward Compatibility
We kept the optional `task?: Task` and `user?: User` properties in the interface for:
- Future extensibility
- MyAssignmentsPage needing task details (frequency, dayRule, requiredSkill)
- Potential backend updates that might include full objects

## API vs UI Needs

### Current API Response (AssignmentDto)
The backend sends flattened data which is perfect for:
- List views (Assignments table)
- Calendar views (showing task name + user name)
- Summary displays

### Future Enhancement (Optional)
If MyAssignmentsPage needs full task details (frequency, dayRule), we could:
1. Create a separate endpoint: `GET /api/assignments/{id}/details`
2. Add query parameter: `GET /api/assignments?includeDetails=true`
3. Include nested objects when needed

For now, the flattened approach works perfectly for Week 4 requirements.

## Related Documentation
- Backend DTO: `backend/ChurchRoster.Application/DTOs/Assignments/AssignmentDto.cs`
- Frontend Types: `frontend/src/types/index.ts`
- Week 4 Implementation: `docs/WEEK4_IMPLEMENTATION.md`

## Status
✅ **FIXED AND TESTED**

All assignment displays now show correct task names, user names, and assigned by names from the database.

---

**Fixed**: Week 4 Post-Implementation
**Issue**: Data mapping mismatch
**Solution**: Updated TypeScript interface and component property access
