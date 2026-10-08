
public class StudentServices
{

    private readonly StudentStorage _studentStorage;

    public StudentServices(StudentStorage studentStorage) {

        _studentStorage = studentStorage;

    public IReadOnlyList<Student> GetStudents()
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "name" => students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList().AsReadOnly(),
            "status" => students.OrderBy(s => s.Status).ThenBy(s => s.StudentNumber).ToList().AsReadOnly(),
            "programme" => students.OrderBy(s => s.Course).ThenBy(s => s.StudentNumber).ToList().AsReadOnly(),
            _ => students.OrderBy(s => s.StudentNumber).ToList().AsReadOnly()
        };
    }


    public Student? GetStudent(string studentNumber)
    {
        return _studentStorage.GetStudents();
    }



    public void CreateStudent(Student student)
    {
        if (student == null)
        {
            return ServiceResult<Student>.Fail("Student payload cannot be null.");
        }

        // Validate mandatory attributes
        if (string.IsNullOrWhiteSpace(student.StudentNumber))
        {
            return ServiceResult<Student>.Fail("Student number is mandatory.", "Required_StudentNumber");
        }

        if (string.IsNullOrWhiteSpace(student.FirstName))
        {
            return ServiceResult<Student>.Fail("First name is mandatory.", "Required_FirstName");
        }

        if (string.IsNullOrWhiteSpace(student.LastName))
        {
            return ServiceResult<Student>.Fail("Last name is mandatory.", "Required_LastName");
        }

        // Normalize student number
        student.StudentNumber = student.StudentNumber.Trim();
        student.FirstName = student.FirstName.Trim();
        student.LastName = student.LastName.Trim();
        if (student.Course != null) student.Course = student.Course.Trim();
        if (student.ModeOfStudy != null) student.ModeOfStudy = student.ModeOfStudy.Trim();
        if (student.ThesisTitle != null) student.ThesisTitle = student.ThesisTitle.Trim();

        // Check for duplicate student number
        if (_studentStorage.Exists(student.StudentNumber))
        {
            return ServiceResult<Student>.Fail(
                $"A student with student number '{student.StudentNumber}' already exists.",
                "DuplicateStudentNumber");
        }

        // Validate dates if present
        if (student.StartDate.HasValue && student.ExpectedSubmissionDate.HasValue &&
            student.ExpectedSubmissionDate.Value < student.StartDate.Value)
        {
            return ServiceResult<Student>.Fail(
                "Expected submission date cannot be earlier than the start date.",
                "Invalid_ExpectedDate");
        }

        if (student.StartDate.HasValue && student.ActualSubmissionDate.HasValue &&
            student.ActualSubmissionDate.Value < student.StartDate.Value)
        {
            return ServiceResult<Student>.Fail(
                "Actual submission date cannot be earlier than the start date.",
                "Invalid_ActualDate");
        }

        // Initialize OriginalExpectedSubmissionDate if not explicitly provided
        if (student.ExpectedSubmissionDate.HasValue && !student.OriginalExpectedSubmissionDate.HasValue)
        {
            student.OriginalExpectedSubmissionDate = student.ExpectedSubmissionDate;
        }

        // If actual submission date is supplied at creation, ensure status is Submitted
        if (student.ActualSubmissionDate.HasValue && student.Status != Status.Completed)
        {
            student.Status = Status.Submitted;
        }

        _studentStorage.AddStudent(student);
    
    }



    public void UpdateStatus(string studentNumber, Status status)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = Students.FirstOrDefault(s =>
            s.StudentNumber.Equals(studentNumber.Trim(), StringComparison.OrdinalIgnoreCase));

        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        if (!Enum.IsDefined(typeof(Status), status))
        {
            ModelState.AddModelError(nameof(status), "Invalid status value provided.");
            return View(student);
        }

        student.Status = status;

        TempData["SuccessMessage"] = $"Status updated to {status} for student {student.StudentNumber}.";
        return RedirectToAction(nameof(EnrolmentDetails), new { studentNumber = student.StudentNumber });
    }



    public void UpdateExpectedSubmissionDate(string studentNumber, DateTime? expectedSubmissionDate)
    {

        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = Students.FirstOrDefault(s =>
            s.StudentNumber.Equals(studentNumber.Trim(), StringComparison.OrdinalIgnoreCase));

        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        if (!expectedSubmissionDate.HasValue)
        {
            ModelState.AddModelError("expectedSubmissionDate", "Invalid expected submission date");
            return View(student);
        }

        if (student.StartDate.HasValue && expectedSubmissionDate.Value < student.StartDate.Value)
        {
            ModelState.AddModelError("expectedSubmissionDate", "Invalid expected submission date: Expected date cannot be prior to start date.");
            return View(student);
        }

        student.ExpectedSubmissionDate = expectedSubmissionDate.Value;

        TempData["SuccessMessage"] = $"Expected submission date updated to {expectedSubmissionDate.Value:yyyy-MM-dd} for student {student.StudentNumber}.";
        return RedirectToAction(nameof(ThesisDetails), new { studentNumber = student.StudentNumber });
        
    }





    public void RecordSubmission(string studentNumber, DateTime? actualSubmissionDate)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = Students.FirstOrDefault(s =>
            s.StudentNumber.Equals(studentNumber.Trim(), StringComparison.OrdinalIgnoreCase));

        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        if (!actualSubmissionDate.HasValue)
        {
            ModelState.AddModelError("actualSubmissionDate", "Invalid submission date");
            return View(student);
        }

        if (student.StartDate.HasValue && actualSubmissionDate.Value < student.StartDate.Value)
        {
            ModelState.AddModelError("actualSubmissionDate", "Invalid submission date: Submission date cannot be before enrolment start date.");
            return View(student);
        }

        // Apply atomic change: both submission date and status change together
        student.ActualSubmissionDate = actualSubmissionDate.Value;
        student.Status = Status.Submitted;

        TempData["SuccessMessage"] = $"Actual thesis submission recorded on {actualSubmissionDate.Value:yyyy-MM-dd} and status updated to Submitted for student {student.StudentNumber}.";
        return RedirectToAction(nameof(ThesisDetails), new { studentNumber = student.StudentNumber });
    }


}
