#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using NipponQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace NipponQuest.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Bound separately so it doesn't clutter InputModel validation summary.
        /// The checkbox posts "on" when ticked; absence means false.
        /// </summary>
        [BindProperty]
        public bool TermsAccepted { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please choose a username.")]
            [StringLength(12, MinimumLength = 4, ErrorMessage = "GamerTag must be between 4 and 12 characters.")]
            [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "GamerTag can only contain letters, numbers, underscores, and hyphens.")]
            [Display(Name = "GamerTag")]
            public string GamerTag { get; set; }

            [Required(ErrorMessage = "Please enter an email address.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Please enter a password.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // ── Server-side Terms & Privacy enforcement ──────────────────────
            // The checkbox posts "on" or "true" when ticked; an absent value
            // means the user bypassed client-side validation (e.g. via cURL).
            if (!TermsAccepted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You must accept the Terms of Service and Privacy Policy to create an account.");
                return Page();
            }

            if (ModelState.IsValid)
            {
                // ── Duplicate GamerTag check ─────────────────────────────────
                var gamerTagTaken = await _userManager.Users
                    .AnyAsync(u => u.GamerTag.ToLower() == Input.GamerTag.ToLower());

                if (gamerTagTaken)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "That GamerTag is already taken. Please choose a different legend name.");
                    return Page();
                }

                var user = CreateUser();

                user.GamerTag = Input.GamerTag.Trim();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "New account created for GamerTag '{GamerTag}' with email '{Email}'.",
                        user.GamerTag, Input.Email);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Re-render the page with validation errors
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException(
                    $"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    "Ensure that it is not abstract and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("The default UI requires a user store with email support.");
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
