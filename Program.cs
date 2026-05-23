using System;
using System.Collections.Generic;

namespace UniversitySystem
{
    // ─────────────────────────────────────────────
    //  BASE CLASS
    // ─────────────────────────────────────────────
    abstract class Person
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        protected string Password { get; set; }

        protected Person(int id, string name, string password)
        {
            ID = id;
            Name = name;
            Password = password;
        }

        // Shared login logic — no duplication across subclasses
        public bool Login(string inputPassword)
        {
            if (inputPassword == Password)
            {
                Console.WriteLine($"{Name} logged in successfully.");
                return true;
            }
            Console.WriteLine("Incorrect password.");
            return false;
        }

        public abstract void DisplayInfo();
        public abstract string GetRole();
    }

    // ─────────────────────────────────────────────
    //  INTERFACE
    // ─────────────────────────────────────────────
    interface ILogin
    {
        bool Login();   // returns success/failure
    }

    // ─────────────────────────────────────────────
    //  STUDENT
    // ─────────────────────────────────────────────
    class Student : Person, ILogin
    {
        public int Semester { get; set; }
        public int MaxHours { get; set; }
        public List<Course> RegisteredCourses { get; set; } = new List<Course>();

        public Student(int id, string name, string password, int semester, int maxHours = 18)
            : base(id, name, password)
        {
            Semester = semester;
            MaxHours = maxHours;
        }

        public bool Login()
        {
            Console.Write("Enter password: ");
            string input = Console.ReadLine();
            return base.Login(input);
        }

        public void RegisterCourse(Course course)
        {
            if (RegisteredCourses.Contains(course))
            {
                Console.WriteLine("You are already registered in this course.");
                return;
            }
            if (!course.HasAvailableSeat())
            {
                Console.WriteLine("Course is full.");
                return;
            }
            if (GetTotalHours() + course.CreditHours > MaxHours)
            {
                Console.WriteLine($"Cannot register: would exceed your maximum of {MaxHours} credit hours.");
                return;
            }

            course.AddStudent();
            RegisteredCourses.Add(course);
            Console.WriteLine($"Successfully registered in \"{course.CourseName}\".");
            Console.WriteLine($"Current registered hours: {GetTotalHours()} / {MaxHours}");
        }

        public void DropCourse(Course course)
        {
            if (!RegisteredCourses.Contains(course))
            {
                Console.WriteLine("You are not registered in that course.");
                return;
            }

            RegisteredCourses.Remove(course);
            course.RemoveStudent();
            Console.WriteLine($"Successfully dropped \"{course.CourseName}\".");
            Console.WriteLine($"Current registered hours: {GetTotalHours()} / {MaxHours}");
        }

        public int GetTotalHours()
        {
            int total = 0;
            foreach (Course c in RegisteredCourses)
                total += c.CreditHours;
            return total;
        }

        public override void DisplayInfo()
        {
            Console.WriteLine("\n----- Student Information -----");
            Console.WriteLine($"ID       : {ID}");
            Console.WriteLine($"Name     : {Name}");
            Console.WriteLine($"Semester : {Semester}");
            Console.WriteLine($"Hours    : {GetTotalHours()} / {MaxHours}");
        }

        public override string GetRole() => "Student";

        public override string ToString() => $"[Student] ID: {ID}, Name: {Name}";
    }

    // ─────────────────────────────────────────────
    //  PROFESSOR
    // ─────────────────────────────────────────────
    class Professor : Person, ILogin
    {
        public string Department { get; set; }

        public Professor(int id, string name, string password, string department)
            : base(id, name, password)
        {
            Department = department;
        }

        public bool Login()
        {
            Console.Write("Enter password: ");
            string input = Console.ReadLine();
            return base.Login(input);
        }

        public override void DisplayInfo()
        {
            Console.WriteLine("\n----- Professor Information -----");
            Console.WriteLine($"ID         : {ID}");
            Console.WriteLine($"Name       : {Name}");
            Console.WriteLine($"Department : {Department}");
        }

        public override string GetRole() => "Professor";

        public override string ToString() => $"[Professor] ID: {ID}, Name: {Name}";
    }

    // ─────────────────────────────────────────────
    //  ADMIN
    // ─────────────────────────────────────────────
    class Admin : Person, ILogin
    {
        public string Position { get; set; }

        public Admin(int id, string name, string password, string position)
            : base(id, name, password)
        {
            Position = position;
        }

        public bool Login()
        {
            Console.Write("Enter password: ");
            string input = Console.ReadLine();
            return base.Login(input);
        }

        public void AddCourseToSystem(UniversitySystem system)
        {
            Console.Write("Course name  : ");
            string name = Console.ReadLine();
            Console.Write("Course code  : ");
            string code = Console.ReadLine();
            Console.Write("Credit hours : ");
            if (!int.TryParse(Console.ReadLine(), out int hours) || hours <= 0)
            {
                Console.WriteLine("Invalid credit hours.");
                return;
            }
            Console.Write("Capacity     : ");
            if (!int.TryParse(Console.ReadLine(), out int cap) || cap <= 0)
            {
                Console.WriteLine("Invalid capacity.");
                return;
            }
            system.AddCourse(new Course(hours, cap, name, code));
        }

        public void RemoveCourseFromSystem(UniversitySystem system)
        {
            Console.Write("Enter course code to remove: ");
            string code = Console.ReadLine();
            system.RemoveCourse(code);
        }

        public void AddStudentToSystem(UniversitySystem system)
        {
            Console.Write("Student name    : ");
            string name = Console.ReadLine();
            Console.Write("Password        : ");
            string pass = Console.ReadLine();
            Console.Write("Semester        : ");
            if (!int.TryParse(Console.ReadLine(), out int sem))
            {
                Console.WriteLine("Invalid semester.");
                return;
            }
            Console.Write("Max credit hrs  : ");
            if (!int.TryParse(Console.ReadLine(), out int max) || max <= 0)
            {
                Console.WriteLine("Invalid max hours.");
                return;
            }
            system.AddStudent(new Student(IDGenerator.GenerateID(), name, pass, sem, max));
        }

        public override void DisplayInfo()
        {
            Console.WriteLine("\n----- Admin Information -----");
            Console.WriteLine($"ID       : {ID}");
            Console.WriteLine($"Name     : {Name}");
            Console.WriteLine($"Position : {Position}");
        }

        public override string GetRole() => "Admin";

        public override string ToString() => $"[Admin] ID: {ID}, Name: {Name}";
    }

    // ─────────────────────────────────────────────
    //  COURSE
    // ─────────────────────────────────────────────
    class Course
    {
        public int RegisteredStudents { get; private set; }
        public int CreditHours { get; private set; }
        public int Capacity { get; private set; }
        public string CourseName { get; set; }
        public string CourseCode { get; private set; }

        public Course(int creditHours, int capacity, string courseName, string courseCode)
        {
            CreditHours = creditHours;
            Capacity = capacity;
            CourseName = courseName;
            CourseCode = courseCode;
            RegisteredStudents = 0;
        }

        public void AddStudent()
        {
            if (RegisteredStudents < Capacity)
                RegisteredStudents++;
        }

        public void RemoveStudent()
        {
            if (RegisteredStudents > 0)
                RegisteredStudents--;
        }

        public bool HasAvailableSeat() => RegisteredStudents < Capacity;

        public override string ToString() =>
            $"[{CourseCode}] {CourseName} | {CreditHours} hrs | Seats: {RegisteredStudents}/{Capacity}";
    }

    // ─────────────────────────────────────────────
    //  ID GENERATOR
    // ─────────────────────────────────────────────
    static class IDGenerator
    {
        private static int _next = 999;
        public static int GenerateID() => ++_next;
    }

    // ─────────────────────────────────────────────
    //  REPORT GENERATOR
    // ─────────────────────────────────────────────
    sealed class ReportGenerator
    {
        public void GenerateStudentReport(Student student)
        {
            Console.WriteLine("\n========== Student Report ==========");
            Console.WriteLine($"Name     : {student.Name}");
            Console.WriteLine($"Semester : {student.Semester}");
            Console.WriteLine($"Hours    : {student.GetTotalHours()} / {student.MaxHours}");

            if (student.RegisteredCourses.Count == 0)
            {
                Console.WriteLine("No registered courses.");
            }
            else
            {
                Console.WriteLine("\nRegistered Courses:");
                foreach (Course c in student.RegisteredCourses)
                    Console.WriteLine("  " + c);
            }
            Console.WriteLine("====================================");
        }

        public void GenerateCourseReport(Course course)
        {
            Console.WriteLine("\n========== Course Report ==========");
            Console.WriteLine(course);
            Console.WriteLine($"Enrolled : {course.RegisteredStudents} / {course.Capacity}");
            int available = course.Capacity - course.RegisteredStudents;
            Console.WriteLine($"Available: {available} seat(s)");
            Console.WriteLine("====================================");
        }
    }

    // ─────────────────────────────────────────────
    //  UNIVERSITY SYSTEM
    // ─────────────────────────────────────────────
    class UniversitySystem
    {
        public List<Student> Students { get; } = new List<Student>();
        public List<Professor> Professors { get; } = new List<Professor>();
        public List<Course> Courses { get; } = new List<Course>();

        public void AddStudent(Student student, bool showMessage = true)
        {
            Students.Add(student);

            if (showMessage)
                Console.WriteLine($"Student \"{student.Name}\" added successfully.");
        }

        public void AddProfessor(Professor professor)
        {
            Professors.Add(professor);
            Console.WriteLine($"Professor \"{professor.Name}\" added successfully.");
        }

        public void AddCourse(Course course, bool showMessage = true)
        {
            // Prevent duplicate course codes
            if (FindCourse(course.CourseCode) != null)
            {
                Console.WriteLine($"A course with code \"{course.CourseCode}\" already exists.");
                return;
            }

            Courses.Add(course);

            if (showMessage)
                Console.WriteLine($"Course \"{course.CourseName}\" added successfully.");
        }

        public void RemoveCourse(string courseCode)
        {
            Course course = FindCourse(courseCode);
            if (course == null)
            {
                Console.WriteLine("Course not found.");
                return;
            }
            Courses.Remove(course);
            Console.WriteLine($"Course \"{course.CourseName}\" removed successfully.");
        }

        public Course FindCourse(string courseCode)
        {
            foreach (Course c in Courses)
                if (c.CourseCode.Equals(courseCode, StringComparison.OrdinalIgnoreCase))
                    return c;
            return null;
        }

        public Student FindStudent(string name)
        {
            foreach (Student s in Students)
                if (s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return s;
            return null;
        }

        public void DisplayCourses()
        {
            if (Courses.Count == 0)
            {
                Console.WriteLine("No courses available.");
                return;
            }
            Console.WriteLine("\n========== Available Courses ==========");
            foreach (Course c in Courses)
                Console.WriteLine("  " + c);
            Console.WriteLine("=======================================");
        }

        public void DisplayStudents()
        {
            if (Students.Count == 0)
            {
                Console.WriteLine("No students enrolled.");
                return;
            }
            Console.WriteLine("\n========== Enrolled Students ==========");
            foreach (Student s in Students)
                Console.WriteLine($"  {s}  |  Semester {s.Semester}  |  Hours {s.GetTotalHours()}/{s.MaxHours}");
            Console.WriteLine("=======================================");
        }
    }

    // ─────────────────────────────────────────────
    //  PROGRAM / ENTRY POINT
    // ─────────────────────────────────────────────
    class Program
    {
        static UniversitySystem system = new UniversitySystem();
        static ReportGenerator report = new ReportGenerator();

        // Tracks who is currently logged in
        static Student activeStudent = null;
        static Professor activeProfessor = null;
        static Admin activeAdmin = null;
        static void Main(string[] args)
        {
            Console.Title = "University Registration System";

            // ── Seed data ──
            system.AddCourse(new Course(3, 30, "Programming Fundamentals", "CE1011"), false);
            system.AddCourse(new Course(4, 25, "Data Structures", "CE1010"), false);
            system.AddCourse(new Course(3, 20, "Object-Oriented Programming", "ECE1013"), false);

            system.AddStudent(new Student(IDGenerator.GenerateID(), "Eslam", "1234", 2, 18), false);
            system.AddStudent(new Student(IDGenerator.GenerateID(), "Sara", "pass99", 3, 21), false);

            system.Professors.Add(new Professor(IDGenerator.GenerateID(), "Dr.Noha", "prof123", "Computer Engineering"));

            Admin admin = new Admin(IDGenerator.GenerateID(), "Amr", "admin123", "Registration Manager");


            int choice = 0;

            while (true)
            {
                PrintMainMenu();
                Console.Write("\nEnter choice: ");

                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Please enter a valid number.");
                    continue;
                }

                switch (choice)
                {
                    // ── Authentication ──
                    case 1:
                        StudentLoginMenu();
                        break;

                    case 2:
                        ProfessorLoginMenu();
                        break;

                    case 3:
                        if (admin.Login())
                        {
                            activeAdmin = admin;
                            activeStudent = null;
                            activeProfessor = null;
                        }
                        break;

                    // ── Course browsing (anyone) ──
                    case 4:
                        system.DisplayCourses();
                        break;

                    case 5:
                        SearchCourseMenu();
                        break;

                    // ── Student actions (must be logged in) ──
                    case 6:
                        RequireStudent(() => RegisterCourseMenu());
                        break;

                    case 7:
                        RequireStudent(() => DropCourseMenu());
                        break;

                    case 8:
                        RequireStudent(() => report.GenerateStudentReport(activeStudent));
                        break;

                    case 9:
                        RequireStudent(() => activeStudent.DisplayInfo());
                        break;

                    // ── Proffesor actions ──
                    case 10:
                        RequireProfessor(() => activeProfessor.DisplayInfo());
                        break;

                    case 11:
                        RequireProfessor(() => system.DisplayCourses());
                        break;

                    // ── Admin actions ──
                    case 12:
                        RequireAdmin(() => admin.AddCourseToSystem(system));
                        break;

                    case 13:
                        RequireAdmin(() => admin.RemoveCourseFromSystem(system));
                        break;

                    case 14:
                        RequireAdmin(() => admin.AddStudentToSystem(system));
                        break;

                    case 15:
                        RequireAdmin(() => system.DisplayStudents());
                        break;

                    case 16:
                        RequireAdmin(() => GenerateCourseReportMenu());
                        break;

                    case 99:
                        activeStudent = null;
                        activeProfessor = null;
                        activeAdmin = null;

                        Console.WriteLine("Logged out successfully.");
                        break;

                    case 0:
                        Console.WriteLine("Goodbye!");
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        // ── Menu helpers ──

        static void PrintMainMenu()
        {
            Console.WriteLine("║     UNIVERSITY REGISTRATION SYSTEM   ║");
            Console.WriteLine("║  --- General ---                     ║");
            Console.WriteLine("║  1. Student Login                    ║");
            Console.WriteLine("║  2. Professor Login                  ║");
            Console.WriteLine("║  3. Admin Login                      ║");
            Console.WriteLine("║  4. Display All Courses              ║");
            Console.WriteLine("║  5. Search Course by Code            ║");
            Console.WriteLine("║  --- Student (login required) ---    ║");
            Console.WriteLine("║  6. Register for a Course            ║");
            Console.WriteLine("║  7. Drop a Course                    ║");
            Console.WriteLine("║  8. My Report                        ║");
            Console.WriteLine("║  9. My Info                          ║");
            Console.WriteLine("║  --- Professor (login required) ---  ║");
            Console.WriteLine("║  10. Professor Info                  ║");
            Console.WriteLine("║  11. View Courses                    ║");
            Console.WriteLine("║  --- Admin (login required) ---      ║");
            Console.WriteLine("║  12. Add Course                      ║");
            Console.WriteLine("║  13. Remove Course                   ║");
            Console.WriteLine("║  14. Add Student                     ║");
            Console.WriteLine("║  15. List All Students               ║");
            Console.WriteLine("║  16. Course Report                   ║");
            Console.WriteLine("║  --- ---                             ║");
            Console.WriteLine("║  99. Log Out                         ║");
            Console.WriteLine("║  0. Exit                             ║");

            string sessionInfo = activeStudent != null ? $"Student: {activeStudent.Name}"
                               : activeProfessor != null ? $"Professor: {activeProfessor.Name}"
                               : activeAdmin != null ? $"Admin: {activeAdmin.Name}"
                               : "Not logged in";
            Console.WriteLine($"  Session: {sessionInfo}");
        }

        static void StudentLoginMenu()
        {
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();
            Student s = system.FindStudent(name);
            if (s == null)
            {
                Console.WriteLine("Student not found.");
                return;
            }
            if (s.Login())
            {
                activeStudent = s;
                activeProfessor = null;
                activeAdmin = null;
            }
        }

        static void ProfessorLoginMenu()
        {
            if (system.Professors.Count == 0) { Console.WriteLine("No professors in the system."); return; }
            Console.WriteLine("Professors:");
            foreach (Professor p in system.Professors)
                Console.WriteLine("  " + p);
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();
            Professor prof = null;
            foreach (Professor p in system.Professors)
                if (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) { prof = p; break; }
            if (prof == null) { Console.WriteLine("Professor not found."); return; }
            if (prof.Login())
            {
                activeProfessor = prof;
                activeStudent = null;
                activeAdmin = null;
            }
        }

        static void SearchCourseMenu()
        {
            Console.Write("Enter course code: ");
            string code = Console.ReadLine();
            Course course = system.FindCourse(code);
            if (course != null) Console.WriteLine("\n  " + course);
            else Console.WriteLine("Course not found.");
        }

        static void RegisterCourseMenu()
        {
            system.DisplayCourses();
            Console.Write("Enter course code to register: ");
            string code = Console.ReadLine();
            Course course = system.FindCourse(code);
            if (course == null) { Console.WriteLine("Course not found."); return; }
            activeStudent.RegisterCourse(course);
        }

        static void DropCourseMenu()
        {
            if (activeStudent.RegisteredCourses.Count == 0)
            {
                Console.WriteLine("You have no registered courses to drop.");
                return;
            }
            Console.WriteLine("Your registered courses:");
            foreach (Course c in activeStudent.RegisteredCourses)
                Console.WriteLine("  " + c);
            Console.Write("Enter course code to drop: ");
            string code = Console.ReadLine();
            Course course = system.FindCourse(code);
            if (course == null) { Console.WriteLine("Course not found."); return; }
            activeStudent.DropCourse(course);
        }

        static void GenerateCourseReportMenu()
        {
            system.DisplayCourses();
            Console.Write("Enter course code: ");
            string code = Console.ReadLine();
            Course course = system.FindCourse(code);
            if (course == null) { Console.WriteLine("Course not found."); return; }
            report.GenerateCourseReport(course);
        }

        // Guards — only run action if the right session is active
        static void RequireStudent(Action action)
        {
            if (activeStudent == null) { Console.WriteLine("Please log in as a student first (option 1)."); return; }
            action();
        }

        static void RequireAdmin(Action action)
        {
            if (activeAdmin == null)
            {
                Console.WriteLine("Please log in as admin first (option 3).");
                return;
            }

            action();
        }
        static void RequireProfessor(Action action)
        {
            if (activeProfessor == null)
            {
                Console.WriteLine("Please log in as professor first (option 2).");
                return;
            }

            action();
        }
    }
}
