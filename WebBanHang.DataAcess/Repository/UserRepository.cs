using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebBanHang.Data;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.DataAcess.Repository
{
    public class UserRepository : Repository<User>, IUserRepsitory
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMapper _Mapper;
        public UserRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db, IConfiguration configuration, SignInManager<User> signInManager, IMapper mapper) : base(db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _configuration = configuration;
            _signInManager = signInManager;
            _Mapper = mapper;
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddDays(1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature)
            );
            return token;
        }
        public async Task<ApiReponse<string>> Login(SignIn loginModel)
        {

            var result = await _signInManager.PasswordSignInAsync(loginModel.UserName, loginModel.Password, loginModel.Remeber, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(loginModel.UserName);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim("Role",user.Role),
                    new Claim("FullName",user.FullName),
                    new Claim("Id",user.Id),

                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                };
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }
                var jwtToken = GetToken(authClaims);
                string token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                return new ApiReponse<string> { IsSuccess = true, StatusCode = 200, Message = "Logged in successfully", Response = token };
            }
            else if (result.IsLockedOut)
                return new ApiReponse<string> { IsSuccess = false, StatusCode = 500, Message = "User looked" };
            else
                return new ApiReponse<string> { IsSuccess = false, StatusCode = 401, Message = "Logged in not successful" };
        }

        public async Task<ApiReponse<string>> Register(SignUp registerUser, string owner)
        {

            var userExist = await _userManager.FindByNameAsync(registerUser.UserName);
            if (string.IsNullOrEmpty(registerUser.OwnerID))
                registerUser.OwnerID = null;

            if (userExist != null)
            {
                return new ApiReponse<string> { IsSuccess = false, StatusCode = 403, Message = "User already exists" };
            }
            registerUser.OwnerID = owner;
            if (string.Compare(registerUser.Role, "HouseHolder", StringComparison.OrdinalIgnoreCase) == 0)
                registerUser.OwnerID = null;
            //Add the user in the database
            User Iuser = new()
            {
                FullName = registerUser.FullName,
                UserName = registerUser.UserName,
                Email = registerUser.Email,
                PhoneNumber = registerUser.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                Identification = registerUser.Identification,
                IssuedBy = registerUser.IssuedBy,
                DateRange = registerUser.DateRange,
                Role = registerUser.Role,
                BOD = registerUser.bOD,
                PermanentAddress = registerUser.PermanentAddress
            };
            if (await _roleManager.RoleExistsAsync(registerUser.Role))
            {
                var result = await _userManager.CreateAsync(Iuser, registerUser.Password);
                if (!result.Succeeded)
                {

                    return new ApiReponse<string> { IsSuccess = false, StatusCode = 500, Message = "User already exists" };
                }

                await _userManager.AddToRoleAsync(Iuser, registerUser.Role);
                _db.SaveChanges();
                return new ApiReponse<string> { IsSuccess = true, StatusCode = 201, Message = "User created successfully" };
            }
            else
            {
                return new ApiReponse<string> { IsSuccess = false, StatusCode = 500, Message = "Provided role doesnot exist in the database " };
            }
        }
        public async Task<List<User>> GetUserByIDHouseholderAsync(string id)
        {
            //return _db.Users.Where(t => t.OwnerID == id).ToList();
            return null;
        }
        public async Task<ApiReponse<string>> ResertPassword(User u)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(u);
            var result = await _userManager.ResetPasswordAsync(u, token, u.PhoneNumber);
            if (!result.Succeeded)
            {
                return new ApiReponse<string> { IsSuccess = false, StatusCode = 500, Message = "Resert password no Succeeded" };
            }
            return new ApiReponse<string> { IsSuccess = true, StatusCode = 200, Message = "Resert password Succeeded" };
        }
        public void Update(User u)
        {
            _db.Users.Update(u);
        }

        public async Task<ApiReponse<string>> LookUserByIdAsync(string id)
        {
            User user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new ApiReponse<string> { IsSuccess = false, StatusCode = 404, Message = "User not found" };
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return new ApiReponse<string> { IsSuccess = false, StatusCode = 500, Message = "User is already locked out" };
            }
            user.IsLocked = true;
            _db.Users.Update(user);
            // Thực hiện khóa tài khoản bằng cách đặt thời điểm kết thúc khóa

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(1)); // Ví dụ: khóa tài khoản trong 1 năm
            await _userManager.SetLockoutEnabledAsync(user, true);

            return new ApiReponse<string> { IsSuccess = true, StatusCode = 200, Message = "User has been locked successfully" };
        }

        public async Task<ApiReponse<string>> UnLookUserByIdAsync(string id)
        {
            User user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new ApiReponse<string> { IsSuccess = false, StatusCode = 404, Message = "User not found" };
            }
            user.IsLocked = false;
            _db.Users.Update(user);
            // Mở khóa tài khoản bằng cách đặt thời điểm kết thúc khóa trong quá khứ
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);

            // Hoặc vô hiệu hóa chức năng khóa tài khoản
            // await _userManager.SetLockoutEnabledAsync(user, false);

            return new ApiReponse<string> { IsSuccess = true, StatusCode = 200, Message = "User has been unlocked successfully" };
        }
    }
}