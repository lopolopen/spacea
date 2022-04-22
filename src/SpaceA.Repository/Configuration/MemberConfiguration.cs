using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceA.Common;
using SpaceA.Model.Entities;

namespace SpaceA.Repository.Configuration
{
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            var salt = PasswordUtil.GenSalt();
            var pwd = PasswordUtil.HashPasswordWithMD5("admin");
            var finalPwd = PasswordUtil.SaltHashPasswordWithSHA1(pwd, salt);
            builder.HasData(new Member
            {
                Id = 1u,
                AccountName = "admin",
                Disabled = false,
                FirstName = "Admin",
                Password = finalPwd,
                Salt = salt
            });
        }
    }
}
