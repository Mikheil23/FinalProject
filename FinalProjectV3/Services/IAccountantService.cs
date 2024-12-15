using FinalProjectV3.DTOS;
using static FinalProjectV3.Models.Enums;

namespace FinalProjectV3.Services
{
    public interface IAccountantService
    {
        Task<List<RegistrationResponse>> ViewAllUsersAsync();
        Task<List<LoanRequestResponse>> GetAllLoanRequestsAsync();
        Task<bool> BlockOrUnblockUserAsync(int userId, bool isBlocked);
        Task<bool> ChangeLoanStatusAsync(int userId, int loanId, LoanStatus newStatus);
        Task<bool> DeleteLoanAsync(int loanId);
    }
}
