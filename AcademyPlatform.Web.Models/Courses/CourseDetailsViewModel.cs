﻿namespace AcademyPlatform.Web.Models.Courses
{
    using System.Collections.Generic;

    using AcademyPlatform.Models.Courses;

    public class CourseDetailsViewModel
    {
        public CourseDetailsViewModel()
        {
            Modules = new List<ModuleViewModel>();
        }

        public int CourseId { get; set; }

        public string Title { get; set; }

        public string CoursesPageUrl { get; set; }

        public string JoinCourseUrl { get; set; }

        public string ImageUrl { get; set; }

        public Category Category { get; set; }

        public string LecturerName { get; set; }

        public string DetailedDescription { get; set; }

        public ICollection<ModuleViewModel> Modules { get; set; }

        public string ShortDescription { get; set; }
    }
}
