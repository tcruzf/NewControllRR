using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using ControllRR.Application.Dto;
using ControllRR.Application.Interfaces;
using ControllRR.Domain.Entities;
using ControllRR.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace ControllRR.Presentation.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "ADMIN,Manager")]

    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly IUserService _userService;


        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IUserService userService
             )

        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            //_userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public List<SelectListItem> UserRoles { get; set; } = new List<SelectListItem>();

        public class InputModel
        {
            [Required(ErrorMessage = "Número de registro é obrigatório")]
            public int? Register { get; set; }

            [Required(ErrorMessage = "Nome é obrigatório")]
            [StringLength(100, MinimumLength = 3)]
            public string? Name { get; set; }
            public string? Phone { get; set; }

            public ICollection<Maintenance>? Maintenances { get; set; }

            [Required]
            [Display(Name = "Permissões")]
            public string? Role { get; set; }
            public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();




            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            /*if (!User.IsInRole("ADMIN")) // Substitua "Admin" pela role desejada
            {
                _logger.LogWarning($"Acesso negado para: {User.Identity.Name}");
                return RedirectToPage("/Account/AccessDenied"); // Ou redirecione para outra página
            }*/
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            UserRoles = _roleManager.Roles
                 .Select(role => new SelectListItem { Value = role.Name, Text = role.Name })
                 .ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            
            
            returnUrl ??= Url.Content("~/");
            //  Verifica se o usuário atual tem permissão
           /* if (!User.IsInRole("ADMIN"))
            {
               // _logger.LogInformation($"Usuário atual: {User.Identity.Name}");
               // _logger.LogInformation($"Roles do usuário: {string.Join(",", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

                return RedirectToPage("/Account/AccessDenied");
            }
            else
            {
                _logger.LogInformation($"Teste fora do if");
                _logger.LogInformation($"Usuário atual: {User.Identity.Name}");
                _logger.LogInformation($"Roles do usuário: {string.Join(",", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

            }*/
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                System.Console.WriteLine(user);
                user.Register = Input.Register;
                System.Console.WriteLine(user.Register);
                user.Name = Input.Name;
                System.Console.WriteLine(user.Name);
                user.Role = Input.Role;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);



                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario criado com sucesso.");
                    var role = Input.Role.ToString(); // Obter o nome da role
                    if (await _roleManager.RoleExistsAsync(role))
                    {
                        var roleAssignResult = await _userManager.AddToRoleAsync(user, role);

                        if (!roleAssignResult.Succeeded)
                        {
                            foreach (var error in roleAssignResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return Page();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Role selecionada não existe.");
                        return Page();
                    }
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
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
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
