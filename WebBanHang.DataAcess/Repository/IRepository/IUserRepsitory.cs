using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.DataAcess.Repository.IRepository
{
    public interface IUserRepsitory:IRepository<User>
    {
        Task<ApiReponse<string>> Register(SignUp registerUser, string onwer);
        Task<ApiReponse<string>> Login(SignIn loginModel);
        Task<List<User>> GetUserByIDHouseholderAsync(string id);
        Task<ApiReponse<string>> ResertPassword(User u);
        void Update(User u);
        Task<ApiReponse<string>> LookUserByIdAsync(string id);
        Task<ApiReponse<string>> UnLookUserByIdAsync(string id);
    }
}
