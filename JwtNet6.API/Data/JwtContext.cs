using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace JwtNet6.API.Data
{
    public class JwtContext : IdentityDbContext<ApplicationUser>
    {
        #pragma warning disable CS8618
        public JwtContext(DbContextOptions<JwtContext> options) : base(options) { 
        
        }
        #pragma warning restore CS8618 
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema(ClaseGlobal.EsquemaDB);

        }

        //Esta entidad representa la tabla UserRefreshTokens, que guarda tokens de actualización para usuarios válidos
        public DbSet<UserRefreshTokens> UserRefreshTokens { get; set; }
        public DbSet<TBT_TIPOS_DOCUM> TBT_TIPOS_DOCUM { get; set; }
    }
}
