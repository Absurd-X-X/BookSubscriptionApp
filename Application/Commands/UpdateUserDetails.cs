using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Command
{
    public class UpdatePersonalSettings
    {
        public record UpdatePersonalSettingsCommand(
            Guid UserId,
            string FullName,
            string Email,
            string Username,
            string? Bio,
            string? ImageFileName,
            Stream? ImageFileStream) : IRequest<Result<string>>;

        public class UpdatePersonalSettingsHandler(
            IUserRepository _userRepository,
            IUnitOfWork _unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository
            ) : IRequestHandler<UpdatePersonalSettingsCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdatePersonalSettingsCommand request, CancellationToken cToken)
            {
                var user = await _userRepository.GetAsync(request.UserId);

                if (user == null)
                    return Result<string>.Failure("You've not logged in");

                var getUsername = await _userRepository.GetByUserNameAsync(request.Username);

                if (getUsername != null && getUsername.Id != user.Id)
                    return Result<string>.Failure("Username is already taken");

                var getEmail = await _userRepository.GetAsync(request.Email);

                if (getEmail != null && getEmail.Id != user.Id)
                    return Result<string>.Failure("Email is already in use");

                if (string.IsNullOrWhiteSpace(request.FullName))
                    return Result<string>.Failure("Full name is required.");

                var previousImageUrl = user.ImageUrl;

                if (!string.IsNullOrWhiteSpace(request.ImageFileName) && request.ImageFileStream != null)
                {
                    var extension = Path.GetExtension(request.ImageFileName).ToLower();
                    var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                    if (!extensions.Contains(extension))
                        return Result<string>.Failure("Invalid image type. Only JPG, JPEG, PNG, and WEBP files are allowed.");

                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var unique = $"{Guid.NewGuid()}_{user.Id}{extension}";
                    var combine = Path.Combine(folder, unique);

                    using (var fileStream = new FileStream(combine, FileMode.Create))
                    {
                        await request.ImageFileStream.CopyToAsync(fileStream, cToken);
                    }

                    user.ImageUrl = $"/uploads/avatars/{unique}";

                    if (!string.IsNullOrEmpty(previousImageUrl) && previousImageUrl.StartsWith("/uploads/avatars/"))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", previousImageUrl.TrimStart('/'));
                        if (File.Exists(oldPath))
                        {
                            try { File.Delete(oldPath); } catch { /* non-fatal cleanup */ }
                        }
                    }
                }

                user.Reader?.Name = request.FullName;
                user.UserName = request.Username;

                if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = request.Email.Trim();
                    user.IsVerified = false;
                }

                 _userRepository.Update(user);

                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();

                var audit = new AuditLog
                {
                    ActionType = "Update",
                    Description = $"{user.Reader?.Name} updated their personal settings",
                    Icon = "👤",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = user.Id,
                };

                await auditLogRepository.AddAsync(audit);

                await _unitOfWork.SaveAsync();

                return Result<string>.Success(user.ImageUrl ?? string.Empty, "Changes saved successfully");
            }
        }
    }
}