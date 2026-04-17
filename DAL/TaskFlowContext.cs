using Microsoft.EntityFrameworkCore;
using DAL.Entities;
using Task = DAL.Entities.Task;

namespace DAL
{
    
    public class TaskFlowContext : DbContext
    {
        private string _connectionString = "";
        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }


        public TaskFlowContext(string connectionString) {
            _connectionString = connectionString;
        }

        public TaskFlowContext(DbContextOptions<TaskFlowContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();

            if (!string.IsNullOrWhiteSpace(this._connectionString))
                optionsBuilder.UseSqlite(this._connectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
