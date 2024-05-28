using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class ConnectifyDbContext : IdentityDbContext<AppUser, AppRole, Guid,
        IdentityUserClaim<Guid>, AppUserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public ConnectifyDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Entities.File> Files { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Connection> Connections { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            builder.Entity<Contact>()
                .HasOne(c => c.ParticipantA)
                .WithMany(u => u.ContactsAsParticipantA)
                .HasForeignKey(c => c.ParticipantAId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Contact>()
                .HasOne(c => c.ParticipantB)
                .WithMany(u => u.ContactsAsParticipantB)
                .HasForeignKey(c => c.ParticipantBId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GroupMember>()
                .HasOne(c => c.Group)
                .WithMany(u => u.Members)
                .HasForeignKey(c => c.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
