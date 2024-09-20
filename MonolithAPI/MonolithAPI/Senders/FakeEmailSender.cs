using Microsoft.AspNetCore.Identity;
using MonolithAPI.Models;

namespace MonolithAPI.Senders;

public class FakeEmailSender : IEmailSender<UserModel>
{
    public Task SendConfirmationLinkAsync(UserModel user, string email, string confirmationLink)
    {
        Console.WriteLine("Send confirmation link to {0} with {1}", email, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(UserModel user, string email, string resetCode)
    {
        Console.WriteLine("Send password reset code to {0} with {1}", email, resetCode);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(UserModel user, string email, string resetLink)
    {
        Console.WriteLine("Send password reset link to {0} with {1}", email, resetLink);
        return Task.CompletedTask;
    }
}
