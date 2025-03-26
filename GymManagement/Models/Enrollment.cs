using Microsoft.CodeAnalysis;
using System.Numerics;

namespace GymManagement.Models
{
    public class Enrollment
    {
        public int ClientID { get; set; }
        public Client? Client { get; set; }

        public int GroupClassID { get; set; }
        public GroupClass? GroupClass { get; set; }
    }
}
