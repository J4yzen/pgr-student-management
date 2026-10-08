using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Web.Models;
using PgrStudentManagement.Web.Controllers;

using namespace PgrStudentManagement.Web.Data;

public class StudentStorage
{
    private readonly List<Student> _students;

    public IReadOnlyList<Student> GetStudents()
    {
        return _students;
    }
    public Student? GetStudent(string studentNumber)
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

        return View(student);

    }
    public void AddStudent(Student student)
    {
        if (student is null)
            throw new ArgumentNullException(nameof(student));

        if (string.IsNullOrWhiteSpace(student.StudentNumber))
            throw new ArgumentException("Student number is required.", nameof(student));

        if (Students.Any(s => s.StudentNumber.Equals(student.StudentNumber, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(
                $"A student with number {student.StudentNumber} already exists.");

        _students.Add(student);
       
    }
    public void UpdateStudent(Student student)
    {
        if (student is null)
            throw new ArgumentNullException(nameof(student));

        if (string.IsNullOrWhiteSpace(student.StudentNumber))
            throw new ArgumentException("Student number is required.", nameof(student));

        ArgumentNullException.ThrowIfNull(student);
        
        var index = _students.FindIndex(s =>
            s.StudentNumber.Equals(student.StudentNumber.Trim(), StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
        {
            _students[index] = student;
        }
    }
}