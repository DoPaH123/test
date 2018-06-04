using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace test.Models
{
    public class UserPlaylist
    {
        public int ID { get; set; }
        public int SongID { get; set; }
        public Song Song { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
    }
}
