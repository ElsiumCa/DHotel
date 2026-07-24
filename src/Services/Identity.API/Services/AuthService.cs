using System.Security.Claims;
using Identity.API.DTOs;
using Identity.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;

    
    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDTO { IsSuccess = false, Message = "Bu e-posta adresi zaten kullanılıyor." };
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber,
            CreatedDate = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponseDTO { IsSuccess = false, Message = $"Kullanıcı oluşturulamadı: {errors}" };
        }

        // Rol Belirleme (Varsayılan: Receptionist)
        var roleName = string.IsNullOrWhiteSpace(registerDto.Role) ? "Receptionist" : registerDto.Role;
        
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        await _userManager.AddToRoleAsync(user, roleName);

        var roles = await _userManager.GetRolesAsync(user);
        var token = await _tokenService.GenerateTokenAsync(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(30);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDTO
        {
            IsSuccess = true,
            Message = "Kullanıcı başarıyla oluşturuldu.",
            Token = token,
            RefreshToken = refreshToken,
            Roles = roles.ToList()
        };
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return new AuthResponseDTO { IsSuccess = false, Message = "Geçersiz e-posta veya şifre." };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = await _tokenService.GenerateTokenAsync(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(30);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDTO
        {
            IsSuccess = true,
            Message = "Giriş başarılı.",
            Token = token,
            RefreshToken = refreshToken,
            Roles = roles.ToList()
        };
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO request)
    {
        ClaimsPrincipal principal;
        try
        {
            principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        }
        catch
        {
            return new AuthResponseDTO { IsSuccess = false, Message = "Geçersiz token." };
        }
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return new AuthResponseDTO { IsSuccess = false, Message = "Token içeriğinde ID bulunamadı." };

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResponseDTO { IsSuccess = false, Message = "Geçersiz veya süresi dolmuş refresh token." };
        }
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = await _tokenService.GenerateTokenAsync(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(30);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDTO
        {
            IsSuccess = true,
            Message = "Token yenilendi.",
            Token = newAccessToken,
            Expiration = DateTime.UtcNow.AddMinutes(30),
            RefreshToken = newRefreshToken,
            Roles = roles.ToList()
        };
    }
}
