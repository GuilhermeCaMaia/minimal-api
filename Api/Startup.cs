using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTO;
using MinimalApi.Infraestrutura.Db;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        Key = Configuration.GetSection("Jwt").ToString() ?? "";
    }

    private string Key = "";

    public IConfiguration Configuration { get; set; } = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT aqui"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });


        services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("MySql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("mysql"))
            );
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endPoints =>
        {
            endPoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");

            string GerarTokenJwt(Administrador administrador)
            {
                if (string.IsNullOrEmpty(Key)) return string.Empty;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim("Nome", administrador.Nome),
                    new Claim("Email", administrador.Email),
                    new Claim("Perfil", administrador.Perfil),
                    new Claim(ClaimTypes.Role, administrador.Perfil)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endPoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
            {
                var adm = administradorServico.Login(loginDTO);
                if (adm != null)
                {
                    string token = GerarTokenJwt(adm);
                    return Results.Ok(new AdministradorLogado
                    {
                        Nome = adm.Nome,
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token
                    });
                }
                else
                    return Results.Unauthorized();
                }).AllowAnonymous().WithTags("Administradores");


            endPoints.MapPost("/Administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
            {
                // Mensagens de validação
                var validacao = new ErrosDeValidacao
                {
                    Erros = new List<string>()
                };

                if (string.IsNullOrEmpty(administradorDTO.Nome))
                    validacao.Erros.Add("O nome do administrador é obrigatório.");
                if (string.IsNullOrEmpty(administradorDTO.Email))
                    validacao.Erros.Add("O email do administrador é obrigatório.");
                if (string.IsNullOrEmpty(administradorDTO.Senha))
                    validacao.Erros.Add("A senha do administrador é obrigatória.");
                if (administradorDTO.Perfil == null)
                    validacao.Erros.Add("O perfil do administrador é obrigatório.");

                if (validacao.Erros.Count > 0)
                    return Results.BadRequest(validacao);

                var administrador = new Administrador
                {
                    Nome = administradorDTO.Nome,
                    Email = administradorDTO.Email,
                    Senha = administradorDTO.Senha,
                    Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                };
                administradorServico.Incluir(administrador);
                return Results.Created($"/administradores/{administrador.Id}", new AdministradorModelView
                {
                    Id = administrador.Id,
                    Nome = administrador.Nome,
                    Email = administrador.Email,
                    Perfil = administrador.Perfil
                });
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endPoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
            {
                var adms = new List<AdministradorModelView>();
                var administradores = administradorServico.Todos(pagina);
                foreach (var adm in administradores)
                {
                    adms.Add(new AdministradorModelView
                    {
                        Id = adm.Id,
                        Nome = adm.Nome,
                        Email = adm.Email,
                        Perfil = adm.Perfil
                    });
                }
                return Results.Ok(administradorServico.Todos(pagina));
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm"})
            .WithTags("Administradores");

            endPoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
            {
                var administrador = administradorServico.BuscarPorId(id);
                if (administrador == null) return Results.NotFound();
                return Results.Ok(new AdministradorModelView
                {
                    Id = administrador.Id,
                    Nome = administrador.Nome,
                    Email = administrador.Email,
                    Perfil = administrador.Perfil
                });
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
            {
                var validacao = new ErrosDeValidacao
                {
                    Erros = new List<string>()
                };
                if (string.IsNullOrEmpty(veiculoDTO.Nome))
                    validacao.Erros.Add("O nome do veiculo é obrigatório.");

                if (string.IsNullOrEmpty(veiculoDTO.Marca))
                    validacao.Erros.Add("A marca do veiculo é obrigatória.");

                if (veiculoDTO.Ano < 1886 || veiculoDTO.Ano > DateTime.Now.Year + 1)
                    validacao.Erros.Add("O ano do veiculo é inválido.");

                return validacao;
            }

            endPoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
            {
                // Mensagens de validação
                var validacao = validaDTO(veiculoDTO);
                if (validacao.Erros.Count > 0)
                    return Results.BadRequest(validacao);

                var veiculo = new Veiculo
                {
                    Nome = veiculoDTO.Nome,
                    Marca = veiculoDTO.Marca,
                    Ano = veiculoDTO.Ano
                };
                veiculoServico.Incluir(veiculo);

                return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Veiculos");

            endPoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
            {
                var veiculos = veiculoServico.Todos(pagina);
                return Results.Ok(veiculos);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Veiculos");

            endPoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(id);
                if (veiculo == null) return Results.NotFound();
                return Results.Ok(veiculo);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Veiculos");

            endPoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(id);
                if (veiculo == null) return Results.NotFound();

                // Mensagens de validação
                var validacao = validaDTO(veiculoDTO);
                if (validacao.Erros.Count > 0)
                    return Results.BadRequest(validacao);

                veiculo.Nome = veiculoDTO.Nome;
                veiculo.Marca = veiculoDTO.Marca;
                veiculo.Ano = veiculoDTO.Ano;

                veiculoServico.Atualizar(veiculo);

                return Results.Ok(veiculo);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veiculos");

            endPoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(id);
                if (veiculo == null) return Results.NotFound();

                veiculoServico.Apagar(veiculo);
                return Results.NoContent();
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veiculos");
            
        });
    }
}