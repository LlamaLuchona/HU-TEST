using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using CeplanAuth.Models;

namespace CeplanAuth.Data
{
    public interface IUsuarioRepository
    {
        Task<ResultadoLogin> ValidarLoginAsync(string tipoDoc, string numeroDoc, string contrasena, string ip);
        Task<UsuarioModel?> ObtenerUsuarioPorIdAsync(int id);
    }

    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CeplanDB")
                ?? throw new InvalidOperationException("Connection string 'CeplanDB' not found.");
        }

        public static string HashContrasena(string contrasena)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(contrasena));
            return Convert.ToHexString(bytes).ToLower();
        }

        public async Task<ResultadoLogin> ValidarLoginAsync(
            string tipoDoc, string numeroDoc, string contrasena, string ip)
        {
            var hash = HashContrasena(contrasena);

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.sp_ValidarLogin", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TipoDocumento", tipoDoc);
            cmd.Parameters.AddWithValue("@NumeroDoc", numeroDoc);
            cmd.Parameters.AddWithValue("@Contrasena", hash);
            cmd.Parameters.AddWithValue("@IPAddress", ip ?? "");

            var resultado = new ResultadoLogin();

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                resultado.Resultado  = reader["Resultado"]?.ToString() ?? "";
                resultado.Mensaje    = reader["Mensaje"]?.ToString() ?? "";
                resultado.CVF        = reader["CVF"] != DBNull.Value ? Convert.ToInt32(reader["CVF"]) : 0;
                resultado.UsuarioId  = reader["UsuarioId"] != DBNull.Value
                    ? Convert.ToInt32(reader["UsuarioId"])
                    : null;
            }

            return resultado;
        }

        public async Task<UsuarioModel?> ObtenerUsuarioPorIdAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.sp_ObtenerUsuario", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UsuarioId", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new UsuarioModel
            {
                Id                  = Convert.ToInt32(reader["Id"]),
                TipoDocumento       = reader["TipoDocumento"]?.ToString() ?? "",
                NumeroDocumento     = reader["NumeroDocumento"]?.ToString() ?? "",
                Nombres             = reader["Nombres"]?.ToString() ?? "",
                PrimerApellido      = reader["PrimerApellido"]?.ToString() ?? "",
                SegundoApellido     = reader["SegundoApellido"]?.ToString(),
                Cargo               = reader["Cargo"]?.ToString(),
                Dependencia         = reader["Dependencia"]?.ToString(),
                Sexo                = reader["Sexo"]?.ToString(),
                FechaNacimiento     = reader["FechaNacimiento"] != DBNull.Value
                    ? Convert.ToDateTime(reader["FechaNacimiento"]) : null,
                CorreoPrincipal     = reader["CorreoPrincipal"]?.ToString(),
                CorreoSecundario    = reader["CorreoSecundario"]?.ToString(),
                TelefonoMovil       = reader["TelefonoMovil"]?.ToString(),
                TelefonoSecundario  = reader["TelefonoSecundario"]?.ToString(),
                Nacionalidad        = reader["Nacionalidad"]?.ToString(),
                TipoContratacion    = reader["TipoContratacion"]?.ToString(),
                FechaContratacion   = reader["FechaContratacion"] != DBNull.Value
                    ? Convert.ToDateTime(reader["FechaContratacion"]) : null,
                Estado              = reader["Estado"]?.ToString() ?? "Activo",
                CVF                 = Convert.ToInt32(reader["CVF"]),
                FechaUltimoAcceso   = reader["FechaUltimoAcceso"] != DBNull.Value
                    ? Convert.ToDateTime(reader["FechaUltimoAcceso"]) : null
            };
        }
    }
}
