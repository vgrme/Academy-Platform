﻿namespace AcademyPlatform.Web.Models.Courses
{
    using AcademyPlatform.Models.Courses;
    using AcademyPlatform.Web.Infrastructure.Mappings;

    public class CoursesListViewModel : IMapFrom<Course>
    {
        public string Title { get; set; }

        public string CourseUrl { get; set; }

        public string ImageUrl { get; set; }

        public string Category { get; set; }

        public string ShortDescription { get; set; }

    }
}