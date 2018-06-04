using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace test.Models
{
    public class GeneralContext:DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<UserPlaylist> UserPlaylist { get; set; }     
        public GeneralContext(DbContextOptions<GeneralContext> options) : base(options)
       { }

       
    }
}
