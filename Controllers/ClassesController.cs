using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementApp.MVC.Data;
using SchoolManagementApp.MVC.Models;

namespace SchoolManagementApp.MVC.Controllers
{
    [Authorize]
    public class ClassesController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly INotyfService _notyfService;

        public ClassesController(SchoolManagementDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            var schoolManagementDbContext = _context.Classes.Include(q => q.Course).Include(q => q.Lecturer);
            return View(await schoolManagementDbContext.ToListAsync());
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            var courses = _context.Courses.Select(q => new
            {
                Name = $"{q.Code} - {q.Name} ({q.Credits} Credits)",
                Id = q.Id
            });
            ViewData["CourseId"] = new SelectList(courses, "Id", "Name");
            var lecturers = _context.Lecturers.Select(q => new
            {
                FullName = $"{q.FirstName} {q.LastName}",
                Id = q.Id
            });
            ViewData["LecturerId"] = new SelectList(lecturers, "Id", "FullName");
            return View();
        }

        // POST: Classes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LecturerId,CourseId,Time")] Class @class)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id", @class.CourseId);
            ViewData["LecturerId"] = new SelectList(_context.Lecturers, "Id", "Id", @class.LecturerId);
            return View(@class);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // if (id == null)
            // {
            //     return NotFound();
            // }

            // var @class = await _context.Classes.FindAsync(id);
            // if (@class == null)
            // {
            //     return NotFound();
            // }
            // ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id", @class.CourseId);
            // ViewData["LecturerId"] = new SelectList(_context.Lecturers, "Id", "Id", @class.LecturerId);
            // return View(@class);

            var courses = _context.Courses.Select(q => new
            {
                Name = $"{q.Code} - {q.Name} ({q.Credits} Credits)",
                Id = q.Id
            });
            ViewData["CourseId"] = new SelectList(courses, "Id", "Name");
            var lecturers = _context.Lecturers.Select(q => new
            {
                FullName = $"{q.FirstName} {q.LastName}",
                Id = q.Id
            });
            ViewData["LecturerId"] = new SelectList(lecturers, "Id", "FullName");
            return View();
        }

        // POST: Classes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LecturerId,CourseId,Time")] Class @class)
        {
            if (id != @class.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id", @class.CourseId);
            ViewData["LecturerId"] = new SelectList(_context.Lecturers, "Id", "Id", @class.LecturerId);
            return View(@class);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                _context.Classes.Remove(@class);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ManageEnrollments(int classId)
        {
            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Lecturer)
                .Include(q => q.Enrollments)
                .ThenInclude(q => q.Student)
                .FirstOrDefaultAsync(m => m.Id == classId);
            if (@class == null)
            {
                return NotFound();
            }

            var students = await _context.Students.ToListAsync();
            var model = new ClassEnrollmentViewModel();
            model.Class = new ClassViewModel
            {
                Id = @class.Id,
                CourseName = $"{@class.Course.Code} - {@class.Course.Name} ({@class.Course.Credits} Credits)",
                LecturerName = $"{@class.Lecturer.FirstName} {@class.Lecturer.LastName}",
                Time = @class.Time.ToString()
            };

            foreach(var stu in students)
            {
                model.Students.Add(new StudentEnrollmentViewModel
                {
                    Id = stu.Id,
                    FirstName = stu.FirstName,
                    LastName = stu.LastName,
                    IsEnrolled = (@class?.Enrollments?.Any(q => q.StudentId == stu.Id)).GetValueOrDefault()
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnrollStudent(int classId, int studentId, bool shouldEnroll)
        {
            var enrollment = new Enrollment();
            if (shouldEnroll)
            {
                enrollment.ClassId = classId;
                enrollment.StudentId = studentId;
                await _context.AddAsync(enrollment);
                _notyfService.Success("Student enrolled successfully");
            }
            else
            {
                enrollment = await _context.Enrollments.FirstOrDefaultAsync(q => q.ClassId == classId && q.StudentId == studentId);
                if (enrollment != null)
                {
                    _context.Remove(enrollment);
                    _notyfService.Warning("Student unenrolled successfully");
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageEnrollments),
            new { classId = classId });
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }
    }
}
