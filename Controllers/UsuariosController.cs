using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using APIRestJWT.Data;
using APIRestJWT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace APIRestJWT.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _database;

        public UsuariosController(AppDbContext database) {
            _database = database;
        }

        [HttpPost("registro")]
        public IActionResult Registro([FromBody] Usuario usuario) {
            _database.Add(usuario);
            _database.SaveChanges();

            return Ok(new { msg = "Usuario cadastrado com sucesso"});
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Usuario credenciais) {
            try {
                Usuario usuario = _database.Usuarios.First(p => p.Email == credenciais.Email);
                
                if (usuario != null) {
                    if (usuario.Senha.Equals(credenciais.Senha)) {
                        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("mysymmetrickeyjwtmorse2020"));
                        var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256Signature);
                        
                        var claims = new List<Claim>();
                        claims.Add(new Claim("usuarioid", usuario.Id.ToString()));
                        claims.Add(new Claim("usuarioemail", usuario.Email.ToString()));
                        claims.Add(new Claim(ClaimTypes.Role, "admin"));
                        
                        var JWT = new JwtSecurityToken(
                            issuer: "morse",
                            expires: DateTime.Now.AddHours(1),
                            audience: "usuario_comum",
                            signingCredentials: credentials,
                            claims: claims
                        );

                        return Ok(new JwtSecurityTokenHandler().WriteToken(JWT));
                    }
                    else {
                        Response.StatusCode = 401;
                        return new ObjectResult("");    
                    }
                }
                else {
                    Response.StatusCode = 401;
                    return new ObjectResult("");
                }
            }
            catch {
                Response.StatusCode = 401;
                return new ObjectResult("");
            }
        }
    }
}