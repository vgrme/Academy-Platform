﻿namespace AcademyPlatform.Web.Models.Courses
{
    using AcademyPlatform.Web.Models.Common;

    public class CourseContentViewModel
    {
        public int CourseId { get; set; }

        public ImageViewModel CoursePicture { get; set; }

        public string CourseUrl { get; set; }

        public string ShortDescription { get; set; }

        public string DetailedDescription { get; set; }
    }
}
