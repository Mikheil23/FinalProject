using FinalProjectV3.DTOS;
using System.Security.Claims;

namespace FinalProjectV3.Services
{
    public interface IUserService
    {
        Task<LoanRequestResponse> AddLoanRequestAsync(LoanRequestDto loanRequestDto, int userId, string role, string loggedInUserId);
        Task<RegistrationResponse> ViewUserCabinetByUserIdAsync(int userId, string loggedInUserId);
        Task<IEnumerable<LoanRequestResponse>> ViewAllLoansAsync(int userId, string loggedInUserId);
        Task<LoanRequestResponse> UpdateLoanAsync(int loanId, LoanRequestDto loanRequestDto, int userId, string role, string loggedInUserId);
        Task<bool> DeleteLoanAsync(int userId, int loanId, string role, string loggedInUserId);
    }
}
