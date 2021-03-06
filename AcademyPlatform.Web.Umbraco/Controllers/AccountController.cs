﻿namespace AcademyPlatform.Web.Umbraco.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;

    using AcademyPlatform.Models;
    using AcademyPlatform.Models.Account;
    using AcademyPlatform.Models.Exceptions;
    using AcademyPlatform.Services.Contracts;
    using AcademyPlatform.Web.Infrastructure.Extensions;
    using AcademyPlatform.Web.Infrastructure.Filters;
    using AcademyPlatform.Web.Models.Account;
    using AcademyPlatform.Web.Models.Umbraco.DocumentTypes;
    using AcademyPlatform.Web.Umbraco.ViewModels;

    using global::Umbraco.Core.Models;
    using global::Umbraco.Web;
    using global::Umbraco.Web.Mvc;

    using Recaptcha.Web;
    using Recaptcha.Web.Mvc;

    using Zone.UmbracoMapper;

    using UmbracoContextExtensions = AcademyPlatform.Web.Umbraco.UmbracoConfiguration.Extensions.UmbracoContextExtensions;

    [RequireHttps]
    [EnsurePublishedContentRequest(2055)]//TODO create an attribute that specifies a node name, rather than node Id
    public class AccountController : UmbracoController
    {
        private readonly IMembershipService _membership;
        private readonly IUserService _user;
        private readonly IEmailService _email;
        private readonly IUmbracoMapper _mapper;

        private IPublishedContent _accountSection;

        public IPublishedContent AccountSection
        {
            get
            {
                if (_accountSection == null)
                {
                    _accountSection =
                        Umbraco.TypedContentAtRoot()
                               .DescendantsOrSelf(nameof(Models.Umbraco.DocumentTypes.AccountSection))
                               .SingleOrDefault();
                }

                return _accountSection;
            }
        }

        // Umbraco ignores routes that end with something that looks like a file extension.
        // Such as the '.com' domain extensions of emails used in the validate page
        // That's why we need to manually force the creation of UmbracoContext
        public AccountController(IMembershipService membership, IEmailService email, IUmbracoMapper mapper, IUserService user) : base(UmbracoContextExtensions.GetOrCreateContext())
        {
            _membership = membership;
            _email = email;
            _mapper = mapper;
            _user = user;
        }

        [HttpGet]
        [RequireAnonymous]
        public ActionResult Register()
        {
            RegisterViewModel viewModel = new RegisterViewModel();
            IPublishedContent legalPage = Umbraco.TypedContentAtRoot().DescendantsOrSelf(nameof(LegalContent)).FirstOrDefault();
            int licenseAgreement = legalPage.GetPropertyValue<int>(nameof(LegalContent.LicenseTerms));
            if (licenseAgreement == default(int))
            {
                throw new InvalidOperationException("License terms page is not configured. Please select a license terms page");
            }

            IPublishedContent licenseAgreementPage = Umbraco.TypedContent(licenseAgreement);
            viewModel.LicenseTermsUrl = licenseAgreementPage.Url;

            return View(viewModel);
        }

        [HttpPost]
        [RequireAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel registerViewModel)
        {
            var validator = this.GetRecaptchaVerificationHelper().VerifyRecaptchaResponse();
            if (validator != RecaptchaVerificationResult.Success)
            {
                ModelState.AddModelError("ReCaptcha", "Моля, попълнете предизвикателството за да сме сигурни че не сте робот");
            }

            if (ModelState.IsValid)
            {
                AccountCreationStatus status = _membership.CreateUser(registerViewModel.Email, registerViewModel.Password, registerViewModel.FirstName, registerViewModel.LastName);

                if (status == AccountCreationStatus.Success)
                {
                    int thankYouPageId =
                        AccountSection.GetPropertyValue<int>(
                            nameof(Models.Umbraco.DocumentTypes.AccountSection.RegistrationThankYouPage));
                    return new RedirectToUmbracoPageResult(thankYouPageId);

                }
                if (status == AccountCreationStatus.DuplicateEmail)
                {
                    ModelState.AddModelError(nameof(registerViewModel.Email), "Имейлът вече се използва");
                }
            }

            return View(registerViewModel);
        }


        [HttpGet]
        [RequireAnonymous]
        public ActionResult Validate(string email, string validationCode = null)
        {
            if (_membership.IsApproved(email))
            {
                return Redirect("/");
            }

            ValidateAccountViewModel model = new ValidateAccountViewModel { Email = email, ValidationCode = validationCode };
            if (!string.IsNullOrWhiteSpace(model.Email) && !string.IsNullOrWhiteSpace(model.ValidationCode))
            {
                return Validate(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAnonymous]
        public ActionResult Validate(ValidateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (_membership.IsApproved(model.Email))
                    {
                        return Redirect("/");
                    }

                    if (_membership.ApproveUser(model.Email, model.ValidationCode))
                    {
                        return Redirect("/");
                    }

                    ModelState.AddModelError(string.Empty, "Невалиден валидационен код");

                }
                catch (UserNotFoundException)
                {
                    ModelState.AddModelError(string.Empty, "Потребител с това име не съществува в системата");
                }
            }

            return View(model);
        }

        [HttpGet]
        [RequireAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAnonymous]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            User user = _user.GetByUsername(model.Email);
            if (user != null)
            {
                _membership.ResetPassword(user.Username);
                return Redirect("/Login"); //TODO add routeName or umbraco content finder
            }

            ModelState.AddModelError(nameof(ForgotPasswordViewModel.Email), "Не успяхме да намерим потребител с този Email");
            return View(model);
        }

        [HttpGet]
        [RequireAnonymous]
        public ActionResult ResendValidationEmail(string email = null)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                return ResendValidationEmail(new ResendValidationEmailViewModel { Email = email });
            }

            return View(new ResendValidationEmailViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAnonymous]
        public ActionResult ResendValidationEmail(ResendValidationEmailViewModel model)
        {
            User user = _user.GetByUsername(model.Email);
            if (user != null)
            {
                if (user.IsApproved)
                {
                    return Redirect("/");
                }

                _membership.ResendValidationEmail(model.Email);
                return RedirectToAction(nameof(Validate), nameof(AccountController).StripControllerSufix(), new { model.Email });
            }

            ModelState.AddModelError(nameof(ResendValidationEmailViewModel.Email), "Не успяхме да намерим потребител с този Email");
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool changeSuccessful = _membership.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                if (changeSuccessful)
                {
                    return Redirect("/");
                }

                ModelState.AddModelError(nameof(ChangePasswordViewModel.OldPassword), "Грешна парола");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult LogOut()
        {
            _membership.LogOut();
            return Redirect("/");
        }
    }
}