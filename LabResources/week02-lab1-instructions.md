# Week 2 Lab1: Introductory Student Management using a Basic MVC

## Lab Purpose

In this lab, you will implement five Student Management use cases in a small ASP.NET Core MVC application.

The starting project deliberately leaves important modelling and implementation decisions incomplete. Review the supplied domain model and use-case descriptions before changing the code.

## Files Provided

- `week02-lab1-instructions.md`: The tasks and constraints for the lab.
- `week02-domain-model-student-lab.md`: A candidate Student domain model containing more fields than you are expected to implement.
- `week02-usecases-student.md`: The five Week 2 use-case descriptions.

## Learning Outcomes

By completing this lab, you should be able to:

1. Interpret user stories and use-case descriptions.
2. Select domain-model fields that support a user goal.
3. Implement basic ASP.NET Core MVC controller actions and Razor views.
4. Distinguish view-only behaviour from update behaviour.
5. Apply simple business rules.
6. Keep use cases, the domain model, views, and implementation consistent.
7. Create test cases for specified behaviour.
8. Extend a use-case description by identifying additional alternative flows.

## Week 2 Technical Scope

Use:

- ASP.NET Core MVC
- One web project
- A `Student` model
- A `Status` enumeration
- An in-memory `List<Student>`
- Controllers and Razor views

Do not add:

- A database or Entity Framework
- A repository
- A service layer
- File storage
- Authentication or authorisation
- External integrations
- Background jobs or notifications

Changes made to the in-memory collection are lost when the application stops or restarts. This is an intentional Week 2 limitation.

## Getting Started

From the solution directory, build the project:

```bash
dotnet build
```

Run the web project:

```bash
dotnet run --project PgrStudentManagement.Web
```

Open the local URL displayed in the terminal and select **Students** from the navigation.

# Core Tasks

## Task 1: Review the Use Cases

Open `week02-usecases-student.md`.

1. Read all five use cases.
2. Identify the information each use case must display or update.
3. Note the supplied alternative flow for each use case.
4. Identify the actors, triggers, preconditions, and postconditions.
5. Note terminology that must also appear in the domain model and implementation.

Do not add new alternative flows during the core tasks. This is an extension activity later in the lab.

## Task 2: Review and Refine the Domain Model

Open `week02-domain-model-student-lab.md`.

1. Compare the candidate fields with the information required by the use cases.
2. Select the fields you will implement.
3. Identify mandatory and optional fields.
4. Decide how expected and actual thesis submission dates are represented.
5. Rename, remove, or add fields where necessary.
6. Review the `Status` enumeration against the use cases.

Document your decisions before implementing them.

## Task 3: Implement W2-UC01

Implement **View Enrolment Details**.

Your implementation must:

- Find the requested student.
- Follow the supplied **Student Not Found** alternative flow when no record matches the student number.
- Display the enrolment fields selected in your design.
- Leave the Student record unchanged.

## Task 4: Implement W2-UC02

Implement **View Thesis Details**.

Your implementation must:

- Find the requested student.
- Determine whether thesis information is available.
- Follow the supplied **Thesis Information Not Available** alternative flow when appropriate.
- Display the thesis fields selected in your design.
- Represent missing optional information consistently.
- Leave the Student record unchanged.

## Task 5: Implement W2-UC03

Implement **Update Student Status**.

Your implementation must:

- Find the selected student.
- Display the student's current status and available status values.
- Allow a new status to be selected and submitted.
- Follow the supplied **Student Not Found** alternative flow when no record matches the student number.
- Update only the selected student.
- Display the updated status.

## Task 6: Implement W2-UC04

Implement **Update Expected Thesis Submission Date**.

Your implementation must:

- Display the existing expected thesis submission date, if recorded.
- Accept a new expected thesis submission date.
- Follow the supplied **Invalid Expected Submission Date** alternative flow when the submitted value cannot be used.
- Leave the previous date unchanged when the operation does not succeed.
- Avoid changing the actual thesis submission date or student status.
- Display the updated expected thesis submission date after a successful update.

## Task 7: Implement W2-UC05

Implement **Record Actual Thesis Submission**.

Your implementation must:

- Display the student's current thesis information.
- Accept an actual thesis submission date.
- Follow the supplied **Invalid Submission Date** alternative flow when the submitted value cannot be used.
- Record the actual thesis submission date when the operation succeeds.
- Change the Student status to `Submitted` when the operation succeeds.
- Leave both values unchanged when the operation does not succeed.
- Display the updated submission information and status.

The submission date and status change form one business operation. Either both changes succeed or neither change is applied.

## Task 8: Exercise the Core Use Cases

For every use case, exercise:

1. The main success scenario.
2. The supplied alternative flow.
3. A student number that matches an existing record.
4. A student number that does not match a record, where applicable.
5. Missing optional information.
6. Whether view-only operations leave data unchanged.
7. Whether unsuccessful update operations leave existing data unchanged.
8. Whether W2-UC05 changes the submission date and status together.

## Task 9: Check Consistency

Check that the following artefacts agree:

- User stories
- Use-case descriptions
- Domain model
- `Status` enumeration
- Controller actions
- Razor views and forms
- Messages displayed by the application
- Postconditions
- Observed application behaviour

If you rename or add a model field, update every use case, view, and controller action that refers to that concept.

# Extension Tasks

## Extension Task 1: Create Test Cases

After completing the core implementation, create your own test cases for the five use cases.

No automated tests are provided in the starter project. Decide what should be tested and how each expected outcome can be demonstrated.

For each use case:

1. Identify the main success scenario to test.
2. Identify the supplied alternative flow to test.
3. Define the starting data and input values.
4. State the action to perform.
5. State the expected result.
6. State which Student fields should remain unchanged.
7. Implement the test where practical, or document it clearly if automated testing has not yet been introduced.

Your test set should demonstrate that:

- The correct student record is used.
- View-only operations do not change data.
- Successful updates change the intended fields.
- Unsuccessful updates leave existing data unchanged.
- W2-UC05 changes the actual thesis submission date and status together.

If you create an automated test project, add it to the solution and reference the web project.

Run automated tests, if created, using:

```bash
dotnet test
```

## Extension Task 2: Identify Additional Alternative Flows

After creating test cases for the supplied scenarios, extend the use-case descriptions.

For each use case:

1. Identify at least two additional situations in which the use case might follow a different path or fail to complete.
2. Give each alternative flow a meaningful name.
3. State the step in the main success scenario where the alternative begins.
4. Describe the actor's action and the observable system response.
5. State the resulting success or failure postconditions.
6. Create test cases for the new flow.
7. Update the model, view, controller action, messages, and tests if you implement the flow.

Possible areas to investigate include:

- Missing or malformed student numbers
- Unsupported status values
- Missing required information
- Conflicting dates
- Existing values that could be overwritten
- Operations that should result in no change
- Decisions made when selecting fields from the supplied domain model

The extension is complete only when the revised use-case descriptions, test cases, and implemented behaviour remain consistent.

# Completion Checklist

## Core

- [ ] The solution builds without errors.
- [ ] The application starts successfully.
- [ ] I reviewed all five use cases.
- [ ] I selected and documented the Student fields used by each use case.
- [ ] I distinguished expected and actual thesis submission dates.
- [ ] I updated the domain model to match my decisions.
- [ ] I implemented W2-UC01 to W2-UC05.
- [ ] I exercised each main success scenario.
- [ ] I exercised each supplied alternative flow.
- [ ] I used only the status values selected for my implementation.
- [ ] Unsuccessful updates leave existing data unchanged.
- [ ] W2-UC05 does not allow a partial update.
- [ ] View-only use cases do not modify data.
- [ ] My use cases, model, views, and implementation are consistent.

## Extension 1: Test Cases

- [ ] I created test cases for each main success scenario.
- [ ] I created test cases for each supplied alternative flow.
- [ ] I documented the starting data, action, and expected result.
- [ ] I implemented automated tests where practical.

## Extension 2: Additional Alternative Flows

- [ ] I identified at least two additional alternative flows for each use case.
- [ ] I documented the starting step, observable response, and postconditions for each flow.
- [ ] I created test cases for the additional flows.
- [ ] I updated the implementation for any additional flow I chose to implement.
- [ ] My extended use cases, test cases, and implementation remain consistent.

# Keeping the Project for Later Weeks

There is no submission for this Week 2 project.

Keep the complete project in your own files or source-control repository. You will extend and refactor it in later weeks as new architectural concepts and system capabilities are introduced.

Before finishing the lab:

1. Stop the running application.
2. Run `dotnet build` and confirm that the project still builds.
3. Save or commit the source files, use cases, domain-model decisions, and any tests you created.
4. Record incomplete work or known issues in the project README or repository notes.
5. Ensure that the project can be restored and opened in a later lab.

Recommended repository practice:

```bash
git status
git add .
git commit -m "Complete Week 2 student management iteration"
```

Do not commit:

- `bin` or `obj` directories
- Credentials or secrets
- Machine-specific absolute paths
- Database files or packages not required by this lab
