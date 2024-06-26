using Microsoft.AspNetCore.Mvc;
using ApiDonAppBle.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiDonAppBle.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly DataContext _contexto;
    private readonly IConfiguration _config;

    public UsuarioController(DataContext contexto, IConfiguration config)
    {
        _contexto = contexto;
        _config = config;
    }

    //registro
    [HttpPost("Registrar")]
    public Usuario Registro(Usuario User)
    {
        try
        {
            var hashed = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: User.Password,
                    salt: System.Text.Encoding.ASCII.GetBytes(_config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8
                )
            );
            User.Password = hashed;
            _contexto.Add(User);
            return User;
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    //login

    [HttpPost("login")]
    public IActionResult Login(LoginView lv)
    {
        var Usuario = _contexto.Usuario.FirstOrDefault(u => u.Email == lv.Email);

        if (Usuario == null)
        {
            return BadRequest("Credenciales Incorrectas");
        }

        var hashed = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password: lv.Password,
                salt: System.Text.Encoding.ASCII.GetBytes(_config["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            )
        );
        if (Usuario.Password != hashed)
        {
            return BadRequest("Credenciales incorrectas");
        }

        var token = GenerarToken(Usuario.Email);
        return Ok(token);
    }

    //miperfil

    //editarPerfil
    //token
    public string GenerarToken(string username)
    {
        var Usuario = _contexto.Usuario.FirstOrDefault(u => u.Email == username);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, Usuario.Email),
            new Claim("FullName", Usuario.Nombre + " " + Usuario.Apellido)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30000),
            signingCredentials: credenciales
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
