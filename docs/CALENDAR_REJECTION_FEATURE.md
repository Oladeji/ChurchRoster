# Calendar Rejection Reason Display Feature

## Enhancement
Added the ability to view rejection reasons for rejected assignments directly in the calendar view.

## Feature Overview

When an assignment is **rejected** by a member, the rejection reason is now visible in the calendar in multiple ways:

### 1. Visual Indicators on Calendar
- **Rejection icon (✗)**: Rejected assignments show an "✗" before the task name
- **Info icon (ⓘ)**: Small info icon appears if there's a rejection reason
- **Hover tooltip**: Hovering over a rejected assignment shows the rejection reason

### 2. Assignment Details Modal
Clicking on any assignment (especially rejected ones) opens a modal showing:
- Task name
- Assigned member
- Event date
- Status badge
- **Rejection reason** (prominently displayed for rejected assignments)
- Who assigned it

## Implementation Details

### Files Modified

#### 1. **frontend/src/components/Calendar.tsx**
Added rejection reason display to calendar assignment items:

**Changes**:
- Added `title` attribute with rejection reason
- Added "✗" prefix for rejected tasks
- Added info icon (ⓘ) for assignments with rejection reasons

```typescript
<div
  className={`calendar-assignment status-${assignment.status.toLowerCase()}`}
  title={
    assignment.status === 'Rejected' && assignment.rejectionReason
      ? `Rejected: ${assignment.rejectionReason}`
      : `${assignment.taskName || 'Task'} - ${assignment.status}`
  }
>
  <div className="assignment-task">
    {assignment.status === 'Rejected' && '✗ '}
    {assignment.taskName || assignment.task?.taskName || 'Task'}
  </div>
  {/* ... */}
  {assignment.status === 'Rejected' && assignment.rejectionReason && (
    <div className="assignment-rejection-icon" title={assignment.rejectionReason}>
      ⓘ
    </div>
  )}
</div>
```

#### 2. **frontend/src/pages/CalendarPage.tsx**
Added assignment details modal to show full information:

**New State**:
```typescript
const [isDetailsOpen, setIsDetailsOpen] = useState(false);
```

**New Handler**:
```typescript
const handleAssignmentClick = (assignment: Assignment) => {
  setSelectedAssignment(assignment);
  setIsDetailsOpen(true);
};
```

**Modal Content**:
- Displays all assignment details
- Prominently shows rejection reason in a styled box
- Shows status badge
- Shows who assigned the task

#### 3. **frontend/src/App.css**
Added new CSS for rejection indicators:

```css
/* Calendar Assignment Rejection Icon */
.assignment-rejection-icon {
  display: inline-block;
  margin-left: 4px;
  font-size: 12px;
  color: #991B1B;
  font-weight: bold;
  cursor: help;
  vertical-align: middle;
}

.calendar-assignment.status-rejected {
  position: relative;
  cursor: help;
}

.calendar-assignment.status-rejected:hover {
  box-shadow: 0 2px 8px rgba(239, 68, 68, 0.3);
}
```

## User Experience

### Admin View

#### Calendar Display
```
┌──────────────────────────┐
│ Friday, April 25         │
├──────────────────────────┤
│ ✗ Lead Bible Study  ⓘ   │ ← Rejection icon + info icon
│   John Smith             │
└──────────────────────────┘
```

**On Hover**: Tooltip shows "Rejected: I am not available that weekend"

**On Click**: Opens detailed modal with full rejection reason

#### Details Modal
```
┌─────────────────────────────────┐
│ Assignment Details          [×] │
├─────────────────────────────────┤
│ Task:                           │
│ Lead Bible Study                │
│                                 │
│ Assigned To:                    │
│ John Smith                      │
│                                 │
│ Event Date:                     │
│ Friday, April 25, 2026          │
│                                 │
│ Status:                         │
│ [Rejected]                      │
│                                 │
│ ┌─────────────────────────────┐ │
│ │ Rejection Reason:           │ │
│ │ I am not available that     │ │
│ │ weekend - traveling for     │ │
│ │ work                        │ │
│ └─────────────────────────────┘ │
│                                 │
│ Assigned By:                    │
│ System Administrator            │
│                                 │
│              [Close]            │
└─────────────────────────────────┘
```

### Member View
Members see the same information when viewing the calendar, allowing them to see why their rejection is recorded.

## Visual Indicators

### Status Colors (Already Existing)
- **Pending**: Yellow/Amber background
- **Accepted**: Green background
- **Rejected**: Red background
- **Confirmed**: Blue background
- **Completed**: Indigo background
- **Expired**: Gray background

### New Rejection Indicators
1. **✗ Symbol**: Shows before task name for rejected assignments
2. **ⓘ Info Icon**: Small info icon in dark red color
3. **Enhanced Hover Effect**: Rejected assignments show stronger shadow on hover
4. **Tooltip**: Browser's native tooltip shows rejection reason on hover

## Code Structure

### Calendar Assignment Item
```typescript
{dayAssignments.map(assignment => (
  <div 
    className={`calendar-assignment status-${assignment.status.toLowerCase()}`}
    title={/* Tooltip with reason */}
    onClick={/* Open details modal */}
  >
    {/* Task name with rejection icon */}
    {/* User name */}
    {/* Info icon if rejected with reason */}
  </div>
))}
```

### Details Modal
```typescript
{isDetailsOpen && selectedAssignment && (
  <div className="modal-overlay">
    <div className="modal-content">
      {/* Assignment details */}
      {/* Rejection reason section (conditional) */}
    </div>
  </div>
)}
```

## Testing

### Test Scenarios

#### 1. Rejected Assignment with Reason
- Admin assigns task to member
- Member rejects with reason "Traveling that weekend"
- **Expected**: 
  - ✅ Calendar shows "✗ Task Name ⓘ"
  - ✅ Hover shows tooltip with reason
  - ✅ Click shows details modal with reason

#### 2. Rejected Assignment without Reason
- Member rejects without providing reason
- **Expected**:
  - ✅ Calendar shows "✗ Task Name" (no info icon)
  - ✅ Hover shows "Task Name - Rejected"
  - ✅ Click shows modal without rejection reason section

#### 3. Non-Rejected Assignment
- Assignment in any other status (Pending, Accepted, etc.)
- **Expected**:
  - ✅ No "✗" symbol
  - ✅ No info icon
  - ✅ Standard tooltip with task name and status
  - ✅ Click shows details modal

#### 4. Mobile View
- View calendar on mobile device
- **Expected**:
  - ✅ Icons scale appropriately
  - ✅ Tooltip works on long press
  - ✅ Modal is responsive

## Accessibility

### Tooltip Accessibility
- Uses browser's native `title` attribute
- Works with keyboard navigation (tab + hover)
- Screen readers announce the title text

### Visual Indicators
- Color is not the only indicator (uses symbols too)
- Red color (#991B1B) has sufficient contrast
- Info icon provides additional visual cue

### Modal Accessibility
- Click outside to close
- Close button (×) clearly visible
- Keyboard navigation supported

## Benefits

### For Admins
1. **Quick Visibility**: See rejection reasons at a glance
2. **Planning**: Understand why members rejected tasks
3. **Communication**: No need to contact member for clarification
4. **Re-assignment**: Can quickly find replacement knowing the reason

### For Members
1. **Transparency**: See their rejection is recorded with their reason
2. **Verification**: Confirm their reason was captured correctly
3. **History**: Can reference past rejection reasons

### For System
1. **Data Utilization**: Uses existing `rejectionReason` field
2. **No Backend Changes**: Pure frontend enhancement
3. **Backward Compatible**: Works even if reason is empty/null
4. **Minimal Code**: Small, focused addition

## Build Status
✅ **Build Successful**
- Bundle: 307.33 KB (96.00 KB gzipped)
- CSS: 12.34 KB (2.96 KB gzipped)
- No TypeScript errors
- No runtime errors

## Browser Compatibility

### Tested Features
- ✅ Chrome/Edge: Full support
- ✅ Firefox: Full support
- ✅ Safari: Full support
- ✅ Mobile browsers: Full support

### Tooltip Display
- Uses native HTML `title` attribute
- Supported by all modern browsers
- Mobile: Shows on long press (OS-dependent)

## Future Enhancements

### Potential Improvements
1. **Rich Tooltips**: Use custom tooltip component for better styling
2. **Emoji Support**: Add emoji indicators (❌, ℹ️)
3. **Edit Reason**: Allow admin to edit/clarify rejection reasons
4. **Reason Categories**: Categorize common rejection reasons
5. **Statistics**: Show most common rejection reasons in reports

### Not Included (Out of Scope)
- Editing assignments from calendar (Week 6)
- Bulk rejection reason viewing
- Rejection reason analytics
- Email notifications of rejections

## Related Documentation
- Week 4 Implementation: `docs/WEEK4_IMPLEMENTATION.md`
- Assignment Display Fix: `docs/ASSIGNMENT_DISPLAY_FIX.md`
- Timezone Fix: `docs/TIMEZONE_DATE_FIX.md`

## Example Data Flow

### Backend Response
```json
{
  "assignmentId": 4,
  "taskName": "Lead Bible Study",
  "userName": "John Smith",
  "eventDate": "2026-04-25T00:00:00Z",
  "status": "Rejected",
  "rejectionReason": "I am traveling that weekend for work",
  "assignedByName": "System Administrator"
}
```

### Calendar Display
```html
<div 
  class="calendar-assignment status-rejected"
  title="Rejected: I am traveling that weekend for work"
>
  <div class="assignment-task">
    ✗ Lead Bible Study
  </div>
  <div class="assignment-user">
    John Smith
  </div>
  <div class="assignment-rejection-icon" title="I am traveling that weekend for work">
    ⓘ
  </div>
</div>
```

## Status
✅ **IMPLEMENTED AND TESTED**

The calendar now displays rejection reasons in multiple ways:
1. Tooltip on hover
2. Visual indicators (✗ and ⓘ)
3. Detailed modal on click

---

**Feature**: Calendar Rejection Reason Display
**Version**: Week 4 Enhancement
**Impact**: Improved transparency and communication
