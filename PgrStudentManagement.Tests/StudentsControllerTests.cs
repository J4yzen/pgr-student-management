using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PgrStudentManagement.Web.Controllers;
using PgrStudentManagement.Web.Models;
using Xunit;
using Xunit.Abstractions;

namespace PgrStudentManagement.Tests;

[Collection("Sequential")]
public class StudentsControllerTests
{
    private readonly ITestOutputHelper _output;

    public StudentsControllerTests(ITestOutputHelper output)
    {
        _output = output;
        // Reset static state before every single test to guarantee isolation
        StudentsController.ResetDefaultStudents();
    }

    private StudentsController CreateControllerWithTempData()
    {
        var controller = new StudentsController();
        var tempData = new TempDataDictionary(
            new Microsoft.AspNetCore.Http.DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
        controller.TempData = tempData;
        return controller;
    }

    #region W2-UC01: View Enrolment Details

    [Fact(DisplayName = "W2-UC01 [Success]: Displays enrolment details (programme, mode, start date, status) for existing student")]
    [Trait("UseCase", "W2-UC01")]
    [Trait("Flow", "MainSuccess")]
    public void W2_UC01_EnrolmentDetails_ReturnsViewWithStudent_WhenStudentExists()
    {
        // Arrange
        _output.WriteLine("[GIVEN] An existing student with ID 'S100001' (John Doe)");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Requesting Enrolment Details for 'S100001'");
        var result = controller.EnrolmentDetails("S100001");

        // Assert
        _output.WriteLine("[THEN] Verify view model contains full enrolment details");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);

        Assert.True(model.StudentNumber == "S100001", "Expected student number to match 'S100001'");
        Assert.True(model.FullName == "John Doe", "Expected student name to match 'John Doe'");
        Assert.True(model.Course == "PhD Computing", "Expected programme to match 'PhD Computing'");
        Assert.True(model.ModeOfStudy == "Full-time", "Expected mode of study to match 'Full-time'");
        Assert.True(model.Status == Status.Researching, "Expected initial status to match 'Researching'");
        _output.WriteLine("✔ Result: Success - All enrolment attributes correctly populated.");
    }

    [Fact(DisplayName = "W2-UC01 [Alt Flow A1]: Displays 'Student Not Found' when student ID does not match any record")]
    [Trait("UseCase", "W2-UC01")]
    [Trait("Flow", "Alternative")]
    public void W2_UC01_EnrolmentDetails_ReturnsStudentNotFound_WhenStudentDoesNotExist()
    {
        // Arrange
        _output.WriteLine("[GIVEN] A non-existent student number 'UNKNOWN999'");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Requesting Enrolment Details for 'UNKNOWN999'");
        var result = controller.EnrolmentDetails("UNKNOWN999");

        // Assert
        _output.WriteLine("[THEN] System should render 'StudentNotFound' view");
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("StudentNotFound", viewResult.ViewName);
        Assert.Equal("UNKNOWN999", controller.ViewBag.StudentNumber);
        _output.WriteLine("✔ Result: Success - 'StudentNotFound' view returned with missing ID.");
    }

    [Fact(DisplayName = "W2-UC01 [Alt Flow]: Displays 'Student Not Found' when student ID is empty string")]
    [Trait("UseCase", "W2-UC01")]
    [Trait("Flow", "Alternative")]
    public void W2_UC01_EnrolmentDetails_ReturnsStudentNotFound_WhenStudentNumberIsEmpty()
    {
        // Arrange
        _output.WriteLine("[GIVEN] An empty student number");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Requesting Enrolment Details with empty string");
        var result = controller.EnrolmentDetails("");

        // Assert
        _output.WriteLine("[THEN] System should gracefully render 'StudentNotFound' view");
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("StudentNotFound", viewResult.ViewName);
        _output.WriteLine("✔ Result: Success - Handled empty input without error.");
    }

    [Fact(DisplayName = "W2-UC01 [View-Only]: Enrolment details request does not alter student state (Idempotent)")]
    [Trait("UseCase", "W2-UC01")]
    [Trait("Invariant", "Idempotence")]
    public void W2_UC01_EnrolmentDetails_DoesNotModifyStudentData()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Initial student record state for 'S100001'");
        var controller = CreateControllerWithTempData();
        var initialStudent = StudentsController.Students.First(s => s.StudentNumber == "S100001");
        var initialStatus = initialStudent.Status;

        // Act
        _output.WriteLine("[WHEN] Executing view-only Enrolment Details action");
        var result = controller.EnrolmentDetails("S100001");

        // Assert
        _output.WriteLine("[THEN] In-memory student data must remain completely unchanged");
        Assert.Equal(initialStatus, initialStudent.Status);
        _output.WriteLine("✔ Result: Verified view-only safety.");
    }

    #endregion

    #region W2-UC02: View Thesis Details

    [Fact(DisplayName = "W2-UC02 [Success]: Displays thesis details (title, expected date, actual date, status) when recorded")]
    [Trait("UseCase", "W2-UC02")]
    [Trait("Flow", "MainSuccess")]
    public void W2_UC02_ThesisDetails_ReturnsViewWithStudent_WhenThesisInfoExists()
    {
        // Arrange
        _output.WriteLine("[GIVEN] A student 'S100001' with recorded thesis information");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Requesting Thesis Details for 'S100001'");
        var result = controller.ThesisDetails("S100001");

        // Assert
        _output.WriteLine("[THEN] View model should confirm HasThesisInformation is true and provide title and expected date");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);

        Assert.True(model.HasThesisInformation, "Expected student to have thesis information recorded.");
        Assert.Equal("Explainable Models for Clinical Decision Support", model.ThesisTitle);
        Assert.NotNull(model.ExpectedSubmissionDate);
        _output.WriteLine($"✔ Result: Thesis Title='{model.ThesisTitle}', Expected={model.ExpectedSubmissionDate:yyyy-MM-dd}");
    }

    [Fact(DisplayName = "W2-UC02 [Alt Flow A1]: Flags missing thesis information when student has no thesis recorded")]
    [Trait("UseCase", "W2-UC02")]
    [Trait("Flow", "Alternative")]
    public void W2_UC02_ThesisDetails_RecognisesMissingThesisInfo_ForStudentWithoutThesis()
    {
        // Arrange
        _output.WriteLine("[GIVEN] A student 'S100002' with NO recorded thesis information");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Requesting Thesis Details for 'S100002'");
        var result = controller.ThesisDetails("S100002");

        // Assert
        _output.WriteLine("[THEN] Model should indicate HasThesisInformation is false");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);

        Assert.False(model.HasThesisInformation, "Expected HasThesisInformation to be false for student S100002.");
        Assert.Null(model.ThesisTitle);
        Assert.Null(model.ExpectedSubmissionDate);
        Assert.Null(model.ActualSubmissionDate);
        _output.WriteLine("✔ Result: Success - Missing thesis information correctly flagged.");
    }

    [Fact(DisplayName = "W2-UC02 [Alt Flow]: Displays 'Student Not Found' when thesis requested for non-existent student")]
    [Trait("UseCase", "W2-UC02")]
    [Trait("Flow", "Alternative")]
    public void W2_UC02_ThesisDetails_ReturnsStudentNotFound_WhenStudentDoesNotExist()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Non-existent student ID 'NONEXISTENT'");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Requesting Thesis Details for 'NONEXISTENT'");
        var result = controller.ThesisDetails("NONEXISTENT");

        // Assert
        _output.WriteLine("[THEN] Returns 'StudentNotFound' view");
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("StudentNotFound", viewResult.ViewName);
        _output.WriteLine("✔ Result: Handled missing student.");
    }

    [Fact(DisplayName = "W2-UC02 [View-Only]: Thesis details request does not alter student thesis milestone dates")]
    [Trait("UseCase", "W2-UC02")]
    [Trait("Invariant", "Idempotence")]
    public void W2_UC02_ThesisDetails_DoesNotModifyStudentData()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Student 'S100001' with expected submission date");
        var controller = CreateControllerWithTempData();
        var initialStudent = StudentsController.Students.First(s => s.StudentNumber == "S100001");
        var initialExpectedDate = initialStudent.ExpectedSubmissionDate;

        // Act
        _output.WriteLine("[WHEN] Viewing Thesis Details");
        var result = controller.ThesisDetails("S100001");

        // Assert
        _output.WriteLine("[THEN] Expected date remains identical");
        Assert.Equal(initialExpectedDate, initialStudent.ExpectedSubmissionDate);
        _output.WriteLine("✔ Result: View-only operation verified.");
    }

    #endregion

    #region W2-UC03: Update Student Status

    [Fact(DisplayName = "W2-UC03 [GET]: EditStatus returns model for selected student")]
    [Trait("UseCase", "W2-UC03")]
    [Trait("Flow", "MainSuccess")]
    public void W2_UC03_EditStatus_Get_ReturnsStudent_WhenFound()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Existing student 'S100001'");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Fetching EditStatus form (GET)");
        var result = controller.EditStatus("S100001");

        // Assert
        _output.WriteLine("[THEN] Form view is returned with student model");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);
        Assert.Equal("S100001", model.StudentNumber);
        _output.WriteLine($"✔ Result: Form ready with Current Status: {model.Status}");
    }

    [Fact(DisplayName = "W2-UC03 [POST Success]: Updates student status and redirects to enrolment details")]
    [Trait("UseCase", "W2-UC03")]
    [Trait("Flow", "MainSuccess")]
    public void W2_UC03_EditStatus_Post_UpdatesStatusAndRedirects_WhenValid()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Student 'S100001' in 'Researching' status");
        var controller = CreateControllerWithTempData();
        var student = StudentsController.Students.First(s => s.StudentNumber == "S100001");
        var newStatus = Status.WritingUp;

        // Act
        _output.WriteLine($"[WHEN] Submitting new status: '{newStatus}'");
        var result = controller.EditStatus("S100001", newStatus);

        // Assert
        _output.WriteLine("[THEN] Redirects to EnrolmentDetails and status property in collection is updated");
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StudentsController.EnrolmentDetails), redirectResult.ActionName);
        Assert.Equal(newStatus, student.Status);
        _output.WriteLine($"✔ Result: Status successfully changed to '{student.Status}'.");
    }

    [Fact(DisplayName = "W2-UC03 [POST Alt Flow]: EditStatus returns 'Student Not Found' for unknown student ID")]
    [Trait("UseCase", "W2-UC03")]
    [Trait("Flow", "Alternative")]
    public void W2_UC03_EditStatus_Post_ReturnsStudentNotFound_WhenStudentDoesNotExist()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Non-existent student ID 'NONEXISTENT'");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Submitting status update for non-existent student");
        var result = controller.EditStatus("NONEXISTENT", Status.Completed);

        // Assert
        _output.WriteLine("[THEN] Returns 'StudentNotFound' view");
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("StudentNotFound", viewResult.ViewName);
        _output.WriteLine("✔ Result: Alternative flow confirmed.");
    }

    #endregion

    #region W2-UC04: Update Expected Thesis Submission Date

    [Fact(DisplayName = "W2-UC04 [GET]: EditExpectedSubmissionDate returns model for selected student")]
    [Trait("UseCase", "W2-UC04")]
    [Trait("Flow", "MainSuccess")]
    public void W2_UC04_EditExpectedSubmissionDate_Get_ReturnsStudent_WhenFound()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Existing student 'S100001'");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Loading EditExpectedSubmissionDate form (GET)");
        var result = controller.EditExpectedSubmissionDate("S100001");

        // Assert
        _output.WriteLine("[THEN] Returns view with student model");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);
        Assert.Equal("S100001", model.StudentNumber);
        _output.WriteLine($"✔ Result: Existing expected date: {model.ExpectedSubmissionDate:yyyy-MM-dd}");
    }

    [Fact(DisplayName = "W2-UC04 [POST Success]: Updates expected submission date and redirects to ThesisDetails")]
    [Trait("UseCase", "W2-UC04")]
    [Trait("Flow", "MainSuccess")]
    public void W2_UC04_EditExpectedSubmissionDate_Post_UpdatesDateAndRedirects_WhenValid()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Student 'S100001'");
        var controller = CreateControllerWithTempData();
        var student = StudentsController.Students.First(s => s.StudentNumber == "S100001");
        var newDate = new DateTime(2028, 12, 15);

        // Act
        _output.WriteLine($"[WHEN] Submitting new expected submission date: {newDate:yyyy-MM-dd}");
        var result = controller.EditExpectedSubmissionDate("S100001", newDate);

        // Assert
        _output.WriteLine("[THEN] Redirects to ThesisDetails and in-memory date is updated");
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StudentsController.ThesisDetails), redirectResult.ActionName);
        Assert.Equal(newDate, student.ExpectedSubmissionDate);
        _output.WriteLine($"✔ Result: Expected submission date updated to {student.ExpectedSubmissionDate:yyyy-MM-dd}");
    }

    [Fact(DisplayName = "W2-UC04 [POST Alt Flow A1]: Invalid null date rejected without altering existing date")]
    [Trait("UseCase", "W2-UC04")]
    [Trait("Flow", "Alternative")]
    public void W2_UC04_EditExpectedSubmissionDate_Post_DoesNotUpdate_WhenDateIsNull()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Student 'S100001' with existing expected date");
        var controller = CreateControllerWithTempData();
        var student = StudentsController.Students.First(s => s.StudentNumber == "S100001");
        var previousDate = student.ExpectedSubmissionDate;

        // Act
        _output.WriteLine("[WHEN] Submitting null date for expected submission date");
        var result = controller.EditExpectedSubmissionDate("S100001", null);

        // Assert
        _output.WriteLine("[THEN] ModelState has error and previous date remains intact");
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid, "ModelState should be invalid when null date is provided.");
        Assert.Equal(previousDate, student.ExpectedSubmissionDate);
        _output.WriteLine("✔ Result: Unsuccessful update left existing date unchanged.");
    }

    [Fact(DisplayName = "W2-UC04 [POST Alt Flow]: Returns 'Student Not Found' when student ID does not exist")]
    [Trait("UseCase", "W2-UC04")]
    [Trait("Flow", "Alternative")]
    public void W2_UC04_EditExpectedSubmissionDate_Post_ReturnsStudentNotFound_WhenStudentDoesNotExist()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Non-existent student ID 'NONEXISTENT'");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Submitting expected submission date update for non-existent student");
        var result = controller.EditExpectedSubmissionDate("NONEXISTENT", DateTime.Today);

        // Assert
        _output.WriteLine("[THEN] Returns 'StudentNotFound' view");
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("StudentNotFound", viewResult.ViewName);
        _output.WriteLine("✔ Result: Alternative flow confirmed.");
    }

    #endregion

    #region W2-UC05: Record Actual Thesis Submission

    [Fact(DisplayName = "W2-UC05 [GET]: RecordSubmission returns model for selected student")]
    [Trait("UseCase", "W2-UC05")]
    [Trait("Flow", "MainSuccess")]
    public void W2_UC05_RecordSubmission_Get_ReturnsStudent_WhenFound()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Existing student 'S100001'");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Loading RecordSubmission form (GET)");
        var result = controller.RecordSubmission("S100001");

        // Assert
        _output.WriteLine("[THEN] Form view is returned with student model");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);
        Assert.Equal("S100001", model.StudentNumber);
        _output.WriteLine($"✔ Result: Ready to record submission for {model.FullName}.");
    }

    [Fact(DisplayName = "W2-UC05 [POST Success]: Atomically updates ActualSubmissionDate AND changes status to 'Submitted'")]
    [Trait("UseCase", "W2-UC05")]
    [Trait("Flow", "MainSuccess")]
    [Trait("BusinessRule", "AtomicUpdate")]
    public void W2_UC05_RecordSubmission_Post_AtomicallyUpdatesSubmissionDateAndStatus()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Student 'S100001' currently in 'WritingUp' status with no actual submission date");
        var controller = CreateControllerWithTempData();
        var student = StudentsController.Students.First(s => s.StudentNumber == "S100001");
        student.Status = Status.WritingUp;
        student.ActualSubmissionDate = null;
        var submissionDate = new DateTime(2026, 9, 29);

        // Act
        _output.WriteLine($"[WHEN] Recording actual submission date: {submissionDate:yyyy-MM-dd}");
        var result = controller.RecordSubmission("S100001", submissionDate);

        // Assert
        _output.WriteLine("[THEN] Verify both actual submission date is recorded and status is changed to Submitted");
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StudentsController.ThesisDetails), redirectResult.ActionName);
        Assert.Equal(submissionDate, student.ActualSubmissionDate);
        Assert.Equal(Status.Submitted, student.Status);
        _output.WriteLine($"✔ Result: Atomic update verified -> ActualSubmissionDate={student.ActualSubmissionDate:yyyy-MM-dd}, Status={student.Status}");
    }

    [Fact(DisplayName = "W2-UC05 [POST Alt Flow A1]: Invalid null date does not update actual submission date NOR status")]
    [Trait("UseCase", "W2-UC05")]
    [Trait("Flow", "Alternative")]
    [Trait("BusinessRule", "NoPartialUpdate")]
    public void W2_UC05_RecordSubmission_Post_DoesNotUpdateStatusOrSubmissionDate_WhenDateIsNull()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Student 'S100001' with status 'WritingUp' and no actual submission date");
        var controller = CreateControllerWithTempData();
        var student = StudentsController.Students.First(s => s.StudentNumber == "S100001");
        student.Status = Status.WritingUp;
        student.ActualSubmissionDate = null;

        // Act
        _output.WriteLine("[WHEN] Submitting null date to RecordSubmission");
        var result = controller.RecordSubmission("S100001", null);

        // Assert
        _output.WriteLine("[THEN] Form re-rendered with error, neither date nor status was modified (No partial update)");
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid, "ModelState should be invalid when null date is submitted.");
        Assert.Null(student.ActualSubmissionDate);
        Assert.Equal(Status.WritingUp, student.Status);
        _output.WriteLine($"✔ Result: Invariant preserved -> ActualSubmissionDate is still null, Status is still '{student.Status}'.");
    }

    [Fact(DisplayName = "W2-UC05 [POST Alt Flow]: Returns 'Student Not Found' when recording submission for non-existent student")]
    [Trait("UseCase", "W2-UC05")]
    [Trait("Flow", "Alternative")]
    public void W2_UC05_RecordSubmission_Post_ReturnsStudentNotFound_WhenStudentDoesNotExist()
    {
        // Arrange
        _output.WriteLine("[GIVEN] Non-existent student ID 'NONEXISTENT'");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Submitting thesis submission for non-existent student");
        var result = controller.RecordSubmission("NONEXISTENT", DateTime.Today);

        // Assert
        _output.WriteLine("[THEN] Returns 'StudentNotFound' view");
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("StudentNotFound", viewResult.ViewName);
        _output.WriteLine("✔ Result: Alternative flow confirmed.");
    }

    #endregion
}
