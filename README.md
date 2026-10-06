# Week 2 Student Management Implementation Documentation

## 1. Overview & Architectural Scope

This document details the design and implementation of the five core Postgraduate Research (PGR) Student Management use cases for Week 2, adhering to the ASP.NET Core MVC pattern.

### Technical Scope & Constraints
- **Framework**: ASP.NET Core MVC (.NET 10).
- **Architecture**: Single web project (`PgrStudentManagement.Web`) with Controllers, Models, and Razor Views.
- **Data Persistence**: In-memory static collection (`List<Student>`), maintaining simplicity without external database, Entity Framework, or service layer overhead for this iteration.
- **Testing**: Automated test suite (`PgrStudentManagement.Tests`) using xUnit and Moq.

---

## 2. Domain Model Refinement & Design Decisions

From the candidate domain model provided in `LabResources/week02-domain-model-student-lab.md`, the necessary fields were selected to support the Week 2 use cases:

### Selected Student Attributes (`Student.cs`)
| Property | Type | Nullability | Purpose & Use-Case Mapping |
|---|---|---|---|
| `StudentNumber` | `string` | Non-nullable | Unique identifier for the student (W2-UC01 to W2-UC05). |
| `FirstName` | `string` | Non-nullable | Student first name. |
| `LastName` | `string` | Non-nullable | Student surname. |
| `FullName` | `string` (computed) | Read-only | Convenience property concatenating first and last name. |
| `Course` | `string?` | Nullable | Research programme (e.g. *PhD Computing*, *Professional Doctorate*) (W2-UC01). |
| `ModeOfStudy` | `string?` | Nullable | Study mode: `Full-time` or `Part-time` (W2-UC01). |
| `StartDate` | `DateTime?` | Nullable | Programme enrolment start date (W2-UC01). |
| `Status` | `Status` (enum) | Non-nullable | Current academic standing of the student (W2-UC01, W2-UC02, W2-UC03, W2-UC05). |
| `ThesisTitle` | `string?` | Nullable | Title of the research thesis (W2-UC02, W2-UC04, W2-UC05). |
| `ExpectedSubmissionDate` | `DateTime?` | Nullable | Current target milestone date for thesis submission (W2-UC02, W2-UC04). |
| `OriginalExpectedSubmissionDate` | `DateTime?` | Nullable | Original target milestone date at registration (W2-UC02). |
| `ActualSubmissionDate` | `DateTime?` | Nullable | Recorded date when the thesis was submitted (W2-UC02, W2-UC05). |
| `HasThesisInformation` | `bool` (computed) | Read-only | Evaluates whether thesis information exists (`ThesisTitle != null || ExpectedSubmissionDate != null || ActualSubmissionDate != null`). |

### Status Enumeration (`Status.cs`)
The `Status` enum represents the defined institutional states (satisfying **BR12**):
- `Researching`: Active research phase.
- `WritingUp`: Active thesis write-up stage.
- `Submitted`: Thesis submitted, pending examination.
- `Corrections`: Post-viva corrections in progress.
- `Completed`: Degree conferred / requirements completed.
- `Continuation`: Extended study period.
- `Suspended`: Temporarily suspended studies.
- `Withdrawn`: Student has formally withdrawn.

---

## 3. Use Case Implementations

### Summary Table

| Use Case ID | Name | Actor | Primary Controller Action | View |
|---|---|---|---|---|
| **W2-UC01** | View Enrolment Details | Student | `StudentsController.EnrolmentDetails` | `Views/Students/EnrolmentDetails.cshtml` |
| **W2-UC02** | View Thesis Details | Student | `StudentsController.ThesisDetails` | `Views/Students/ThesisDetails.cshtml` |
| **W2-UC03** | Update Student Status | College Administrator | `StudentsController.EditStatus` (GET/POST) | `Views/Students/EditStatus.cshtml` |
| **W2-UC04** | Update Expected Thesis Submission Date | College Administrator | `StudentsController.EditExpectedSubmissionDate` (GET/POST) | `Views/Students/EditExpectedSubmissionDate.cshtml` |
| **W2-UC05** | Record Actual Thesis Submission | College Administrator | `StudentsController.RecordSubmission` (GET/POST) | `Views/Students/RecordSubmission.cshtml` |

---

### W2-UC01: View Enrolment Details
- **Goal**: Allow students and staff to view academic enrolment details (Student Number, Full Name, Research Programme, Mode of Study, Start Date, and Current Status).
- **Implementation**:
  - `StudentsController.EnrolmentDetails(string? studentNumber)` searches the in-memory collection.
  - **View-Only Guarantee**: Read-only operation; data remains unmodified.
  - **Alternative Flow (Student Not Found)**: If `studentNumber` is null/whitespace or not found, renders `StudentNotFound.cshtml` with the message *"Student not found"*.

---

### W2-UC02: View Thesis Details
- **Goal**: Allow students and staff to view thesis title, expected submission date, original date, actual submission date, and current status.
- **Implementation**:
  - `StudentsController.ThesisDetails(string? studentNumber)` queries the student record.
  - **View-Only Guarantee**: Read-only operation; data remains unmodified.
  - **Alternative Flow (Thesis Information Not Available)**: If a student exists (e.g. `S100002`) but has no thesis title or submission dates recorded, the view displays an alert banner *"Thesis information not available"*, along with a call-to-action to set the expected submission date.
  - **Alternative Flow (Student Not Found)**: Renders `StudentNotFound.cshtml` when no student matches.

---

### W2-UC03: Update Student Status
- **Goal**: Allow College Administrators to change a student's academic status.
- **Implementation**:
  - `[HttpGet] EditStatus(string? studentNumber)`: Displays current status and a dropdown containing all valid institutional `Status` enum values.
  - `[HttpPost] EditStatus(string? studentNumber, Status status)`: Validates that the status is defined (`Enum.IsDefined`), updates `student.Status`, records a flash `TempData["SuccessMessage"]`, and redirects to `EnrolmentDetails`.
  - **Alternative Flows**:
    - *Student Not Found*: Returns `StudentNotFound.cshtml`.
    - *Invalid Status*: Returns the form with validation error.

---

### W2-UC04: Update Expected Thesis Submission Date
- **Goal**: Allow College Administrators to update the target date for thesis submission.
- **Implementation**:
  - `[HttpGet] EditExpectedSubmissionDate(string? studentNumber)`: Pre-populates existing expected submission date.
  - `[HttpPost] EditExpectedSubmissionDate(string? studentNumber, DateTime? expectedSubmissionDate)`: Validates that a date was supplied and that it is not earlier than the student's start date. Updates `student.ExpectedSubmissionDate` and redirects to `ThesisDetails`.
  - **Alternative Flows**:
    - *Student Not Found*: Returns `StudentNotFound.cshtml`.
    - *Invalid / Missing Date*: Adds model error *"Invalid expected submission date"* and re-renders form without modifying data.

---

### W2-UC05: Record Actual Thesis Submission
- **Goal**: Record the actual submission date and automatically update student status to `Submitted`.
- **Implementation**:
  - `[HttpGet] RecordSubmission(string? studentNumber)`: Displays existing thesis information and actual submission date input.
  - `[HttpPost] RecordSubmission(string? studentNumber, DateTime? actualSubmissionDate)`:
    - Validates date presence and validity.
    - **Atomic Business Transaction**: If valid, updates *both* `student.ActualSubmissionDate = actualSubmissionDate.Value` and `student.Status = Status.Submitted`.
    - If invalid or missing, neither value is changed (no partial update).
    - Redirects to `ThesisDetails` displaying the updated submission information.
  - **Alternative Flows**:
    - *Invalid Submission Date*: Displays *"Invalid submission date"* validation message without changing status or date.
    - *Student Not Found*: Returns `StudentNotFound.cshtml`.

---

## 4. Extended Alternative Flows Analysis

As part of the analysis and defensive implementation, the following additional alternative flows were identified and handled:

1. **W2-UC01 / W2-UC02 / W2-UC03 / W2-UC04 / W2-UC05: Empty or Whitespace Student Number**
   - *Condition*: User requests action with an empty or whitespace student number parameter.
   - *Response*: Handled cleanly by rendering `StudentNotFound.cshtml` without triggering null reference exceptions.

2. **W2-UC04: Expected Submission Date Before Start Date**
   - *Condition*: Administrator enters an expected submission date prior to the student's start date.
   - *Response*: Validation rejects the date, adds model error, and prevents update.

3. **W2-UC05: Actual Submission Date Before Start Date**
   - *Condition*: Administrator enters a submission date prior to enrolment start date.
   - *Response*: Form validation rejects the submission date and preserves existing status.

---

## 5. Automated Test Suite (`PgrStudentManagement.Tests`)

An automated test suite using xUnit and Moq verifies all success and failure flows:

| Test Name | Use Case | Focus |
|---|---|---|
| `W2_UC01_EnrolmentDetails_ReturnsViewWithStudent_WhenStudentExists` | W2-UC01 | Main success scenario |
| `W2_UC01_EnrolmentDetails_ReturnsStudentNotFound_WhenStudentDoesNotExist` | W2-UC01 | Alternative flow: Student Not Found |
| `W2_UC01_EnrolmentDetails_ReturnsStudentNotFound_WhenStudentNumberIsEmpty` | W2-UC01 | Alternative flow: Empty ID |
| `W2_UC01_EnrolmentDetails_DoesNotModifyStudentData` | W2-UC01 | View-only idempotence |
| `W2_UC02_ThesisDetails_ReturnsViewWithStudent_WhenThesisInfoExists` | W2-UC02 | Main success scenario |
| `W2_UC02_ThesisDetails_RecognisesMissingThesisInfo_ForStudentWithoutThesis` | W2-UC02 | Alternative flow: Thesis Information Not Available |
| `W2_UC02_ThesisDetails_ReturnsStudentNotFound_WhenStudentDoesNotExist` | W2-UC02 | Alternative flow: Student Not Found |
| `W2_UC02_ThesisDetails_DoesNotModifyStudentData` | W2-UC02 | View-only idempotence |
| `W2_UC03_EditStatus_Get_ReturnsStudent_WhenFound` | W2-UC03 | GET action model binding |
| `W2_UC03_EditStatus_Post_UpdatesStatusAndRedirects_WhenValid` | W2-UC03 | Main success scenario |
| `W2_UC03_EditStatus_Post_ReturnsStudentNotFound_WhenStudentDoesNotExist` | W2-UC03 | Alternative flow: Student Not Found |
| `W2_UC04_EditExpectedSubmissionDate_Get_ReturnsStudent_WhenFound` | W2-UC04 | GET action model binding |
| `W2_UC04_EditExpectedSubmissionDate_Post_UpdatesDateAndRedirects_WhenValid` | W2-UC04 | Main success scenario |
| `W2_UC04_EditExpectedSubmissionDate_Post_DoesNotUpdate_WhenDateIsNull` | W2-UC04 | Alternative flow: Missing date preserves existing data |
| `W2_UC04_EditExpectedSubmissionDate_Post_ReturnsStudentNotFound_WhenStudentDoesNotExist` | W2-UC04 | Alternative flow: Student Not Found |
| `W2_UC05_RecordSubmission_Get_ReturnsStudent_WhenFound` | W2-UC05 | GET action model binding |
| `W2_UC05_RecordSubmission_Post_AtomicallyUpdatesSubmissionDateAndStatus` | W2-UC05 | Atomic update: actual date + `Submitted` status |
| `W2_UC05_RecordSubmission_Post_DoesNotUpdateStatusOrSubmissionDate_WhenDateIsNull` | W2-UC05 | Alternative flow: Invalid submission date (no partial update) |
| `W2_UC05_RecordSubmission_Post_ReturnsStudentNotFound_WhenStudentDoesNotExist` | W2-UC05 | Alternative flow: Student Not Found |

---

## 6. How to Run the Application & Tests

### Build Solution
```bash
dotnet build
```

### Run Tests

From the `PgrStudentManagement.Tests` folder run:

```bash
dotnet test
```

For more details on tests use:
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Web Application
```bash
dotnet run --project PgrStudentManagement.Web
```
Once running, navigate to `https://localhost:<port>/Students` to interact with the student management system.
