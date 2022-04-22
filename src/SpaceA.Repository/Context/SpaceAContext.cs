using Microsoft.EntityFrameworkCore;
using SpaceA.Model.Entities;
using SpaceA.Repository.Configuration;

//TODO: 如果仅仅确保唯一，应该使用AlternateKey

namespace SpaceA.Repository.Context
{
    public sealed partial class SpaceAContext : DbContext
    {
        public SpaceAContext(DbContextOptions options) : base(options)
        {
            Database.SetCommandTimeout(200);
        }

#if MYSQL
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
#endif

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
            .HasOne(p => p.Owner)
            .WithMany(o => o.OwnedProjects)
            .HasForeignKey(p => p.OwnerId);

            modelBuilder.Entity<Project>()
            .HasOne(p => p.DefaultTeam)
            .WithOne(t => t.Project)
            .HasForeignKey<Project>(p => p.DefaultTeamId);

            //不允许有同名项目
            modelBuilder.Entity<Project>()
            .HasIndex(p => new { p.Name, p.DeletedFlag })
            .IsUnique();

            modelBuilder.Entity<Project>()
            .HasOne(p => p.RootFolder)
            .WithOne(f => f.Project)
            .HasForeignKey<Project>(p => p.RootFolderId);

            modelBuilder.Entity<Project>()
            .HasOne(p => p.RootIteration)
            .WithOne(i => i.Project)
            .HasForeignKey<Project>(p => p.RootIterationId);

            modelBuilder.Entity<ProjectHistory>()
            .HasKey(p => new { p.Id, p.Rev });

            //------------------------------------

            modelBuilder.Entity<Team>()
            .HasOne(t => t.DefaultFolder)
            .WithMany(f => f.Teams)
            .HasForeignKey(t => t.DefaultFolderId)
            .OnDelete(DeleteBehavior.SetNull);

            //同项目中不允许有同名团队
            modelBuilder.Entity<Team>()
            .HasIndex(t => new { t.ProjectId, t.Name })
            .IsUnique();

            modelBuilder.Entity<TeamFolder>()
            .HasKey(tf => new { tf.Id1, tf.Id2 });

            modelBuilder.Entity<TeamFolder>()
            .HasOne(tf => tf.Team)
            .WithMany(t => t.TeamFolders)
            .HasForeignKey(tf => tf.Id1);

            modelBuilder.Entity<TeamFolder>()
            .HasOne(tf => tf.Folder)
            .WithMany(f => f.TeamFolders)
            .HasForeignKey(tf => tf.Id2);

            modelBuilder.Entity<TeamIteration>()
            .HasKey(ti => new { ti.Id1, ti.Id2 });

            modelBuilder.Entity<TeamIteration>()
            .HasOne(ti => ti.Team)
            .WithMany(t => t.TeamIterations)
            .HasForeignKey(ti => ti.Id1);

            modelBuilder.Entity<TeamIteration>()
            .HasOne(ti => ti.Iteration)
            .WithMany(i => i.TeamIterations)
            .HasForeignKey(ti => ti.Id2);

            modelBuilder.Entity<TeamMember>()
            .HasKey(tm => new { tm.Id1, tm.Id2 });

            modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(t => t.TeamMembers)
            .HasForeignKey(tm => tm.Id1);

            modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.Member)
            .WithMany(m => m.TeamMembers)
            .HasForeignKey(tm => tm.Id2);


            //------------------------------------

            modelBuilder.Entity<Iteration>()
            .HasIndex(i => i.Path)
            .IsUnique();

            modelBuilder.Entity<Folder>()
            .HasIndex(f => f.Path)
            .IsUnique();

            //------------------------------------

            modelBuilder.Entity<Member>()
            .HasIndex(m => m.AccountName)
            .IsUnique();

            modelBuilder.Entity<WorkItem>()
            .HasOne(wi => wi.AssignedTo)
            .WithMany(m => m.WorkItems)
            .HasForeignKey(wi => wi.AssigneeId);

            modelBuilder.Entity<WorkItem>()
            .HasOne(wi => wi.Changer)
            .WithMany(m => m.ChangedWorkItems)
            .HasForeignKey(wi => wi.ChangerId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkItem>()
            .HasOne(wi => wi.Parent)
            .WithMany(wi => wi.Children)
            .HasForeignKey(wi => wi.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkItem>()
            .HasOne(wi => wi.Folder)
            .WithMany(f => f.WorkItems)
            .HasForeignKey(wi => wi.FolderId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkItem>()
            .HasOne(wi => wi.Iteration)
            .WithMany(i => i.WorkItems)
            .HasForeignKey(wi => wi.IterationId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkItemHistory>()
            .HasKey(h => new { h.Id, h.Rev });

            modelBuilder.Entity<Tag>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tags)
            .HasForeignKey(t => t.ProjectId);

            modelBuilder.Entity<WorkItem>()
            .Property(wi => wi.Rev)
            .HasDefaultValue(0);

            modelBuilder.Entity<Tag>()
            .HasOne(t => t.WorkItem)
            .WithMany(wi => wi.Tags)
            .HasForeignKey(t => t.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attachment>()
            .HasOne(a => a.AttachedTo)
            .WithMany(wi => wi.Attachments)
            .HasForeignKey(a => a.WorkItemId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Creator)
            .WithMany(m => m.Attachments)
            .HasForeignKey(a => a.CreatorId)
            .OnDelete(DeleteBehavior.SetNull);

            //------------------------------------

            modelBuilder.Entity<GroupMember>()
            .HasKey(gm => new { gm.Id1, gm.Id2 });

            modelBuilder.Entity<GroupMember>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.GroupMembers)
            .HasForeignKey(gm => gm.Id1);

            modelBuilder.Entity<GroupMember>()
            .HasOne(gm => gm.Member)
            .WithMany(m => m.GroupMembers)
            .HasForeignKey(gm => gm.Id2);

            //------------------------------------

            modelBuilder.Entity<Group>()
            .HasOne(g => g.Leader)
            .WithOne(m => m.Group)
            .HasForeignKey<Group>(g => g.LeaderId);

            modelBuilder.Entity<Group>(e => e.Property(g => g.Disabled)
            .HasDefaultValue(false));

            //------------------------------------

            modelBuilder.Entity<ProjectRepo>()
            .HasKey(pr => new { pr.Id1, pr.Id2 });

            modelBuilder.Entity<ProjectRepo>()
            .HasOne(pr => pr.Project)
            .WithMany(p => p.ProjectRepos)
            .HasForeignKey(pr => pr.Id1);

            modelBuilder.Entity<ProjectRepo>()
            .HasOne(pr => pr.Repo)
            .WithMany(r => r.ProjectRepos)
            .HasForeignKey(pr => pr.Id2);

            //------------------------------------

            modelBuilder.Entity<Config>()
            .HasKey(cf => new { cf.Id1, cf.Id2 });

            modelBuilder.Entity<Config>()
            .HasOne(cf => cf.Member)
            .WithMany(m => m.Configs)
            .HasForeignKey(cf => cf.Id1);

            //------------------------------------

            modelBuilder.Entity<PersonalAccessToken>()
                .HasOne(pat => pat.Owner)
                .WithMany(o => o.PersonalAccessTokens)
                .HasForeignKey(pat => pat.OwnerId);

            //------------------------------------

            modelBuilder.Entity<RemainingWork>()
            .HasKey(r => new
            {
                r.TeamId,
                r.IterationId,
                r.WorkItemType,
                r.AccountingDate
            });

            modelBuilder.Entity<RemainingWork>()
            .HasIndex(r => new { r.TeamId, r.IterationId })
            .IsUnique(false);

            //------------------------------------

            modelBuilder.Entity<MemberCapacity>()
            .HasKey(c => new
            {
                c.TeamId,
                c.IterationId,
                c.MemberId,
                c.Type
            });

            modelBuilder.Entity<MemberCapacity>()
            .HasIndex(c => new { c.TeamId, c.IterationId })
            .IsUnique(false);

            modelBuilder.ApplyConfiguration(new MemberConfiguration());
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectHistory> ProjectHistories { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamFolder> TeamFolders { get; set; }
        public DbSet<TeamIteration> TeamIterations { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Iteration> Iterations { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<WorkItemHistory> WorkItemHistories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Repo> Repos { get; set; }
        public DbSet<ProjectRepo> ProjectRepos { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<RemainingWork> RemainingWorks { get; set; }
        public DbSet<MemberCapacity> MemberCapacities { get; set; }
        public DbSet<PersonalAccessToken> PersonalAccessTokens { get; set; }
    }
}