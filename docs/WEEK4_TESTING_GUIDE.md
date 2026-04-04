# Week 4 - Quick Start Testing Guide

## Prerequisites
- Backend API running and deployed (Week 1-2 complete)
- Frontend running locally (`npm run dev` in frontend folder)
- At least one admin user and one member user created
- Some tasks created in the system
- Some members with skills assigned

## Testing Flow

### 1. Admin: Create Assignments via Calendar

**Steps:**
1. Log in as **Admin**
2. Click **📅 Calendar** card from dashboard
3. You'll see the current month calendar
4. Click **+ New Assignment** button (or click any date)
5. Assignment modal opens:
   - **Select a Task**: Choose from dropdown (e.g., "Lead Bible Study - Tuesday")
   - Notice: If task requires a skill, member list automatically filters to show only qualified members
   - **Select a Member**: Choose from filtered list
   - **Event Date**: Select the date (pre-filled if you clicked a date)
   - **Override checkbox**: Only use if you need to assign an unqualified member
6. Click **Create Assignment**
7. Assignment appears on the calendar on the selected date
8. Assignment color reflects status (yellow = Pending)

**Expected Result:**
✅ Assignment created successfully
✅ Assignment visible on calendar
✅ Only qualified members shown in dropdown (unless override enabled)

### 2. Admin: View All Assignments

**Steps:**
1. From Calendar page, click **📅 Calendar View** in header OR
2. From Dashboard, click **📋 Assignments** card
3. See table with all assignments
4. Try filtering by status:
   - Select "Pending" from dropdown
   - Only pending assignments show
5. View summary statistics at bottom

**Expected Result:**
✅ All assignments displayed in table
✅ Status filter works correctly
✅ Summary shows correct counts

### 3. Member: View My Assignments

**Steps:**
1. Log out from admin account
2. Log in as a **Member** (the one you assigned a task to)
3. Click **📝 My Assignments** card from dashboard
4. See your assignments listed as cards
5. Notice:
   - Days until event countdown
   - Task details
   - Status badge
   - Accept/Reject buttons (if status is Pending)

**Expected Result:**
✅ Member sees only their own assignments
✅ Countdown shows correctly
✅ Accept/Reject buttons visible for pending items

### 4. Member: Accept Assignment

**Steps:**
1. On "My Assignments" page
2. Find a pending assignment
3. Click **✓ Accept** button
4. Status updates to "Accepted"
5. Badge turns green
6. Accept/Reject buttons disappear

**Expected Result:**
✅ Status changes to "Accepted"
✅ Badge color changes to green
✅ Action buttons removed

### 5. Member: Reject Assignment

**Steps:**
1. On "My Assignments" page
2. Find another pending assignment
3. Click **✗ Reject** button
4. Browser prompt appears asking for reason
5. Enter reason (e.g., "Traveling that weekend")
6. Click OK
7. Status updates to "Rejected"
8. Badge turns red
9. Rejection reason displays below task details

**Expected Result:**
✅ Rejection reason prompt appears
✅ Status changes to "Rejected"
✅ Badge color changes to red
✅ Reason displayed in UI

### 6. Admin: View Updated Statuses

**Steps:**
1. Log back in as **Admin**
2. Go to **Calendar** or **Assignments** page
3. See updated assignment statuses:
   - Accepted assignments show green badge
   - Rejected assignments show red badge

**Expected Result:**
✅ Admin sees updated statuses
✅ Colors match member's view

### 7. Test Qualification Filtering

**Steps:**
1. Log in as **Admin**
2. Create a new assignment
3. Select a task that **requires a specific skill** (e.g., "Lead Bible Study" requires "CanLeadBibleStudy")
4. Notice: Member dropdown only shows members who have that skill
5. Try selecting a different task with no skill requirement
6. Notice: Member dropdown shows all active members

**Expected Result:**
✅ Qualified members only shown for restricted tasks
✅ All members shown for unrestricted tasks
✅ Info message displays when filtering is active

### 8. Test Override Feature

**Steps:**
1. As **Admin**, create assignment
2. Select a task requiring a skill
3. Notice: Only 2-3 qualified members show
4. Check **Override qualification check** checkbox
5. Notice: Member dropdown now disabled (allowing you to assign anyway)
6. Uncheck override
7. Dropdown returns to qualified members only

**Expected Result:**
✅ Override checkbox enables/disables validation
✅ Warning message explains override purpose

### 9. Test Calendar Navigation

**Steps:**
1. On Calendar page
2. Click **›** (next month) button
3. Calendar advances one month
4. Click **‹** (previous month) button twice
5. Calendar goes back two months
6. Click **Today** button
7. Calendar returns to current month
8. Click on different dates
9. Assignment modal opens with selected date pre-filled

**Expected Result:**
✅ Month navigation works smoothly
✅ Today button returns to current month
✅ Date clicks open modal with correct date

### 10. Test Filtering on My Assignments

**Steps:**
1. As **Member** with multiple assignments
2. On "My Assignments" page
3. Try different filters:
   - **Upcoming**: Shows future assignments (not completed/expired)
   - **Pending Response**: Shows only assignments awaiting your response
   - **Past Assignments**: Shows completed or past-date assignments
   - **All Assignments**: Shows everything

**Expected Result:**
✅ Each filter shows correct subset
✅ Filters update immediately
✅ Counts in summary reflect filtered view

### 11. Test Responsive Design

**Steps:**
1. Open browser DevTools (F12)
2. Toggle device toolbar (Ctrl+Shift+M)
3. Select mobile device (e.g., iPhone 12)
4. Navigate through:
   - Calendar page
   - Assignments page
   - My Assignments page
5. Check that:
   - Calendar cells are readable
   - Assignment cards stack vertically
   - Buttons are touch-friendly
   - Modals fit screen

**Expected Result:**
✅ All pages responsive on mobile
✅ No horizontal scrolling
✅ Text remains readable
✅ Interactive elements accessible

## Common Issues & Solutions

### Issue: No members show in dropdown
**Solution**: 
- Check that members exist in database
- Verify members are marked as `isActive: true`
- If task requires skill, ensure some members have that skill assigned
- Enable "Override" checkbox to bypass filtering

### Issue: Assignment not appearing on calendar
**Solution**:
- Refresh the page (calendar should auto-refresh, but manual refresh helps)
- Check that the assignment was created (no error message)
- Verify the date is in the current month view
- Navigate to the correct month

### Issue: Can't accept/reject assignment
**Solution**:
- Verify you're logged in as the assigned member
- Check assignment status is "Pending"
- Check browser console for errors
- Verify backend API is running

### Issue: Status doesn't update
**Solution**:
- Check browser console for errors
- Verify API endpoint is correct
- Check network tab to see if request succeeded
- Refresh the page

## Week 4 Success Criteria

✅ **Calendar Component**
- Displays month view with assignments
- Color-coded by status
- Clickable dates and assignments
- Navigation works (prev/next/today)

✅ **Assignment Creation**
- Task selection works
- Member filtering by qualification
- Date selection works
- Override option available
- Validation messages display

✅ **Status Management**
- Accept workflow functional
- Reject workflow with reason
- Status colors correct
- Status updates in real-time

✅ **Member View**
- My Assignments page loads
- Filtering works (upcoming/pending/past)
- Accept/Reject buttons functional
- Countdown displays correctly

✅ **Admin View**
- Assignments table displays
- Status filter works
- Summary statistics accurate
- Delete function works

## Next Steps

After confirming all Week 4 features work:

1. **Review and Document Issues**: Note any bugs found
2. **Performance Check**: Test with 20+ assignments
3. **User Feedback**: Get input from potential users
4. **Prepare for Week 5**: 
   - Email invitation system
   - Push notifications
   - Reports/PDF generation

## Support

If you encounter issues:
1. Check browser console for errors
2. Check network tab for failed API calls
3. Verify backend is running and accessible
4. Review `WEEK4_IMPLEMENTATION.md` for technical details
5. Check that database has required data (tasks, members, skills)

---

**Week 4 Status**: ✅ COMPLETE AND READY FOR TESTING

All features implemented and build successful!
