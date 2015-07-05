﻿namespace AcademyPlatform.Web.Areas.Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using AcademyPlatform.Models.Courses;
    using AcademyPlatform.Services;
    using AcademyPlatform.Web.Infrastructure.Sanitizers;
    using AcademyPlatform.Web.Models.Courses;
    using AutoMapper;
    using AcademyPlatform.Web.Infrastructure.Filters;
    using System.Web;
    using System.IO;
    using AcademyPlatform.Web.Infrastructure.Helpers;

    [CustomAuthorize(Roles = "admin")]
    public class CoursesController : Controller
    {
        private const string ImagesFolderFormat = "~\\Images\\Courses\\{0}";
        private readonly ICoursesService _coursesService;
        private readonly IHtmlSanitizer _sanitizer;

        public CoursesController(ICoursesService coursesService, IHtmlSanitizer sanitizer)
        {
            _sanitizer = sanitizer;
            _coursesService = coursesService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var courses = _coursesService.GetActiveCourses().ToList();
            return View(courses);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CourseViewModel courseViewModel)
        {
            if (ModelState.IsValid)
            {
                var imagePath = FileUploadHelper.UploadImage(courseViewModel.CourseImage, string.Format(ImagesFolderFormat, courseViewModel.Id));
                var course = Mapper.Map<Course>(courseViewModel);
                course.ImageUrl = imagePath;
                _coursesService.CreateCourse(course);

                return RedirectToAction("Index");
            }

            return View(courseViewModel);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var courseInDb = _coursesService.GetCourseById(id);
            if (courseInDb == null)
            {
                return HttpNotFound();
            }

            var course = Mapper.Map<CourseViewModel>(courseInDb);
            return View(course);
        }

        [HttpPost]
        public ActionResult Edit(int id, CourseViewModel courseViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(courseViewModel);
            }
            var courseInDb = _coursesService.GetCourseById(id);

            try
            {
                courseInDb.ImageUrl = FileUploadHelper.UploadImage(courseViewModel.CourseImage, string.Format(ImagesFolderFormat, courseViewModel.Id));
            }
            catch (Exception)
            {
                //TODO catch more specific exceptions and add more detailed messages
                ModelState.AddModelError(string.Empty, @"Файлът не може да бъде качен!");
                return View(courseViewModel);
            }
            courseViewModel.ShortDescription = _sanitizer.Sanitize(courseViewModel.ShortDescription);
            courseViewModel.DetailedDescription = _sanitizer.Sanitize(courseViewModel.DetailedDescription);

            var course = Mapper.Map(courseViewModel, courseInDb);
            _coursesService.UpdateCourse(course);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var course = _coursesService.GetCourseById(id);
            if (course == null)
            {
                return HttpNotFound("Invalid course id");
            }

            _coursesService.DeleteCourse(course);
            return RedirectToAction("Index");
        }
    }
}