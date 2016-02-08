﻿namespace AcademyPlatform.Web.Umbraco.Startup
{
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Mvc;

    using AcademyPlatform.Common.Providers;
    using AcademyPlatform.Data.Repositories;
    using AcademyPlatform.Services.Contracts;
    using AcademyPlatform.Web.Infrastructure.Mappings;
    using AcademyPlatform.Web.Infrastructure.Sanitizers;
    using AcademyPlatform.Web.Models.Courses;
    using AcademyPlatform.Web.Models.Properties;
    using AcademyPlatform.Web.Umbraco.Services.Contracts;

    using Autofac;
    using Autofac.Integration.Mvc;
    using Autofac.Integration.WebApi;

    using FluentValidation;

    using global::Umbraco.Core;
    using global::Umbraco.Web;

    using Zone.UmbracoMapper;

    public class AutofacConfig : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            var builder = new ContainerBuilder();

            builder.Register(c => UmbracoContext.Current).AsSelf();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterControllers(typeof(UmbracoApplication).Assembly);
            builder.RegisterApiControllers(typeof(UmbracoApplication).Assembly);
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterFluentValidators(typeof(AcademyPlatform.Web.Validators.AssemblyInfo).Assembly);

            builder.RegisterAssemblyTypes(typeof(ICoursesService).Assembly).AsImplementedInterfaces().InstancePerRequest();
            builder.RegisterAssemblyTypes(typeof(ICoursesContentService).Assembly).AsImplementedInterfaces().InstancePerRequest();
            builder.RegisterAssemblyTypes(typeof(IRepository<>).Assembly).AsImplementedInterfaces().InstancePerRequest();
            builder.RegisterAssemblyTypes(typeof(IHtmlSanitizer).Assembly).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(IRandomProvider).Assembly).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(IUmbracoMapper).Assembly).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerRequest();
            builder.RegisterType(typeof(UmbracoMapper)).As(typeof(IUmbracoMapper)).InstancePerRequest();

            var container = builder.Build();
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            //TODO MOVE
            var autoMapperConfig = new AutoMapperConfig(Assembly.GetAssembly(typeof(CourseEditViewModel)));
            autoMapperConfig.Execute();
        }


    }

    public static class BuilderExtensions
    {
        public static void RegisterFluentValidators(this ContainerBuilder builder, Assembly assembly)
        {
            AssemblyScanner.FindValidatorsInAssembly(assembly).ForEach(
                result =>
                {
                    builder.RegisterType(result.ValidatorType).As(result.InterfaceType).InstancePerRequest();
                });
        }
    }
}