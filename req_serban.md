## Doctor Schedule

### 1) Build the Doctor Schedule screen
**Type:** Frontend  
**Description:** Create the main page where doctors can see appointments and shifts in one place.

**What to implement**
- Add a dedicated Doctor Schedule page in doctor navigation.
- Show content in calendar/agenda style.
- Include loading, empty, and error states.
- Restrict access to doctor users.

---

### 2) Fetch upcoming appointments for the logged-in doctor
**Type:** Backend  
**Description:** Provide reliable appointment data scoped to the current doctor.

**What to implement**
- API/service call that returns upcoming appointments for the current doctor only.
- Include: appointment id, date, start time, end time, status, location.
- Default to future appointments.
- Handle API failure and larger response sets safely.

---

### 3) Show appointments inside the calendar
**Type:** Frontend  
**Description:** Render appointment data in a way that is easy to read and interact with.

**What to implement**
- Render appointments in day/week views.
- Support overlapping events without UI breakage.
- Click/tap opens details panel/modal.
- Use clear visual markers for appointment status/type.

---

### 4) Add doctor shift roster (day/week)
**Type:** Frontend + Backend  
**Description:** Let doctors view their on-duty shifts by day or by week.

**What to implement**
- Day/Week toggle for shift roster.
- Sort shifts by time.
- Handle multiple shifts in the same day.
- Show clear empty state when no shifts exist.

---

### 5) Show shift location (ER, clinic room, etc.)
**Type:** Frontend  
**Description:** Display where each doctor shift takes place.

**What to implement**
- Display location on each shift row/block.
- Show location in both list and shift details.
- Fallback text for missing values (e.g., `Location TBD`).

---

### 6) Display exact shift start/end time blocks
**Type:** Frontend  
**Description:** Make shift timing explicit and accurate in the UI.

**What to implement**
- Show Shift Start Time and Shift End Time clearly.
- Render shifts as precise time blocks in timeline/calendar.
- Correctly handle overnight shifts.
- Respect 12h/24h user preference.

---

### 7) Doctor access rules + data validation
**Type:** Security / Backend  
**Description:** Protect schedule privacy and enforce valid shift data.

**What to implement**
- Enforce that doctors can only access their own schedule.
- Reject unauthorized access attempts and log them.
- Validate shift time ranges (`start < end`).
- Handle malformed/duplicate records safely.

---

### 8) Test coverage for doctor schedule
**Type:** QA / Test Automation  
**Description:** Add enough coverage to keep core doctor schedule flows stable.

**What to implement**
- Unit tests for mapping/transformation logic.
- Integration tests for API + UI flow.
- E2E checks for:
  - appointments visible
  - day/week switch
  - location visibility
  - correct shift time blocks

---

## Pharmacist Schedule

### 9) Build Pharmacy Schedule screen
**Type:** Frontend  
**Description:** Create a dedicated schedule screen for pharmacists.

**What to implement**
- Add Pharmacy Schedule page in pharmacist navigation.
- Show roster/calendar style schedule.
- Include loading, empty, and error states.
- Restrict access to pharmacist role.

---

### 10) Fetch pharmacist shifts and rotation assignments
**Type:** Backend  
**Description:** Provide schedule data with rotation details for the logged-in pharmacist.

**What to implement**
- API/service for pharmacist schedule data.
- Include: shift id, rotation assignment, start/end time, status, location/unit (if available).
- Support day/week date range queries.
- Handle large datasets safely.

---

### 11) Add day/week roster mode for pharmacists
**Type:** Frontend  
**Description:** Support quick switching between daily and weekly pharmacist schedule views.

**What to implement**
- Day/Week toggle.
- Sort shifts chronologically.
- Support multiple shifts per day.
- Clear empty state when no data exists.

---

### 12) Show rotation assignment clearly on each shift
**Type:** Frontend  
**Description:** Make pharmacy rotation assignment visible and obvious in schedule views.

**What to implement**
- Show rotation label on each shift.
- Include in list and in details panel.
- Fallback value when missing (e.g., `Rotation TBD`).
- Keep labels aligned with backend values.

---

### 13) Show exact shift duration (start/end + computed duration)
**Type:** Frontend  
**Description:** Display complete time information for pharmacist shifts.

**What to implement**
- Always show start and end times.
- Show computed duration (example: `8h 30m`).
- Handle overnight shifts correctly.
- Respect 12h/24h display setting.

---

### 14) Show shift status (Scheduled / Active / Completed)
**Type:** Frontend  
**Description:** Surface real-time shift status clearly.

**What to implement**
- Display status in roster and details.
- Use clear visual treatment (badge/color/icon).
- Safe fallback for unknown statuses.

---

### 15) Pharmacist access rules + data validation
**Type:** Security / Backend  
**Description:** Apply strict access control and schedule data validation for pharmacists.

**What to implement**
- Enforce that pharmacists can only access their own schedule.
- Reject and log unauthorized requests.
- Validate shift ranges (`start < end`).
- Handle duplicate/malformed entries safely.

---

### 16) Test coverage for pharmacist schedule
**Type:** QA / Test Automation  
**Description:** Add robust tests for pharmacist scheduling features and edge cases.

**What to implement**
- Unit tests for mapping, duration, and status logic.
- Integration tests for API-to-UI rendering.
- E2E checks for:
  - day/week visibility
  - rotation assignment visibility
  - correct start/end + duration
  - status badge behavior

---

## Optional implementation notes
- Reuse shared schedule components between doctor and pharmacist modules.
- Keep labels/wording consistent across both views.
- Validate timezone behavior early to avoid hard-to-debug issues.
