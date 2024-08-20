using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BookWorm.Identity.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options);
