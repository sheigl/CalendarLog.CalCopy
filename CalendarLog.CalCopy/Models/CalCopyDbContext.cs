using Microsoft.EntityFrameworkCore;
using System;

namespace CalendarLog.CalCopy.Models
{
    public class CalCopyDbContext : DbContext
    {
        public virtual DbSet<Settings> Settings { get; set; }

        public CalCopyDbContext(Func<DbContextOptionsBuilder, DbContextOptionsBuilder> builder)
            : this(builder(new DbContextOptionsBuilder()).Options)
        {

        }
        public CalCopyDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}
